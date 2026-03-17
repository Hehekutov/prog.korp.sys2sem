using System.ComponentModel.DataAnnotations;

namespace LibraryApp
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        public int AuthorId { get; set; }
        public Author? Author { get; set; }

        public int? GenreId { get; set; }
        public Genre? Genre { get; set; }

        // Дополнительные поля, используемые в UI
        public int? PublishYear { get; set; }
        public string ISBN { get; set; } = string.Empty;
        public int QuantityInStock { get; set; }
    }
}csharp LibraryApp/Models/Book.cs
using System.Collections.Generic;

namespace LibraryApp
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int? PublishYear { get; set; }
        public string ISBN { get; set; } = string.Empty;
        public int QuantityInStock { get; set; }

        // Many-to-Many связи
        public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
        public ICollection<BookGenre> BookGenres { get; set; } = new List<BookGenre>();
    }
}