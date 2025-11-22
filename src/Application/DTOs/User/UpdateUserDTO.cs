using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.User;

public enum UpdateUserRole
{
    Admin,
    Agent,
    Customer
}

public enum UpdateUserStatus
{
    Active
}

public record UpdateUserDTO(
    [EmailAddress] [MinLength(5)] string? Email,
    [MinLength(1)] string? FullName,
    UpdateUserRole? Role,
    UpdateUserStatus? Status
);
