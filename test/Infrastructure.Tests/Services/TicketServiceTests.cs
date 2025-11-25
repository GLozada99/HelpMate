using Application.DTOs.Board.Board;
using Application.DTOs.Board.BoardMembership;
using Application.DTOs.Ticket.Ticket;
using Application.Errors;
using Domain.Entities.Board;
using Domain.Entities.User;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Context;
using Infrastructure.Services.Auth;
using Infrastructure.Services.Board;
using Infrastructure.Services.Ticket;
using Infrastructure.Services.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Infrastructure.Tests.Services;

public class TicketServiceTests
{
    private readonly BoardService _boardService;
    private readonly HelpMateDbContext _db;
    private readonly TicketService _ticketService;
    private readonly UserService _userService;

    public TicketServiceTests()
    {
        var options = new DbContextOptionsBuilder<HelpMateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new HelpMateDbContext(options);
        _userService = new UserService(_db, new NullLogger<UserService>(),
            new PasswordHasher());
        _boardService = new BoardService(_db, new NullLogger<BoardService>());
        _ticketService = new TicketService(_db, new NullLogger<TicketService>());
    }

    private async Task<User> CreateUser(string differentiator,
        UserRole role = UserRole.Admin)
    {
        var user = new User
        {
            FullName = $"User{differentiator}",
            Email = $"user{differentiator}@mail.com",
            Password = "pass",
            Status = UserStatus.Active,
            Role = role
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return user;
    }

    [Theory]
    [InlineData(MembershipRole.Editor)]
    [InlineData(MembershipRole.Agent)]
    [InlineData(MembershipRole.Owner)]
    public async Task CreateTicket_ShouldSucceed_ForAllowedRoles(MembershipRole role)
    {
        // Arrange
        var user = await CreateUser(role.ToString());
        var boardResult = await _boardService.CreateBoard(new CreateBoardDTO
        {
            Code = $"BRD_{role}",
            Name = "Support",
            Description = "Support tickets"
        }, user.Id);
        boardResult.Errors.Should().BeEmpty();
        boardResult.IsSuccess.Should().BeTrue();

        var membership = await _db.BoardMemberships
            .FirstAsync(m => m.BoardId == boardResult.Value.Id && m.UserId == user.Id);

        membership.Role = role;
        await _db.SaveChangesAsync();

        var dto = new CreateTicketDTO
        {
            Title = "Test ticket"
        };

        // Act
        var result =
            await _ticketService.CreateTicket(boardResult.Value.Id, dto, user.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var ticket = await _db.Tickets.FirstAsync();
        ticket.Title.Should().Be("Test ticket");
        ticket.ReporterId.Should().Be(user.Id);
    }

    [Fact]
    public async Task CreateTicket_ShouldFail_ForViewer()
    {
        // Arrange
        var user = await CreateUser("bla");

        var boardResult = await _boardService.CreateBoard(new CreateBoardDTO
        {
            Code = "VIEWBRD",
            Name = "X",
            Description = "Y"
        }, user.Id);
        boardResult.Errors.Should().BeEmpty();
        boardResult.IsSuccess.Should().BeTrue();
        var board = boardResult.Value;

        var membership = await _db.BoardMemberships
            .FirstAsync(m => m.BoardId == board.Id && m.UserId == user.Id);

        membership.Role = MembershipRole.Viewer;
        await _db.SaveChangesAsync();

        var dto = new CreateTicketDTO { Title = "Test" };

        // Act
        var result = await _ticketService.CreateTicket(board.Id, dto, user.Id);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<InsufficientUserMembershipError>();
    }

    [Fact]
    public async Task CreateTicket_ShouldFail_WhenRequesterIsInactive()
    {
        // Arrange
        var user = await CreateUser("inactive");
        user.Status = UserStatus.Inactive;
        await _db.SaveChangesAsync();

        var boardResult = await _boardService.CreateBoard(new CreateBoardDTO
        {
            Code = "INACTIVE1",
            Name = "Board",
            Description = "Desc"
        }, user.Id);

        boardResult.IsSuccess.Should().BeTrue();
        var board = boardResult.Value;

        var membership = await _db.BoardMemberships
            .FirstAsync(m => m.BoardId == board.Id && m.UserId == user.Id);

        membership.Role = MembershipRole.Agent;
        await _db.SaveChangesAsync();

        var dto = new CreateTicketDTO
        {
            Title = "Should fail"
        };

        // Act
        var result = await _ticketService.CreateTicket(board.Id, dto, user.Id);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<InsufficientUserMembershipError>();
    }

    [Fact]
    public async Task CreateTicket_ShouldFail_WhenBoardIsInactive()
    {
        // Arrange
        var user = await CreateUser("inactive");

        var boardResult = await _boardService.CreateBoard(new CreateBoardDTO
        {
            Code = "INACTIVE1",
            Name = "Board",
            Description = "Desc"
        }, user.Id);

        boardResult.IsSuccess.Should().BeTrue();
        var board = _db.Boards.First();
        board.Status = BoardStatus.Inactive;
        await _db.SaveChangesAsync();

        var membership = await _db.BoardMemberships
            .FirstAsync(m => m.BoardId == board.Id && m.UserId == user.Id);

        membership.Role = MembershipRole.Agent;
        await _db.SaveChangesAsync();

        var dto = new CreateTicketDTO
        {
            Title = "Should fail"
        };

        // Act
        var result = await _ticketService.CreateTicket(board.Id, dto, user.Id);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<InsufficientUserMembershipError>();
    }

    [Fact]
    public async Task CreateTicket_ShouldFail_WhenNoMembershipForBoard()
    {
        // Arrange
        var user = await CreateUser("inactive");

        var boardResult = await _boardService.CreateBoard(new CreateBoardDTO
        {
            Code = "INACTIVE1",
            Name = "Board",
            Description = "Desc"
        }, user.Id);

        boardResult.IsSuccess.Should().BeTrue();
        var board = boardResult.Value;

        var membership = await _db.BoardMemberships
            .FirstAsync(m => m.BoardId == board.Id && m.UserId == user.Id);
        _db.Remove(membership);
        await _db.SaveChangesAsync();

        var dto = new CreateTicketDTO
        {
            Title = "Should fail"
        };

        // Act
        var result = await _ticketService.CreateTicket(board.Id, dto, user.Id);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<InsufficientUserMembershipError>();
    }

    [Fact]
    public async Task GetTicket_ShouldSucceed_WhenRequesterIsMember()
    {
        // Arrange
        var user = await CreateUser("get1");

        var boardResult = await _boardService.CreateBoard(new CreateBoardDTO
        {
            Code = "BRD_GET",
            Name = "Board",
            Description = "Desc"
        }, user.Id);

        var board = boardResult.Value;

        var membership = await _db.BoardMemberships
            .FirstAsync(m => m.BoardId == board.Id && m.UserId == user.Id);
        membership.Role = MembershipRole.Agent;
        await _db.SaveChangesAsync();

        // Create ticket
        var ticketResult = await _ticketService.CreateTicket(board.Id,
            new CreateTicketDTO { Title = "Hello" },
            user.Id);

        ticketResult.IsSuccess.Should().BeTrue();
        var ticket = ticketResult.Value;

        // Act
        var result = await _ticketService.GetTicket(board.Id, ticket.Id, user.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(ticket.Id);
        result.Value.Title.Should().Be("Hello");
    }

    [Fact]
    public async Task GetTicket_ShouldFail_WhenRequesterIsNotMember()
    {
        // Arrange
        var owner = await CreateUser("owner");
        var outsider = await CreateUser("outsider");

        var boardResult = await _boardService.CreateBoard(new CreateBoardDTO
        {
            Code = "BRD_G2",
            Name = "Board",
            Description = "Desc"
        }, owner.Id);

        var board = boardResult.Value;

        // Owner makes ticket
        var ticketResult = await _ticketService.CreateTicket(board.Id,
            new CreateTicketDTO { Title = "Private" },
            owner.Id);

        ticketResult.IsSuccess.Should().BeTrue();
        var ticket = ticketResult.Value;

        // Act
        var result = await _ticketService.GetTicket(board.Id, ticket.Id, outsider.Id);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<InsufficientUserMembershipError>();
    }

    [Fact]
    public async Task GetTicket_ShouldFail_WhenTicketDoesNotExist()
    {
        // Arrange
        var user = await CreateUser("get3");

        var boardResult = await _boardService.CreateBoard(new CreateBoardDTO
        {
            Code = "BRD_G3",
            Name = "Board",
            Description = "Desc"
        }, user.Id);

        var board = boardResult.Value;
        await _db.SaveChangesAsync();

        // Act
        var result = await _ticketService.GetTicket(board.Id, 9999, user.Id);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<TicketNotFoundError>();
    }

    [Fact]
    public async Task GetTicket_ShouldFail_WhenTicketBelongsToAnotherBoard()
    {
        // Arrange
        var user = await CreateUser("get4");

        var board1 = (await _boardService.CreateBoard(new CreateBoardDTO
        {
            Code = "BRD_A",
            Name = "A",
            Description = "A"
        }, user.Id)).Value;

        var board2 = (await _boardService.CreateBoard(new CreateBoardDTO
        {
            Code = "BRD_B",
            Name = "B",
            Description = "B"
        }, user.Id)).Value;

        var ticket = (await _ticketService.CreateTicket(board1.Id,
            new CreateTicketDTO { Title = "T1" },
            user.Id)).Value;

        var result = await _ticketService.GetTicket(board2.Id, ticket.Id, user.Id);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<TicketNotFoundError>();
    }

    [Fact]
    public async Task GetTickets_ShouldReturnTickets_WhenRequesterIsMember()
    {
        // Arrange
        var user = await CreateUser("list1");

        var board = (await _boardService.CreateBoard(
            new CreateBoardDTO
                { Code = "BRD_LIST1", Name = "Board", Description = "Desc" },
            user.Id)).Value;
        await _db.SaveChangesAsync();

        await _ticketService.CreateTicket(board.Id,
            new CreateTicketDTO { Title = "T1" }, user.Id);
        await _ticketService.CreateTicket(board.Id,
            new CreateTicketDTO { Title = "T2" }, user.Id);

        // Act
        var result = await _ticketService.GetTickets(board.Id, user.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var list = result.Value.ToList();
        list.Should().HaveCount(2);
        list.Select(t => t.Title).Should().Contain(["T1", "T2"]);
    }

    [Fact]
    public async Task GetTickets_ShouldFail_WhenRequesterIsNotMember()
    {
        // Arrange
        var owner = await CreateUser("owner_list2");
        var outsider = await CreateUser("outsider_list2");

        var board = (await _boardService.CreateBoard(
            new CreateBoardDTO
                { Code = "BRD_LIST2", Name = "Board", Description = "Desc" },
            owner.Id)).Value;

        // Act
        var result = await _ticketService.GetTickets(board.Id, outsider.Id);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<InsufficientUserMembershipError>();
    }

    [Fact]
    public async Task GetTickets_ShouldReturnEmptyList_WhenNoTicketsExist()
    {
        // Arrange
        var user = await CreateUser("list3");

        var board = (await _boardService.CreateBoard(
            new CreateBoardDTO
                { Code = "BRD_LIST3", Name = "Board", Description = "Desc" },
            user.Id)).Value;

        var membership = await _db.BoardMemberships
            .FirstAsync(m => m.BoardId == board.Id && m.UserId == user.Id);
        membership.Role = MembershipRole.Editor;
        await _db.SaveChangesAsync();

        // Act
        var result = await _ticketService.GetTickets(board.Id, user.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AsEnumerable().Should().BeEmpty();
    }

    [Fact]
    public async Task GetTickets_ShouldReturnCorrectAssignee()
    {
        // Arrange
        var owner = await CreateUser("list4_owner");
        var agent = await CreateUser("list4_agent");

        var board = (await _boardService.CreateBoard(
            new CreateBoardDTO
                { Code = "BRD_LIST4", Name = "Board", Description = "Desc" },
            owner.Id)).Value;

        await _boardService.CreateBoardMembership(board.Id, new CreateBoardMembershipDTO
        {
            Role = MembershipRole.Agent,
            UserId = agent.Id
        }, owner.Id);

        var ticketResult = await _ticketService.CreateTicket(
            board.Id,
            new CreateTicketDTO { Title = "Ticket A", AssigneeId = agent.Id },
            owner.Id);
        ticketResult.Errors.Should().BeEmpty();
        ticketResult.IsSuccess.Should().BeTrue();

        // Act
        var result = await _ticketService.GetTickets(board.Id, owner.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var ticketList = result.Value.ToList();
        ticketList.Should().HaveCount(1);

        var ticket = ticketList[0];
        ticket.Title.Should().Be("Ticket A");

        ticket.Assignee.Should().NotBeNull();
        ticket.Assignee!.Id.Should().Be(agent.Id);
        ticket.Assignee.Email.Should().Be(agent.Email);
    }

    [Fact]
    public async Task GetTickets_ShouldReturnNullAssignee_WhenNoAssignee()
    {
        // Arrange
        var user = await CreateUser("list5");

        var board = (await _boardService.CreateBoard(
            new CreateBoardDTO
                { Code = "BRD_LIST5", Name = "Board", Description = "Desc" },
            user.Id)).Value;

        await _ticketService.CreateTicket(
            board.Id,
            new CreateTicketDTO { Title = "No Assignee" },
            user.Id);

        // Act
        var result = await _ticketService.GetTickets(board.Id, user.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var ticket = result.Value.First();
        ticket.Assignee.Should().BeNull();
    }

    [Fact]
    public async Task UpdateTicket_ShouldUpdateFields_WhenRequesterCanEdit()
    {
        // Arrange
        var user = await CreateUser("upd1");

        var board = (await _boardService.CreateBoard(
            new CreateBoardDTO { Code = "UPD1", Name = "Board", Description = "Desc" },
            user.Id)).Value;

        var ticket = (await _ticketService.CreateTicket(
            board.Id,
            new CreateTicketDTO { Title = "Old", Description = "Old desc" },
            user.Id)).Value;

        var dto = new UpdateTicketDTO
        {
            Title = "New Title",
            Description = "New Desc",
            Status = TicketStatus.InProgress,
            Priority = TicketPriority.Critical
        };

        // Act
        var result =
            await _ticketService.UpdateTicket(board.Id, ticket.Id, dto, user.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("New Title");
        result.Value.Description.Should().Be("New Desc");
    }

    [Fact]
    public async Task UpdateTicket_ShouldFail_WhenRequesterIsViewer()
    {
        // Arrange
        var user = await CreateUser("upd2");

        var board = (await _boardService.CreateBoard(
            new CreateBoardDTO { Code = "UPD2", Name = "Board", Description = "Desc" },
            user.Id)).Value;

        var ticket = (await _ticketService.CreateTicket(
            board.Id,
            new CreateTicketDTO { Title = "Test" },
            user.Id)).Value;

        var membership = await _db.BoardMemberships
            .FirstAsync(m => m.BoardId == board.Id && m.UserId == user.Id);
        membership.Role = MembershipRole.Viewer;
        await _db.SaveChangesAsync();
        var dto = new UpdateTicketDTO { Title = "Try update" };

        // Act
        var result =
            await _ticketService.UpdateTicket(board.Id, ticket.Id, dto, user.Id);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<InsufficientUserMembershipError>();
    }

    [Fact]
    public async Task UpdateTicket_ShouldFail_WhenRequesterInactive()
    {
        // Arrange
        var user = await CreateUser("upd3");

        var board = (await _boardService.CreateBoard(
            new CreateBoardDTO { Code = "UPD3", Name = "Board", Description = "Desc" },
            user.Id)).Value;

        var ticket = (await _ticketService.CreateTicket(
            board.Id,
            new CreateTicketDTO { Title = "Old" },
            user.Id)).Value;

        user.Status = UserStatus.Inactive;
        await _db.SaveChangesAsync();
        var dto = new UpdateTicketDTO { Title = "New" };

        // Act
        var result =
            await _ticketService.UpdateTicket(board.Id, ticket.Id, dto, user.Id);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<InsufficientUserMembershipError>();
    }

    [Fact]
    public async Task UpdateTicket_ShouldUpdateReporter_WhenValid()
    {
        // Arrange
        var owner = await CreateUser("upd6_owner");
        var newReporter = await CreateUser("upd6_reporter", UserRole.Agent);

        var board = (await _boardService.CreateBoard(
            new CreateBoardDTO { Code = "UPD6", Name = "Board", Description = "Desc" },
            owner.Id)).Value;

        _db.BoardMemberships.Add(new BoardMembership
        {
            BoardId = board.Id,
            UserId = newReporter.Id,
            Role = MembershipRole.Agent
        });
        await _db.SaveChangesAsync();

        // create ticket
        var ticket = (await _ticketService.CreateTicket(
            board.Id,
            new CreateTicketDTO { Title = "Init" },
            owner.Id)).Value;

        var dto = new UpdateTicketDTO { ReporterId = newReporter.Id };

        // Act
        var result =
            await _ticketService.UpdateTicket(board.Id, ticket.Id, dto, owner.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Reporter.Id.Should().Be(newReporter.Id);
    }

    [Fact]
    public async Task UpdateTicket_ShouldFail_WhenReporterNotAllowed()
    {
        // Arrange
        var owner = await CreateUser("upd7_owner");
        var viewer = await CreateUser("upd7_viewer", UserRole.Agent);

        var board = (await _boardService.CreateBoard(
            new CreateBoardDTO { Code = "UPD7", Name = "Board", Description = "Desc" },
            owner.Id)).Value;

        _db.BoardMemberships.Add(new BoardMembership
        {
            BoardId = board.Id,
            UserId = viewer.Id,
            Role = MembershipRole.Viewer
        });
        await _db.SaveChangesAsync();

        var ticket = (await _ticketService.CreateTicket(
            board.Id,
            new CreateTicketDTO { Title = "Init" },
            owner.Id)).Value;

        var dto = new UpdateTicketDTO { ReporterId = viewer.Id };

        // Act
        var result =
            await _ticketService.UpdateTicket(board.Id, ticket.Id, dto, owner.Id);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<InsufficientUserMembershipError>();
    }

    [Fact]
    public async Task UpdateTicket_ShouldUpdateAssignee_WhenValid()
    {
        // Arrange
        var owner = await CreateUser("upd8_owner");
        var agent = await CreateUser("upd8_agent", UserRole.Agent);

        var board = (await _boardService.CreateBoard(
            new CreateBoardDTO { Code = "UPD8", Name = "Board", Description = "Desc" },
            owner.Id)).Value;

        _db.BoardMemberships.Add(new BoardMembership
        {
            BoardId = board.Id,
            UserId = agent.Id,
            Role = MembershipRole.Agent
        });
        await _db.SaveChangesAsync();

        var ticket = (await _ticketService.CreateTicket(
            board.Id,
            new CreateTicketDTO { Title = "Init" },
            owner.Id)).Value;

        var dto = new UpdateTicketDTO { AssigneeId = agent.Id };

        // Act
        var result =
            await _ticketService.UpdateTicket(board.Id, ticket.Id, dto, owner.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Assignee.Should().NotBeNull();
        result.Value.Assignee!.Id.Should().Be(agent.Id);
    }

    [Fact]
    public async Task UpdateTicket_ShouldFail_WhenAssigneeNotAllowed()
    {
        // Arrange
        var owner = await CreateUser("upd9_owner");
        var viewer = await CreateUser("upd9_viewer");

        var board = (await _boardService.CreateBoard(
            new CreateBoardDTO { Code = "UPD9", Name = "Board", Description = "Desc" },
            owner.Id)).Value;

        _db.BoardMemberships.Add(new BoardMembership
        {
            BoardId = board.Id,
            UserId = viewer.Id,
            Role = MembershipRole.Viewer
        });
        await _db.SaveChangesAsync();

        var ticket = (await _ticketService.CreateTicket(
            board.Id,
            new CreateTicketDTO { Title = "Init" },
            owner.Id)).Value;

        var dto = new UpdateTicketDTO { AssigneeId = viewer.Id };

        // Act
        var result =
            await _ticketService.UpdateTicket(board.Id, ticket.Id, dto, owner.Id);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<InsufficientUserMembershipError>();
    }
}
