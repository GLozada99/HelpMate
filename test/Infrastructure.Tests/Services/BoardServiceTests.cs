using Application.DTOs.Board.Board;
using Application.DTOs.Board.BoardMembership;
using Application.Errors;
using Domain.Entities.Board;
using Domain.Entities.User;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Context;
using Infrastructure.Services.Board;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Infrastructure.Tests.Services;

public class BoardServiceTests
{
    private HelpMateDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<HelpMateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new HelpMateDbContext(options);
    }

    private BoardService CreateService(HelpMateDbContext context)
    {
        return new BoardService(context, new NullLogger<BoardService>());
    }

    private User CreateUser(int id, UserRole role = UserRole.Admin)
    {
        return new User
        {
            FullName = $"User{id}",
            Email = $"user{id}@mail.com",
            Password = "pass",
            Status = UserStatus.Active,
            Role = role
        };
    }

    [Fact]
    public async Task CreateBoard_ShouldCreateBoard_AndAssignOwnerMembership()
    {
        // Arrange
        var db = CreateDbContext();
        var service = CreateService(db);

        var creator = CreateUser(1);
        db.Users.Add(creator);
        await db.SaveChangesAsync();

        var dto = new CreateBoardDTO
        {
            Code = "ABCD",
            Name = "Support",
            Description = "Support Board"
        };

        // Act
        var result = await service.CreateBoard(dto, creator.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        db.Boards.Count().Should().Be(1);
        db.BoardMemberships.Count().Should().Be(1);

        var membership = db.BoardMemberships.First();
        membership.UserId.Should().Be(creator.Id);
        membership.Role.Should().Be(MembershipRole.Owner);
    }

    [Fact]
    public async Task
        CreateBoard_ShouldCreateBoard_AndAssignOwnerMembershipToAllAdminUsers()
    {
        // Arrange
        var db = CreateDbContext();
        var service = CreateService(db);

        var creator = CreateUser(1);
        db.Users.Add(creator);

        var superAdmin1 = CreateUser(2, UserRole.SuperAdmin);
        var superAdmin2 = CreateUser(3, UserRole.SuperAdmin);
        db.Users.Add(superAdmin1);
        db.Users.Add(superAdmin2);
        await db.SaveChangesAsync();

        var dto = new CreateBoardDTO
        {
            Code = "ABCD",
            Name = "Support",
            Description = "Support Board"
        };

        // Act
        var result = await service.CreateBoard(dto, creator.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        db.Boards.Count().Should().Be(1);
        db.BoardMemberships.Count().Should().Be(3);

        var memberships = db.BoardMemberships.OrderBy(x => x.CreatedAt).ToList();
        memberships[0].UserId.Should().Be(creator.Id);
        memberships[1].UserId.Should().Be(superAdmin1.Id);
        memberships[2].UserId.Should().Be(superAdmin2.Id);
        memberships.Should().AllSatisfy(m => m.Role.Should().Be(MembershipRole.Owner));
    }

    [Fact]
    public async Task CreateBoard_ShouldFail_WhenUserDoesNotExist()
    {
        var db = CreateDbContext();
        var service = CreateService(db);

        var dto = new CreateBoardDTO
        {
            Code = "ABCD",
            Name = "X",
            Description = "Y"
        };

        var result = await service.CreateBoard(dto, 999);

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<UserNotFoundError>();
    }

    [Fact]
    public async Task CreateBoard_ShouldFail_WhenCodeAlreadyExists()
    {
        var db = CreateDbContext();
        var service = CreateService(db);

        var creator = CreateUser(1);
        db.Users.Add(creator);

        db.Boards.Add(new Board
        {
            Id = 10,
            Code = "DUPL",
            Name = "Existing",
            Description = "Existing",
            CreatedById = creator.Id
        });

        await db.SaveChangesAsync();

        var dto = new CreateBoardDTO
        {
            Code = "DUPL",
            Name = "New",
            Description = "Desc"
        };

        var result = await service.CreateBoard(dto, creator.Id);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetBoard_ShouldReturnBoard_WhenRequesterIsMember()
    {
        var db = CreateDbContext();
        var service = CreateService(db);

        var user = CreateUser(1);
        db.Users.Add(user);

        var board = new Board
        {
            Id = 10,
            Code = "ZZZZ",
            Name = "Main Board",
            Description = "Desc",
            CreatedById = user.Id,
            Status = BoardStatus.Active
        };
        db.Boards.Add(board);

        var membership = new BoardMembership
        {
            BoardId = board.Id,
            UserId = user.Id,
            Role = MembershipRole.Agent
        };
        db.BoardMemberships.Add(membership);

        await db.SaveChangesAsync();

        var result = await service.GetBoard(board.Id, user.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(board.Id);
    }

    [Fact]
    public async Task GetBoard_ShouldFail_WhenBoardDoesNotExist()
    {
        var db = CreateDbContext();
        var service = CreateService(db);

        var requester = CreateUser(1);
        db.Users.Add(requester);
        await db.SaveChangesAsync();

        var result = await service.GetBoard(999, requester.Id);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetBoard_ShouldFail_WhenRequesterNotMember()
    {
        var db = CreateDbContext();
        var service = CreateService(db);

        var owner = CreateUser(1);
        var requester = CreateUser(2);

        db.Users.AddRange(owner, requester);

        var board = new Board
        {
            Id = 30,
            Code = "MEMB",
            Name = "Board",
            Description = "Desc",
            CreatedById = owner.Id
        };
        db.Boards.Add(board);

        db.BoardMemberships.Add(new BoardMembership
        {
            BoardId = board.Id,
            UserId = owner.Id,
            Role = MembershipRole.Owner
        });

        await db.SaveChangesAsync();

        var result = await service.GetBoard(board.Id, requester.Id);

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<InsufficientUserMembershipError>();
    }

    [Fact]
    public async Task UpdateBoard_ShouldUpdate_WhenRequesterIsOwner()
    {
        var db = CreateDbContext();
        var service = CreateService(db);

        var owner = CreateUser(1);
        db.Users.Add(owner);

        var board = new Board
        {
            Id = 30,
            Code = "UUUU",
            Name = "Old",
            Description = "Old Desc",
            CreatedById = owner.Id,
            Status = BoardStatus.Active
        };
        db.Boards.Add(board);

        var membership = new BoardMembership
        {
            BoardId = board.Id,
            UserId = owner.Id,
            Role = MembershipRole.Owner
        };
        db.BoardMemberships.Add(membership);

        await db.SaveChangesAsync();

        var dto = new UpdateBoardDTO
        {
            Name = "New Name",
            Description = "New Desc",
            Status = UpdateBoardStatus.Active
        };

        var result = await service.UpdateBoard(board.Id, dto, owner.Id);

        result.IsSuccess.Should().BeTrue();
        board.Name.Should().Be("New Name");
        board.Description.Should().Be("New Desc");
    }

    [Fact]
    public async Task UpdateBoard_ShouldFail_WhenRequesterIsNotOwner()
    {
        var db = CreateDbContext();
        var service = CreateService(db);

        var owner = CreateUser(1);
        var member = CreateUser(2);
        db.Users.AddRange(owner, member);

        var board = new Board
        {
            Id = 40,
            Code = "UPDT",
            Name = "Old",
            Description = "Old",
            CreatedById = owner.Id
        };
        db.Boards.Add(board);

        db.BoardMemberships.Add(new BoardMembership
        {
            BoardId = board.Id,
            UserId = owner.Id,
            Role = MembershipRole.Owner
        });
        db.BoardMemberships.Add(new BoardMembership
        {
            BoardId = board.Id,
            UserId = member.Id,
            Role = MembershipRole.Agent
        });

        await db.SaveChangesAsync();

        var dto = new UpdateBoardDTO
        {
            Name = "New",
            Description = "New",
            Status = UpdateBoardStatus.Active
        };

        var result = await service.UpdateBoard(board.Id, dto, member.Id);

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<InsufficientUserMembershipError>();
    }

    [Fact]
    public async Task UpdateBoard_ShouldFail_WhenBoardDoesNotExist()
    {
        var db = CreateDbContext();
        var service = CreateService(db);

        var user = CreateUser(1);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var dto = new UpdateBoardDTO
        {
            Name = "X",
            Description = "Y",
            Status = UpdateBoardStatus.Active
        };

        var result = await service.UpdateBoard(999, dto, user.Id);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task CreateBoardMembership_ShouldAddMember_WhenRequesterIsOwner()
    {
        var db = CreateDbContext();
        var service = CreateService(db);

        var owner = CreateUser(1);
        var target = CreateUser(2, UserRole.Agent);
        db.Users.AddRange(owner, target);

        var board = new Board
        {
            Id = 50,
            Code = "JOHN",
            Name = "Team",
            Description = "Desc",
            CreatedById = owner.Id
        };
        db.Boards.Add(board);

        db.BoardMemberships.Add(new BoardMembership
        {
            BoardId = board.Id,
            UserId = owner.Id,
            Role = MembershipRole.Owner
        });

        await db.SaveChangesAsync();

        var dto = new CreateBoardMembershipDTO
        {
            UserId = target.Id,
            Role = MembershipRole.Agent
        };

        var result = await service.CreateBoardMembership(board.Id, dto, owner.Id);

        result.IsSuccess.Should().BeTrue();
        db.BoardMemberships.Count().Should().Be(2);
    }

    [Fact]
    public async Task RemoveBoardMembership_ShouldRemove_WhenNotLastOwner()
    {
        var db = CreateDbContext();
        var service = CreateService(db);

        var owner1 = CreateUser(1);
        var owner2 = CreateUser(2);
        db.Users.AddRange(owner1, owner2);

        var board = new Board
        {
            Id = 60,
            Code = "RMVE",
            Name = "Board",
            Description = "Desc",
            CreatedById = owner1.Id
        };
        db.Boards.Add(board);

        db.BoardMemberships.Add(new BoardMembership
        {
            Id = 10,
            BoardId = board.Id,
            UserId = owner1.Id,
            Role = MembershipRole.Owner
        });
        db.BoardMemberships.Add(new BoardMembership
        {
            Id = 11,
            BoardId = board.Id,
            UserId = owner2.Id,
            Role = MembershipRole.Owner
        });

        await db.SaveChangesAsync();

        var result = await service.RemoveBoardMembership(10, owner2.Id);

        result.IsSuccess.Should().BeTrue();
        db.BoardMemberships.Count().Should().Be(1);
    }

    [Fact]
    public async Task RemoveBoardMembership_ShouldFail_WhenLastOwner()
    {
        var db = CreateDbContext();
        var service = CreateService(db);

        var owner = CreateUser(1);
        db.Users.AddRange(owner);
        await db.SaveChangesAsync();

        var dto = new CreateBoardDTO
        {
            Code = "ABCD",
            Name = "Support",
            Description = "Support Board"
        };
        await service.CreateBoard(dto, owner.Id);

        var membership = await db.BoardMemberships.FirstOrDefaultAsync();
        membership.Should().NotBeNull();

        var result = await service.RemoveBoardMembership(membership.Id, owner.Id);

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<InvalidBoardState>();
        db.BoardMemberships.Count().Should().Be(1);
    }
}
