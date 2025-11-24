using Application.DTOs.Tickets;
using Application.Errors;
using Application.Helpers.Ticket;
using Application.Interfaces.Ticket;
using Domain.Entities.Board;
using Domain.Enums;
using FluentResults;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Ticket;

public class TicketService(
    HelpMateDbContext context,
    ILogger<TicketService> logger
) : ITicketService
{
    public async Task<Result<TicketDTO>> CreateTicket(
        int boardId,
        CreateTicketDTO dto,
        int requesterId)
    {
        var requesterMembershipResult = await GetUserMembership(boardId, requesterId);
        if (requesterMembershipResult.IsFailed)
            return Result.Fail(requesterMembershipResult.Errors);
        var requesterMembership = requesterMembershipResult.Value;

        var requester = requesterMembership.User!;
        if (requester.Status != UserStatus.Active)
            return Result.Fail<TicketDTO>(new UserNotFoundError(requesterId));

        var board = requesterMembership.Board!;
        if (board.Status != BoardStatus.Active)
            return Result.Fail<TicketDTO>(new BoardNotFoundError(boardId));

        var canCreateResult =
            TicketRulesHelper.CanCreateTicket(requesterMembership.Role);
        if (canCreateResult.IsFailed) return Result.Fail(canCreateResult.Errors);

        TicketUserDTO? assigneeDto = null;
        int? assigneeId = null;

        if (dto.AssigneeId.HasValue)
        {
            var assigneeMembershipResult =
                await GetUserMembership(boardId, dto.AssigneeId.Value);
            if (assigneeMembershipResult.IsFailed)
                return Result.Fail(assigneeMembershipResult.Errors);
            var assignee = assigneeMembershipResult.Value.User!;

            var canBeAssignedResult =
                TicketRulesHelper.CanBeAssigned(assigneeMembershipResult.Value.Role);
            if (canBeAssignedResult.IsFailed)
                return Result.Fail(canBeAssignedResult.Errors);

            assigneeId = dto.AssigneeId.Value;

            assigneeDto = new TicketUserDTO
            {
                Id = assignee.Id,
                Email = assignee.Email,
                FullName = assignee.FullName
            };
        }

        var ticket = new Domain.Entities.Ticket.Ticket
        {
            BoardId = boardId,
            Title = dto.Title,
            Description = dto.Description ?? "",
            ReporterId = requesterId,
            CreatedById = requesterId,
            AssigneeId = assigneeId
        };
        if (dto.Status.HasValue)
            ticket.Status = dto.Status.Value;

        if (dto.Priority.HasValue)
            ticket.Priority = dto.Priority.Value;

        context.Tickets.Add(ticket);

        var saveResult = await context.SaveChangesResultAsync(
            logger,
            () => new BaseError()
        );

        if (saveResult.IsFailed)
            return saveResult.ToResult<TicketDTO>();


        var reporterDto = new TicketUserDTO
        {
            Id = requester.Id,
            Email = requester.Email,
            FullName = requester.FullName
        };

        var resultDto = new TicketDTO
        {
            Id = ticket.Id,
            BoardId = ticket.BoardId,
            Title = ticket.Title,
            Description = ticket.Description,
            Reporter = reporterDto,
            CreatedBy = reporterDto,
            Assignee = assigneeDto,
            Status = ticket.Status,
            Priority = ticket.Priority,
            CreatedAt = ticket.CreatedAt
        };

        logger.LogInformation(
            "Ticket '{TicketId}' was created on Board '{BoardId}' by User '{UserId}'.",
            ticket.Id, boardId, requesterId
        );

        return Result.Ok(resultDto);
    }

    public async Task<Result<TicketDTO>> GetTicket(
        int boardId,
        int ticketId,
        int requesterId)
    {
        var membershipResult = await GetUserMembership(boardId, requesterId);
        if (membershipResult.IsFailed)
            return Result.Fail(membershipResult.Errors);

        var ticketResult = await GetTicket(ticketId, boardId);
        if (ticketResult.IsFailed) return Result.Fail<TicketDTO>(ticketResult.Errors);
        var ticket = ticketResult.Value!;

        // 3. Build DTOs
        var reporterDto = new TicketUserDTO
        {
            Id = ticket.Reporter!.Id,
            Email = ticket.Reporter.Email,
            FullName = ticket.Reporter.FullName
        };

        var createdByDto = new TicketUserDTO
        {
            Id = ticket.CreatedBy!.Id,
            Email = ticket.CreatedBy.Email,
            FullName = ticket.CreatedBy.FullName
        };

        TicketUserDTO? assigneeDto = null;
        if (ticket.Assignee != null)
            assigneeDto = new TicketUserDTO
            {
                Id = ticket.Assignee.Id,
                Email = ticket.Assignee.Email,
                FullName = ticket.Assignee.FullName
            };

        var dto = new TicketDTO
        {
            Id = ticket.Id,
            BoardId = ticket.BoardId,
            Title = ticket.Title,
            Description = ticket.Description,
            Reporter = reporterDto,
            CreatedBy = createdByDto,
            Assignee = assigneeDto,
            Status = ticket.Status,
            Priority = ticket.Priority,
            CreatedAt = ticket.CreatedAt
        };

        logger.LogInformation(
            "Ticket '{TicketId}' retrieved from Board '{BoardId}' by User '{UserId}'.",
            ticketId, boardId, requesterId
        );

        return Result.Ok(dto);
    }

    public async Task<Result<IQueryable<TicketListDTO>>> GetTickets(
        int boardId,
        int requesterId)
    {
        var membershipResult = await GetUserMembership(boardId, requesterId);
        if (membershipResult.IsFailed)
            return Result.Fail<IQueryable<TicketListDTO>>(membershipResult.Errors);

        var query = context.Tickets
            .AsNoTracking()
            .Where(t => t.BoardId == boardId)
            .Include(t => t.Assignee)
            .Select(t => new TicketListDTO
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Code = t.Code(),
                Assignee = t.Assignee == null
                    ? null
                    : new TicketUserDTO
                    {
                        Id = t.Assignee.Id,
                        Email = t.Assignee.Email,
                        FullName = t.Assignee.FullName
                    },
                Status = t.Status,
                Priority = t.Priority,
                CreatedAt = t.CreatedAt
            })
            .AsQueryable();

        logger.LogInformation(
            "Retrieved tickets for Board '{BoardId}' by User '{RequesterId}'.",
            boardId, requesterId
        );

        return Result.Ok(query);
    }

    public async Task<Result<TicketDTO>> UpdateTicket(
        int boardId,
        int ticketId,
        UpdateTicketDTO dto,
        int requesterId)
    {
        var requesterMembershipResult = await GetUserMembership(boardId, requesterId);
        if (requesterMembershipResult.IsFailed)
            return Result.Fail(requesterMembershipResult.Errors);
        var requesterMembership = requesterMembershipResult.Value;

        var requester = requesterMembership.User!;
        if (requester.Status != UserStatus.Active)
            return Result.Fail<TicketDTO>(new UserNotFoundError(requesterId));

        var board = requesterMembership.Board!;
        if (board.Status != BoardStatus.Active)
            return Result.Fail<TicketDTO>(new BoardNotFoundError(boardId));

        var ticketResult = await GetTicket(ticketId, boardId);
        if (ticketResult.IsFailed) return Result.Fail<TicketDTO>(ticketResult.Errors);
        var ticket = ticketResult.Value;

        var canUpdateResult =
            TicketRulesHelper.CanEditTicket(requesterMembership.Role);
        if (canUpdateResult.IsFailed)
            return Result.Fail<TicketDTO>(canUpdateResult.Errors);

        if (!string.IsNullOrWhiteSpace(dto.Title) && dto.Title != ticket.Title)
            ticket.Title = dto.Title;

        if (dto.Description != null && dto.Description != ticket.Description)
            ticket.Description = dto.Description;

        if (dto.ReporterId.HasValue)
        {
            var reporterMembershipResult =
                await GetUserMembership(boardId, dto.ReporterId.Value);
            if (reporterMembershipResult.IsFailed)
                return Result.Fail<TicketDTO>(reporterMembershipResult.Errors);
            var newReporterRole = reporterMembershipResult.Value.Role;

            var canBeReporter =
                TicketRulesHelper.CanBeReporter(newReporterRole);
            if (canBeReporter.IsFailed)
                return Result.Fail<TicketDTO>(canBeReporter.Errors);
            ticket.ReporterId = dto.ReporterId.Value;
        }

        TicketUserDTO? assigneeDto = null;
        if (dto.AssigneeId.HasValue)
        {
            var assigneeMembershipResult =
                await GetUserMembership(boardId, dto.AssigneeId.Value);
            if (assigneeMembershipResult.IsFailed)
                return Result.Fail<TicketDTO>(assigneeMembershipResult.Errors);
            var assigneeRole = assigneeMembershipResult.Value.Role;

            var canBeAssigned =
                TicketRulesHelper.CanBeAssigned(assigneeRole);
            if (canBeAssigned.IsFailed)
                return Result.Fail<TicketDTO>(canBeAssigned.Errors);
            ticket.AssigneeId = dto.AssigneeId.Value;

            var assignee = assigneeMembershipResult.Value.User!;
            assigneeDto = new TicketUserDTO
            {
                Id = assignee.Id,
                Email = assignee.Email,
                FullName = assignee.FullName
            };
        }
        else
        {
            // TODO: This could be improved, currently the client needs to provide the current user
            //  on each update call to keep the user.
            ticket.AssigneeId = null;
        }

        if (dto.Status.HasValue)
            ticket.Status = dto.Status.Value;

        if (dto.Priority.HasValue)
            ticket.Priority = dto.Priority.Value;

        var saveResult = await context.SaveChangesResultAsync(
            logger,
            () => new BaseError()
        );

        if (saveResult.IsFailed)
            return saveResult.ToResult<TicketDTO>();

        var reporterDto = new TicketUserDTO
        {
            Id = ticket.Reporter!.Id,
            Email = ticket.Reporter.Email,
            FullName = ticket.Reporter.FullName
        };

        var createdByDto = new TicketUserDTO
        {
            Id = ticket.CreatedBy!.Id,
            Email = ticket.CreatedBy.Email,
            FullName = ticket.CreatedBy.FullName
        };

        var dtoResult = new TicketDTO
        {
            Id = ticket.Id,
            BoardId = ticket.BoardId,
            Title = ticket.Title,
            Description = ticket.Description,
            Reporter = reporterDto,
            CreatedBy = createdByDto,
            Assignee = assigneeDto,
            Status = ticket.Status,
            Priority = ticket.Priority,
            CreatedAt = ticket.CreatedAt
        };

        logger.LogInformation(
            "Ticket '{TicketId}' updated on Board '{BoardId}' by User '{UserId}'.",
            ticketId, boardId, requesterId
        );

        return Result.Ok(dtoResult);
    }

    private async Task<Result<Domain.Entities.Ticket.Ticket>> GetTicket(int ticketId,
        int boardId)
    {
        var ticket = await context.Tickets
            .AsNoTracking()
            .Include(t => t.Reporter)
            .Include(t => t.CreatedBy)
            .Include(t => t.Assignee)
            .FirstOrDefaultAsync(t =>
                t.Id == ticketId &&
                t.BoardId == boardId);

        if (ticket != null) return Result.Ok(ticket);
        logger.LogWarning(
            "Ticket '{TicketId}' not found on Board '{BoardId}'.",
            ticketId, boardId
        );
        return Result.Fail<Domain.Entities.Ticket.Ticket>(
            new TicketNotFoundError(ticketId)
        );
    }

    private async Task<Result<BoardMembership>> GetUserMembership(
        int boardId,
        int userId)
    {
        var membership = await context.BoardMemberships
            .Include(m => m.Board)
            .Include(m => m.User)
            .FirstOrDefaultAsync(m =>
                m.BoardId == boardId && m.UserId == userId && m.User!.IsActive);

        if (membership != null) return Result.Ok(membership);

        logger.LogWarning(
            "User '{UserId}' has no membership in Board '{BoardId}'.",
            userId, boardId
        );

        return Result.Fail<BoardMembership>(
            new InsufficientUserMembershipError(
                "N/A",
                $"Access Board {boardId}"
            )
        );
    }
}
