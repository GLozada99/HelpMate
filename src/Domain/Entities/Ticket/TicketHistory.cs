using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Entities.Ticket;

public class TicketHistory : BaseEntity
{
    public required TicketHistoryActionType Action { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    [ForeignKey("User")] public int UserId { get; set; }

    public User.User? User { get; set; }

    [ForeignKey("Ticket")] public int TicketId { get; set; }

    public Ticket? Ticket { get; set; }
}
