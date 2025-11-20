using Domain.Entities;
using Domain.Entities.Board;
using Domain.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context;

public class HelpMateDbContext(DbContextOptions<HelpMateDbContext> options) : DbContext(options)
{

    public DbSet<User> Users { get; set; }
    public DbSet<Board> Boards { get; set; }
    public DbSet<BoardMembership> BoardMemberships { get; set; }

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
            .Property(o => o.Roles)
            .HasConversion<string>();
    }
}
