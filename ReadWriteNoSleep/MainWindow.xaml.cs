using ReadWriteNoSleep.Pages;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ReadWriteNoSleep.Services;

namespace ReadWriteNoSleep
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetupSidebar();
            // Открываем каталог по умолчанию
            MainFrame.Navigate(new CatalogPage());
            SetActiveButton(BtnCatalog);
        }

        // --- Настройка видимости кнопок по роли ---

        private void SetupSidebar()
        {
            BtnAdmin.Visibility = Session.IsAdmin
                ? Visibility.Visible : Visibility.Collapsed;

            BtnAuthor.Visibility = Session.IsAuthor
                ? Visibility.Visible : Visibility.Collapsed;

            BtnFrozen.Visibility = Session.IsFrozen
                ? Visibility.Visible : Visibility.Collapsed;
        }

        // --- Навигация ---

        private void BtnCatalog_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CatalogPage());
            SetActiveButton(BtnCatalog);
        }

        private void BtnLists_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new BookListsPage());
            SetActiveButton(BtnLists);
        }

        private void BtnAuthor_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AuthorPage());
            SetActiveButton(BtnAuthor);
        }

        private void BtnAdmin_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AdminPage());
            SetActiveButton(BtnAdmin);
        }

        private void BtnFrozen_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProfilePage());
            SetActiveButton(BtnFrozen);
        }

        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProfilePage());
            SetActiveButton(BtnProfile);
        }

        // --- Подсветка активной кнопки ---

        private void SetActiveButton(Button active)
        {
            var buttons = new[] { BtnCatalog, BtnLists, BtnAuthor, BtnAdmin, BtnProfile, BtnFrozen };

            foreach (var btn in buttons)
            {
                btn.Background = new SolidColorBrush(Colors.Transparent);
            }

            active.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#313244"));
        }
    }
}