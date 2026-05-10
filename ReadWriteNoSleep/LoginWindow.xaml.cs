using Microsoft.EntityFrameworkCore;
using ReadWriteNoSleep.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReadWriteNoSleep
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            var savedLogin = Properties.Settings.Default.SavedLogin;
            var savedPassword = Properties.Settings.Default.SavedPassword;

            if (!string.IsNullOrEmpty(savedLogin) && !string.IsNullOrEmpty(savedPassword))
            {
                try
                {
                    using var db = new AppDbContext();
                    var user = db.AppUsers
                        .Include(u => u.Role)
                        .FirstOrDefault(u => u.Login == savedLogin
                                          && u.Password == savedPassword);
                    if (user != null)
                    {
                        Session.CurrentUser = user;
                        var main = new MainWindow();
                        main.Show();
                        this.Close();
                        return;
                    }
                }
                catch { }
            }
        }

        private void BtnToggleLogin_Click(object sender, RoutedEventArgs e)
        {
            PanelLogin.Visibility = Visibility.Visible;
            PanelRegister.Visibility = Visibility.Collapsed;
            TxtError.Visibility = Visibility.Collapsed;

            BtnToggleLogin.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#cba6f7"));
            BtnToggleLogin.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#1e1e2e"));
            BtnToggleRegister.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#313244"));
            BtnToggleRegister.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#cdd6f4"));
        }

        private void BtnToggleRegister_Click(object sender, RoutedEventArgs e)
        {
            PanelLogin.Visibility = Visibility.Collapsed;
            PanelRegister.Visibility = Visibility.Visible;
            TxtError.Visibility = Visibility.Collapsed;

            BtnToggleRegister.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#cba6f7"));
            BtnToggleRegister.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#1e1e2e"));
            BtnToggleLogin.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#313244"));
            BtnToggleLogin.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#cdd6f4"));
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            TxtError.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(TxtLoginLogin.Text) ||
                string.IsNullOrWhiteSpace(TxtLoginPassword.Password))
            {
                ShowError("Заполните все поля");
                return;
            }

            try
            {
                using var db = new AppDbContext();

                var user = db.AppUsers
                    .Include(u => u.Role)
                    .FirstOrDefault(u => u.Login == TxtLoginLogin.Text
                                      && u.Password == TxtLoginPassword.Password);

                if (user == null)
                {
                    ShowError("Неверный логин или пароль");
                    return;
                }

                Properties.Settings.Default.SavedLogin = user.Login;
                Properties.Settings.Default.SavedPassword = user.Password;
                Properties.Settings.Default.Save();

                Session.CurrentUser = user;

                var main = new MainWindow();
                main.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка подключения к БД: " + ex.Message);
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            TxtError.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(TxtRegDisplayName.Text) ||
                string.IsNullOrWhiteSpace(TxtRegLogin.Text) ||
                string.IsNullOrWhiteSpace(TxtRegEmail.Text) ||
                string.IsNullOrWhiteSpace(TxtRegPassword.Password))
            {
                ShowError("Заполните все поля");
                return;
            }

            try
            {
                using var db = new AppDbContext();

                if (db.AppUsers.Any(u => u.Login == TxtRegLogin.Text.Trim()))
                {
                    ShowError("Пользователь с таким логином уже существует");
                    return;
                }

                if (db.AppUsers.Any(u => u.Email == TxtRegEmail.Text.Trim()))
                {
                    ShowError("Пользователь с таким email уже существует");
                    return;
                }

                // Ищем любую читательскую роль
                var readerRole = db.Roles.FirstOrDefault(r =>
                    r.RoleName == "Читатель" ||
                    r.RoleName == "Пользователь" ||
                    r.RoleName == "User");

                if (readerRole == null)
                {
                    // Берём роль с минимальным RoleId если ничего не нашли
                    readerRole = db.Roles.OrderBy(r => r.RoleId).FirstOrDefault();
                }

                if (readerRole == null)
                {
                    ShowError("Не найдено ни одной роли в БД");
                    return;
                }

                var newUser = new AppUser
                {
                    Login = TxtRegLogin.Text.Trim(),
                    Password = TxtRegPassword.Password,
                    Email = TxtRegEmail.Text.Trim(),
                    DisplayName = TxtRegDisplayName.Text.Trim(),
                    RoleId = readerRole.RoleId,
                    IsFrozen = false
                };

                db.AppUsers.Add(newUser);
                db.SaveChanges();

                MessageBox.Show("Регистрация прошла успешно! Войдите в аккаунт.",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                BtnToggleLogin_Click(null!, null!);
            }
            catch (Exception ex)
            {
                ShowError("Ошибка: " + ex.Message);
            }
        }

        private void ShowError(string message)
        {
            TxtError.Text = message;
            TxtError.Visibility = Visibility.Visible;
        }
    }
}