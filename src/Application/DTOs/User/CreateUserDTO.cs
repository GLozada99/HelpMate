using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.User;

public enum CreateUserRole
{
    Admin,
    Agent,
    Customer
}

public record CreateUserDTO
{
    [Required] [EmailAddress] public required string Email { get; init; }
    [Required] [MinLength(1)] public required string Password { get; init; }
    [Required] [MinLength(1)] public required string FullName { get; init; }
    [Required] public required CreateUserRole Role { get; init; }
}
