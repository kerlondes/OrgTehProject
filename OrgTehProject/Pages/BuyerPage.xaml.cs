using System;
using System.Collections.Generic;
using System.Data.Entity;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OrgTehProject.Pages
{
    /// <summary>
    /// Логика взаимодействия для BuyerPage.xaml
    /// </summary>
    public partial class BuyerPage : Page
    {
        private OrgTehEntities m_entities = OrgTehEntities.GetInstance();
        public BuyerPage()
        {
            InitializeComponent();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }
        private void LoadProducts()
        {
            var projectPath = @"..\..\..\Images";
            // Очистка панели на случай повторной загрузки
            ProductPanel.Children.Clear();

            // Получаем данные из базы
            var products = m_entities.Tehnikas.ToList();

            foreach (var product in products)
            {
                // Создаём панель для каждого товара
                var stackPanel = new StackPanel
                {
                    Margin = new Thickness(10),
                    Width = 200
                };

                // Добавляем изображение
                var image = new Image
                {
                    Source = new BitmapImage(new Uri(System.IO.Path.Combine(projectPath, product.Image), UriKind.Relative)),
                    Height = 150,
                    Stretch = System.Windows.Media.Stretch.Uniform
                };
                stackPanel.Children.Add(image);

                // Добавляем название
                var label = new Label
                {
                    Content = product.Name,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                stackPanel.Children.Add(label);

                // Панель управления (кнопки + текстбокс)
                var controlPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 0)
                };

                // Кнопка "-"
                var minusButton = new Button
                {
                    Content = "-",
                    Width = 30,
                    Margin = new Thickness(5, 0, 5, 0)
                };

                // Текстбокс
                var textBox = new TextBox
                {
                    Text = "0",
                    Width = 50,
                    HorizontalContentAlignment = HorizontalAlignment.Center
                };

                // Кнопка "+"
                var plusButton = new Button
                {
                    Content = "+",
                    Width = 30,
                    Margin = new Thickness(5, 0, 5, 0)
                };

                // Логика кнопок
                minusButton.Click += (s, e) =>
                {
                    if (int.TryParse(textBox.Text, out int count) && count > 0)
                        textBox.Text = (count - 1).ToString();
                };

                plusButton.Click += (s, e) =>
                {
                    if (int.TryParse(textBox.Text, out int count))
                        textBox.Text = (count + 1).ToString();
                };

                controlPanel.Children.Add(minusButton);
                controlPanel.Children.Add(textBox);
                controlPanel.Children.Add(plusButton);

                stackPanel.Children.Add(controlPanel);

                // Добавляем готовый элемент в общий WrapPanel
                ProductPanel.Children.Add(stackPanel);
            }
        }
    }
}
