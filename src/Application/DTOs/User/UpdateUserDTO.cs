using Domain.Enums;

namespace Application.DTOs.User;

public record UpdateUserDTO(
    string? Email,
    string? FullName,
    UpdateUserRole? Role,
    UpdateUserStatus? Status
);
