using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public record LoginDTO
{
    [Required] public required string Password { get; init; }
    [Required] public required string Email { get; init; }
}
