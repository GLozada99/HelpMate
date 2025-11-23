using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.User;

public enum CreateUserRole
{
    Admin,
    Agent,
    Customer
}

public record CreateUserDTO(
    [Required] [EmailAddress] string Email,
    [Required] string Password,
    [Required] string FullName,
    [Required] CreateUserRole Role
);
