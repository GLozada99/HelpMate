using Application.DTOs.Ticket.Ticket;

namespace Application.DTOs.Ticket.TicketComment;

public record TicketCommentDTO
{
    public required int Id { get; init; }
    public required int TicketId { get; init; }
    public required string Text { get; init; }
    public required bool Edited { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required TicketUserDTO User { get; init; }
}
