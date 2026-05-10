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
    /// Логика взаимодействия для ComplaintDialog.xaml
    /// </summary>
    // ComplaintDialog.xaml.cs
    public partial class ComplaintDialog : Window
    {
        private readonly int? _targetBookId;
        private readonly int? _targetReviewId;

        public ComplaintDialog(int? targetBookId, int? targetReviewId)
        {
            InitializeComponent();
            _targetBookId = targetBookId;
            _targetReviewId = targetReviewId;

            TxtTarget.Text = targetBookId.HasValue
                ? "Жалоба на книгу"
                : "Жалоба на отзыв";
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtReason.Text))
            {
                MessageBox.Show("Укажите причину жалобы.");
                return;
            }

            using var db = new AppDbContext();

            db.Complaints.Add(new Complaint
            {
                UserId = Session.CurrentUser!.UserId,
                TargetBookId = _targetBookId,
                TargetReviewId = _targetReviewId,
                Reason = TxtReason.Text.Trim()
            });

            db.SaveChanges();

            MessageBox.Show("Жалоба отправлена.", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);

            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
            => this.Close();
    }
}
