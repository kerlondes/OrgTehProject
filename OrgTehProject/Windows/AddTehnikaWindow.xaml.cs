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

namespace OrgTehProject.Windows
{
    /// <summary>
    /// Логика взаимодействия для AddTehnikaWindow.xaml
    /// </summary>
    public partial class AddTehnikaWindow : Window
    {
        private OrgTehEntities m_entities;

        public AddTehnikaWindow()
        {
            InitializeComponent();
            m_entities = OrgTehEntities.GetInstance();
            LoadCombo();
        }
        private void LoadCombo()
        {
            // Загружаем типы, категории и страны с их идентификаторами
            var productTypes = m_entities.TypeTehnikas.ToList();
            var productCategory = m_entities.CategoryOfTehnikas.ToList();
            var productCountry = m_entities.CountryForMades.ToList();

            // Заполнение комбобоксов для типа, категории и страны
            TypeComboBox.ItemsSource = productTypes;
            TypeComboBox.DisplayMemberPath = "Name"; // Отображаем название типа
            TypeComboBox.SelectedValuePath = "Id_TypeTehnika"; // Связываем с id

            CategoryComboBox.ItemsSource = productCategory;
            CategoryComboBox.DisplayMemberPath = "Name"; // Отображаем название категории
            CategoryComboBox.SelectedValuePath = "Id_CategoryOfTehnika"; // Связываем с id

            CountryComboBox.ItemsSource = productCountry;
            CountryComboBox.DisplayMemberPath = "Name"; // Отображаем название страны
            CountryComboBox.SelectedValuePath = "Id_CountryForMade"; // Связываем с id
        }


        // Сохранение нового товара
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newTehnika = new Tehnika
                {
                    Name = NameTextBox.Text,
                    Description = DescriptionTextBox.Text,
                    Price = decimal.Parse(PriceTextBox.Text),
                    Id_TypeOfTehnika = (int)TypeComboBox.SelectedValue,
                    Id_CategoryOfTehnika = (int)CategoryComboBox.SelectedValue,
                    Id_CountryForMade = (int)CountryComboBox.SelectedValue,
                    Image = ImageTextBox.Text,
                    IsEnabel = true // можно сделать поле для включения/выключения товара
                };

                m_entities.Tehnikas.Add(newTehnika);
                m_entities.SaveChanges();

                MessageBox.Show("Товар успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении товара: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Отмена
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
