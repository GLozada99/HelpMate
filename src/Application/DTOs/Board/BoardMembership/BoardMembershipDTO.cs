using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.Board.BoardMembership;

public record BoardMembershipDTO
{
    [Required] public int Id { get; init; }

    [Required] public int BoardId { get; init; }

    [Required] public int UserId { get; init; }

    [Required] public MembershipRole Role { get; init; }

    [Required] public DateTime CreatedAt { get; init; }
}
