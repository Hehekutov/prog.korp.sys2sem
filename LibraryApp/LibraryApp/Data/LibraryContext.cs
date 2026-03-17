using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;

namespace LibraryApp
{
    public class LibraryContext : DbContext
    {
        private const string DbFileName = "library.db";

        public DbSet<Author> Authors { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookAuthor> BookAuthors { get; set; }
        public DbSet<BookGenre> BookGenres { get; set; }

        public LibraryContext()
        {
        }

        public void EnsureDatabaseCompatibility()
        {
            try
            {
                Authors.AsNoTracking().FirstOrDefault();
                Genres.AsNoTracking().FirstOrDefault();
                Books.AsNoTracking().FirstOrDefault();
                BookAuthors.AsNoTracking().FirstOrDefault();
                BookGenres.AsNoTracking().FirstOrDefault();
            }
            catch (Exception)
            {
                try { Database.EnsureDeleted(); } catch { }

                Database.EnsureCreated();
                SeedInitialData();
            }
        }

        private void SeedInitialData()
        {
            if (!Genres.Any())
            {
                Genres.AddRange(
                    new Genre { Name = "Фантастика" },
                    new Genre { Name = "Детектив" },
                    new Genre { Name = "Научпоп" }
                );
            }

            if (!Authors.Any())
            {
                Authors.AddRange(
                    new Author { FirstName = "Иван", LastName = "Иванов", BirthDate = new DateTime(1975, 5, 1), Country = "Россия" },
                    new Author { FirstName = "Мария", LastName = "Петрова", BirthDate = new DateTime(1982, 10, 12), Country = "Россия" }
                );
            }

            SaveChanges();

            if (!Books.Any())
            {
                var book1 = new Book { Title = "Книга 1", PublishYear = 2020, ISBN = "123-456-789-0", QuantityInStock = 5 };
                var book2 = new Book { Title = "Книга 2", PublishYear = 2021, ISBN = "123-456-789-1", QuantityInStock = 3 };

                Books.AddRange(book1, book2);
                SaveChanges();

                BookAuthors.AddRange(
                    new BookAuthor { BookId = book1.Id, AuthorId = Authors.First().Id },
                    new BookAuthor { BookId = book2.Id, AuthorId = Authors.Skip(1).First().Id }
                );

                BookGenres.AddRange(
                    new BookGenre { BookId = book1.Id, GenreId = Genres.First().Id },
                    new BookGenre { BookId = book2.Id, GenreId = Genres.Skip(1).First().Id }
                );

                SaveChanges();
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var dbPath = Path.Combine(AppContext.BaseDirectory, DbFileName);
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Author>(b =>
            {
                b.HasKey(a => a.Id);
                b.Property(a => a.FirstName).HasMaxLength(100);
                b.Property(a => a.LastName).HasMaxLength(100);
                b.Property(a => a.Country).HasMaxLength(100);
            });

            modelBuilder.Entity<Genre>(b =>
            {
                b.HasKey(g => g.Id);
                b.Property(g => g.Name).HasMaxLength(100).IsRequired();
                b.HasIndex(g => g.Name).IsUnique();
            });

            modelBuilder.Entity<Book>(b =>
            {
                b.HasKey(bk => bk.Id);
                b.Property(bk => bk.Title).HasMaxLength(200).IsRequired();
                b.Property(bk => bk.ISBN).HasMaxLength(20);
            });

            // Many-to-Many: Book <-> Author
            modelBuilder.Entity<BookAuthor>(b =>
            {
                b.HasKey(ba => new { ba.BookId, ba.AuthorId });
                b.HasOne(ba => ba.Book)
                    .WithMany(bk => bk.BookAuthors)
                    .HasForeignKey(ba => ba.BookId)
                    .OnDelete(DeleteBehavior.Cascade);
                b.HasOne(ba => ba.Author)
                    .WithMany(a => a.BookAuthors)
                    .HasForeignKey(ba => ba.AuthorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Many-to-Many: Book <-> Genre
            modelBuilder.Entity<BookGenre>(b =>
            {
                b.HasKey(bg => new { bg.BookId, bg.GenreId });
                b.HasOne(bg => bg.Book)
                    .WithMany(bk => bk.BookGenres)
                    .HasForeignKey(bg => bg.BookId)
                    .OnDelete(DeleteBehavior.Cascade);
                b.HasOne(bg => bg.Genre)
                    .WithMany(g => g.BookGenres)
                    .HasForeignKey(bg => bg.GenreId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}