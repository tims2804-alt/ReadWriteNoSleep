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
    /// Логика взаимодействия для AddToListDialog.xaml
    /// </summary>
    // AddToListDialog.xaml.cs
    public partial class AddToListDialog : Window
    {
        private readonly int _bookId;

        public AddToListDialog(int bookId)
        {
            InitializeComponent();
            _bookId = bookId;
        }

        private void AddToList(string section)
        {
            using var db = new AppDbContext();

            var existing = db.ReadingLists.FirstOrDefault(rl =>
                rl.UserId == Session.CurrentUser!.UserId && rl.BookId == _bookId);

            if (existing != null)
            {
                existing.Section = section;
            }
            else
            {
                db.ReadingLists.Add(new ReadingList
                {
                    UserId = Session.CurrentUser!.UserId,
                    BookId = _bookId,
                    Section = section
                });
            }

            db.SaveChanges();
            MessageBox.Show($"Книга добавлена в список «{section}»", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void BtnReading_Click(object sender, RoutedEventArgs e)
            => AddToList("Читаю");

        private void BtnPlanned_Click(object sender, RoutedEventArgs e)
            => AddToList("В планах");

        private void BtnCompleted_Click(object sender, RoutedEventArgs e)
            => AddToList("Прочитано");

        private void BtnAbandoned_Click(object sender, RoutedEventArgs e)
            => AddToList("Заброшено");
    }
}
