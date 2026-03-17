using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;

namespace LibraryApp
{
    public partial class BookWindow : Window
    {
        private readonly LibraryContext _context;
        private readonly Book? _editing;

        public BookWindow(LibraryContext context, Book? editing = null)
        {
            InitializeComponent();
            _context = context;
            _editing = editing;

            LoadAuthorsAndGenres();

            if (_editing != null)
            {
                TitleBox.Text = _editing.Title;
                YearBox.Text = _editing.PublishYear?.ToString() ?? string.Empty;
                ISBNBox.Text = _editing.ISBN;
                QuantityBox.Text = _editing.QuantityInStock.ToString();
                Title = "Редактировать книгу";

                var selectedAuthorIds = _editing.BookAuthors.Select(ba => ba.AuthorId).ToList();
                var selectedGenreIds = _editing.BookGenres.Select(bg => bg.GenreId).ToList();

                foreach (var item in AuthorsList.Items)
                {
                    if (item is AuthorDisplay author && selectedAuthorIds.Contains(author.Id))
                    {
                        AuthorsList.SelectedItems.Add(item);
                    }
                }

                foreach (var item in GenresList.Items)
                {
                    if (item is Genre genre && selectedGenreIds.Contains(genre.Id))
                    {
                        GenresList.SelectedItems.Add(item);
                    }
                }
            }
            else
            {
                Title = "Добавить книгу";
                QuantityBox.Text = "1";
            }

            SaveButton.Click += SaveButton_Click;
            CancelButton.Click += (s, e) => Close();
        }

        private void LoadAuthorsAndGenres()
        {
            var authors = _context.Authors
                .AsNoTracking()
                .Select(a => new AuthorDisplay { Id = a.Id, Display = a.ToString() })
                .ToList();

            var genres = _context.Genres.AsNoTracking().ToList();

            AuthorsList.ItemsSource = authors;
            GenresList.ItemsSource = genres;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var title = TitleBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Введите название книги.");
                return;
            }

            if (AuthorsList.SelectedItems.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы одного автора.");
                return;
            }

            if (GenresList.SelectedItems.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы один жанр.");
                return;
            }

            if (!int.TryParse(QuantityBox.Text, out int quantity))
            {
                MessageBox.Show("Количество должно быть числом.");
                return;
            }

            if (_editing != null)
            {
                _editing.Title = title;
                _editing.ISBN = ISBNBox.Text?.Trim() ?? string.Empty;
                _editing.PublishYear = int.TryParse(YearBox.Text, out int year) ? year : null;
                _editing.QuantityInStock = quantity;

                _context.BookAuthors.RemoveRange(_context.BookAuthors.Where(ba => ba.BookId == _editing.Id));
                _context.BookGenres.RemoveRange(_context.BookGenres.Where(bg => bg.BookId == _editing.Id));

                foreach (var selectedAuthor in AuthorsList.SelectedItems.Cast<AuthorDisplay>())
                {
                    _context.BookAuthors.Add(new BookAuthor { BookId = _editing.Id, AuthorId = selectedAuthor.Id });
                }

                foreach (var selectedGenre in GenresList.SelectedItems.Cast<Genre>())
                {
                    _context.BookGenres.Add(new BookGenre { BookId = _editing.Id, GenreId = selectedGenre.Id });
                }
            }
            else
            {
                var book = new Book
                {
                    Title = title,
                    ISBN = ISBNBox.Text?.Trim() ?? string.Empty,
                    PublishYear = int.TryParse(YearBox.Text, out int year) ? year : null,
                    QuantityInStock = quantity
                };

                _context.Books.Add(book);
                _context.SaveChanges();

                foreach (var selectedAuthor in AuthorsList.SelectedItems.Cast<AuthorDisplay>())
                {
                    _context.BookAuthors.Add(new BookAuthor { BookId = book.Id, AuthorId = selectedAuthor.Id });
                }

                foreach (var selectedGenre in GenresList.SelectedItems.Cast<Genre>())
                {
                    _context.BookGenres.Add(new BookGenre { BookId = book.Id, GenreId = selectedGenre.Id });
                }
            }

            _context.SaveChanges();
            DialogResult = true;
            Close();
        }
    }

    public class AuthorDisplay
    {
        public int Id { get; set; }
        public string Display { get; set; } = string.Empty;
    }
}
