using Domain.Entities.Board;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities.User;

[Index(nameof(Email), IsUnique = true)]
public class User : BaseEntity
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string FullName { get; set; }
    public required UserStatus Status { get; set; } = UserStatus.Active;

    public required UserRole Role { get; set; }

    public List<BoardMembership> Memberships { get; } = [];
    public List<Ticket.Ticket> CreatedTickets { get; } = [];
    public List<Ticket.Ticket> AssignedTickets { get; } = [];
    public List<Ticket.Ticket> ReportingTickets { get; } = [];
    public List<Ticket.Ticket> WatchingTickets { get; } = [];

    public bool IsActive => Status == UserStatus.Active;
}
