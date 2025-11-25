using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities.Board;

[Index(nameof(BoardId), nameof(UserId), IsUnique = true)]
public class BoardMembership : BaseEntity
{
    public int BoardId { get; set; }
    public Board? Board { get; set; }

    public int UserId { get; set; }
    public User.User? User { get; set; }

    public MembershipRole Role { get; set; }
}
