using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities.Board;

[Index(nameof(Code), IsUnique = true)]
public class Board : BaseEntity
{
    public enum Statuses
    {
        Active,
        Inactive
    }

    [MaxLength(4),MinLength(4)]
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required Statuses Status { get; set; } = Statuses.Active;

    [ForeignKey("User")]
    public required int CreatedById { get; set; }
    public User.User? CreatedBy { get; set; }

    public List<BoardMembership> Memberships { get; } = [];
}
