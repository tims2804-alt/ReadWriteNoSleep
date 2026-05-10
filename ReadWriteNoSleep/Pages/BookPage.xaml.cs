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
    /// Логика взаимодействия для BookPage.xaml
    /// </summary>
    // BookPage.xaml.cs
   

    public partial class BookPage : Page
    {
        private readonly int _bookId;
        private Book? _book;

        public BookPage(int bookId)
        {
            InitializeComponent();
            _bookId = bookId;
            LoadBook();
        }

        private void LoadBook()
        {
            using var db = new AppDbContext();

            _book = db.Books
                .Include(b => b.Author)
                .Include(b => b.BookGenres).ThenInclude(bg => bg.Genre)
                .Include(b => b.Reviews).ThenInclude(r => r.User)
                .FirstOrDefault(b => b.BookId == _bookId);

            if (_book == null) return;

            TxtTitle.Text = _book.Title;
            TxtAuthor.Text = "Автор: " + _book.Author.DisplayName;
            TxtContent.Text = _book.TextContent;
            TxtDescription.Text = _book.Description ?? "Описание отсутствует";

            var avgRating = _book.Reviews.Any()
                ? _book.Reviews.Average(r => r.Rating).ToString("F1")
                : "Нет оценок";
            TxtRating.Text = $"⭐ {avgRating}";

            var genres = _book.BookGenres.Select(bg => bg.Genre.GenreName);
            TxtGenres.Text = "Жанры: " + string.Join(", ", genres);

            // Обложка
            if (!string.IsNullOrEmpty(_book.CoverPath))
            {
                ImgCover.Source = new BitmapImage(new Uri(_book.CoverPath));
                TxtCoverPlaceholder.Visibility = Visibility.Collapsed;
            }
            else
            {
                ImgCover.Visibility = Visibility.Collapsed;
            }

            // Кнопка заморозки — только для админа
            BtnFreezeBook.Visibility = Session.IsAdmin
                ? Visibility.Visible : Visibility.Collapsed;

            // Загружаем отзывы
            var reviews = _book.Reviews
                .Select(r => new ReviewViewModel
                {
                    ReviewId = r.ReviewId,
                    User = r.User,
                    Rating = r.Rating,
                    ReviewText = r.ReviewText,
                    CreatedAt = r.CreatedAt,
                    IsAdminVisibility = Session.IsAdmin
                        ? Visibility.Visible : Visibility.Collapsed
                }).ToList();

            ReviewsList.ItemsSource = reviews;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void BtnAddToList_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddToListDialog(_bookId);
            dialog.ShowDialog();
        }

        private void BtnComplainBook_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ComplaintDialog(_bookId, null);
            dialog.ShowDialog();
        }

        private void BtnComplainAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (_book == null) return;
            // Жалоба на автора — жалоба на пользователя, пока показываем сообщение
            MessageBox.Show("Жалоба на автора отправлена.", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnFreezeBook_Click(object sender, RoutedEventArgs e)
        {
            if (_book == null) return;

            var result = MessageBox.Show(
                "Заморозить книгу?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            using var db = new AppDbContext();
            var book = db.Books.Find(_bookId);
            if (book == null) return;

            book.IsFrozen = true;
            db.SaveChanges();

            MessageBox.Show("Книга заморожена.", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);

            NavigationService.GoBack();
        }

        private void BtnComplainReview_Click(object sender, RoutedEventArgs e)
        {
            var reviewId = (int)((Button)sender).Tag;
            var dialog = new ComplaintDialog(null, reviewId);
            dialog.ShowDialog();
        }

        private void BtnFreezeReview_Click(object sender, RoutedEventArgs e)
        {
            var reviewId = (int)((Button)sender).Tag;

            var result = MessageBox.Show(
                "Заморозить отзыв?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            using var db = new AppDbContext();
            var review = db.Reviews.Find(reviewId);
            if (review == null) return;

            // В БД нет IsFrozen у Review — удаляем отзыв или помечаем через жалобу
            // Пока просто удаляем
            db.Reviews.Remove(review);
            db.SaveChanges();

            MessageBox.Show("Отзыв удалён.", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);

            LoadBook();
        }

        private void BtnSubmitReview_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtReviewText.Text))
            {
                MessageBox.Show("Напишите текст отзыва.");
                return;
            }

            if (CmbRating.SelectedItem == null)
            {
                MessageBox.Show("Выберите оценку.");
                return;
            }

            var rating = int.Parse(((ComboBoxItem)CmbRating.SelectedItem).Content.ToString()!);

            using var db = new AppDbContext();

            // Проверяем — уже оставлял отзыв?
            var existing = db.Reviews.FirstOrDefault(r =>
                r.BookId == _bookId && r.UserId == Session.CurrentUser!.UserId);

            if (existing != null)
            {
                MessageBox.Show("Вы уже оставляли отзыв на эту книгу.");
                return;
            }

            var review = new Review
            {
                BookId = _bookId,
                UserId = Session.CurrentUser!.UserId,
                ReviewText = TxtReviewText.Text.Trim(),
                Rating = rating,
                CreatedAt = DateTime.Now
            };

            db.Reviews.Add(review);
            db.SaveChanges();

            TxtReviewText.Text = "";
            CmbRating.SelectedIndex = -1;
            LoadBook();
        }
    }

    // Вспомогательная модель для отзывов (чтобы передать Visibility)
    public class ReviewViewModel
    {
        public int ReviewId { get; set; }
        public AppUser User { get; set; } = null!;
        public int Rating { get; set; }
        public string ReviewText { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public Visibility IsAdminVisibility { get; set; }
    }
}
