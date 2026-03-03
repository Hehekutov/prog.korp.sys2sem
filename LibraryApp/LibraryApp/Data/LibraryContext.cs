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

        public LibraryContext()
        {
        }

        // Простая и надёжная подготовка БД: если модель и схема не совпадают,
        // пересоздаём БД и заполняем начальными данными.
        public void EnsureDatabaseCompatibility()
        {
            try
            {
                // Пробуем выполнить безопасные запросы; если столбцов не хватает, EF/SQL бросит исключение.
                // Проверяем несколько таблиц, чтобы обнаружить несоответствие схемы для любой из них.
                Authors.AsNoTracking().FirstOrDefault();
                Genres.AsNoTracking().FirstOrDefault();
                Books.AsNoTracking().FirstOrDefault();
            }
            catch (Exception)
            {
                try
                {
                    Database.EnsureDeleted();
                }
                catch { }

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
                Books.AddRange(
                    new Book { Title = "Книга 1", AuthorId = Authors.First().Id, GenreId = Genres.First().Id },
                    new Book { Title = "Книга 2", AuthorId = Authors.Skip(1).First().Id, GenreId = Genres.Skip(1).First().Id }
                );
            }

            SaveChanges();
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
                b.Property(g => g.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Book>(b =>
            {
                b.HasKey(bk => bk.Id);
                b.Property(bk => bk.Title).HasMaxLength(200);
                b.HasOne(bk => bk.Author).WithMany(a => a.Books).HasForeignKey(bk => bk.AuthorId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(bk => bk.Genre).WithMany(g => g.Books).HasForeignKey(bk => bk.GenreId).OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}