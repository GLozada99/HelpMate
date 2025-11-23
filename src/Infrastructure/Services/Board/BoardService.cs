using Application.DTOs.Board;
using Application.DTOs.Board.Board;
using Application.DTOs.Board.Membership;
using Application.Errors;
using Application.Interfaces.Board;
using Domain.Entities.Board;
using Domain.Enums;
using FluentResults;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Board;

public class BoardService(
    HelpMateDbContext context,
    ILogger<BoardService> logger
) : IBoardService
{
    public async Task<Result<BoardDTO>> CreateBoard(CreateBoardDTO dto, int userId)
    {
        var userResult = await GetUser(userId);

        if (userResult.IsFailed)
        {
            logger.LogWarning(
                "Cannot create Board because User with Id '{UserId}' does not exist.",
                userId
            );
            return Result.Fail(userResult.Errors);
        }

        var userCanCreateBoardResult = ValidateUserCanCreateBoard(userResult.Value);
        if (userCanCreateBoardResult.IsFailed)
        {
            logger.LogWarning(
                "Cannot create Board because User with Id '{UserId}' does not have permissions for it.",
                userId
            );
            return Result.Fail(userResult.Errors);
        }

        var existingCode = await context.Boards
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Code == dto.Code);

        if (existingCode != null)
        {
            logger.LogWarning(
                "Cannot create Board with code '{Code}' because another board already exists with that code.",
                dto.Code
            );
            return Result.Fail(new BoardCodeAlreadyExistsError(dto.Code));
        }

        var board = new Domain.Entities.Board.Board
        {
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description,
            CreatedById = userId,
            Status = BoardStatus.Active
        };

        context.Boards.Add(board);

        var membership = new BoardMembership
        {
            Board = board,
            UserId = userId,
            Role = MembershipRole.Owner
        };

        context.BoardMemberships.Add(membership);

        var saveResult = await context.SaveChangesResultAsync(
            logger,
            () => new BaseError()
        );

        if (saveResult.IsFailed)
            return saveResult;

        await SuperAdminMemberships(board.Id);

        var resultDTO = new BoardDTO(
            board.Id,
            board.Code,
            board.Name,
            board.Description,
            board.CreatedById,
            board.Status,
            board.CreatedAt
        );

        return Result.Ok(resultDTO);
    }

    public Task<Result<BoardDTO>> GetBoard(int boardId, int requesterId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<BoardDTO>>> GetBoards(int requesterId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<BoardDTO>> UpdateBoard(int boardId, UpdateBoardDTO dto,
        int requesterId)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeactivateBoard(int boardId, int requesterId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<BoardMembershipDTO>> CreateBoardMembership(int boardId,
        CreateBoardMembershipDTO dto, int requesterId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<BoardMembershipDTO>>> GetBoardMemberships(
        int boardId, int requesterId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<BoardMembershipDTO>> UpdateBoardMembership(int membershipId,
        UpdateBoardMembershipDTO dto,
        int requesterId)
    {
        throw new NotImplementedException();
    }

    public Task<Result> RemoveBoardMembership(int membershipId, int requesterId)
    {
        throw new NotImplementedException();
    }

    private async Task<Result<Domain.Entities.User.User>> GetUser(int userId)
    {
        var user = await context.Users.FindAsync(userId);

        if (user == null)
            return Result.Fail<Domain.Entities.User.User>(
                new UserNotFoundError(userId));

        return Result.Ok(user);
    }

    private Result ValidateUserCanCreateBoard(
        Domain.Entities.User.User user)
    {
        if (!user.CanCreateBoards)
            return Result.Fail(
                new InsufficientUserPermissionsError(user.Id, "CreateBoard"));

        return Result.Ok();
    }

    private async Task<Result> SuperAdminMemberships(int boardId)
    {
        var superAdmins = await context.Users
            .AsNoTracking()
            .Where(u => u.Role == UserRole.SuperAdmin && u.Status == UserStatus.Active)
            .ToListAsync();

        if (superAdmins.Count == 0)
            return Result.Ok();

        var existingMemberships = await context.BoardMemberships
            .Where(m => m.BoardId == boardId)
            .ToListAsync();

        var membershipsToAdd = new List<BoardMembership>();
        var membershipsToUpdate = new List<BoardMembership>();

        foreach (var superAdmin in superAdmins)
        {
            var membership =
                existingMemberships.FirstOrDefault(m => m.UserId == superAdmin.Id);

            if (membership == null)
            {
                membershipsToAdd.Add(new BoardMembership
                {
                    BoardId = boardId,
                    UserId = superAdmin.Id,
                    Role = MembershipRole.Owner
                });
            }
            else if (membership.Role != MembershipRole.Owner)
            {
                membership.Role = MembershipRole.Owner;
                membershipsToUpdate.Add(membership);
            }
        }

        if (membershipsToAdd.Count > 0)
            await context.BoardMemberships.AddRangeAsync(membershipsToAdd);

        if (membershipsToUpdate.Count > 0)
            context.BoardMemberships.UpdateRange(membershipsToUpdate);

        if (membershipsToAdd.Count == 0 && membershipsToUpdate.Count == 0)
            return Result.Ok();

        var saveResult = await context.SaveChangesResultAsync(
            logger,
            () => new BaseError()
        );

        if (saveResult.IsFailed)
        {
            logger.LogError(
                "Failed to sync SuperAdmin memberships for Board '{BoardId}'.",
                boardId
            );
            return saveResult;
        }

        logger.LogInformation(
            "Successfully synced {Count} SuperAdmin memberships for Board '{BoardId}'.",
            membershipsToAdd.Count + membershipsToUpdate.Count,
            boardId
        );

        return Result.Ok();
    }
}
