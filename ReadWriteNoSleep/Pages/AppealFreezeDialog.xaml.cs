using ReadWriteNoSleep.Services;
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
    /// Логика взаимодействия для AppealFreezeDialog.xaml
    /// </summary>
    // AppealFreezeDialog.xaml.cs
    public partial class AppealFreezeDialog : Window
    {
        public AppealFreezeDialog()
        {
            InitializeComponent();
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtReason.Text))
            {
                MessageBox.Show("Опишите причину.");
                return;
            }

            using var db = new AppDbContext();

            db.UnfreezeRequests.Add(new UnfreezeRequest
            {
                UserId = Session.CurrentUser!.UserId,
                TargetBookId = null,
                IsAccountUnfreeze = true,
                Reason = TxtReason.Text.Trim(),
                RequestDate = DateTime.Now
            });

            db.SaveChanges();

            MessageBox.Show("Заявка отправлена. Ожидайте решения администратора.",
                "Готово", MessageBoxButton.OK, MessageBoxImage.Information);

            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
            => this.Close();
    }
}
