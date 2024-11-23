using OrgTehProject.Windows;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OrgTehProject.Pages
{
    /// <summary>
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {

        private OrgTehEntities m_entities = OrgTehEntities.GetInstance();
        private ObservableCollection<Tehnika> allTehnika; // Храним все товары
        private ObservableCollection<Tehnika> filteredTehnika; // Храним отфильтрованные товары

        public AdminPage()
        {
            InitializeComponent();
            LoadData();
            SetupFilters();
            LoadOrders();
        }
        private void LoadOrders()
        {
            try
            {
                // Загружаем список заказов с пользователями и статусами
                var orders = m_entities.Zakazs
                    .Include(o => o.StatusZakaza)
                    .Include(o => o.User)
                    .ToList();

                // Устанавливаем источник данных для DataGrid
                ProductsInOrder.ItemsSource = orders;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateTable2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Сохраняем изменения в базе данных
                m_entities.SaveChanges();
                MessageBox.Show("Изменения успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Загружаем все товары и отображаем в DataGrid
        private void LoadData()
        {
            try
            {
                var tehnikaList = m_entities.Tehnikas
                                            .Include(t => t.CategoryOfTehnika)
                                            .Include(t => t.CountryForMade)
                                            .Include(t => t.TypeTehnika)
                                            .ToList();

                allTehnika = new ObservableCollection<Tehnika>(tehnikaList); // Сохраняем все товары
                filteredTehnika = new ObservableCollection<Tehnika>(tehnikaList); // Изначально фильтруем все товары
                ProductsInOrder2.ItemsSource = filteredTehnika; // Устанавливаем источник данных
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Настройка фильтров для комбобокса и текстового поля
        private void SetupFilters()
        {
            // Загрузка типов техники в комбобокс
            TypeProduct.ItemsSource = m_entities.TypeTehnikas.ToList();
            TypeProduct.DisplayMemberPath = "Name";
            TypeProduct.SelectedValuePath = "Id_TypeTehnika"; // Путь для выбора значения

            NameFind.Text = string.Empty;
        }

        // Обработчик выбора типа техники
        private void TypeProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterData();
        }

        // Обработчик изменения текста для поиска по названию
        private void NameFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterData();
        }

        // Фильтрация данных
        private void FilterData()
        {
            string searchName = NameFind.Text.ToLower();
            int? selectedType = TypeProduct.SelectedValue as int?;

            var filteredList = allTehnika.Where(t =>
                (string.IsNullOrEmpty(searchName) || t.Name.ToLower().Contains(searchName)) &&
                (!selectedType.HasValue || t.Id_TypeOfTehnika == selectedType.Value)
            ).ToList();

            filteredTehnika.Clear();
            foreach (var item in filteredList)
            {
                filteredTehnika.Add(item);
            }
        }

        // Обработчик кнопки для обновления данных в базе
        private void UpdateTable_Click(object sender, RoutedEventArgs e)
        {
            var context = m_entities;

            try
            {
                // Сохраняем изменения
                context.SaveChanges();
                MessageBox.Show("Данные успешно обновлены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddNewTehnika_Click(object sender, RoutedEventArgs e)
        {
            var addTehnikaWindow = new AddTehnikaWindow();
            addTehnikaWindow.ShowDialog(); // Показываем окно в модальном режиме
            LoadData(); // Перезагружаем данные после добавления нового товара
        }
    }
}
