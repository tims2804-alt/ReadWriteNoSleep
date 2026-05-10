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
    /// Логика взаимодействия для AuthorPage.xaml
    /// </summary>
    // AuthorPage.xaml.cs
  

    public partial class AuthorPage : Page
    {
        private bool _showFrozen = false;

        public AuthorPage()
        {
            InitializeComponent();
            LoadBooks();
        }

        private void LoadBooks()
        {
            using var db = new AppDbContext();

            var books = db.Books
                .Where(b => b.AuthorId == Session.CurrentUser!.UserId
                         && b.IsFrozen == _showFrozen)
                .ToList();

            var items = books.Select(b => new AuthorBookViewModel
            {
                BookId = b.BookId,
                Title = b.Title,
                Description = b.Description ?? "Нет описания",
                IsFrozen = b.IsFrozen,
                FrozenVisibility = b.IsFrozen ? Visibility.Visible : Visibility.Collapsed,
                PublishedVisibility = b.IsFrozen ? Visibility.Collapsed : Visibility.Visible
            }).ToList();

            BooksList.ItemsSource = items;
            TxtEmpty.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnPublished_Click(object sender, RoutedEventArgs e)
        {
            _showFrozen = false;
            SetActiveTab(BtnPublished, BtnFrozen);
            LoadBooks();
        }

        private void BtnFrozen_Click(object sender, RoutedEventArgs e)
        {
            _showFrozen = true;
            SetActiveTab(BtnFrozen, BtnPublished);
            LoadBooks();
        }

        private void SetActiveTab(Button active, Button inactive)
        {
            active.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#cba6f7"));
            active.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#1e1e2e"));

            inactive.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#313244"));
            inactive.Foreground = new SolidColorBrush(Colors.White);
        }

        private void BtnAddBook_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditBookPage(null));
        }

        private void BtnEditBook_Click(object sender, RoutedEventArgs e)
        {
            var bookId = (int)((Button)sender).Tag;
            NavigationService.Navigate(new EditBookPage(bookId));
        }

        private void BtnViewBook_Click(object sender, RoutedEventArgs e)
        {
            var bookId = (int)((Button)sender).Tag;
            NavigationService.Navigate(new BookPage(bookId));
        }

        private void BtnAppealBook_Click(object sender, RoutedEventArgs e)
        {
            var bookId = (int)((Button)sender).Tag;
            var dialog = new AppealBookFreezeDialog(bookId);
            dialog.ShowDialog();
            LoadBooks();
        }
    }

    // ViewModel для списка книг автора
    public class AuthorBookViewModel
    {
        public int BookId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsFrozen { get; set; }
        public Visibility FrozenVisibility { get; set; }
        public Visibility PublishedVisibility { get; set; }
    }
}
