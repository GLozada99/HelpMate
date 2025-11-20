namespace Domain.Entities.Ticket;

public class Tag : BaseEntity
{
    public required string Name { get; set; }

    public List<Ticket> Tickets { get; } = [];
}
