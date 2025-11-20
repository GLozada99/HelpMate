using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Ticket;

public class TicketComment : BaseEntity
{
    public required string Text { get; set; }

    [ForeignKey("User")]
    public required int UserId { get; set; }
    public User.User? User { get; set; }

    [ForeignKey("Ticket")]
    public required int TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    public bool Edited { get; set; }
}
