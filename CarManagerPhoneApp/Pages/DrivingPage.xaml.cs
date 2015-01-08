using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CarManagerPhoneApp.ConnectorManager;
using Microsoft.Phone.Controls;

namespace CarManagerPhoneApp.Pages
{
    public partial class DrivingPage : PhoneApplicationPage
    {
        private const int MaxDisconectionCount = 8;
        private const string Emergency = "Emergency";
        private readonly DispatcherTimer _dispatcherTimer;
        private readonly TimeSpan _prepareTIme = new TimeSpan(0, 0, 5);
        private readonly TimeSpan _updateTIme = new TimeSpan(0, 0, 5);
        private CarData _carData;
        private bool initFail = false;
        private int _disconectionCount;
        // Constructor
        public DrivingPage()
        {
            try
            {
                InitializeComponent();
                _disconectionCount = 0;
                _carData = new CarData(NoBluetoothConectedMessage, returnToMain);
                _dispatcherTimer = new DispatcherTimer();
                _dispatcherTimer.Interval = _prepareTIme;
                _dispatcherTimer.Tick += checkIfConnectedInterval;
                _dispatcherTimer.Start();
            }
            catch (Exception)
            {
                initFail = true;
                if (_dispatcherTimer != null) _dispatcherTimer.Stop();
                Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri("/Pages/HomePage.xaml", UriKind.Relative)));
            }
        }


        private async Task<eConnectionStatus> NoBluetoothConectedMessage()
        {
            eConnectionStatus res = eConnectionStatus.NotConnected;
            MessageBoxResult result = MessageBox.Show("Warning", "No Bluetooth Conncected\n Try to connect?",
                MessageBoxButton.OKCancel);
            _disconectionCount++;
            if (result == MessageBoxResult.OK)
            {
                if (await _carData.Connect() == eConnectionStatus.OK)
                {
                    res = eConnectionStatus.OK;
                    _disconectionCount = 0;
                }
            }

            if (_disconectionCount > MaxDisconectionCount)
                ShowExitMenu();

            return res;
        }

        private void ShowExitMenu()
        {
            MessageBoxResult result = MessageBox.Show("Do you want to continue useing the app?",
                "Too much time without connection", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                _disconectionCount = 0;
            }
            else
            {
                NavigationService.Navigate(new Uri("/Pages/HomePage.xaml", UriKind.Relative));
            }
        }


        void checkIfConnectedInterval(object sender, EventArgs e)
        {
            _dispatcherTimer.Stop();
            if (_carData.IsConnected())
            {
                _dispatcherTimer.Interval = _updateTIme;
                _dispatcherTimer.Tick -= checkIfConnectedInterval;
                _dispatcherTimer.Tick += UpdateValuesInterval;
            }
            _dispatcherTimer.Start();
        }

        private void UpdateValuesInterval(object sender, EventArgs e)
        {
            _dispatcherTimer.Stop();
            if (_carData.needToUpdate)
            {
                CarDataBll.CheckAndProcessData(_carData);
                UpdateValues();
            }
            _dispatcherTimer.Start();
        }

        private void UpdateValues()
        {
            TempTextBlock.Text = _carData.TemperatureString +"C";
            SpeedTextBlock.Text = _carData.SpeedString;
            RpmTextBlock.Text = _carData.RpmString;
        }

        private void PhoneApplicationPage_Unloaded(object sender, RoutedEventArgs e)
        {
            UnConnect();
        }

        

        private void UnConnect()
        {
            if (_dispatcherTimer != null)
            {
                _dispatcherTimer.Stop();
            }
            if (!initFail)
            {
                this.DataContext = _carData;
            }
            if (_carData != null)
            {
                _carData.UnConnect();
                _carData = null;
            }
            returnToMain();
        }

        private void returnToMain()
        {
            NavigationService.Navigate(new Uri("/Pages/HomePage.xaml", UriKind.Relative));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EndRideButton.IsEnabled = false;
            UnConnect();
        }
    }
}