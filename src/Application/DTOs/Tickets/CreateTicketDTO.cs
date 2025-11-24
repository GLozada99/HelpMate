using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.Tickets;

public record CreateTicketDTO
{
    [Required] public required string Title { get; init; }
    public string? Description { get; init; }
    public int? AssigneeId { get; init; }
    public TicketStatus? Status { get; init; }
    public TicketPriority? Priority { get; init; }
}
