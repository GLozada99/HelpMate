using Domain.Enums;

namespace Application.DTOs.Auth;

public record LoggedInUserDTO(
    int Id,
    string Email,
    string FullName,
    UserRole Role,
    UserStatus Status,
    DateTime CreatedAt
);
