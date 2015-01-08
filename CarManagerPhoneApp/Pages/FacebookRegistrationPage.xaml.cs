using System;
using System.Windows;
using CarManagerPhoneApp.ConnectorManager;
using CarManagerPhoneApp.Facebook;
using Microsoft.Phone.Controls;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;
using CarManagerPhoneApp.CarManagerService;

namespace CarManagerPhoneApp.Pages
{


    public partial class FacebookAddNewDriverPage : PhoneApplicationPage
    {
        private const string driverId = "DriverID";
        private const string userId = "UserID";
        private readonly CarManagerApiClient _service;
        private readonly IsolatedStorageSettings _appSettings = IsolatedStorageSettings.ApplicationSettings;
        private FacebookLoginUtils facebookUtils = new FacebookLoginUtils();
        public FacebookAddNewDriverPage()
        {

            InitializeComponent();
            AnimateSmiley.AutoReverse = true;

            AnimateSmiley.Begin();
            try
            {
                _service = new CarManagerApiClient();
                this.Loaded += FacebookAddNewDriverPage_Loaded;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                goToStartPage();
            }
        }

        private void AnimateSmiley_Completed(object sender, EventArgs e)
        {
            AnimateSmiley.Begin();
        }


        private async void FacebookAddNewDriverPage_Loaded(object sender, RoutedEventArgs e)
        {

            if (!App.isAuthenticated || App.ResetFacebookUser)
            {
                if (_appSettings.Contains(userId))
                {
                    if (_appSettings.Contains(driverId))
                    {
                        NavigationService.Navigate(new Uri("/Pages/DrivingPage.xaml", UriKind.Relative));
                    }
                    else
                    {
                        NavigationService.Navigate(new Uri("/Pages/RegisterPage.xaml", UriKind.Relative));
                    }
                }
                else
                {
                    await Authenticate();
                }
            }
        }


        private async Task Authenticate()
        {
            string message = String.Empty;
            try
            {
                if (App.ResetFacebookUser)
                {
                    facebookUtils.ResetFacebookUser();
                    App.ResetFacebookUser = false;
                }
                App.isAuthenticated = true;
                if (WindowsPhoneManager.checkNetworkConnection())
                {
                    await facebookUtils.Login();
                    App.AccessToken = facebookUtils.AccessToken;
                    App.FacebookId = facebookUtils.FacebookId;
                    _service.UserIdByProviderIdAsync(facebookUtils.FacebookId);
                    _service.UserIdByProviderIdCompleted += service_UserIdByProviderIdCompleted;
                }
                else
                {
                    showNoCommunicationMessage();
                }
            }
            catch (InvalidOperationException e)
            {
                App.isAuthenticated = false;
                message = "Login failed! Exception details: " + e.Message;
                MessageBox.Show(message);
                goToStartPage();
            }
        }

        private void showNoCommunicationMessage()
        {
            MessageBox.Show("No InternetConnection");
            goToStartPage();
        }

        private void goToStartPage()
        {
            App.isAuthenticated = false;
            Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri("/Pages/HomePage.xaml", UriKind.Relative)));
        }


        void service_UserIdByProviderIdCompleted(object sender, UserIdByProviderIdCompletedEventArgs e)
        {
            if (e.Error == null && e.Result.HasValue)
            {
                _appSettings[userId] = e.Result.Value;
                _appSettings.Save();
                MessageBox.Show(string.Format("User id is {0}", e.Result.Value));
                CheckIfDriverExist();
            }
            else
            {
                MessageBox.Show("Bad user ID!");
                goToStartPage();
            }
        }

        private void CheckIfDriverExist()
        {
            int userIdentification = ((int?)_appSettings[userId]).Value;
            if (WindowsPhoneManager.checkNetworkConnection())
            {
                _service.DriverIdByUserIdAsync(userIdentification);
                _service.DriverIdByUserIdCompleted += service_DriverIdByUserIdCompleted;
            }
            else
            {
                showNoCommunicationMessage();
            }
         
        }


        void service_DriverIdByUserIdCompleted(object sender, DriverIdByUserIdCompletedEventArgs e)
        {
            if (e.Error == null && e.Result.HasValue)
            {
                _appSettings[driverId] = e.Result;
                _appSettings.Save();
                NavigationService.Navigate(new Uri("/Pages/HomePage.xaml", UriKind.Relative));
            }
            else
            {
                _appSettings.Remove(driverId);
                _appSettings.Save();
                NavigationService.Navigate(new Uri("/Pages/RegisterPage.xaml", UriKind.Relative));
            }
        }

        private void LayoutRoot_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void StackPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            AnimateSmiley.Stop();
        }
    }
}