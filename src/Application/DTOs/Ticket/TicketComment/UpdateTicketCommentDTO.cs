namespace Application.DTOs.Ticket.TicketComment;

public record UpdateTicketCommentDTO
{
    public required string Text { get; init; }
}
