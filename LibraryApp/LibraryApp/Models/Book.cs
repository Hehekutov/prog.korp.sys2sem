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
}