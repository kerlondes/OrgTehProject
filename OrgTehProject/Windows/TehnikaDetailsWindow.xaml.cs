using System;
using System.Collections.Generic;
using System.IO;
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

namespace OrgTehProject.Windows
{
    /// <summary>
    /// Логика взаимодействия для TehnikaDetailsWindow.xaml
    /// </summary>
    public partial class TehnikaDetailsWindow : Window
    {
        public TehnikaDetailsWindow()
        {
            InitializeComponent();
        }
        public void LoadTehnikaDetails(Tehnika tehnika)
        {
            // Загружаем данные в элементы управления
            TehnikaName.Text = tehnika.Name;
            TehnikaPrice.Text = $"Цена: {tehnika.Price} руб.";
            TehnikaDescription.Text = tehnika.Description;

            // Загружаем изображение
            string projectPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\Images");
            string imagePath = System.IO.Path.Combine(projectPath, tehnika.Image);

            // Преобразуем путь в абсолютный
            string absoluteImagePath = System.IO.Path.GetFullPath(imagePath);

            TehnikaImage.Source = new BitmapImage(new Uri(absoluteImagePath));
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
