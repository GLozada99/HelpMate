using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.Board.BoardMembership;

public record BoardMembershipDTO(
    [Required] int Id,
    [Required] int BoardId,
    [Required] int UserId,
    [Required] MembershipRole Role,
    [Required] DateTime CreatedAt
);
