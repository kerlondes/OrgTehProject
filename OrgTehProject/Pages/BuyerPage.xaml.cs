using OrgTehProject.Windows;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace OrgTehProject.Pages
{
    /// <summary>
    /// Логика взаимодействия для BuyerPage.xaml
    /// </summary>
    public partial class BuyerPage : Page
    {
        private OrgTehEntities m_entities = OrgTehEntities.GetInstance();
        private List<Tehnika> allProducts; // Список всех товаров для фильтрации
        private List<Basket> basketItems;
        public BuyerPage()
        {
            InitializeComponent();
        }
        private void LoadBasket()
        {
            // Загружаем корзину из базы данных, включая связанные сущности
            basketItems = m_entities.Baskets
                .Include(b => b.User) // Загрузка данных пользователя
                .Include(b => b.Tehnika.TypeTehnika).Where(b=> b.IsContinued == false) // Загрузка типа техники
                .ToList();

            // Добавляем вычисляемую стоимость для каждого товара
            foreach (var basket in basketItems)
            {
                // Вычисляем стоимость для каждого элемента корзины
                basket.TotalPrice = basket.Quantity * basket.Tehnika.Price;
            }

            // Обновляем DataGrid
            ProductsInOrder2.ItemsSource = basketItems;

            // Подсчитываем общую стоимость
            UpdateTotalPrice2();
        }
        // Метод для обновления общей стоимости
        private void UpdateTotalPrice2()
        {
            if (basketItems == null || !basketItems.Any())
            {
                TotalPriceLabel2.Content = "Итого: 0";
                return;
            }

            // Суммируем стоимости всех элементов корзины
            decimal totalPrice = Convert.ToDecimal(basketItems.Sum(b => b.TotalPrice));
            TotalPriceLabel2.Content = $"Итого: {totalPrice} руб.";
        }
        private void RefreshBasket_Click(object sender, RoutedEventArgs e)
        {
            LoadBasket();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProducts();
            LoadProductTypes();
            LoadBasket();
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

                var image = new System.Windows.Controls.Image
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
                    Orientation = System.Windows.Controls.Orientation.Horizontal,
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
                            var selectedProduct = stackPanel.Children.OfType<System.Windows.Controls.Image>().FirstOrDefault()?.Tag as Tehnika;
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
                var image = sender as System.Windows.Controls.Image;
                if (image?.Tag is Tehnika tehnika)
                {
                    // Открываем окно с информацией
                    var detailsWindow = new TehnikaDetailsWindow();
                    detailsWindow.LoadTehnikaDetails(tehnika);
                    detailsWindow.ShowDialog();
                }
            }
        }

        private void AddToOrder(object sender, RoutedEventArgs e)
        {
            if (basketItems == null || !basketItems.Any())
            {
                MessageBox.Show("Корзина пуста. Добавьте товары перед оформлением заказа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Создаем новый заказ
            var newZakaz = new Zakaz
            {
                Id_User = Session.currentUser.Id_User,
                Id_Status = 1 // Предполагаем, что "1" — статус нового заказа
            };
            m_entities.Zakazs.Add(newZakaz);
            m_entities.SaveChanges();

            // Добавляем товары в заказ и обновляем их статус в корзине
            foreach (var item in basketItems)
            {
                var itemInZakaz = new ItemInZakaz
                {
                    Id_Zakaz = newZakaz.Id_Zakaz,
                    Id_Basket = item.Id_Basket
                };
                m_entities.ItemInZakazs.Add(itemInZakaz);

                // Обновляем статус элемента корзины
                var basketItem = m_entities.Baskets.FirstOrDefault(b => b.Id_Basket == item.Id_Basket);
                if (basketItem != null)
                {
                    basketItem.IsContinued = true;
                }
            }

            // Сохраняем изменения
            m_entities.SaveChanges();

            // Генерируем чек
            GenerateWordReceiptOpenXml(newZakaz, basketItems);

            MessageBox.Show("Заказ успешно оформлен!", "Оформление заказа", MessageBoxButton.OK, MessageBoxImage.Information);

            // Обновляем данные корзины
            LoadBasket();
        }

        private void GenerateWordReceiptOpenXml(Zakaz zakaz, List<Basket> items)
        {
            // Путь к папке для сохранения чеков
            string receiptDirectory = @"Receipts";
            if (!Directory.Exists(receiptDirectory))
            {
                Directory.CreateDirectory(receiptDirectory);
            }

            // Формируем имя файла чека
            string receiptFileName = System.IO.Path.Combine(receiptDirectory, $"Receipt_{zakaz.Id_Zakaz}.docx");


            // Создаем Word-документ
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(receiptFileName, WordprocessingDocumentType.Document))
            {
                // Добавляем основную часть документа
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());

                // Заголовок
                body.Append(CreateParagraph("ООО \"Бытовая техника\"", 14, true, JustificationValues.Center));
                body.Append(CreateParagraph("Добро пожаловать", 12, false, JustificationValues.Center));
                body.Append(CreateParagraph("ККМ 00075411     #3969", 10, false, JustificationValues.Center));
                body.Append(CreateParagraph("ИНН 1087746942040", 10, false, JustificationValues.Center));
                body.Append(CreateParagraph("ЭКЛЗ 3851495566", 10, false, JustificationValues.Center));
                body.Append(CreateParagraph($"Чек № {zakaz.Id_Zakaz}", 12, false, JustificationValues.Left));
                body.Append(CreateParagraph("СИС.", 10, false, JustificationValues.Left));

                // Таблица товаров
                DocumentFormat.OpenXml.Wordprocessing.Table table = new DocumentFormat.OpenXml.Wordprocessing.Table();

                // Заголовок таблицы
                DocumentFormat.OpenXml.Wordprocessing.TableRow headerRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
                headerRow.Append(CreateCell("Наименование товара", true));
                headerRow.Append(CreateCell("Количество", true));
                headerRow.Append(CreateCell("Стоимость", true));
                table.Append(headerRow);

                // Заполнение таблицы товарами
                decimal total = 0;
                foreach (var item in items)
                {
                    DocumentFormat.OpenXml.Wordprocessing.TableRow row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
                    row.Append(CreateCell(item.Tehnika.Name));
                    row.Append(CreateCell(item.Quantity.ToString()));
                    decimal price = item.Quantity * item.Tehnika.Price;
                    total += price;
                    row.Append(CreateCell(price.ToString("C")));
                    table.Append(row);
                }

                // Добавляем таблицу в документ
                body.Append(table);

                // Итоговая сумма
                body.Append(CreateParagraph($"Итог: {total:C}", 12, true, JustificationValues.Left));
                body.Append(CreateParagraph("************************", 10, false, JustificationValues.Center));
                body.Append(CreateParagraph("      00003751# 059705", 10, false, JustificationValues.Center));

                // Сохраняем документ
                mainPart.Document.Save();
            }

            // Уведомляем пользователя
            MessageBox.Show($"Чек сохранен по пути: {receiptFileName}", "Чек", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Создание параграфа
        private DocumentFormat.OpenXml.Wordprocessing.Paragraph CreateParagraph(string text, int fontSize, bool isBold, JustificationValues alignment)
        {
            DocumentFormat.OpenXml.Wordprocessing.Paragraph paragraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
            DocumentFormat.OpenXml.Wordprocessing.Run run = new DocumentFormat.OpenXml.Wordprocessing.Run();
            DocumentFormat.OpenXml.Wordprocessing.RunProperties runProperties = new DocumentFormat.OpenXml.Wordprocessing.RunProperties();

            // Устанавливаем размер шрифта и жирность
            runProperties.Append(new DocumentFormat.OpenXml.Wordprocessing.FontSize { Val = (fontSize * 2).ToString() });
            if (isBold) runProperties.Append(new DocumentFormat.OpenXml.Wordprocessing.Bold());

            run.Append(runProperties);
            run.Append(new DocumentFormat.OpenXml.Wordprocessing.Text(text));

            // Добавляем текст в параграф
            paragraph.Append(run);
            paragraph.ParagraphProperties = new DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties(new DocumentFormat.OpenXml.Wordprocessing.Justification { Val = alignment });

            return paragraph;
        }

        // Создание ячейки таблицы
        private DocumentFormat.OpenXml.Wordprocessing.TableCell CreateCell(string text, bool isBold = false)
        {
            DocumentFormat.OpenXml.Wordprocessing.TableCell cell = new DocumentFormat.OpenXml.Wordprocessing.TableCell();
            DocumentFormat.OpenXml.Wordprocessing.Paragraph paragraph = CreateParagraph(text, 12, isBold, JustificationValues.Left);
            cell.Append(paragraph);
            return cell;
        }
    }
}
