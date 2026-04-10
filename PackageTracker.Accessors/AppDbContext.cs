using Microsoft.EntityFrameworkCore;
using PackageTracker.Models;

namespace PackageTracker.Accessors.Data;
public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
	public DbSet<Depot> Depots { get; set; }
	public DbSet<Drone> Drones { get; set; }
	public DbSet<Location> Locations { get; set; }
	public DbSet<Package> Packages { get; set; }
	public DbSet<User> Users { get; set; }
	public DbSet<PackageStatusEvent> PackageStatusEvents { get; set; }


	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// Drone has two FKs to Depot — must be explicit so EF doesn't get confused
		modelBuilder.Entity<Drone>()
			.HasOne(d => d.HomeDepot)
			.WithMany()
			.HasForeignKey(d => d.HomeDepotId)
			.OnDelete(DeleteBehavior.Restrict);

		// Package has two FKs to Location — same issue
		modelBuilder.Entity<Package>()
			.HasOne(p => p.OriginLocation)
			.WithMany()
			.HasForeignKey(p => p.OriginLocationId)
			.OnDelete(DeleteBehavior.Restrict);

		modelBuilder.Entity<Package>()
			.HasOne(p => p.DestinationLocation)
			.WithMany()
			.HasForeignKey(p => p.DestinationLocationId)
			.OnDelete(DeleteBehavior.Restrict);

		modelBuilder.Entity<Package>()
			.HasOne(p => p.Sender)
			.WithMany()
			.HasForeignKey(p => p.SenderId)
			.OnDelete(DeleteBehavior.Restrict);

		// Drone's CurrentDepot can be null (if drone is in transit)
		modelBuilder.Entity<Drone>()
			.HasOne(d => d.CurrentDepot)
			.WithMany()
			.HasForeignKey(d => d.CurrentDepotId)
			.IsRequired(false)
			.OnDelete(DeleteBehavior.Restrict);
		
		modelBuilder.Entity<Drone>()
			.HasOne(d => d.CurrentPackage)
			.WithMany()
			.HasForeignKey(d => d.CurrentPackageId)
			.IsRequired(false)
			.OnDelete(DeleteBehavior.Restrict);

		modelBuilder.Entity<Drone>()
			.HasOne(d => d.DestinationDepot)
			.WithMany()
			.HasForeignKey(d => d.DestinationDepotId)
			.IsRequired(false)
			.OnDelete(DeleteBehavior.Restrict);

		modelBuilder.Entity<PackageStatusEvent>()
			.HasOne(e => e.Package)
			.WithMany()
			.HasForeignKey(e => e.PackageId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<PackageStatusEvent>()
			.HasOne(e => e.Depot)
			.WithMany()
			.HasForeignKey(e => e.DepotId)
			.IsRequired(false)
			.OnDelete(DeleteBehavior.Restrict);

	}
}
