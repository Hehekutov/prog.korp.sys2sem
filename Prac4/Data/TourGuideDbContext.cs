using Microsoft.EntityFrameworkCore;
using Prac4.Models;

namespace Prac4.Data;

public class TourGuideDbContext(DbContextOptions<TourGuideDbContext> options) : DbContext(options)
{
    public DbSet<City> Cities => Set<City>();

    public DbSet<Attraction> Attractions => Set<Attraction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<City>()
            .HasMany(city => city.Attractions)
            .WithOne(attraction => attraction.City)
            .HasForeignKey(attraction => attraction.CityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
