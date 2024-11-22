using OrgTehProject.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

namespace OrgTehProject.Pages
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        private OrgTehEntities m_entities = OrgTehEntities.GetInstance();
        private readonly MainWindow m_mainWindow;
        public AuthPage()
        {
            InitializeComponent();
            m_mainWindow = Application.Current.MainWindow as MainWindow;
        }

        private void TogglePasswordVisibility(object sender, RoutedEventArgs e)
        {
            if (Password.Visibility == Visibility.Visible)
            {
                // Если видим PasswordBox, показываем TextBox с текстом
                PasswordTextBox.Text = Password.Password;
                Password.Visibility = Visibility.Collapsed;
                PasswordTextBox.Visibility = Visibility.Visible;
                EyeIcon.Source = new System.Windows.Media.Imaging.BitmapImage(new System.Uri("/Images/glas.jpg", System.UriKind.Relative)); // Открытый глаз
            }
            else
            {
                // Если видим TextBox, переключаем обратно на PasswordBox с символами
                Password.Password = PasswordTextBox.Text;
                PasswordTextBox.Visibility = Visibility.Collapsed;
                Password.Visibility = Visibility.Visible;
                EyeIcon.Source = new System.Windows.Media.Imaging.BitmapImage(new System.Uri("/Images/neglas.jpg", System.UriKind.Relative)); // Закрытый глаз
            }
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Login.Text))
            {
                MessageBox.Show("Введите логин");
                return;
            }

            // Хэшируем введённые логин и пароль
            string loginHash = ComputeHash(Login.Text);
            string passwordHash = ComputeHash(Password.Password);

            // Ищем пользователя с совпадающим хэшем логина
            var user = m_entities.Users
                .Where(a => a.Login_Hash == loginHash)
                .FirstOrDefault();

            if (user == null)
            {
                MessageBox.Show("Пользователь не найден");
                return;
            }

            // Проверяем хэш пароля
            if (user.Password_Hash != passwordHash)
            {
                MessageBox.Show("Неверный пароль");
                return;
            }

            Session.currentUser = user;

            switch (user.Role.Name)
            {
                case "Покупатель":
                    m_mainWindow.NavigateToBuyerPage();
                    break;
                case "Администратор":
                    m_mainWindow.NavigateToAdminPage();
                    break;
            }
        }

        // Хэширование строки с использованием SHA-256
        private string ComputeHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
