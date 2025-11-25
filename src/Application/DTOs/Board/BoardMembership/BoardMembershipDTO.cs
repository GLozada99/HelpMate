using Domain.Enums;

namespace Application.DTOs.Board.BoardMembership;

public record BoardMembershipDTO
{
    public int Id { get; init; }
    public int BoardId { get; init; }
    public int UserId { get; init; }
    public MembershipRole Role { get; init; }
    public DateTime CreatedAt { get; init; }
}
