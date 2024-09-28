using Authoservice.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Authoservice.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageKlients.xaml
    /// </summary>
    public partial class PageKlients : Page
    {
        public Client client;
        // Добавляем свойство для доступных тегов
        public List<Tag> AvailableTags { get; set; }
        private readonly string imagesBasePath = @"C:\Users\elozo\OneDrive\Рабочий стол\4 курс\МДК.02.02 Инструментальные средства разработки программного обеспечения\Автосервис - клиенты\Authoservice\Images";
        public string pathNewClient;

        public PageKlients(Client client)
        {
            InitializeComponent();
            this.client = client;
            LoadTags();
            if (client != null)
            {
                tbID.IsReadOnly = true;
                LoadData(client);

            }
            else if (client == null)
            {
                tbID.Visibility = Visibility.Hidden;
                lblID.Visibility = Visibility.Hidden;
                btnSaveTag.Visibility = Visibility.Hidden;
                btnDelete.Visibility = Visibility.Hidden;
            }
        }

        private void LoadTags()
        {
            var allTags = Number2Entities.GetContext().Tag.ToList();
            var clientTags = client?.Tag.Select(t => t.ID).ToList() ?? new List<int>();

            AvailableTags = allTags.Select(t => new Tag
            {
                ID = t.ID,
                Title = t.Title,
                Color = t.Color,
                IsChecked = clientTags.Contains(t.ID)
            }).ToList();

            DataContext = this;
        }

        private void btnSaveTag_Click(object sender, RoutedEventArgs e)
        {
            if (client != null)
            {
                // Удаляем текущие теги клиента из базы данных
                var existingTags = Number2Entities.GetContext().Entry(client).Collection(c => c.Tag).Query().ToList();

                foreach (var tag in existingTags)
                {
                    client.Tag.Remove(tag);
                }

                // Проходим по всем выбранным тегам
                foreach (var tag in AvailableTags.Where(t => t.IsChecked))
                {
                    // Предполагается, что теги уже загружены и отслеживаются
                    var trackedTag = Number2Entities.GetContext().Tag.Find(tag.ID);
                    if (trackedTag != null)
                    {
                        client.Tag.Add(trackedTag);
                    }
                }

                // Сохраняем изменения в базе данных
                Number2Entities.GetContext().SaveChanges();
                MessageBox.Show("Теги успешно сохранены.");

                LoadTags(); // обновляем список тегов после сохранения
            }
        }

        private void LoadData(Client client)
        {
            try
            {
                if (client.PhotoPath != null)
                {
                    string dbPath = client.PhotoPath.Replace("/", "\\").Trim();
                    string fullPath = System.IO.Path.Combine(imagesBasePath, dbPath.TrimStart('\\'));

                    if (File.Exists(fullPath))
                    {
                        BitmapImage bitmap = new BitmapImage(new Uri(fullPath, UriKind.Absolute));
                        imageClient.Source = bitmap;
                    }
                    else
                    {
                        string defaultImagePath = System.IO.Path.Combine(imagesBasePath, "service_logo.png");
                        BitmapImage bitmap = new BitmapImage(new Uri(defaultImagePath, UriKind.Absolute));
                        imageClient.Source = bitmap;
                    }
                }
                tbID.Text = client.ID.ToString();
                tbFirstName.Text = client.FirstName;
                tbLastName.Text = client.LastName;
                tbPatronymic.Text = client.Patronymic;
                tbEmail.Text = client.Email;
                tbPhone.Text = client.Phone;
                dpBirthday.SelectedDate = client.Birthday;

                if (client.GenderCode == "1") // Мужской
                {
                    rbMale.IsChecked = true;
                }
                else if (client.GenderCode == "2") // Женский
                {
                    rbFemale.IsChecked = true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при загрузке данных: " + ex.Message);
            }
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите удалить клиента?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    Number2Entities.GetContext().Client.Remove(client);
                    Number2Entities.GetContext().SaveChanges();
                    MessageBox.Show("Клиент удалён.");
                    NavigationService.Navigate(new ListKlients(null));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении клиента: " + ex.Message);
                }
            }
        }

        private bool ValidateFields()
        {
            string namePattern = @"^[A-Za-zА-Яа-яЁё\s-]+$";
            string phonePattern = @"^[\d\s\+\-\(\)]+$";
            string emailPattern = @"^[\w\.-]+@[\w\.-]+\.\w+$";

            if (string.IsNullOrWhiteSpace(tbFirstName.Text) ||
                string.IsNullOrWhiteSpace(tbLastName.Text) ||
                string.IsNullOrWhiteSpace(tbPhone.Text) ||
                string.IsNullOrWhiteSpace(tbEmail.Text))
            {
                MessageBox.Show("Все поля должны быть заполнены.");
                return false;
            }

            if (!Regex.IsMatch(tbFirstName.Text, namePattern) ||
                !Regex.IsMatch(tbLastName.Text, namePattern) ||
                !Regex.IsMatch(tbPatronymic.Text, namePattern))
            {
                MessageBox.Show("ФИО может содержать только буквы, пробелы и дефисы.");
                return false;
            }

            if (tbFirstName.Text.Length > 50 || tbLastName.Text.Length > 50 || tbPatronymic.Text.Length > 50)
            {
                MessageBox.Show("Фамилия, имя и отчество не могут быть длиннее 50 символов.");
                return false;
            }

            if (!Regex.IsMatch(tbPhone.Text, phonePattern))
            {
                MessageBox.Show("Телефон может содержать только цифры, пробелы и допустимые символы.");
                return false;
            }

            if (!Regex.IsMatch(tbEmail.Text, emailPattern))
            {
                MessageBox.Show("Неверный формат email.");
                return false;
            }

            return true;
        }

        private void btnWrite_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
            {
                return;
            }

            try
            {
                if (client == null) // Если клиент null, то создаем нового
                {
                    client = new Client
                    {
                        ID = Number2Entities.GetContext().Client.Any() ? Number2Entities.GetContext().Client.Max(c => c.ID) + 1 : 1,
                        FirstName = tbFirstName.Text,
                        LastName = tbLastName.Text,
                        Patronymic = tbPatronymic.Text,
                        Email = tbEmail.Text,
                        Phone = tbPhone.Text,
                        Birthday = dpBirthday.SelectedDate,
                        RegistrationDate = DateTime.Now,
                        GenderCode = rbMale.IsChecked == true ? "1" : "2",
                        PhotoPath = pathNewClient
                    };

                    Number2Entities.GetContext().Client.Add(client);
                    Number2Entities.GetContext().SaveChanges();
                    btnSaveTag_Click(sender,e);                }
                else // Если клиент уже существует, то обновляем данные
                {
                    client.FirstName = tbFirstName.Text;
                    client.LastName = tbLastName.Text;
                    client.Patronymic = tbPatronymic.Text;
                    client.Email = tbEmail.Text;
                    client.Phone = tbPhone.Text;
                    client.Birthday = dpBirthday.SelectedDate;
                    client.GenderCode = rbMale.IsChecked == true ? "1" : "2";

                    Number2Entities.GetContext().SaveChanges();

                }

                MessageBox.Show("Данные клиента успешно сохранены.");
                NavigationService.Navigate(new ListKlients(null));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении данных: " + ex.Message);
            }
        }

        private void btnChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "Image Files (*.jpg; *.jpeg; *.png)|*.jpg;*.jpeg;*.png"
            };

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                string selectedFilePath = dlg.FileName;
                FileInfo fileInfo = new FileInfo(selectedFilePath);

                if (fileInfo.Length > 2 * 1024 * 1024)
                {
                    MessageBox.Show("Размер файла не должен превышать 2 МБ.");
                    return;
                }

                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFilePath);
                    bitmap.DecodePixelWidth = 100;
                    bitmap.EndInit();
                    imageClient.Source = bitmap;

                    string relativePath = System.IO.Path.Combine("clients", System.IO.Path.GetFileName(selectedFilePath));
                    if(client != null)
                    {
                        client.PhotoPath = relativePath;
                    }
                    else
                    {
                        pathNewClient = relativePath;
                    }

                    string destinationPath = System.IO.Path.Combine(imagesBasePath, relativePath);
                    File.Copy(selectedFilePath, destinationPath, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка при изменении фото: {ex.Message}");
                }
            }
        }

        
    }
}
