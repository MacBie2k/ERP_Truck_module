using Microsoft.EntityFrameworkCore;
using Web.Api.Entities;

namespace Web.Api.Database;

public class ApplicationDBContext : DbContext
{
    public ApplicationDBContext(DbContextOptions options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Truck>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id)
                .UseIdentityColumn();

            entity.HasIndex(x => x.Code)
                .IsUnique();

            entity.Property(x => x.Status)
                .HasDefaultValue(TruckStatusEnum.OutOfService);
        });
    }
    public DbSet<Truck> Trucks { get; set; }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is Entity && e.State is EntityState.Added or EntityState.Modified);

        foreach (var entityEntry in entries)
        {
            ((Entity)entityEntry.Entity).UpdatedTime = DateTime.Now;
            entityEntry.Property("UpdatedTime").IsModified = true;

            if (entityEntry.State == EntityState.Added)
            {
                ((Entity)entityEntry.Entity).CreatedTime = DateTime.Now;
            }
            else
            {
                entityEntry.Property("CreatedTime").IsModified = false;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}