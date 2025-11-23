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
    public async Task<Result<BoardDTO>> CreateBoard(CreateBoardDTO dto, int userId)
    {
        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            logger.LogWarning(
                "Cannot create Board because User with Id '{UserId}' does not exist.",
                userId
            );
            return Result.Fail<BoardDTO>(
                new UserNotFoundError(userId));
        }

        var canCreate = BoardRulesHelper.CanCreateBoard(user.Role);
        if (canCreate.IsFailed) return Result.Fail(canCreate.Errors);

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

        await SetSuperAdminMemberships(board.Id);

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

    public async Task<Result<BoardDTO>> GetBoard(int boardId, int requesterId)
    {
        var boardResult = await GetBoard(boardId);
        if (boardResult.IsFailed) return Result.Fail(boardResult.Errors);
        var board = boardResult.Value;

        var membershipResult = await GetUserMembership(boardId, requesterId);
        if (membershipResult.IsFailed) return Result.Fail(membershipResult.Errors);

        var dto = new BoardDTO(
            board.Id,
            board.Code,
            board.Name,
            board.Description,
            board.CreatedById,
            board.Status,
            board.CreatedAt
        );

        return Result.Ok(dto);
    }


    public async Task<Result<IEnumerable<BoardDTO>>> GetBoards(int requesterId)
    {
        var boards = await context.BoardMemberships
            .AsNoTracking()
            .Where(m => m.UserId == requesterId)
            .Include(m => m.Board)
            .Select(m => m.Board!)
            .ToListAsync();

        if (boards.Count == 0)
        {
            logger.LogInformation(
                "User '{RequesterId}' is not a member of any boards.",
                requesterId
            );

            return Result.Ok(Enumerable.Empty<BoardDTO>());
        }

        var dtos = boards.Select(board => new BoardDTO(
            board.Id,
            board.Code,
            board.Name,
            board.Description,
            board.CreatedById,
            board.Status,
            board.CreatedAt
        ));

        return Result.Ok(dtos);
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

        var dtoResult = new BoardDTO(
            board.Id,
            board.Code,
            board.Name,
            board.Description,
            board.CreatedById,
            board.Status,
            board.CreatedAt
        );

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
        var boardResult = await GetBoard(boardId);
        if (boardResult.IsFailed) return Result.Fail(boardResult.Errors);

        var membershipResult = await GetUserMembership(boardId, requesterId);
        if (membershipResult.IsFailed) return Result.Fail(membershipResult.Errors);

        var canCreate =
            BoardRulesHelper.CanCreateMembership(membershipResult.Value.Role);
        if (canCreate.IsFailed) return Result.Fail(canCreate.Errors);

        var targetUser = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == dto.UserId);

        if (targetUser == null)
        {
            logger.LogWarning(
                "Cannot create membership because User '{UserId}' does not exist.",
                dto.UserId
            );
            return Result.Fail<BoardMembershipDTO>(new UserNotFoundError(dto.UserId));
        }

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

        var resultDto = new BoardMembershipDTO(
            membership.Id,
            membership.BoardId,
            membership.UserId,
            membership.Role,
            membership.CreatedAt
        );

        logger.LogInformation(
            "User '{UserId}' was added as '{Role}' to Board '{BoardId}' by Requester '{RequesterId}'.",
            dto.UserId, role, boardId, requesterId
        );

        return Result.Ok(resultDto);
    }


    public async Task<Result<IEnumerable<BoardMembershipDTO>>> GetBoardMemberships(
        int boardId,
        int requesterId)
    {
        var boardResult = await GetBoard(boardId);
        if (boardResult.IsFailed) return Result.Fail(boardResult.Errors);

        var memberships = await context.BoardMemberships
            .AsNoTracking()
            .Where(m => m.BoardId == boardId)
            .ToListAsync();

        if (memberships.All(m => m.UserId != requesterId))
        {
            logger.LogWarning(
                "User '{RequesterId}' attempted to retrieve memberships for Board '{BoardId}' but is not a member.",
                requesterId, boardId
            );

            return Result.Fail<IEnumerable<BoardMembershipDTO>>(
                new InsufficientUserMembershipError(
                    "N/A",
                    $"View memberships for Board {boardId}"
                )
            );
        }

        var dtoList = memberships.Select(m => new BoardMembershipDTO(
            m.Id,
            m.BoardId,
            m.UserId,
            m.Role,
            m.CreatedAt
        ));

        logger.LogInformation(
            "Retrieved {Count} memberships for Board '{BoardId}' by User '{RequesterId}'.",
            memberships.Count, boardId, requesterId
        );

        return Result.Ok(dtoList);
    }


    public async Task<Result<BoardMembershipDTO>> UpdateBoardMembership(
        int membershipId,
        UpdateBoardMembershipDTO dto,
        int requesterId)
    {
        var membership = await context.BoardMemberships.Include(m => m.Board)
            .FirstOrDefaultAsync(m => m.Id == membershipId);

        if (membership == null)
        {
            logger.LogWarning(
                "Cannot update BoardMembership '{MembershipId}' because it does not exist.",
                membershipId
            );
            return Result.Fail<BoardMembershipDTO>(
                new BoardMembershipNotFoundError(membershipId)
            );
        }

        var membershipResult = await GetUserMembership(membership.BoardId, requesterId);
        if (membershipResult.IsFailed) return Result.Fail(membershipResult.Errors);

        var canUpdate =
            BoardRulesHelper.CanUpdateMembership(membershipResult.Value.Role);
        if (canUpdate.IsFailed) return Result.Fail(canUpdate.Errors);

        membership.Role = dto.Role;

        var saveResult = await context.SaveChangesResultAsync(
            logger,
            () => new BaseError()
        );

        if (saveResult.IsFailed)
            return saveResult.ToResult<BoardMembershipDTO>();

        var resultDto = new BoardMembershipDTO(
            membership.Id,
            membership.BoardId,
            membership.UserId,
            membership.Role,
            membership.CreatedAt
        );

        logger.LogInformation(
            "Membership '{MembershipId}' on Board '{BoardId}' was updated to role '{Role}' by User '{RequesterId}'.",
            membershipId, membership.BoardId, membership.Role, requesterId
        );

        return Result.Ok(resultDto);
    }

    public async Task<Result> RemoveBoardMembership(int membershipId, int requesterId)
    {
        var membership = await context.BoardMemberships
            .FirstOrDefaultAsync(m => m.Id == membershipId);

        if (membership == null)
        {
            logger.LogWarning(
                "Cannot remove BoardMembership '{MembershipId}' because it does not exist.",
                membershipId
            );
            return Result.Fail(new BoardMembershipNotFoundError(membershipId));
        }

        var membershipResult = await GetUserMembership(membership.BoardId, requesterId);
        if (membershipResult.IsFailed) return Result.Fail(membershipResult.Errors);

        var canRemove =
            BoardRulesHelper.CanRemoveMembership(membershipResult.Value.Role);
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
            membershipId, membership.BoardId, requesterId
        );

        return Result.Ok();
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
        GetBoard(int boardId)
    {
        var board = await context.Boards
            .FirstOrDefaultAsync(b => b.Id == boardId);

        if (board != null) return Result.Ok(board);

        logger.LogWarning(
            "Board '{BoardId}' does not exist.",
            boardId
        );
        return Result.Fail<Domain.Entities.Board.Board>(
            new BoardNotFoundError(boardId)
        );
    }

    private async Task<Result<BoardMembership>>
        GetUserMembership(int boardId, int requesterId)
    {
        var membership = await context.BoardMemberships
            .FirstOrDefaultAsync(m => m.BoardId == boardId && m.UserId == requesterId);

        if (membership != null) return Result.Ok(membership);
        logger.LogWarning(
            "User '{RequesterId}' has no membership in Board '{BoardId}'.",
            requesterId, boardId
        );

        return Result.Fail<BoardMembership>(
            new InsufficientUserMembershipError(
                "N/A",
                $"Access Board {boardId}"
            )
        );
    }
}
