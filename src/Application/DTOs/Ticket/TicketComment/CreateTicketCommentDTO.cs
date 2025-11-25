using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Ticket.TicketComment;

public record CreateTicketCommentDTO
{
    [Required] public required string Text { get; init; }
}
