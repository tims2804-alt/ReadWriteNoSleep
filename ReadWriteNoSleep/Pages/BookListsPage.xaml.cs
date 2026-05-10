using Microsoft.EntityFrameworkCore;

using ReadWriteNoSleep.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReadWriteNoSleep.Pages
{
    public partial class BookListsPage : Page
    {
        private string _currentSection = "Читаю";
        private List<ReadingList> _allItems = new();
        private List<Genre> _allGenres = new();
        private int? _selectedGenreId = null;
        private bool _searchFocused = false;
        private int _sortIndex = 0;

        public BookListsPage()
        {
            InitializeComponent();
            LoadGenres();
            LoadList();
        }

        private void LoadGenres()
        {
            using var db = new AppDbContext();
            _allGenres = db.Genres.ToList();
            BuildGenreButtons();
        }

        private void LoadList()
        {
            using var db = new AppDbContext();

            _allItems = db.ReadingLists
                .Include(rl => rl.Book).ThenInclude(b => b.Author)
                .Include(rl => rl.Book).ThenInclude(b => b.Reviews)
                .Include(rl => rl.Book).ThenInclude(b => b.BookGenres)
                .Where(rl => rl.UserId == Session.CurrentUser!.UserId
                          && rl.Section == _currentSection)
                .ToList();

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

            var result = _allItems.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                result = result.Where(rl =>
                    rl.Book.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    rl.Book.Author.DisplayName.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            if (_selectedGenreId.HasValue)
            {
                result = result.Where(rl =>
                    rl.Book.BookGenres.Any(bg => bg.GenreId == _selectedGenreId.Value));
            }

            result = _sortIndex switch
            {
                0 => result.OrderBy(rl => rl.Book.Title),
                1 => result.OrderByDescending(rl => rl.Book.Title),
                2 => result.OrderByDescending(rl =>
                    rl.Book.Reviews.Any() ? rl.Book.Reviews.Average(r => r.Rating) : 0),
                3 => result.OrderBy(rl =>
                    rl.Book.Reviews.Any() ? rl.Book.Reviews.Average(r => r.Rating) : 0),
                _ => result.OrderBy(rl => rl.Book.Title)
            };

            var list = result.ToList();
            BooksGrid.ItemsSource = list;
            TxtEmpty.Visibility = list.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SortOption_Click(object sender, RoutedEventArgs e)
        {
            _sortIndex = int.Parse(((Button)sender).Tag.ToString()!);
            TxtSortSelected.Text = ((Button)sender).Content.ToString();
            SortToggle.IsChecked = false;
            ApplyFilters();
        }

        private void SetActiveTab(Button active)
        {
            var tabs = new[] { BtnReading, BtnPlanned, BtnCompleted, BtnAbandoned };
            foreach (var btn in tabs)
            {
                btn.Background = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#313244"));
                btn.Foreground = new SolidColorBrush(Colors.White);
            }
            active.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#cba6f7"));
            active.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#1e1e2e"));
        }

        private void BtnReading_Click(object sender, RoutedEventArgs e)
        {
            _currentSection = "Читаю";
            SetActiveTab(BtnReading);
            LoadList();
        }

        private void BtnPlanned_Click(object sender, RoutedEventArgs e)
        {
            _currentSection = "В планах";
            SetActiveTab(BtnPlanned);
            LoadList();
        }

        private void BtnCompleted_Click(object sender, RoutedEventArgs e)
        {
            _currentSection = "Прочитано";
            SetActiveTab(BtnCompleted);
            LoadList();
        }

        private void BtnAbandoned_Click(object sender, RoutedEventArgs e)
        {
            _currentSection = "Заброшено";
            SetActiveTab(BtnAbandoned);
            LoadList();
        }

        private void BtnOpenBook_Click(object sender, RoutedEventArgs e)
        {
            var bookId = (int)((Button)sender).Tag;
            NavigationService.Navigate(new BookPage(bookId));
        }

        private void BtnMoveBook_Click(object sender, RoutedEventArgs e)
        {
            var bookId = (int)((Button)sender).Tag;
            var dialog = new MoveToListDialog(bookId, _currentSection);
            if (dialog.ShowDialog() == true)
                LoadList();
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

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_searchFocused) ApplyFilters();
        }
    }
}