using Microsoft.EntityFrameworkCore;
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
    /// Логика взаимодействия для ChangeRoleDialog.xaml
    /// </summary>
    // ChangeRoleDialog.xaml.cs
    public partial class ChangeRoleDialog : Window
    {
        private readonly int _userId;

        public ChangeRoleDialog(int userId)
        {
            InitializeComponent();
            _userId = userId;

            using var db = new AppDbContext();
            var user = db.AppUsers.Include(u => u.Role).FirstOrDefault(u => u.UserId == userId);
            if (user != null)
                TxtUserName.Text = $"{user.DisplayName} — сейчас: {user.Role.RoleName}";
        }

        private void SetRole(string roleName)
        {
            using var db = new AppDbContext();

            var role = db.Roles.FirstOrDefault(r => r.RoleName == roleName);
            if (role == null)
            {
                MessageBox.Show($"Роль '{roleName}' не найдена в БД.");
                return;
            }

            var user = db.AppUsers.Find(_userId);
            if (user == null) return;

            user.RoleId = role.RoleId;
            db.SaveChanges();

            MessageBox.Show($"Роль изменена на «{roleName}».", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);

            this.DialogResult = true;
            this.Close();
        }

        private void BtnUser_Click(object sender, RoutedEventArgs e)
            => SetRole("Пользователь");

        private void BtnAuthor_Click(object sender, RoutedEventArgs e)
            => SetRole("Автор");

        private void BtnAdmin_Click(object sender, RoutedEventArgs e)
            => SetRole("Администратор");
    }
}
