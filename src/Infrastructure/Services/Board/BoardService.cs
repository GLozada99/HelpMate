using Application.DTOs.Board.Board;
using Application.DTOs.Board.BoardMembership;
using Application.Errors;
using Application.Helpers.Board;
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
    public async Task<Result<BoardDTO>> CreateBoard(CreateBoardDTO dto, int requesterId)
    {
        var userResult = await GetUser(requesterId, false);
        if (userResult.IsFailed) return Result.Fail(userResult.Errors);

        var canCreate = BoardRulesHelper.CanCreateBoard(userResult.Value.Role);
        if (canCreate.IsFailed) return Result.Fail(canCreate.Errors);

        var duplicateExists =
            BoardRulesHelper.BoardWithCodeExists(dto.Code, context.Boards);
        if (duplicateExists.IsFailed) return Result.Fail(duplicateExists.Errors);

        var board = new Domain.Entities.Board.Board
        {
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description,
            CreatedById = requesterId,
            Status = BoardStatus.Active
        };

        context.Boards.Add(board);

        var membership = new BoardMembership
        {
            Board = board,
            UserId = requesterId,
            Role = MembershipRole.Owner
        };

        context.BoardMemberships.Add(membership);

        var saveResult = await context.SaveChangesResultAsync(
            logger,
            () => new BaseError()
        );

        if (saveResult.IsFailed)
            return saveResult;

        await SetSuperAdminMemberships(board.Id);

        var resultDTO = new BoardDTO
        {
            Id = board.Id,
            Code = board.Code,
            Name = board.Name,
            Description = board.Description,
            CreatedById = board.CreatedById,
            Status = board.Status,
            CreatedAt = board.CreatedAt
        };

        return Result.Ok(resultDTO);
    }

    public async Task<Result<BoardDTO>> GetBoard(int boardId, int requesterId)
    {
        var boardResult = await GetBoard(boardId, false);
        if (boardResult.IsFailed) return Result.Fail(boardResult.Errors);
        var board = boardResult.Value;

        var membershipResult = await GetUserMembership(boardId, requesterId);
        if (membershipResult.IsFailed) return Result.Fail(membershipResult.Errors);

        var dto = new BoardDTO
        {
            Id = board.Id,
            Code = board.Code,
            Name = board.Name,
            Description = board.Description,
            CreatedById = board.CreatedById,
            Status = board.Status,
            CreatedAt = board.CreatedAt
        };

        return Result.Ok(dto);
    }


    public Task<Result<IQueryable<BoardDTO>>> GetBoards(int requesterId)
    {
        var boards = context.BoardMemberships
            .AsNoTracking()
            .Where(m => m.UserId == requesterId)
            .Include(m => m.Board)
            .Select(m => m.Board!)
            .AsQueryable();

        if (!boards.Any())
        {
            logger.LogInformation(
                "User '{RequesterId}' is not a member of any boards.",
                requesterId
            );

            return Task.FromResult(
                Result.Ok(Enumerable.Empty<BoardDTO>().AsQueryable()));
        }

        var dtos = boards.Select(board => new BoardDTO
            {
                Id = board.Id,
                Code = board.Code,
                Name = board.Name,
                Description = board.Description,
                CreatedById = board.CreatedById,
                Status = board.Status,
                CreatedAt = board.CreatedAt
            }
        );

        return Task.FromResult(Result.Ok(dtos));
    }

    public async Task<Result<BoardDTO>> UpdateBoard(
        int boardId,
        UpdateBoardDTO dto,
        int requesterId)
    {
        var boardResult = await GetBoard(boardId);
        if (boardResult.IsFailed) return Result.Fail(boardResult.Errors);
        var board = boardResult.Value;

        var membershipResult = await GetUserMembership(boardId, requesterId);
        if (membershipResult.IsFailed) return Result.Fail(membershipResult.Errors);

        var canUpdate =
            BoardRulesHelper.CanUpdateBoard(membershipResult.Value.Role);
        if (canUpdate.IsFailed) return Result.Fail(canUpdate.Errors);

        if (dto.Name != board.Name)
            board.Name = dto.Name;

        if (dto.Description != board.Description)
            board.Description = dto.Description;

        if (dto.Status == UpdateBoardStatus.Active &&
            board.Status != BoardStatus.Active)
            board.Status = BoardStatus.Active;

        var saveResult = await context.SaveChangesResultAsync(
            logger,
            () => new BaseError()
        );

        if (saveResult.IsFailed)
            return saveResult.ToResult<BoardDTO>();

        var dtoResult = new BoardDTO
        {
            Id = board.Id,
            Code = board.Code,
            Name = board.Name,
            Description = board.Description,
            CreatedById = board.CreatedById,
            Status = board.Status,
            CreatedAt = board.CreatedAt
        };

        logger.LogInformation(
            "Board '{BoardId}' was updated successfully by User '{UserId}'.",
            boardId, requesterId
        );

        return Result.Ok(dtoResult);
    }

    public async Task<Result> DeactivateBoard(int boardId, int requesterId)
    {
        var boardResult = await GetBoard(boardId);
        if (boardResult.IsFailed) return Result.Fail(boardResult.Errors);
        var board = boardResult.Value;

        var membershipResult = await GetUserMembership(boardId, requesterId);
        if (membershipResult.IsFailed) return Result.Fail(membershipResult.Errors);

        var canDeactivate =
            BoardRulesHelper.CanDeactivateBoard(membershipResult.Value.Role);
        if (canDeactivate.IsFailed) return Result.Fail(canDeactivate.Errors);

        if (board.Status == BoardStatus.Inactive)
        {
            logger.LogInformation(
                "Board '{BoardId}' is already inactive. No changes were made.",
                boardId
            );
            return Result.Ok();
        }

        board.Status = BoardStatus.Inactive;
        var saveResult = await context.SaveChangesResultAsync(
            logger,
            () => new BaseError()
        );
        if (saveResult.IsFailed)
            return saveResult;

        logger.LogInformation(
            "Board '{BoardId}' was deactivated successfully by User '{RequesterId}'.",
            boardId, requesterId
        );

        return Result.Ok();
    }

    public async Task<Result<BoardMembershipDTO>> CreateBoardMembership(
        int boardId,
        CreateBoardMembershipDTO dto,
        int requesterId)
    {
        var boardResult = await GetBoard(boardId, false);
        if (boardResult.IsFailed) return Result.Fail(boardResult.Errors);

        var membershipResult = await GetUserMembership(boardId, requesterId);
        if (membershipResult.IsFailed) return Result.Fail(membershipResult.Errors);

        var canCreate =
            BoardRulesHelper.CanCreateMembership(membershipResult.Value.Role);
        if (canCreate.IsFailed) return Result.Fail(canCreate.Errors);

        var targetUserResult = await GetUser(dto.UserId, false);
        if (targetUserResult.IsFailed) return Result.Fail(targetUserResult.Errors);
        var targetUser = targetUserResult.Value;

        var userMembershipResult = await GetUserMembership(boardId, dto.UserId);
        if (userMembershipResult.IsSuccess)
        {
            logger.LogWarning(
                "Cannot create membership because User '{UserId}' is already a member of Board '{BoardId}'.",
                dto.UserId, boardId
            );
            return Result.Fail<BoardMembershipDTO>(
                new BoardMembershipAlreadyExistsError(boardId, dto.UserId)
            );
        }

        var canHaveMembershipRole =
            BoardRulesHelper.CanHaveMembershipRole(targetUser.Role, dto.Role);
        if (canHaveMembershipRole.IsFailed)
            return Result.Fail(canHaveMembershipRole.Errors);

        // If the user is SuperAdmin → always Owner
        var role = targetUser.Role == UserRole.SuperAdmin
            ? MembershipRole.Owner
            : dto.Role;

        var membership = new BoardMembership
        {
            BoardId = boardId,
            UserId = dto.UserId,
            Role = role
        };

        context.BoardMemberships.Add(membership);

        var saveResult = await context.SaveChangesResultAsync(
            logger,
            () => new BaseError()
        );

        if (saveResult.IsFailed)
            return saveResult.ToResult<BoardMembershipDTO>();

        var resultDto = new BoardMembershipDTO
        {
            Id = membership.Id,
            BoardId = membership.BoardId,
            UserId = membership.UserId,
            Role = membership.Role,
            CreatedAt = membership.CreatedAt
        };

        logger.LogInformation(
            "User '{UserId}' was added as '{Role}' to Board '{BoardId}' by Requester '{RequesterId}'.",
            dto.UserId, role, boardId, requesterId
        );

        return Result.Ok(resultDto);
    }

    public async Task<Result> DeleteBoardMembership(int boardId, int userId,
        int requesterId)
    {
        var boardResult = await GetBoard(boardId, false);
        if (boardResult.IsFailed) return Result.Fail(boardResult.Errors);

        var membershipResult = await GetUserMembership(boardId, userId);
        if (membershipResult.IsFailed)
            return Result.Fail(
                new BoardMembershipNotFoundError(
                    $"No membership on board {boardId} for user {userId}"));
        var membership = membershipResult.Value;

        var requesterMembershipResult =
            await GetUserMembership(boardId, requesterId);
        if (requesterMembershipResult.IsFailed)
            return Result.Fail(requesterMembershipResult.Errors);

        var canRemove =
            BoardRulesHelper.CanRemoveMembership(requesterMembershipResult.Value.Role);
        if (canRemove.IsFailed) return Result.Fail(canRemove.Errors);

        var ownersCount = await context.BoardMemberships
            .AsNoTracking()
            .CountAsync(m =>
                m.BoardId == membership.BoardId &&
                m.Role == MembershipRole.Owner
            );
        var canRemoveLast =
            BoardRulesHelper.CanRemoveMembershipConsideringLastOwner(
                membership.Role, ownersCount);
        if (canRemoveLast.IsFailed) return Result.Fail(canRemoveLast.Errors);

        context.BoardMemberships.Remove(membership);

        var saveResult = await context.SaveChangesResultAsync(
            logger,
            () => new BaseError()
        );

        if (saveResult.IsFailed)
            return saveResult;

        logger.LogInformation(
            "Membership '{MembershipId}' was removed from Board '{BoardId}' by Requester '{RequesterId}'.",
            membership.Id, membership.BoardId, requesterId
        );

        return Result.Ok();
    }

    public async Task<Result<BoardMembershipDTO>> UpdateBoardMembership(
        int boardId,
        int userId,
        UpdateBoardMembershipDTO dto,
        int requesterId)
    {
        var boardResult = await GetBoard(boardId, false);
        if (boardResult.IsFailed) return Result.Fail(boardResult.Errors);

        var userResult = await GetUser(userId, false);
        if (userResult.IsFailed) return Result.Fail(userResult.Errors);

        var membershipResult = await GetUserMembership(boardId, userId);
        if (membershipResult.IsFailed)
            return Result.Fail(
                new BoardMembershipNotFoundError(
                    $"No membership on board {boardId} for user {userId}"));
        var membership = membershipResult.Value;

        var requesterMembershipResult =
            await GetUserMembership(boardId, requesterId);
        if (requesterMembershipResult.IsFailed)
            return Result.Fail(requesterMembershipResult.Errors);

        var canUpdate =
            BoardRulesHelper.CanUpdateMembership(requesterMembershipResult.Value.Role);
        if (canUpdate.IsFailed) return Result.Fail(canUpdate.Errors);

        var canHaveMembershipRole =
            BoardRulesHelper.CanHaveMembershipRole(userResult.Value.Role, dto.Role);
        if (canHaveMembershipRole.IsFailed)
            return Result.Fail(canHaveMembershipRole.Errors);

        membership.Role = dto.Role;

        var saveResult = await context.SaveChangesResultAsync(
            logger,
            () => new BaseError()
        );

        if (saveResult.IsFailed)
            return saveResult.ToResult<BoardMembershipDTO>();

        var resultDto = new BoardMembershipDTO
        {
            Id = membership.Id,
            BoardId = membership.BoardId,
            UserId = membership.UserId,
            Role = membership.Role,
            CreatedAt = membership.CreatedAt
        };

        logger.LogInformation(
            "Membership '{MembershipId}' on Board '{BoardId}' was updated to role '{Role}' by User '{RequesterId}'.",
            boardId, membership.BoardId, membership.Role, requesterId
        );

        return Result.Ok(resultDto);
    }


    public async Task<Result<IQueryable<BoardMembershipDTO>>> GetBoardMemberships(
        int boardId,
        int requesterId)
    {
        var boardResult = await GetBoard(boardId, false);
        if (boardResult.IsFailed) return Result.Fail(boardResult.Errors);

        var memberships = context.BoardMemberships
            .AsNoTracking()
            .Where(m => m.BoardId == boardId)
            .AsQueryable();

        if (memberships.All(m => m.UserId != requesterId))
        {
            logger.LogWarning(
                "User '{RequesterId}' attempted to retrieve memberships for Board '{BoardId}' but is not a member.",
                requesterId, boardId
            );

            return Result.Fail<IQueryable<BoardMembershipDTO>>(
                new InsufficientUserMembershipPermissionsError(
                    "no membership for this board",
                    $"View memberships for Board {boardId}"
                )
            );
        }

        var dtoList = memberships.Select(m => new BoardMembershipDTO
        {
            Id = m.Id,
            BoardId = m.BoardId,
            UserId = m.UserId,
            Role = m.Role,
            CreatedAt = m.CreatedAt
        });

        logger.LogInformation(
            "Retrieved {Count} memberships for Board '{BoardId}' by User '{RequesterId}'.",
            memberships.Count(), boardId, requesterId
        );

        return Result.Ok(dtoList);
    }

    private async Task<Result> SetSuperAdminMemberships(int boardId)
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

    private async Task<Result<Domain.Entities.Board.Board>>
        GetBoard(int boardId, bool track = true)
    {
        var board = track
            ? await context.Boards.FindAsync(boardId)
            : await context.Boards.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == boardId);

        if (board != null) return Result.Ok(board);

        logger.LogWarning(
            "Board '{BoardId}' does not exist.",
            boardId
        );
        return Result.Fail<Domain.Entities.Board.Board>(
            new BoardNotFoundError(boardId)
        );
    }

    private async Task<Result<Domain.Entities.User.User>> GetUser(int userId,
        bool track = true)
    {
        var user = track
            ? await context.Users.FindAsync(userId)
            : await context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

        if (user != null) return Result.Ok(user);
        logger.LogWarning(
            "Cannot create Board because User with Id '{UserId}' does not exist.",
            userId
        );
        return Result.Fail<Domain.Entities.User.User>(
            new UserNotFoundError(userId));
    }

    private async Task<Result<BoardMembership>>
        GetUserMembership(int boardId, int userId)
    {
        var membership = await context.BoardMemberships
            .FirstOrDefaultAsync(m => m.BoardId == boardId && m.UserId == userId);

        if (membership != null) return Result.Ok(membership);
        logger.LogWarning(
            "User '{UserId}' has no membership in Board '{BoardId}'.",
            userId, boardId
        );

        return Result.Fail<BoardMembership>(
            new InsufficientUserMembershipPermissionsError(
                "no membership for this board",
                $"Access Board {boardId}"
            )
        );
    }
}