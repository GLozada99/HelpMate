using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Tickets;

public record TicketUserDTO
{
    [EmailAddress] public required string Email { get; init; }
    public required string FullName { get; init; }
    public required int Id { get; init; }
}
