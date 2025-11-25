namespace Domain.Enums;

public enum TicketPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum TicketStatus
{
    Backlog,
    Open,
    InProgress,
    Blocked,
    Closed,
    WontDo
}

public enum TicketHistoryActionType
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
