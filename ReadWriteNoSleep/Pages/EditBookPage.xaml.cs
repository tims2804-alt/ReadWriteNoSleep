using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using ReadWriteNoSleep.Services;

namespace ReadWriteNoSleep.Pages
{
    /// <summary>
    /// Логика взаимодействия для EditBookPage.xaml
    /// </summary>
    // EditBookPage.xaml.cs
   

    public partial class EditBookPage : Page
    {
        private readonly int? _bookId;
        private List<Genre> _allGenres = new();
        private List<int> _selectedGenreIds = new();

        public EditBookPage(int? bookId)
        {
            InitializeComponent();
            _bookId = bookId;
            TxtPageTitle.Text = bookId == null ? "Добавить книгу" : "Редактировать книгу";
            LoadGenres();

            if (bookId != null)
                LoadBook();
        }

        private void LoadGenres()
        {
            using var db = new AppDbContext();
            _allGenres = db.Genres.ToList();
            BuildGenreCheckboxes();
        }

        private void BuildGenreCheckboxes()
        {
            PanelGenres.Children.Clear();

            foreach (var genre in _allGenres)
            {
                var cb = new CheckBox
                {
                    Content = genre.GenreName,
                    Foreground = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(0, 0, 12, 6),
                    Tag = genre.GenreId,
                    IsChecked = _selectedGenreIds.Contains(genre.GenreId)
                };
                cb.Checked += Genre_Checked;
                cb.Unchecked += Genre_Unchecked;
                PanelGenres.Children.Add(cb);
            }
        }

        private void Genre_Checked(object sender, RoutedEventArgs e)
        {
            var id = (int)((CheckBox)sender).Tag;
            if (!_selectedGenreIds.Contains(id))
                _selectedGenreIds.Add(id);
        }

        private void Genre_Unchecked(object sender, RoutedEventArgs e)
        {
            var id = (int)((CheckBox)sender).Tag;
            _selectedGenreIds.Remove(id);
        }

        private void LoadBook()
        {
            using var db = new AppDbContext();

            var book = db.Books
                .Include(b => b.BookGenres)
                .FirstOrDefault(b => b.BookId == _bookId);

            if (book == null) return;

            TxtTitle.Text = book.Title;
            TxtDescription.Text = book.Description;
            TxtContent.Text = book.TextContent;

            _selectedGenreIds = book.BookGenres
                .Select(bg => bg.GenreId)
                .ToList();

            // Перестраиваем чекбоксы с отмеченными жанрами
            BuildGenreCheckboxes();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtTitle.Text))
            {
                MessageBox.Show("Введите название книги.");
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtContent.Text))
            {
                MessageBox.Show("Введите текст книги.");
                return;
            }

            using var db = new AppDbContext();

            if (_bookId == null)
            {
                // Добавление новой книги
                var book = new Book
                {
                    AuthorId = Session.CurrentUser!.UserId,
                    Title = TxtTitle.Text.Trim(),
                    Description = TxtDescription.Text.Trim(),
                    TextContent = TxtContent.Text.Trim(),
                    IsFrozen = false
                };

                db.Books.Add(book);
                db.SaveChanges();

                // Добавляем жанры
                foreach (var genreId in _selectedGenreIds)
                {
                    db.BookGenres.Add(new BookGenre
                    {
                        BookId = book.BookId,
                        GenreId = genreId
                    });
                }

                db.SaveChanges();

                MessageBox.Show("Книга опубликована!", "Готово",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Редактирование
                var book = db.Books
                    .Include(b => b.BookGenres)
                    .FirstOrDefault(b => b.BookId == _bookId);

                if (book == null) return;

                book.Title = TxtTitle.Text.Trim();
                book.Description = TxtDescription.Text.Trim();
                book.TextContent = TxtContent.Text.Trim();

                // Обновляем жанры — удаляем старые, добавляем новые
                db.BookGenres.RemoveRange(book.BookGenres);

                foreach (var genreId in _selectedGenreIds)
                {
                    db.BookGenres.Add(new BookGenre
                    {
                        BookId = book.BookId,
                        GenreId = genreId
                    });
                }

                db.SaveChanges();

                MessageBox.Show("Книга обновлена!", "Готово",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }

            NavigationService.GoBack();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
            => NavigationService.GoBack();
    }
}
