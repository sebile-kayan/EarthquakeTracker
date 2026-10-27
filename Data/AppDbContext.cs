using EarthquakeTracker.Entities;
using Microsoft.EntityFrameworkCore;

namespace EarthquakeTracker.Data;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
	}

	public DbSet<Earthquake> Earthquakes => Set<Earthquake>();
	public DbSet<Alert> Alerts => Set<Alert>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Earthquake>(entity =>
		{
			entity.Property(e => e.ExternalId)
				  .HasMaxLength(50)
				  .IsRequired();

			entity.Property(e => e.Place)
				  .HasMaxLength(250);

			// Aynı USGS depremi iki kez kaydedilemez
			entity.HasIndex(e => e.ExternalId)
				  .IsUnique();

			// Listeleme (son depremler) ve gece temizliği bu kolona göre çalışacak
			entity.HasIndex(e => e.OccurredAtUtc);
		});

		modelBuilder.Entity<Alert>(entity =>
		{
			entity.Property(a => a.Message)
				  .HasMaxLength(250)
				  .IsRequired();

			entity.HasOne(a => a.Earthquake)
				  .WithOne(e => e.Alert)
				  .HasForeignKey<Alert>(a => a.EarthquakeId)
				  .OnDelete(DeleteBehavior.Cascade); //Ana kayda bağlı alttakiler de silinir.
		});
	}
}