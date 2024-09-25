using Authoservice.Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Authoservice.Pages
{
    public partial class ListKlients : Page
    {
        private int currentPage = 1;
        private int pageSize = 10;
        private int totalRecords = 0;

        public ListKlients(Frame frame)
        {
            InitializeComponent();
            LoadData();
        }

        public void LoadData()
        {
            try
            {
                var query = Number2Entities.GetContext().Client.AsQueryable();

                query = ApplySearchFilter(query);
                query = ApplyGenderFilter(query);
                query = ApplySorting(query);

                totalRecords = query.Count(); // Обновляем общее количество записей

                // Пагинация
                var clients = query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                clientsGrid.ItemsSource = clients;

                // Обновляем текст с количеством записей
                recordCountText.Text = $"{(pageSize == -1 ? totalRecords : clients.Count)} из {totalRecords}";

                // Обновляем доступность навигационных кнопок
                btnBack.IsEnabled = currentPage > 1;
                btnNext.IsEnabled = currentPage < (totalRecords + pageSize - 1) / pageSize;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при загрузке данных: " + ex.Message);
            }
        }

        private IQueryable<Client> ApplySearchFilter(IQueryable<Client> query)
        {
            if (!string.IsNullOrEmpty(tbSearch.Text))
            {
                string searchText = tbSearch.Text.ToLower();
                return query.Where(c =>
                    c.FirstName.ToLower().Contains(searchText) ||
                    c.LastName.ToLower().Contains(searchText) ||
                    c.Patronymic.ToLower().Contains(searchText) ||
                    c.Email.ToLower().Contains(searchText) ||
                    c.Phone.ToLower().Contains(searchText)
                );
            }
            return query;
        }

        private IQueryable<Client> ApplyGenderFilter(IQueryable<Client> query)
        {
            if (cbFilter.SelectedItem is ComboBoxItem selectedFilter)
            {
                switch (selectedFilter.Tag.ToString())
                {
                    case "1":
                        return query.Where(c => c.GenderCode == "2"); // Женский
                    case "2":
                        return query.Where(c => c.GenderCode == "1"); // Мужской
                    case "0":
                    default:
                        return query; // Все
                }
            }
            return query;
        }

        private IQueryable<Client> ApplySorting(IQueryable<Client> query)
        {
            if (cbSort.SelectedItem is ComboBoxItem selectedSort)
            {
                switch (selectedSort.Tag.ToString())
                {
                    case "1":
                        return query.OrderBy(c => c.LastName); // По фамилии
                    case "2":
                        return query.OrderByDescending(c => c.RegistrationDate); // По дате добавления
                    case "3":
                        return query.OrderByDescending(c => c.ClientService.Count()); // Полное количество посещений
                    default:
                        return query.OrderBy(c => c.ID); // Сортировка по умолчанию по Id
                }
            }
            return query.OrderBy(c => c.ID); // Сортировка по умолчанию, если сортировка не выбрана
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadData();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < (totalRecords + pageSize - 1) / pageSize)
            {
                currentPage++;
                LoadData();
            }
        }

        private void cbPageSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbPageSize.SelectedItem is ComboBoxItem selectedItem)
            {
                pageSize = int.Parse(selectedItem.Tag.ToString());
                currentPage = 1;
                LoadData();
            }
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentPage = 1;
            LoadData();
        }

        private void Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadData(); // Обновление данных при изменении фильтра
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadData(); // Обновление данных при изменении сортировки
        }

        private void clientGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
