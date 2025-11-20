using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context;

public class HelpMateDbContext(DbContextOptions options) : DbContext(options)
{
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
}
