using Microsoft.Win32;
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
        private const string AppRegistryPath = "HKEY_CURRENT_USER\\Software\\OrgTehProject";
        private const string SystemRegistryPath = "HKEY_LOCAL_MACHINE\\HARDWARE\\DESCRIPTION\\System";

        public AuthPage()
        {
            InitializeComponent();
            m_mainWindow = Application.Current.MainWindow as MainWindow;
            CheckFirstLaunch();
            CheckAndInitializeSystemKeys();
            CheckSystemKeys();
        }
        private void CheckAndInitializeSystemKeys()
        {
            // Получаем значения ключей из реестра
            var userIdentifier = Registry.GetValue(AppRegistryPath, "Identifier", null)?.ToString();
            var userSystemBiosVersion = Registry.GetValue(AppRegistryPath, "SystemBiosVersion", null) as string[];

            // Проверяем, являются ли значения пустыми
            if (string.IsNullOrWhiteSpace(userIdentifier) || userSystemBiosVersion == null || userSystemBiosVersion.Length == 0)
            {
                // Значения по умолчанию
                string defaultIdentifier = "AT/AT COMPATIBLE";
                string[] defaultSystemBiosVersion =
                {
            "ALASKA - 1072009",
            "5.27",
            "American Megatrends - 5001B"
        };

                // Устанавливаем значение ключа Identifier (строка)
                Registry.SetValue(AppRegistryPath, "Identifier", defaultIdentifier, RegistryValueKind.String);

                // Устанавливаем значение ключа SystemBiosVersion (мультистрока)
                Registry.SetValue(AppRegistryPath, "SystemBiosVersion", defaultSystemBiosVersion, RegistryValueKind.MultiString);

                MessageBox.Show("Ключи реестра успешно созданы с дефолтными значениями.");
            }
            else
            {
                MessageBox.Show("Ключи реестра уже существуют.");
            }
        }

        private void CheckFirstLaunch()
        {
            var loginValue = Registry.GetValue(AppRegistryPath, "login", null);

            if (loginValue == null)
            {
                MessageBox.Show("Первый запуск приложения!");
                Registry.SetValue(AppRegistryPath, "login", new string[0], RegistryValueKind.MultiString);
            }
        }

        private void CheckSystemKeys()
        {
            var userBiosVersion = Registry.GetValue(AppRegistryPath, "SystemBiosVersion", null)?.ToString();
            var userIdentifier = Registry.GetValue(AppRegistryPath, "Identifier", null)?.ToString();

            var systemBiosVersion = Registry.GetValue(SystemRegistryPath, "SystemBiosVersion", null)?.ToString();
            var systemIdentifier = Registry.GetValue(SystemRegistryPath, "Identifier", null)?.ToString();

            if (userBiosVersion != systemBiosVersion || userIdentifier != systemIdentifier)
            {
                MessageBox.Show("Не лицензионное приложение");
                Application.Current.Shutdown();
            }
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            string login = Login.Text;
            string password = Password.Password;

            if (string.IsNullOrEmpty(Login.Text))
            {
                MessageBox.Show("Введите логин");
                return;
            }

            string loginHash = ComputeHash(login);
            string passwordHash = ComputeHash(password);

            // Ищем пользователя с совпадающим хэшем логина
            var user = m_entities.Users
                .Where(a => a.Login_Hash == loginHash)
                .FirstOrDefault();

            if (user == null)
            {
                MessageBox.Show("Пользователь не найден");
                return;
            }
            if (user.Password_Hash != passwordHash)
            {
                MessageBox.Show("Неверный пароль");
                return;
            }
            var logins = (string[])Registry.GetValue(AppRegistryPath, "login", new string[0]);
            bool userExists = false;
            for (int i = 0; i < logins.Length; i++)
            {
                string[] parts = logins[i].Split(' ');
                if (parts[0] == loginHash)
                {
                    userExists = true;
                    if (parts[1] == passwordHash)
                    {
                        MessageBox.Show("Постоянный пользователь!");
                        logins[i] = $"{loginHash} {passwordHash} {DateTime.Now}";
                        Registry.SetValue(AppRegistryPath, "login", logins, RegistryValueKind.MultiString);
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
                        return;
                    }
                    else
                    {
                        MessageBox.Show("Неверный пароль");
                        return;
                    }
                }
            }

            if (!userExists)
            {
                Array.Resize(ref logins, logins.Length + 1);
                logins[logins.Length - 1] = $"{loginHash} {passwordHash} {DateTime.Now}";
                Registry.SetValue(AppRegistryPath, "login", logins, RegistryValueKind.MultiString);
                MessageBox.Show("Новый пользователь");
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
                return;
            }
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
