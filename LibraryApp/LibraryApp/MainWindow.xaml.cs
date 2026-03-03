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

            // Надёжная подготовка БД: если модель и схема рассинхронизированы,
            // контекст пересоздаст базу и заполнит тестовыми данными.
            _context.EnsureDatabaseCompatibility();

            LoadFilters();
            LoadBooks();

            // Подписки на события
            SearchBox.TextChanged += (s, e) => ApplyFilters();
            AuthorFilter.SelectionChanged += (s, e) => ApplyFilters();
            GenreFilter.SelectionChanged += (s, e) => ApplyFilters();

            AddButton.Click += AddButton_Click;
            EditButton.Click += EditButton_Click;
            DeleteButton.Click += DeleteButton_Click;
        }

        private void LoadFilters()
        {
            AuthorFilter.ItemsSource = _context.Authors.AsNoTracking().ToList();
            AuthorFilter.SelectedIndex = -1;

            GenreFilter.ItemsSource = _context.Genres.AsNoTracking().ToList();
            GenreFilter.SelectedIndex = -1;
        }

        private void LoadBooks()
        {
            var books = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .AsNoTracking()
                .ToList();

            BooksGrid.ItemsSource = books;
        }

        private void ApplyFilters()
        {
            var query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .AsQueryable();

            // Поиск по названию
            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                query = query.Where(b =>
                    b.Title.Contains(SearchBox.Text));
            }

            // Фильтр по автору
            if (AuthorFilter.SelectedItem is Author selectedAuthor)
            {
                query = query.Where(b =>
                    b.AuthorId == selectedAuthor.Id);
            }

            // Фильтр по жанру
            if (GenreFilter.SelectedItem is Genre selectedGenre)
            {
                query = query.Where(b =>
                    b.GenreId == selectedGenre.Id);
            }

            BooksGrid.ItemsSource = query
                .AsNoTracking()
                .ToList();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (BooksGrid.SelectedItem is Book selectedBook)
            {
                var result = MessageBox.Show(
                    "Удалить выбранную книгу?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // Получаем отслеживаемую сущность по Id и удаляем её
                    var book = _context.Books.Find(selectedBook.Id);
                    if (book != null)
                    {
                        _context.Books.Remove(book);
                        _context.SaveChanges();
                        ApplyFilters();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось найти книгу в контексте для удаления.");
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
                // Сохранение уже выполнено в диалоге; просто обновляем список
                ApplyFilters();
            }
        }

        private void EditButton_Click(object? sender, RoutedEventArgs e)
        {
            if (BooksGrid.SelectedItem is Book selectedBook)
            {
                // Получаем отслеживаемую сущность из контекста
                var book = _context.Books
                    .Include(b => b.Author)
                    .Include(b => b.Genre)
                    .FirstOrDefault(b => b.Id == selectedBook.Id);

                if (book == null)
                {
                    MessageBox.Show("Не удалось найти книгу в контексте для редактирования.");
                    return;
                }

                var win = new BookWindow(_context, book);
                win.Owner = this;
                if (win.ShowDialog() == true)
                {
                    // Изменения уже сохранены в диалоге
                    ApplyFilters();
                }
            }
            else
            {
                MessageBox.Show("Выберите книгу для редактирования.");
            }
        }
    }
}