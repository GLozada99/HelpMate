using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Ticket.Ticket;

public record TicketUserDTO
{
    [EmailAddress] public required string Email { get; init; }
    public required string FullName { get; init; }
    public required int Id { get; init; }
}
