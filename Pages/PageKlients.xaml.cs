using Authoservice.Model;
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
        private readonly string imagesBasePath = @"C:\Users\elozo\OneDrive\Рабочий стол\4 курс\МДК.02.02 Инструментальные средства разработки программного обеспечения\Автосервис - клиенты\Authoservice\Images";

        public PageKlients(Client client)
        {
            InitializeComponent();
            this.client = client;
            if(client != null)
            {
                LoadData(client);
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

                gridTag.ItemsSource = Number2Entities.GetContext().Client
                    .Include("Tag").Where(c=>c.ID == client.ID).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при загрузке данных: " + ex.Message);
            }
        }

        private void btnAddTag_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDalateTag_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnWrite_Click(object sender, RoutedEventArgs e)
        {

        }

        private void gridTag_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
