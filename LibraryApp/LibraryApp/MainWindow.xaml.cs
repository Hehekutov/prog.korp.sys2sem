using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;

namespace LibraryApp
{
    public partial class MainWindow : Window
    {
        private LibraryContext _context;

        public MainWindow()
        {
            InitializeComponent();

            _context = new LibraryContext();
            _context.EnsureDatabaseCompatibility();

            LoadBooks();

            SearchBox.TextChanged += (s, e) => ApplyFilters();

            AddButton.Click += AddButton_Click;
            EditButton.Click += EditButton_Click;
            DeleteButton.Click += DeleteButton_Click;
            AddAuthorButton.Click += AddAuthorButton_Click;
            AddGenreButton.Click += AddGenreButton_Click;
        }

        private void LoadBooks()
        {
            var books = _context.Books
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .Include(b => b.BookGenres)
                .ThenInclude(bg => bg.Genre)
                .AsNoTracking()
                .ToList()
                .Select(BookViewModel.FromBook)
                .ToList();

            BooksGrid.ItemsSource = books;
        }

        private void ApplyFilters()
        {
            var query = _context.Books
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .Include(b => b.BookGenres)
                .ThenInclude(bg => bg.Genre)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                query = query.Where(b => b.Title.Contains(SearchBox.Text));
            }

            BooksGrid.ItemsSource = query
                .AsNoTracking()
                .ToList()
                .Select(BookViewModel.FromBook)
                .ToList();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (BooksGrid.SelectedItem is BookViewModel selectedBook)
            {
                var result = MessageBox.Show("Удалить выбранную книгу?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    var book = _context.Books.Find(selectedBook.Id);
                    if (book != null)
                    {
                        _context.Books.Remove(book);
                        _context.SaveChanges();
                        ApplyFilters();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите книгу для удаления.");
            }
        }

        private void AddButton_Click(object? sender, RoutedEventArgs e)
        {
            var win = new BookWindow(_context);
            win.Owner = this;
            if (win.ShowDialog() == true)
            {
                ApplyFilters();
            }
        }

        private void EditButton_Click(object? sender, RoutedEventArgs e)
        {
            if (BooksGrid.SelectedItem is BookViewModel selectedBook)
            {
                var book = _context.Books
                    .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                    .Include(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
                    .FirstOrDefault(b => b.Id == selectedBook.Id);

                if (book != null)
                {
                    var win = new BookWindow(_context, book);
                    win.Owner = this;
                    if (win.ShowDialog() == true)
                    {
                        ApplyFilters();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите книгу для редактирования.");
            }
        }

        private void AddAuthorButton_Click(object? sender, RoutedEventArgs e)
        {
            var win = new AuthorWindow(_context);
            win.Owner = this;
            win.ShowDialog();
        }

        private void AddGenreButton_Click(object? sender, RoutedEventArgs e)
        {
            var win = new GenreWindow(_context);
            win.Owner = this;
            win.ShowDialog();
        }
    }

    public class BookViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int? PublishYear { get; set; }
        public string ISBN { get; set; } = string.Empty;
        public int QuantityInStock { get; set; }
        public string AuthorsDisplay { get; set; } = string.Empty;
        public string GenresDisplay { get; set; } = string.Empty;

        public static BookViewModel FromBook(Book book)
        {
            return new BookViewModel
            {
                Id = book.Id,
                Title = book.Title,
                PublishYear = book.PublishYear,
                ISBN = book.ISBN,
                QuantityInStock = book.QuantityInStock,
                AuthorsDisplay = string.Join(", ", book.BookAuthors.Select(ba => ba.Author?.ToString() ?? "Неизвестен")),
                GenresDisplay = string.Join(", ", book.BookGenres.Select(bg => bg.Genre?.Name ?? "Неизвестен"))
            };
        }
    }
}