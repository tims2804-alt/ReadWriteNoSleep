using Microsoft.EntityFrameworkCore;
using ReadWriteNoSleep.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReadWriteNoSleep.Pages
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            LoadProfile();
        }

        private void LoadProfile()
        {
            using var db = new AppDbContext();

            var user = db.AppUsers
                .Include(u => u.Role)
                .FirstOrDefault(u => u.UserId == Session.CurrentUser!.UserId);

            if (user == null) return;

            Session.CurrentUser = user;

            TxtDisplayName.Text = user.DisplayName;
            TxtLogin.Text = "Логин: " + user.Login;
            TxtEmail.Text = "Email: " + user.Email;
            TxtRole.Text = user.Role.RoleName;

            if (user.IsFrozen)
            {
                PanelFrozen.Visibility = Visibility.Visible;
                TxtFrozenReason.Text = "Ваш аккаунт был заморожен администратором. " +
                                       "Вы можете оспорить это решение.";
            }

            if (user.Role.RoleName == "Читатель")
            {
                var hasRequest = db.RoleRequests.Any(r => r.UserId == user.UserId);
                PanelAuthorRequest.Visibility = Visibility.Visible;

                if (hasRequest)
                {
                    BtnRequestAuthor.Content = "Заявка уже подана";
                    BtnRequestAuthor.IsEnabled = false;
                    BtnRequestAuthor.Background = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#45475a"));
                }
            }

            var reviews = db.Reviews
                .Include(r => r.Book)
                .Where(r => r.UserId == user.UserId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            if (reviews.Count == 0)
                TxtNoReviews.Visibility = Visibility.Visible;
            else
                ReviewsList.ItemsSource = reviews;
        }

        private void BtnRequestAuthor_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Подать заявку на роль Автора?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            using var db = new AppDbContext();

            var hasRequest = db.RoleRequests
                .Any(r => r.UserId == Session.CurrentUser!.UserId);

            if (hasRequest)
            {
                MessageBox.Show("Вы уже подавали заявку.");
                return;
            }

            db.RoleRequests.Add(new RoleRequest
            {
                UserId = Session.CurrentUser!.UserId,
                RequestDate = DateTime.Now
            });

            db.SaveChanges();

            MessageBox.Show("Заявка подана! Ожидайте решения администратора.",
                "Готово", MessageBoxButton.OK, MessageBoxImage.Information);

            LoadProfile();
        }

        private void BtnAppealFreeze_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AppealFreezeDialog();
            dialog.ShowDialog();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Выйти из аккаунта?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            Properties.Settings.Default.SavedLogin = "";
            Properties.Settings.Default.SavedPassword = "";
            Properties.Settings.Default.Save();

            Session.CurrentUser = null;

            var login = new LoginWindow();
            login.Show();

            Window.GetWindow(this).Close();
        }
    }
}