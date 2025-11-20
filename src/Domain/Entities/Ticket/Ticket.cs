using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Board;

namespace Domain.Entities.Ticket;

public class Ticket : BaseEntity
{
    public enum Statuses
    {
        Backlog,
        Open,
        InProgress,
        Blocked,
        Closed
    }

    public enum Priorities
    {
        Low,
        Medium,
        High,
        Critical
    }

    public required string Title { get; set; }
    public required string Description { get; set; }
    public DateTime? DueDate { get; set; }
    public Statuses Status { get; set; } = Statuses.Backlog;
    public Priorities Priority { get; set; } = Priorities.Low;

    [ForeignKey("CreatedBy")]
    public required int CreatedById { get; set; }
    public User.User? CreatedBy { get; set; }

    [ForeignKey("Reporter")]
    public required int ReporterId { get; set; }
    public User.User? Reporter { get; set; }

    [ForeignKey("Assignee")]
    public int? AssigneeId { get; set; }
    public User.User? Assignee { get; set; }

    [ForeignKey("Board")]
    public required int BoardId { get; set; }
    public Board.Board? Board { get; set; }

    public string Code() => $"{Board?.Code}-{Id}";

    public List<TicketComment> Comments { get; } = [];

    public List<Tag> Tags { get; } = [];
    public List<User.User> Watchers { get; } = [];
    public List<TicketHistory> History { get; } = [];
}
