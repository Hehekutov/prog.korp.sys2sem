using BlazorShared;
using Microsoft.EntityFrameworkCore;

namespace BlazorServer.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
}
