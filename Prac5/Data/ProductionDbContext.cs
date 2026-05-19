using Microsoft.EntityFrameworkCore;
using Prac5.Models;

namespace Prac5.Data;

public class ProductionDbContext : DbContext
{
    public ProductionDbContext(DbContextOptions<ProductionDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<ProductionLine> ProductionLines => Set<ProductionLine>();

    public DbSet<Material> Materials => Set<Material>();

    public DbSet<ProductMaterial> ProductMaterials => Set<ProductMaterial>();

    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductMaterial>()
            .HasKey(item => new { item.ProductId, item.MaterialId });

        modelBuilder.Entity<ProductMaterial>()
            .HasOne(item => item.Product)
            .WithMany(product => product.ProductMaterials)
            .HasForeignKey(item => item.ProductId);

        modelBuilder.Entity<ProductMaterial>()
            .HasOne(item => item.Material)
            .WithMany(material => material.ProductMaterials)
            .HasForeignKey(item => item.MaterialId);

        modelBuilder.Entity<WorkOrder>()
            .HasOne(order => order.Product)
            .WithMany(product => product.WorkOrders)
            .HasForeignKey(order => order.ProductId);

        modelBuilder.Entity<WorkOrder>()
            .HasOne(order => order.ProductionLine)
            .WithMany(line => line.WorkOrders)
            .HasForeignKey(order => order.ProductionLineId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
