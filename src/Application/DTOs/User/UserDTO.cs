using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.User;

public record UserDTO
{
    [Required] public required DateTime CreatedAt { get; init; }
    [Required] [EmailAddress] public required string Email { get; init; }
    [Required] public required string FullName { get; init; }
    [Required] public required int Id { get; init; }
    [Required] public required UserRole Role { get; init; }
    [Required] public required UserStatus Status { get; init; }
}
