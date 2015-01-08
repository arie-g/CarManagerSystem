using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using CarManagerPhoneApp.CarManagerService;
using CarManagerPhoneApp.ConnectorManager;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;
using System.ServiceModel;

namespace CarManagerPhoneApp
{
    public partial class AddNewDriverPage : PhoneApplicationPage
    {
        private const string driverId = "DriverID";
        private const string userId = "UserID";
        private IsolatedStorageSettings appSettings;
        private HashSet<Guid> companyId = new HashSet<Guid>(); 
        private CarManagerApiClient _service;

        public AddNewDriverPage()
        {
            InitializeComponent();
            try
            {
                _service = new CarManagerApiClient();
                appSettings = IsolatedStorageSettings.ApplicationSettings;
                _service.GetCompaniesAsync();
                _service.GetCompaniesCompleted += _service_GetCompaniesCompleted;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri("/Pages/HomePage.xaml", UriKind.Relative)));
            }
            this.Loaded += AddNewDriverPage_Loaded;
        }

        void _service_GetCompaniesCompleted(object sender, CarManagerService.GetCompaniesCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                List<KeyValuePair<Guid,String>> companies = new List<KeyValuePair<Guid, string>>();
                foreach (var company in e.Result)
                {
                    companies.Add(new KeyValuePair<Guid, string>(company.key, company.value));

                }
                companiesLongList.ItemsSource = companies;
                LoginButton.IsEnabled = true;
                companiesLongList.IsEnabled = true;

            }
        }

        void AddNewDriverPage_Loaded(object sender, RoutedEventArgs e)
        {
            companiesLongList.IsEnabled = false;
            LoginButton.IsEnabled = false;
         /*   if (appSettings.Contains(driverId))
            {
                NavigationService.Navigate(new Uri("/Pages/DrivingPage.xaml", UriKind.Relative));
            }*/
        }

        void service_addDriverCompleted(object sender, AddDriverCompletedEventArgs e)
        {
            if (e.Error != null && !e.Result.HasValue)
            {
                string msg = string.Empty;
                if (e.Error is FaultException)
                {
                    msg = "Fault Fault ";
                }
                MessageBox.Show(msg + e.Error.ToString());
                NavigationService.Navigate(new Uri("/Pages/FacebookAddNewDriverPage.xaml", UriKind.Relative));
            }
            else
            {
                MessageBox.Show("Added Driver");
                appSettings[driverId] = e.Result;
                NavigationService.Navigate(new Uri("/Pages/DrivingPage.xaml", UriKind.Relative));
            }
        }


        private void FirstNameTextBox_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            clearText(FirstNameTextBox);
        }

        private void clearText(TextBox textBox)
        {
            textBox.Text = string.Empty;
        }

        private void FirstNameTextBox_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            setDefaultText(FirstNameTextBox, "First Name");
        }

        private void setDefaultText(TextBox textBox, string text)
        {
            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = text;
            }
        }

        private void LastNameTextBox_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            clearText(LastNameTextBox);
        }

        private void LastNameTextBox_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            setDefaultText(LastNameTextBox, "Last Name");
        }

        private void LicenceTextBox_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            clearText(LicenceTextBox);
        }

        private void LicenceTextBox_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            setDefaultText(LicenceTextBox, "Licence");
        }

        private void ExperienceYearsTextBox_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            clearText(ExperienceYearsTextBox);
        }

        private void ExperienceYearsTextBox_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            setDefaultText(ExperienceYearsTextBox, "Experience Years");
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int? Id = (appSettings[userId] as Nullable<int>);
                if (Id.HasValue)
                {
                    int userIdentification = Id.Value;
                    Driver newDriver = new Driver()
                    {
                        Name = FirstNameTextBox.Text,
                        FamilyName = LastNameTextBox.Text,
                        Licence = LicenceTextBox.Text,
                        ExperienceYears = short.Parse(ExperienceYearsTextBox.Text),
                        UserId = userIdentification
                    };
                    if (WindowsPhoneManager.checkNetworkConnection())
                    {
                        ObservableCollection<Guid> companiesIdList = new ObservableCollection<Guid>();
                        foreach (var comp in companiesLongList.SelectedItems)
                        {

                            companiesIdList.Add(((KeyValuePair<Guid, String>) comp).Key);
                        }
                        appSettings.Remove(userId);
                        _service.AddDriverAsync(newDriver, companiesIdList);
                        _service.AddDriverCompleted += service_addDriverCompleted;
                    }
                    else
                    {
                        MessageBox.Show("Cannot create new driver - no Internet");
                        NavigationService.Navigate(new Uri("/Pages/HomePage.xaml", UriKind.Relative));
                    }
                }
                else
                {
                    MessageBox.Show("Bad user - try again");
                    App.ResetFacebookUser = true;
                    NavigationService.Navigate(new Uri("/Pages/FacebookRegistrationPage.xaml", UriKind.Relative));
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Bad input - try again");
            }

        }

        private void companiesLongList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
