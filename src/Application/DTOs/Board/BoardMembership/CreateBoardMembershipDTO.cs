using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.Board.Membership;

public record CreateBoardMembershipDTO(
    [Required] int UserId,
    [Required] MembershipRole Role
);
