using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.Board.Membership;

public record UpdateBoardMembershipDTO(
    [Required] MembershipRole Role
);
