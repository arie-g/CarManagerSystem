using System;
using System.IO.IsolatedStorage;
using System.Windows;
using Microsoft.Phone.Controls;

namespace CarManagerPhoneApp.Pages
{
    public partial class StartPage : PhoneApplicationPage
    {

        private const string DriverId = "DriverID";
        private const string UserId = "UserID";
        private readonly IsolatedStorageSettings _appSettings = IsolatedStorageSettings.ApplicationSettings;

        public StartPage()
        {
            InitializeComponent();
        }

        enum eAuthorization
        {
            Approved,
            NotApproved

        }

        private eAuthorization AskAuthorization(string carId)
        {
            throw new NotImplementedException();
        }

        private string GetCarId()
        {

            throw new NotImplementedException();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_appSettings.Contains(DriverId)) // debug only
            {
                loginCheckBox.Content = "Not Registered";
                loginCheckBox.IsChecked = false;
            }
            else
            {
                loginCheckBox.Content = "Registered";
                loginCheckBox.IsChecked = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/FacebookRegistrationPage.xaml", UriKind.Relative));
        }

        private void DifferentUserButton_Click(object sender, RoutedEventArgs e)
        {
            CheckAndRemove(UserId);
            CheckAndRemove(DriverId);
            App.ResetFacebookUser = true;
            NavigationService.Navigate(new Uri("/Pages/FacebookRegistrationPage.xaml", UriKind.Relative));
        }

        private void CheckAndRemove(string key)
        {
            if (_appSettings.Contains(key))
            {
                _appSettings.Remove(key);
            }
        }

        private void reset_Click(object sender, RoutedEventArgs e)
        {
            CheckAndRemove(DriverId);
        }

        private void main_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/DrivingPage.xaml", UriKind.Relative));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _appSettings["UserID"] = "10201433373351300";
            _appSettings["DriverID"] = "9b6dc7cc-7d40-4e8e-b5e9-1dc8915ce4a8";
        }
    }
}