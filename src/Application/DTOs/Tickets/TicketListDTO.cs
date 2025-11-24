using Domain.Enums;

namespace Application.DTOs.Tickets;

public record TicketListDTO
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Code { get; init; }
    public TicketUserDTO? Assignee { get; init; }
    public required TicketStatus Status { get; init; }
    public required TicketPriority Priority { get; init; }
    public required DateTime CreatedAt { get; init; }
}
