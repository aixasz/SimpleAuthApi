using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SimpleAuthApi.Domain.Entities;

namespace SimpleAuthApi.Domain;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User, Role, Guid>(options)
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>().HasQueryFilter(p => !p.IsDeleted);
        builder.Entity<RefreshToken>().HasQueryFilter(p => !p.IsRevoked);
    }

    public override int SaveChanges()
    {
        UpdateTimeStamp(ChangeTracker);
        SoftDeletion(ChangeTracker);
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimeStamp(ChangeTracker);
        SoftDeletion(ChangeTracker);
        return base.SaveChangesAsync(cancellationToken);
    }

    public static void UpdateTimeStamp(ChangeTracker changeTracker)
    {
        var entries = changeTracker.Entries()
            .Where(entry => entry.Entity is IUpdateTimeStamp
                        && (entry.State == EntityState.Added || entry.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var timestamp = DateTime.UtcNow;

            ((IUpdateTimeStamp)entry.Entity).Updated = timestamp;

            if (entry.State == EntityState.Added)
            {
                ((IUpdateTimeStamp)entry.Entity).Created = timestamp;
            }
        }
    }

    public static void SoftDeletion(ChangeTracker changeTracker)
    {
        var entries = changeTracker.Entries()
            .Where(e => e.Entity is ISoftDelete && e.State == EntityState.Deleted);

        foreach (var entry in entries)
        {
            entry.State = EntityState.Modified;
            ((ISoftDelete)entry.Entity).IsDeleted = true;
            ((ISoftDelete)entry.Entity).Deleted = DateTime.UtcNow;
        }
    }
}
