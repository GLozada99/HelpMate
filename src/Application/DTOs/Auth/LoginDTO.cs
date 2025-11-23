using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public record LoginDTO(
    [Required] [EmailAddress] string Email,
    [Required] string Password
);
