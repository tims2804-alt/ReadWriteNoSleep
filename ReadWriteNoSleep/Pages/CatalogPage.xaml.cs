using Microsoft.EntityFrameworkCore;
using ReadWriteNoSleep.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ReadWriteNoSleep.Pages
{
    public partial class CatalogPage : Page
    {
        private List<BookViewModel> _allBooks = new();
        private List<Genre> _allGenres = new();
        private int? _selectedGenreId = null;
        private bool _searchFocused = false;
        private int _sortIndex = 0;

        public CatalogPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            using var db = new AppDbContext();

            var books = db.Books
                .Include(b => b.Author)
                .Include(b => b.BookGenres).ThenInclude(bg => bg.Genre)
                .Include(b => b.Reviews)
                .Where(b => !b.IsFrozen)
                .ToList();

            _allBooks = books.Select(b => new BookViewModel
            {
                BookId = b.BookId,
                Title = b.Title,
                CoverPath = b.CoverPath,
                Author = b.Author,
                BookGenres = b.BookGenres.ToList(),
                AvgRating = b.Reviews.Any()
                    ? "⭐ " + b.Reviews.Average(r => r.Rating).ToString("F1")
                    : "—"
            }).ToList();

            _allGenres = db.Genres.ToList();

            BuildGenreButtons();
            ApplyFilters();
        }

        private void BuildGenreButtons()
        {
            PanelGenres.Children.Clear();

            var btnAll = CreateGenreButton("Все", null);
            btnAll.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#cba6f7"));
            btnAll.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#1e1e2e"));
            PanelGenres.Children.Add(btnAll);

            foreach (var genre in _allGenres)
                PanelGenres.Children.Add(CreateGenreButton(genre.GenreName, genre.GenreId));
        }

        private Button CreateGenreButton(string name, int? genreId)
        {
            var btn = new Button
            {
                Content = name,
                Tag = genreId,
                Background = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#313244")),
                Foreground = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 0, 6, 0),
                Height = 30,
                FontSize = 12
            };
            btn.Click += GenreButton_Click;
            return btn;
        }

        private void GenreButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            _selectedGenreId = btn.Tag as int?;

            foreach (Button b in PanelGenres.Children)
            {
                bool isActive = (b.Tag as int?) == _selectedGenreId;
                b.Background = new SolidColorBrush(isActive
                    ? (Color)ColorConverter.ConvertFromString("#cba6f7")
                    : (Color)ColorConverter.ConvertFromString("#313244"));
                b.Foreground = new SolidColorBrush(isActive
                    ? (Color)ColorConverter.ConvertFromString("#1e1e2e")
                    : Colors.White);
            }

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (BooksGrid == null) return;

            var searchText = _searchFocused ? TxtSearch.Text : "";
            if (TxtSearch.Text == "Поиск по названию или автору...")
                searchText = "";

            var result = _allBooks.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                result = result.Where(b =>
                    b.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    b.Author.DisplayName.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            if (_selectedGenreId.HasValue)
            {
                result = result.Where(b =>
                    b.BookGenres.Any(bg => bg.GenreId == _selectedGenreId.Value));
            }

            result = _sortIndex switch
            {
                0 => result.OrderBy(b => b.Title),
                1 => result.OrderByDescending(b => b.Title),
                2 => result.OrderByDescending(b => b.AvgRating),
                3 => result.OrderBy(b => b.AvgRating),
                _ => result.OrderBy(b => b.Title)
            };

            BooksGrid.ItemsSource = result.ToList();
        }

        private void SortOption_Click(object sender, RoutedEventArgs e)
        {
            _sortIndex = int.Parse(((Button)sender).Tag.ToString()!);
            TxtSortSelected.Text = ((Button)sender).Content.ToString();
            SortToggle.IsChecked = false;
            ApplyFilters();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_searchFocused) ApplyFilters();
        }

        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            _searchFocused = true;
            if (TxtSearch.Text == "Поиск по названию или автору...")
            {
                TxtSearch.Text = "";
                TxtSearch.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void TxtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            _searchFocused = false;
            if (string.IsNullOrWhiteSpace(TxtSearch.Text))
            {
                TxtSearch.Text = "Поиск по названию или автору...";
                TxtSearch.Foreground = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#6c7086"));
            }
        }

        private void BtnOpenBook_Click(object sender, RoutedEventArgs e)
        {
            var bookId = (int)((Button)sender).Tag;
            NavigationService.Navigate(new BookPage(bookId));
        }

        private void BtnAddToList_Click(object sender, RoutedEventArgs e)
        {
            var bookId = (int)((Button)sender).Tag;
            var dialog = new AddToListDialog(bookId);
            dialog.ShowDialog();
        }
    }

    public class BookViewModel
    {
        public int BookId { get; set; }
        public string Title { get; set; } = null!;
        public string? CoverPath { get; set; }
        public AppUser Author { get; set; } = null!;
        public List<BookGenre> BookGenres { get; set; } = new();
        public string AvgRating { get; set; } = "—";
    }
}