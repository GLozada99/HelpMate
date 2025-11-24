using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.User;

public record CreateUserDTO
{
    [Required] [EmailAddress] public required string Email { get; init; }
    [Required] [MinLength(1)] public required string Password { get; init; }
    [Required] [MinLength(1)] public required string FullName { get; init; }
    [Required] public required CreateUserRole Role { get; init; }
}
