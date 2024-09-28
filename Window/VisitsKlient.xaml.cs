using System;
using System.Linq;
using System.Windows;
using Authoservice.Model;

namespace Authoservice.Window
{
    public partial class VisitsKlient : System.Windows.Window
    {
        private Client selectedClient;

        public VisitsKlient(Client client)
        {
            InitializeComponent();
            selectedClient = client;
            LoadVisits();
        }

        private void LoadVisits()
        {
            var visits = Number2Entities.GetContext().ClientService
                .Where(cs => cs.ClientID == selectedClient.ID)
                .Select(cs => new
                {
                    ServiceName = cs.Service.Title,      // Название услуги
                    Date = cs.StartTime,                 // Время начала услуги
                    FileCount = "Всего файлов: " + cs.DocumentByService.Count(), // Количество прикрепленных документов
                    Documents = cs.DocumentByService.Select(d => d.DocumentPath).ToList() // Пути к прикрепленным документам
                })
                .ToList();

            visitsListBox.ItemsSource = visits;
        }


        private void VisitDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранное посещение
            var selectedVisit = visitsListBox.SelectedItem;

            if (selectedVisit == null)
            {
                MessageBox.Show("Пожалуйста, выберите посещение для просмотра файлов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Приведение к типу анонимного объекта
            dynamic visit = selectedVisit;

            // Получаем список документов
            var documents = visit.Documents;

            if (documents.Count == 0)
            {
                MessageBox.Show("Для данного посещения не прикреплены файлы.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Отображаем список файлов
            string fileList = string.Join("\n", documents);
            MessageBox.Show($"Список прикрепленных файлов:\n{fileList}", "Файлы", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
