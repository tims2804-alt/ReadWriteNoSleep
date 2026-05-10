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
using System.Windows.Shapes;

namespace ReadWriteNoSleep.Pages
{
    /// <summary>
    /// Логика взаимодействия для ChangePasswordDialog.xaml
    /// </summary>
    // ChangePasswordDialog.xaml.cs
    public partial class ChangePasswordDialog : Window
    {
        private readonly int _userId;

        public ChangePasswordDialog(int userId)
        {
            InitializeComponent();
            _userId = userId;

            using var db = new AppDbContext();
            var user = db.AppUsers.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
                TxtUserName.Text = $"{user.DisplayName} (@{user.Login})";
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNewPassword.Password))
            {
                MessageBox.Show("Введите новый пароль.");
                return;
            }

            if (TxtNewPassword.Password != TxtConfirmPassword.Password)
            {
                MessageBox.Show("Пароли не совпадают.");
                return;
            }

            if (TxtNewPassword.Password.Length < 6)
            {
                MessageBox.Show("Пароль должен быть не менее 6 символов.");
                return;
            }

            using var db = new AppDbContext();
            var user = db.AppUsers.Find(_userId);
            if (user == null) return;

            user.Password = TxtNewPassword.Password;
            db.SaveChanges();

            MessageBox.Show("Пароль успешно изменён.", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);

            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
            => this.Close();
    }
}
