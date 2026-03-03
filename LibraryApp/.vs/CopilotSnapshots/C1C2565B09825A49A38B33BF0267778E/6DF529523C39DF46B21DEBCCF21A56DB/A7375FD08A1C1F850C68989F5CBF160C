using Microsoft.EntityFrameworkCore;

public class LibraryContext : DbContext
{
    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Genre> Genres { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlServer(
            "Server=(localdb)\\MSSQLLocalDB;Database=LibraryDB;Trusted_Connection=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Book
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);

            entity.Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(b => b.ISBN)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(b => b.Genre)
                .WithMany(g => g.Books)
                .HasForeignKey(b => b.GenreId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Author
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(a => a.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(a => a.Country)
                .HasMaxLength(100);
        });

        // Genre
        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(g => g.Id);

            entity.Property(g => g.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(g => g.Description)
                .HasMaxLength(500);
        });
    }
}