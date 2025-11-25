using Domain.Enums;

namespace Application.DTOs.Ticket.Ticket;

public record UpdateTicketDTO
{
    public string? Title { get; init; }

    public string? Description { get; init; }

    public int? ReporterId { get; init; }

    public int? AssigneeId { get; init; }

    public TicketStatus? Status { get; init; }

    public TicketPriority? Priority { get; init; }
}
