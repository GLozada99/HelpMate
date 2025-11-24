using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.User;

public record UserDTO
{
    public required DateTime CreatedAt { get; init; }
    [EmailAddress] public required string Email { get; init; }
    public required string FullName { get; init; }
    public required int Id { get; init; }
    public required UserRole Role { get; init; }
    public required UserStatus Status { get; init; }
}
