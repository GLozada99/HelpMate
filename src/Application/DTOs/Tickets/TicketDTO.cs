using Domain.Enums;

namespace Application.DTOs.Tickets;

public record TicketDTO
{
    public required int Id { get; init; }
    public required string Code { get; init; }
    public required int BoardId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required TicketUserDTO Reporter { get; init; }
    public required TicketUserDTO CreatedBy { get; init; }
    public TicketUserDTO? Assignee { get; init; }
    public required TicketStatus Status { get; init; }
    public required TicketPriority Priority { get; init; }
    public required DateTime CreatedAt { get; init; }
}
