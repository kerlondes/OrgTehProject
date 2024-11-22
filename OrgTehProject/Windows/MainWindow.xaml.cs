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
using OrgTehProject.Pages;

namespace OrgTehProject.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MyFrame.Navigate(new AuthPage());
        }
        public void NavigateToBuyerPage()
        {
            MyFrame.Navigate(new BuyerPage());
        }

        public void NavigateToAdminPage()
        {
            MyFrame.Navigate(new AdminPage());
        }
    }
}
