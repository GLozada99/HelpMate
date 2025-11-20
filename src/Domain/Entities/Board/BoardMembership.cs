using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities.Board;

[Index(nameof(BoardId), nameof(UserId), IsUnique = true)]
public class BoardMembership : BaseEntity
{
    public enum MembershipRoles
    {
        Viewer,
        Editor,
        Agent,
        Owner
    }

    public required int BoardId { get; set; }
    public Board? Board { get; set; }

    public required int UserId { get; set; }
    public User.User? User { get; set; }

    public List<MembershipRoles> Roles { get; set; } = [];
}
