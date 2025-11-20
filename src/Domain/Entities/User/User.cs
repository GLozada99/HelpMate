using Domain.Entities.Board;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities.User;

[Index(nameof(Email), IsUnique = true)]
public class User : BaseEntity
{
    public enum Roles
    {
        Admin,
        Agent,
        Customer
    }

    public enum Statuses
    {
        Admin,
        Agent,
        Customer
    }

    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string FullName { get; set; }
    public required Statuses Status { get; set; }
    public required Roles Role { get; set; }

    public List<BoardMembership> Memberships { get; } = [];
}
