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
    /// Логика взаимодействия для MoveToListDialog.xaml
    /// </summary>
    // MoveToListDialog.xaml.cs
    public partial class MoveToListDialog : Window
    {
        private readonly int _bookId;
        private readonly string _currentSection;

        public MoveToListDialog(int bookId, string currentSection)
        {
            InitializeComponent();
            _bookId = bookId;
            _currentSection = currentSection;
            TxtCurrent.Text = $"Сейчас в списке: «{currentSection}»";

            // Скрываем кнопку текущего списка
            switch (currentSection)
            {
                case "Читаю": BtnReading.Visibility = Visibility.Collapsed; break;
                case "В планах": BtnPlanned.Visibility = Visibility.Collapsed; break;
                case "Прочитано": BtnCompleted.Visibility = Visibility.Collapsed; break;
                case "Заброшено": BtnAbandoned.Visibility = Visibility.Collapsed; break;
            }
        }

        private void MoveToList(string section)
        {
            using var db = new AppDbContext();

            var item = db.ReadingLists.FirstOrDefault(rl =>
                rl.UserId == Session.CurrentUser!.UserId && rl.BookId == _bookId);

            if (item == null) return;

            item.Section = section;
            db.SaveChanges();

            MessageBox.Show($"Книга перемещена в «{section}»", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);

            this.DialogResult = true;
            this.Close();
        }

        private void BtnReading_Click(object sender, RoutedEventArgs e)
            => MoveToList("Читаю");

        private void BtnPlanned_Click(object sender, RoutedEventArgs e)
            => MoveToList("В планах");

        private void BtnCompleted_Click(object sender, RoutedEventArgs e)
            => MoveToList("Прочитано");

        private void BtnAbandoned_Click(object sender, RoutedEventArgs e)
            => MoveToList("Заброшено");
    }
}
