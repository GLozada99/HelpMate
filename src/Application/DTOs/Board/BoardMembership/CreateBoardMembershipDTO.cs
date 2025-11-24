using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.Board.BoardMembership;

public record CreateBoardMembershipDTO
{
    [Required] public MembershipRole Role { get; init; }
    [Required] public int UserId { get; init; }
}
