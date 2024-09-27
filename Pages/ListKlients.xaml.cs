using Authoservice.Model;
using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Инициализирует компонент страницу и загружает данные о клиентах.
        /// </summary>
        /// <param name="frame">Фрейм, содержащий эту страницу.</param>
        public ListKlients(Frame frame)
        {
            InitializeComponent();
            LoadData();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        /// <summary>
        /// Загружает данные о клиентах, применяет фильтры, сортировки и обновляет интерфейс.
        /// </summary>
        public void LoadData()
        {
            try
            {
                // Получаем базовый запрос
                var query = Number2Entities.GetContext().Client
                    .Include("Tag")
                    .AsQueryable();

                // Применяем фильтры и сортировку
                query = ApplySearchFilter(query);
                query = ApplyGenderFilter(query);
                query = ApplySorting(query);

                // Подсчитываем общее количество записей
                totalRecords = query.Count();

                IList<Client> clients;

                // Получаем данные в зависимости от размера страницы
                if (pageSize == -1)
                {
                    clients = query.ToList();
                    currentPage = 1;
                }
                else
                {
                    clients = query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                }

                clientsGrid.ItemsSource = clients;

                    // Обновляем текст для отображения количества записей
                recordCountText.Text = $"{(pageSize == -1 ? totalRecords : clients.Count)} из {totalRecords}";

                    // Обновляем доступность кнопок
                btnBack.IsEnabled = currentPage > 1;
                btnNext.IsEnabled = currentPage < (totalRecords + pageSize - 1) / pageSize && pageSize != -1;
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при загрузке данных: " + ex.Message);
            }
        }


        /// <summary>
        /// Применяет фильтрацию данных по введенному тексту.
        /// </summary>
        /// <param name="query">Запрос для фильтрации.</param>
        /// <returns>Отфильтрованный запрос.</returns>
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

        /// <summary>
        /// Применяет фильтрацию данных по полу.
        /// </summary>
        /// <param name="query">Запрос для фильтрации.</param>
        /// <returns>Отфильтрованный запрос.</returns>
        private IQueryable<Client> ApplyGenderFilter(IQueryable<Client> query)
        {
            if (cbFilter.SelectedItem is ComboBoxItem selectedFilter)
            {
                switch (selectedFilter.Tag.ToString())
                {
                    case "1":
                        return query.Where(c => c.GenderCode == "2");
                    case "2":
                        return query.Where(c => c.GenderCode == "1");
                    case "3":
                        var currentMonth = DateTime.Now.Month;
                        return query.Where(c => c.Birthday.HasValue && c.Birthday.Value.Month == currentMonth); // День рождения в этом месяце
                    case "0":
                        return query;
                    default:
                        return query;
                }
            }
            return query;
        }

        /// <summary>
        /// Применяет сортировку данных на основе выбранного критерия.
        /// </summary>
        /// <param name="query">Запрос для сортировки.</param>
        /// <returns>Отсортированный запрос.</returns>
        private IQueryable<Client> ApplySorting(IQueryable<Client> query)
        {
            if (cbSort.SelectedItem is ComboBoxItem selectedSort)
            {
                switch (selectedSort.Tag.ToString())
                {
                    case "1":
                        return query.OrderBy(c => c.LastName);
                    case "2":
                        return query.OrderByDescending(c => c.ClientService
                            .OrderByDescending(cs => cs.StartTime)
                            .FirstOrDefault().StartTime);
                    case "3":
                        return query.OrderByDescending(c => c.ClientService.Count());
                    default:
                        return query.OrderBy(c => c.ID);
                }
            }
            return query.OrderBy(c => c.ID);
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки "Назад" для навигации по страницам.
        /// </summary>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadData();
            }
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки "Вперед" для навигации по страницам.
        /// </summary>
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < (totalRecords + pageSize - 1) / pageSize)
            {
                currentPage++;
                LoadData();
            }
        }

        /// <summary>
        /// Обрабатывает изменения при выборе размера страницы.
        /// </summary>
        private void cbPageSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbPageSize.SelectedItem is ComboBoxItem selectedItem)
            {
                pageSize = int.Parse(selectedItem.Tag.ToString());
                currentPage = 1;
                LoadData();
            }
        }

        /// <summary>
        /// Обрабатывает изменения текста в поле поиска.
        /// </summary>
        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentPage = 1;
            LoadData();
        }

        /// <summary>
        /// Обрабатывает изменения при выборе фильтра.
        /// </summary>
        private void Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadData();
        }

        /// <summary>
        /// Обрабатывает изменения при выборе сортировки.
        /// </summary>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadData();
        }

        private void clientGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        /// <summary>
        /// Удаление клиента.
        /// </summary>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (clientsGrid.SelectedItem is Client selectedClient)
            {
                // Проверяем, есть ли у клиента записи о посещениях
                var clientVisits = selectedClient.ClientService;
                if (clientVisits != null && clientVisits.Any())
                {
                    MessageBox.Show("Невозможно удалить клиента, у которого есть записи о посещениях.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    // Удаляем записи связи клиента с тегами в таблице TagOfClient
                    /*
                    var clientTags = Number2Entities.GetContext().TagOfClient.Where(tc => tc.ClientID == selectedClient.ID).ToList();
                    foreach (var clientTag in clientTags)
                    {
                        Number2Entities.GetContext().TagOfClient.Remove(clientTag);
                    }
                    */

                    // Удаляем самого клиента
                    Number2Entities.GetContext().Client.Remove(selectedClient);
                    Number2Entities.GetContext().SaveChanges();

                    MessageBox.Show("Клиент и его связанные теги успешно удалены.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Обновляем данные на странице после удаления
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка при удалении клиента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите клиента для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (clientsGrid.SelectedItem is Client selectedClient)
            {
                NavigationService.Navigate(new PageKlients(selectedClient));
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите клиента для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PageKlients(null));
        }
    }
}
