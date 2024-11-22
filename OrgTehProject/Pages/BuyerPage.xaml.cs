using OrgTehProject.Windows;
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
        private List<Tehnika> allProducts; // Список всех товаров для фильтрации
        public BuyerPage()
        {
            InitializeComponent();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProducts();
            LoadProductTypes();
        }
        private void UpdateTotalPrice()
        {
            decimal totalPrice = 0m;  // Общая стоимость

            // Проходим по всем товарам на панели
            foreach (var child in ProductPanel.Children)
            {
                if (child is StackPanel stackPanel)
                {
                    var quantityBox = stackPanel.Children.OfType<StackPanel>()
                        .FirstOrDefault(p => p.Children.OfType<TextBox>().Any())?
                        .Children.OfType<TextBox>().FirstOrDefault();

                    var priceLabel = stackPanel.Children.OfType<Label>()
                        .FirstOrDefault(price => price.Content is decimal) as Label;

                    if (quantityBox != null && priceLabel != null)
                    {
                        int quantity = 0;
                        decimal price = (decimal)priceLabel.Content;

                        if (int.TryParse(quantityBox.Text, out quantity) && quantity > 0)
                        {
                            // Подсчитываем стоимость для этого товара
                            decimal productTotalPrice = price * quantity;
                            totalPrice += productTotalPrice;
                        }
                    }
                }
            }

            // Обновляем метку с итоговой стоимостью
            TotalPriceLabel.Content = $"Итого: {totalPrice:C}";
        }
        private void LoadProductTypes()
        {
            // Получаем все уникальные типы техники из базы данных
            var productTypes = m_entities.Tehnikas
                .Select(t => t.TypeTehnika.Name)
                .Distinct() // Получаем уникальные значения типов
                .ToList();

            // Добавляем их в ComboBox
            TypeProduct.Items.Clear();
            TypeProduct.Items.Add(new ComboBoxItem { Content = "Все типы", IsSelected = true }); // Добавляем элемент "Все типы"

            foreach (var type in productTypes)
            {
                TypeProduct.Items.Add(new ComboBoxItem { Content = type });
            }
        }
        private void LoadProducts()
        {
            var projectPath = @"..\..\..\Images";
            // Очистка панели на случай повторной загрузки
            ProductPanel.Children.Clear();

            // Получаем все данные из базы
            allProducts = m_entities.Tehnikas.ToList();

            // Фильтруем по выбранному типу и введенному имени
            var filteredProducts = FilterProducts(allProducts);

            // Создаём визуальные элементы для каждого отфильтрованного товара
            foreach (var product in filteredProducts)
            {
                var stackPanel = new StackPanel
                {
                    Margin = new Thickness(10),
                    Width = 200
                };

                var image = new Image
                {
                    Source = new BitmapImage(new Uri(System.IO.Path.Combine(projectPath, product.Image), UriKind.Relative)),
                    Height = 150,
                    Stretch = System.Windows.Media.Stretch.Uniform,
                    Tag = product // Связываем объект Tehnika с изображением
                };
                image.MouseLeftButtonDown += Image_MouseLeftButtonDown;
                stackPanel.Children.Add(image);

                var label = new Label
                {
                    Content = product.Name,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                stackPanel.Children.Add(label);

                var price = new Label
                {
                    Content = product.Price,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                stackPanel.Children.Add(price);

                var controlPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 0)
                };

                var minusButton = new Button
                {
                    Content = "-",
                    Width = 30,
                    Margin = new Thickness(5, 0, 5, 0)
                };

                var textBox = new TextBox
                {
                    Text = "0",
                    Width = 50,
                    HorizontalContentAlignment = HorizontalAlignment.Center
                };

                // Добавляем обработчики для проверки ввода чисел
                textBox.PreviewTextInput += TextBox_PreviewTextInput;  // Для проверки ввода символов
                textBox.PreviewKeyDown += TextBox_PreviewKeyDown;  // Для обработки клавиш

                var plusButton = new Button
                {
                    Content = "+",
                    Width = 30,
                    Margin = new Thickness(5, 0, 5, 0)
                };

                // Обработчик нажатия на кнопку "-" (уменьшить количество)
                minusButton.Click += (s, e) =>
                {
                    if (int.TryParse(textBox.Text, out int count) && count > 0)
                    {
                        textBox.Text = (count - 1).ToString();
                        UpdateTotalPrice();  // Обновляем общую цену после изменения
                    }
                };

                // Обработчик нажатия на кнопку "+" (увеличить количество)
                plusButton.Click += (s, e) =>
                {
                    if (int.TryParse(textBox.Text, out int count))
                    {
                        textBox.Text = (count + 1).ToString();
                        UpdateTotalPrice();  // Обновляем общую цену после изменения
                    }
                };

                controlPanel.Children.Add(minusButton);
                controlPanel.Children.Add(textBox);
                controlPanel.Children.Add(plusButton);

                stackPanel.Children.Add(controlPanel);
                ProductPanel.Children.Add(stackPanel);
            }
        }

        // Событие для проверки ввода текста в текстбокс
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только числа
            e.Handled = !Char.IsDigit(e.Text, 0);
        }

        // Событие для обработки нажатия клавиш (для предотвращения использования нечисловых клавиш)
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Разрешаем только цифры, Backspace и Delete
            if (!((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key == Key.Back || e.Key == Key.Delete))
            {
                e.Handled = true;
            }
        }


        private List<Tehnika> FilterProducts(List<Tehnika> products)
        {
            // Фильтруем по типу техники, если выбран тип
            var selectedType = TypeProduct.SelectedItem as ComboBoxItem;
            var filterType = selectedType?.Content?.ToString();

            // Фильтруем по наименованию, если оно введено
            var searchText = NameFind.Text.ToLower();

            var filteredProducts = products.Where(p =>
                (string.IsNullOrEmpty(filterType) || filterType == "Все типы" || p.TypeTehnika.Name.Contains(filterType)) &&
                (string.IsNullOrEmpty(searchText) || p.Name.ToLower().Contains(searchText))
            ).ToList();

            return filteredProducts;
        }

        // Обработчик изменения фильтра по типу техники
        private void TypeProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadProducts(); // Перезагружаем с учётом новых фильтров
        }

        // Обработчик изменения фильтра по наименованию техники
        private void NameFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadProducts(); // Перезагружаем с учётом новых фильтров
        }
        private void AddToBasket_Click(object sender, RoutedEventArgs e)
        {
            decimal totalPrice = 0m;  // Общая стоимость
            List<Basket> basketItems = new List<Basket>();  // Список для добавляемых товаров в корзину

            // Проходим по всем товарам на панели
            foreach (var child in ProductPanel.Children)
            {
                if (child is StackPanel stackPanel)
                {
                    // Ищем элементы управления (TextBox для количества и метку с ценой)
                    var quantityBox = stackPanel.Children.OfType<StackPanel>()
                        .FirstOrDefault(p => p.Children.OfType<TextBox>().Any())?
                        .Children.OfType<TextBox>().FirstOrDefault();

                    var priceLabel = stackPanel.Children.OfType<Label>()
                        .FirstOrDefault(label => label.Content is decimal) as Label;

                    if (quantityBox != null && priceLabel != null)
                    {
                        int quantity = 0;
                        decimal price = (decimal)priceLabel.Content;

                        if (int.TryParse(quantityBox.Text, out quantity) && quantity > 0)
                        {
                            // Подсчитываем стоимость для этого товара
                            decimal productTotalPrice = price * quantity;
                            totalPrice += productTotalPrice;

                            // Добавляем товар в корзину
                            var selectedProduct = stackPanel.Children.OfType<Image>().FirstOrDefault()?.Tag as Tehnika;
                            if (selectedProduct != null)
                            {
                                Basket basketItem = new Basket
                                {
                                    Id_User = Session.currentUser.Id_User,  // Используем Id текущего пользователя
                                    Id_Tehnika = selectedProduct.Id_Tehnika,
                                    Quantity = quantity
                                };
                                basketItems.Add(basketItem);
                            }
                        }
                    }
                }
            }

            // Обновляем метку с итоговой стоимостью
            TotalPriceLabel.Content = $"Итого: {totalPrice:C}";

            // Отправляем товары в корзину
            if (basketItems.Count > 0)
            {
                foreach (var item in basketItems)
                {
                    m_entities.Baskets.Add(item);  // Добавляем каждый товар в корзину
                }
                m_entities.SaveChanges();  // Сохраняем изменения в базе данных
                MessageBox.Show("Товары успешно добавлены в корзину!", "Корзина", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите хотя бы один товар.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) // Двойной клик
            {
                var image = sender as Image;
                if (image?.Tag is Tehnika tehnika)
                {
                    // Открываем окно с информацией
                    var detailsWindow = new TehnikaDetailsWindow();
                    detailsWindow.LoadTehnikaDetails(tehnika);
                    detailsWindow.ShowDialog();
                }
            }
        }
    }
}
