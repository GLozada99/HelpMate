using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Board;

namespace Domain.Entities.Ticket;

public class TicketHistory : BaseEntity
{
    public enum ActionTypes
    {
        StatusChanged,
        PriorityChanged,
        CommentAdded,
        AssigneeChanged,
        ReporterChanged,
        DueDateChanged,
        TagAdded,
        TagRemoved,
        OtherChange
    }

    public required ActionTypes Action { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    [ForeignKey("User")]
    public required int UserId { get; set; }
    public User.User? User { get; set; }

    [ForeignKey("Ticket")]
    public required int TicketId { get; set; }
    public Ticket? Ticket { get; set; }
}
