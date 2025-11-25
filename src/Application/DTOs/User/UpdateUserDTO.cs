using Domain.Enums;

namespace Application.DTOs.User;

public record UpdateUserDTO
{
    public string? Email { get; init; }
    public string? FullName { get; init; }
    public UpdateUserRole? Role { get; init; }
    public UpdateUserStatus? Status { get; init; }
}
