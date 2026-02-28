using Microsoft.EntityFrameworkCore;
using System.Windows;

using Microsoft.EntityFrameworkCore;
using System;
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
            _context.Database.Migrate();

            LoadFilters();
            LoadBooks();

            // Подписки на события
            SearchBox.TextChanged += (s, e) => ApplyFilters();
            AuthorFilter.SelectionChanged += (s, e) => ApplyFilters();
            GenreFilter.SelectionChanged += (s, e) => ApplyFilters();

            DeleteButton.Click += DeleteButton_Click;
        }

        private void LoadFilters()
        {
            AuthorFilter.ItemsSource = _context.Authors.ToList();
            AuthorFilter.SelectedIndex = -1;

            GenreFilter.ItemsSource = _context.Genres.ToList();
            GenreFilter.SelectedIndex = -1;
        }

        private void LoadBooks()
        {
            var books = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
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

            BooksGrid.ItemsSource = query.ToList();
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
                    _context.Books.Remove(selectedBook);
                    _context.SaveChanges();
                    ApplyFilters();
                }
            }
            else
            {
                MessageBox.Show("Выберите книгу для удаления.");
            }
        }
    }
}