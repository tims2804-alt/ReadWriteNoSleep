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
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    // AdminPage.xaml.cs
    

    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            LoadComplaints();
        }

        // --- Переключение вкладок ---

        private void SetActiveTab(Button active)
        {
            var tabs = new[] { BtnTabComplaints, BtnTabUnfreeze, BtnTabRoleRequests,
                           BtnTabFrozen, BtnTabUsers };
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

        private void HideAllPanels()
        {
            PanelComplaints.Visibility = Visibility.Collapsed;
            PanelUnfreeze.Visibility = Visibility.Collapsed;
            PanelRoleRequests.Visibility = Visibility.Collapsed;
            PanelFrozen.Visibility = Visibility.Collapsed;
            PanelUsers.Visibility = Visibility.Collapsed;
        }

        private void BtnTabComplaints_Click(object sender, RoutedEventArgs e)
        {
            HideAllPanels();
            PanelComplaints.Visibility = Visibility.Visible;
            SetActiveTab(BtnTabComplaints);
            LoadComplaints();
        }

        private void BtnTabUnfreeze_Click(object sender, RoutedEventArgs e)
        {
            HideAllPanels();
            PanelUnfreeze.Visibility = Visibility.Visible;
            SetActiveTab(BtnTabUnfreeze);
            LoadUnfreezeRequests();
        }

        private void BtnTabRoleRequests_Click(object sender, RoutedEventArgs e)
        {
            HideAllPanels();
            PanelRoleRequests.Visibility = Visibility.Visible;
            SetActiveTab(BtnTabRoleRequests);
            LoadRoleRequests();
        }

        private void BtnTabFrozen_Click(object sender, RoutedEventArgs e)
        {
            HideAllPanels();
            PanelFrozen.Visibility = Visibility.Visible;
            SetActiveTab(BtnTabFrozen);
            LoadFrozen();
        }

        private void BtnTabUsers_Click(object sender, RoutedEventArgs e)
        {
            HideAllPanels();
            PanelUsers.Visibility = Visibility.Visible;
            SetActiveTab(BtnTabUsers);
            LoadUsers();
        }

        // --- Загрузка данных ---

        private void LoadComplaints()
        {
            using var db = new AppDbContext();

            var complaints = db.Complaints
                .Include(c => c.User)
                .Include(c => c.TargetBook)
                .Include(c => c.TargetReview)
                .ToList();

            var items = complaints.Select(c => new ComplaintViewModel
            {
                ComplaintId = c.ComplaintId,
                User = c.User,
                Reason = c.Reason,
                TypeLabel = c.TargetBookId.HasValue ? "Жалоба на книгу" : "Жалоба на отзыв"
            }).ToList();

            ComplaintsList.ItemsSource = items;
            TxtEmptyComplaints.Visibility = items.Count == 0
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadUnfreezeRequests()
        {
            using var db = new AppDbContext();

            var requests = db.UnfreezeRequests
                .Include(r => r.User)
                .Include(r => r.TargetBook)
                .ToList();

            var items = requests.Select(r => new UnfreezeViewModel
            {
                RequestId = r.RequestId,
                User = r.User,
                Reason = r.Reason,
                IsAccountUnfreeze = r.IsAccountUnfreeze,
                TargetBookId = r.TargetBookId,
                UserId = r.UserId,
                TypeLabel = r.IsAccountUnfreeze
                    ? "Разморозка аккаунта"
                    : $"Разморозка книги: {r.TargetBook?.Title}"
            }).ToList();

            UnfreezeList.ItemsSource = items;
            TxtEmptyUnfreeze.Visibility = items.Count == 0
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadRoleRequests()
        {
            using var db = new AppDbContext();

            var requests = db.RoleRequests
                .Include(r => r.User)
                .ToList();

            RoleRequestsList.ItemsSource = requests;
            TxtEmptyRoleRequests.Visibility = requests.Count == 0
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadFrozen()
        {
            using var db = new AppDbContext();

            FrozenBooksList.ItemsSource = db.Books
                .Include(b => b.Author)
                .Where(b => b.IsFrozen)
                .ToList();

            FrozenUsersList.ItemsSource = db.AppUsers
                .Where(u => u.IsFrozen)
                .ToList();
        }

        private void LoadUsers()
        {
            using var db = new AppDbContext();

            var users = db.AppUsers
                .Include(u => u.Role)
                .Where(u => u.UserId != Session.CurrentUser!.UserId)
                .ToList();

            UsersList.ItemsSource = users;
            TxtEmptyUsers.Visibility = users.Count == 0
                ? Visibility.Visible : Visibility.Collapsed;
        }

        // --- Жалобы ---

        private void BtnAcceptComplaint_Click(object sender, RoutedEventArgs e)
        {
            var complaintId = (int)((Button)sender).Tag;

            using var db = new AppDbContext();
            var complaint = db.Complaints.Find(complaintId);
            if (complaint == null) return;

            // Замораживаем цель
            if (complaint.TargetBookId.HasValue)
            {
                var book = db.Books.Find(complaint.TargetBookId.Value);
                if (book != null) book.IsFrozen = true;
            }
            else if (complaint.TargetReviewId.HasValue)
            {
                var review = db.Reviews.Find(complaint.TargetReviewId.Value);
                if (review != null) db.Reviews.Remove(review);
            }

            db.Complaints.Remove(complaint);
            db.SaveChanges();

            MessageBox.Show("Жалоба принята.", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);
            LoadComplaints();
        }

        private void BtnRejectComplaint_Click(object sender, RoutedEventArgs e)
        {
            var complaintId = (int)((Button)sender).Tag;

            using var db = new AppDbContext();
            var complaint = db.Complaints.Find(complaintId);
            if (complaint == null) return;

            db.Complaints.Remove(complaint);
            db.SaveChanges();

            MessageBox.Show("Жалоба отклонена.", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);
            LoadComplaints();
        }

        // --- Заявки на разморозку ---

        private void BtnAcceptUnfreeze_Click(object sender, RoutedEventArgs e)
        {
            var requestId = (int)((Button)sender).Tag;

            using var db = new AppDbContext();
            var request = db.UnfreezeRequests.Find(requestId);
            if (request == null) return;

            if (request.IsAccountUnfreeze)
            {
                var user = db.AppUsers.Find(request.UserId);
                if (user != null) user.IsFrozen = false;
            }
            else if (request.TargetBookId.HasValue)
            {
                var book = db.Books.Find(request.TargetBookId.Value);
                if (book != null) book.IsFrozen = false;
            }

            db.UnfreezeRequests.Remove(request);
            db.SaveChanges();

            MessageBox.Show("Заморозка снята.", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);
            LoadUnfreezeRequests();
        }

        private void BtnRejectUnfreeze_Click(object sender, RoutedEventArgs e)
        {
            var requestId = (int)((Button)sender).Tag;

            using var db = new AppDbContext();
            var request = db.UnfreezeRequests.Find(requestId);
            if (request == null) return;

            db.UnfreezeRequests.Remove(request);
            db.SaveChanges();

            MessageBox.Show("Заявка отклонена.", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);
            LoadUnfreezeRequests();
        }

        // --- Заявки на роль автора ---

        private void BtnAcceptRole_Click(object sender, RoutedEventArgs e)
        {
            var requestId = (int)((Button)sender).Tag;

            using var db = new AppDbContext();
            var request = db.RoleRequests.Find(requestId);
            if (request == null) return;

            var authorRole = db.Roles.FirstOrDefault(r => r.RoleName == "Автор");
            if (authorRole == null) return;

            var user = db.AppUsers.Find(request.UserId);
            if (user != null) user.RoleId = authorRole.RoleId;

            db.RoleRequests.Remove(request);
            db.SaveChanges();

            MessageBox.Show("Роль автора назначена.", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);
            LoadRoleRequests();
        }

        private void BtnRejectRole_Click(object sender, RoutedEventArgs e)
        {
            var requestId = (int)((Button)sender).Tag;

            using var db = new AppDbContext();
            var request = db.RoleRequests.Find(requestId);
            if (request == null) return;

            db.RoleRequests.Remove(request);
            db.SaveChanges();

            MessageBox.Show("Заявка отклонена.", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);
            LoadRoleRequests();
        }

        // --- Замороженные ---

        private void BtnUnfreezeBook_Click(object sender, RoutedEventArgs e)
        {
            var bookId = (int)((Button)sender).Tag;

            using var db = new AppDbContext();
            var book = db.Books.Find(bookId);
            if (book == null) return;

            book.IsFrozen = false;
            db.SaveChanges();

            MessageBox.Show("Книга разморожена.", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);
            LoadFrozen();
        }

        private void BtnUnfreezeUser_Click(object sender, RoutedEventArgs e)
        {
            var userId = (int)((Button)sender).Tag;

            using var db = new AppDbContext();
            var user = db.AppUsers.Find(userId);
            if (user == null) return;

            user.IsFrozen = false;
            db.SaveChanges();

            MessageBox.Show("Пользователь разморожен.", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);
            LoadFrozen();
        }

        // --- Пользователи ---

        private void BtnChangeRole_Click(object sender, RoutedEventArgs e)
        {
            var userId = (int)((Button)sender).Tag;
            var dialog = new ChangeRoleDialog(userId);
            if (dialog.ShowDialog() == true)
                LoadUsers();
        }

        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var userId = (int)((Button)sender).Tag;
            var dialog = new ChangePasswordDialog(userId);
            dialog.ShowDialog();
        }

        private void BtnFreezeUser_Click(object sender, RoutedEventArgs e)
        {
            var userId = (int)((Button)sender).Tag;

            var result = MessageBox.Show("Заморозить пользователя?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            using var db = new AppDbContext();
            var user = db.AppUsers.Find(userId);
            if (user == null) return;

            user.IsFrozen = true;
            db.SaveChanges();

            MessageBox.Show("Пользователь заморожен.", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);
            LoadUsers();
        }
    }

    // ViewModels
    public class ComplaintViewModel
    {
        public int ComplaintId { get; set; }
        public AppUser User { get; set; } = null!;
        public string Reason { get; set; } = null!;
        public string TypeLabel { get; set; } = null!;
    }

    public class UnfreezeViewModel
    {
        public int RequestId { get; set; }
        public AppUser User { get; set; } = null!;
        public string Reason { get; set; } = null!;
        public bool IsAccountUnfreeze { get; set; }
        public int? TargetBookId { get; set; }
        public int UserId { get; set; }
        public string TypeLabel { get; set; } = null!;
    }
}
