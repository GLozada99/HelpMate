using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.Board.BoardMembership;

public record BoardMembershipDTO(
    int Id,
    int BoardId,
    int UserId,
    MembershipRole Role,
    DateTime CreatedAt
);
