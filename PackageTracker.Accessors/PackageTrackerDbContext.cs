using Microsoft.EntityFrameworkCore;
using PackageTracker.Models;

namespace PackageTracker.Accessors
{
    public class PackageTrackerDbContext : DbContext
    {
        public PackageTrackerDbContext(DbContextOptions<PackageTrackerDbContext> options)
            : base(options)
        {
        }

        public DbSet<Package> Packages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Package>(b =>
            {
                b.HasKey(p => p.Id);
                b.Property(p => p.TrackingNumber).IsRequired().HasMaxLength(200);
                b.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}
