using Domain.Entities;
using Domain.Entities.Board;
using Domain.Entities.Ticket;
using Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Context;

public class HelpMateDbContext(DbContextOptions<HelpMateDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Board> Boards { get; set; }
    public DbSet<BoardMembership> BoardMemberships { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<TicketComment> TicketComments { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<TicketHistory> TicketHistories { get; set; }

    public override int SaveChanges()
    {
        AddTimestamps();
        return base.SaveChanges();
    }

    public async Task<int> SaveChangesAsync()
    {
        AddTimestamps();
        return await base.SaveChangesAsync();
    }

    private void AddTimestamps()
    {
        var entities = ChangeTracker.Entries()
            .Where(x => x is
            {
                Entity: BaseEntity, State: EntityState.Modified or EntityState.Added
            });

        foreach (var entity in entities)
        {
            var now = DateTime.UtcNow;

            switch (entity.State)
            {
                case EntityState.Added:
                    ((BaseEntity)entity.Entity).CreatedAt = now;
                    ((BaseEntity)entity.Entity).UpdatedAt = now;
                    break;
                case EntityState.Modified:
                    ((BaseEntity)entity.Entity).UpdatedAt = now;
                    break;
                case EntityState.Detached:
                case EntityState.Unchanged:
                case EntityState.Deleted:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Property(o => o.Role)
            .HasConversion<string>();

        modelBuilder.Entity<User>()
            .Property(o => o.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Board>()
            .Property(o => o.Status)
            .HasConversion<string>();

        modelBuilder.Entity<BoardMembership>()
            .Property(b => b.Roles)
            .HasConversion(
                v => v.Select(r => r.ToString()).ToArray(),
                v => v.Select(Enum.Parse<BoardMembership.MembershipRoles>).ToList()
            )
            .HasColumnType("text[]")
            .Metadata
            .SetValueComparer(new ValueComparer<List<BoardMembership.MembershipRoles>>(
                (l1, l2) =>
                    (l1 != null && l2 != null && l1.SequenceEqual(l2)) ||
                    (l1 == null && l2 == null),
                l => l.GetHashCode()
            ));

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.CreatedBy)
            .WithMany(u => u.CreatedTickets)
            .HasForeignKey(t => t.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Assignee)
            .WithMany(u => u.AssignedTickets)
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Reporter)
            .WithMany(u => u.ReportingTickets)
            .HasForeignKey(t => t.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ticket>()
            .Property(o => o.Priority)
            .HasConversion<string>();

        modelBuilder.Entity<Ticket>()
            .Property(o => o.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Ticket>()
            .HasMany(t => t.Tags)
            .WithMany(tg => tg.Tickets)
            .UsingEntity<Dictionary<string, object>>(
                "TicketTag",
                j => j
                    .HasOne<Tag>()
                    .WithMany()
                    .HasForeignKey("TagId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne<Ticket>()
                    .WithMany()
                    .HasForeignKey("TicketId")
                    .OnDelete(DeleteBehavior.Cascade)
            );

        modelBuilder.Entity<Ticket>()
            .HasMany(t => t.Watchers)
            .WithMany(tw => tw.WatchingTickets)
            .UsingEntity<Dictionary<string, object>>(
                "TicketWatchers",
                j => j
                    .HasOne<User>()
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne<Ticket>()
                    .WithMany()
                    .HasForeignKey("TicketId")
                    .OnDelete(DeleteBehavior.Cascade)
            );
    }
}
