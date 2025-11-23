using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.Board.BoardMembership;

public record UpdateBoardMembershipDTO(
    MembershipRole Role
);
