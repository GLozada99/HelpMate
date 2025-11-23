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
    string? Email,
    string? FullName,
    UpdateUserRole? Role,
    UpdateUserStatus? Status
);
