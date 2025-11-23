using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.Auth;

public record LoggedInUserDTO(
    [Required] int Id,
    [Required] [EmailAddress] string Email,
    [Required] string FullName,
    [Required] UserRole Role,
    [Required] UserStatus Status,
    [Required] DateTime CreatedAt
);
