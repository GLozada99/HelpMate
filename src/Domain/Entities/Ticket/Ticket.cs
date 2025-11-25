using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Entities.Ticket;

public class Ticket : BaseEntity
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public DateTime? DueDate { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Backlog;
    public TicketPriority Priority { get; set; } = TicketPriority.Low;

    [ForeignKey("CreatedBy")] public int CreatedById { get; set; }

    public User.User? CreatedBy { get; set; }

    [ForeignKey("Reporter")] public int ReporterId { get; set; }

    public User.User? Reporter { get; set; }

    [ForeignKey("Assignee")] public int? AssigneeId { get; set; }

    public User.User? Assignee { get; set; }

    [ForeignKey("Board")] public int BoardId { get; set; }

    public Board.Board? Board { get; set; }

    public List<TicketComment> Comments { get; } = [];

    public List<Tag> Tags { get; } = [];
    public List<User.User> Watchers { get; } = [];
    public List<TicketHistory> History { get; } = [];
}
