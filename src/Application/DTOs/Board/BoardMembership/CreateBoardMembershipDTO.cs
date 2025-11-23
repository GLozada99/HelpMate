using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.Board.BoardMembership;

public record CreateBoardMembershipDTO(
    [Required] int UserId,
    [Required] MembershipRole Role
);
