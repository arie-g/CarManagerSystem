using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using CarManagerPhoneApp.Arduino;
using CarManagerPhoneApp.CarManagerService;
using CarManagerPhoneApp.ConnectorManager;
using CarManagerPhoneApp.Data;
using CarManagerPhoneApp.Facebook;
using Microsoft.Phone.Controls;

namespace CarManagerPhoneApp.Pages
{
    public partial class HomePage : PhoneApplicationPage
    {
        private const string offLineRidesFileName = "offlinerides.bin";
        private const string emergancyRideId = "EmergancyRideId";
        private const string userId = "UserID";
        private const string DriverId = "DriverID";
        private ArduinoCommunicator arduinoCommunicator;
        private readonly IsolatedStorageSettings _appSettings;
        private readonly CarManagerApiClient _service;
        private Stack<KeyValuePair<DriveStatistics, bool>> ridesToUpdates;
        private bool emargencyRideAllowed;
        private KeyValuePairOfArrayOfguidArrayOfguid0dMmj3_Sh updatedAllowedLists;
        private bool unloaded;
        public HomePage()
        {
            try
            {
                _service = new CarManagerApiClient();
                _appSettings = IsolatedStorageSettings.ApplicationSettings;
                ridesToUpdates = new Stack<KeyValuePair<DriveStatistics, bool>>();
                arduinoInitialize();
                if (_appSettings.Contains(emergancyRideId))
                {
                    _appSettings.Remove(emergancyRideId);
                    _appSettings.Save();
                }
                InitializeComponent();

            }
            catch (Exception)
            {
                MessageBox.Show("Please Connect the customer service", "Critical Error", MessageBoxButton.OK);
                Application.Current.Terminate();
            }
        }

        private void arduinoInitialize()
        {
            arduinoCommunicator = ArduinoCommunicator.Arduino();
        }

     
        private bool driverLogedIn()
        {
            return getDriverId() != null;
        }

        private async void PhoneApplicaticaonPage_Loaded(object sender, RoutedEventArgs e)
        {
            await getCarId();
            await checkAndUpdateRides();
          //  checkUpdatingAuthorizedList();
            initializeLoginButton();
        }

        private void checkUpdatingAuthorizedList()
        {
            if (!string.IsNullOrEmpty(App.CarId) && WindowsPhoneManager.checkNetworkConnection())
            {
                _service.TimeToUpdateAuthorizedListAsync(App.CarId);
                _service.TimeToUpdateAuthorizedListCompleted += TimeToUpdateAuthorizedListCompleted;
            }
        }

        private void TimeToUpdateAuthorizedListCompleted(object sender, TimeToUpdateAuthorizedListCompletedEventArgs e)
        {
            if (e.Error == null && !unloaded)
            {
                if (e.Result)
                {
                    getUpdatedAuthorizedList();
                }
            }
        }

        private void initializeLoginButton()
        {
            if (driverLogedIn())
            {
                loginImage.Source = new BitmapImage(new Uri("/Images/powerImageOn.png", UriKind.RelativeOrAbsolute));
                LoginButton.Content = "Login other \nUser/Register";
            }
            else
            {
                loginImage.Source = new BitmapImage(new Uri("/Images/powerImageOff.png", UriKind.RelativeOrAbsolute));
                LoginButton.Content = "Login/Register";
            }
        }

        private async Task checkAndUpdateRides()
        {
            await LoadOffLineRides();
            if (ridesToUpdates.Count > 0 && WindowsPhoneManager.checkNetworkConnection())
            {
                var ride = ridesToUpdates.Pop();
                _service.UpdateOfflineRideAsync(ride.Key, ride.Value);
                _service.UpdateOfflineRideCompleted += _service_UpdateOfflineRideCompleted;
            }
        }

        private async Task LoadOffLineRides()
        {
            if (
                await
                    IsolatedStorageOperations.IsExist<Stack<KeyValuePair<DriveStatistics, bool>>>(offLineRidesFileName))
            {
                ridesToUpdates = await
                    IsolatedStorageOperations.Load<Stack<KeyValuePair<DriveStatistics, bool>>>(offLineRidesFileName);
            }
             LoadOffLineRidesFromArduino();
        }

        private async void LoadOffLineRidesFromArduino()
        {
            var ridesList = new GetRidesList { };
            arduinoCommunicator.getRidesComplete += addRidesToUpdate;
            await arduinoCommunicator.GetRidesList(ridesList);
        }

        private void addRidesToUpdate(string rides)
        {
            string ridesMsg = rides.Substring(rides.IndexOf('{'));
            List<Ride> rideList = arduinoCommunicator.ConvertToRideList(ridesMsg);
            foreach (var ride in rideList)
            {
                bool isEmergency = ride.Emergency.HasValue && ride.Emergency.Value;
                ridesToUpdates.Push(new KeyValuePair<DriveStatistics, bool>(
                    new DriveStatistics()
                    {
                        CarId = ride.CarID,
                        StartDrive = ride.StartDrive,
                        FinishDrive = ride.EndDrive.HasValue ? ride.EndDrive.Value : ride.StartDrive,
                        DriverId = ride.DriverID,
                        Data = ride.DrivePackages
                    },
                    isEmergency
                    ));
            }

            arduinoCommunicator.getRidesComplete -= addRidesToUpdate;
        }

      

       





        void _service_UpdateOfflineRideCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            _service.UpdateOfflineRideCompleted -= _service_UpdateOfflineRideCompleted;
            if (ridesToUpdates.Count > 0 && !unloaded)
            {
                var ride = ridesToUpdates.Pop();
                if (WindowsPhoneManager.checkNetworkConnection())
                {
                    _service.UpdateOfflineRideAsync(ride.Key, ride.Value);
                    _service.UpdateOfflineRideCompleted += _service_UpdateOfflineRideCompleted;
                }
                else
                {
                    // No internet access
                }
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            CheckAndRemove(userId);
            CheckAndRemove(DriverId);
            App.ResetFacebookUser = true;

            NavigationService.Navigate(new Uri("/Pages/FacebookRegistrationPage.xaml", UriKind.Relative));
        }

        private void CheckAndRemove(string key)
        {
            if (_appSettings.Contains(key))
            {
                _appSettings.Remove(key);
                _appSettings.Save();
            }
        }

        private void StartRideButton_Click(object sender, RoutedEventArgs e)
        {
            if (driverLogedIn() && !unloaded)
            {
                disableButtons();
                if (WindowsPhoneManager.checkNetworkConnection())
                {
                    Guid? driverGuid = getDriverId();
                    if (driverGuid.HasValue && !string.IsNullOrEmpty(App.CarId))
                    {
                        _service.CheckBreakingRolesAsync(driverGuid.Value, App.CarId);
                        _service.CheckBreakingRolesCompleted += _service_CheckBreakingRolesAsync;
                    }
                    else
                    {
                        enableButtons();
                    }
                }
                else
                {
                    StartRide();
                }
            }
            else
            {
                enableButtons();
            }
        }

        private Guid? getDriverId()
        {
            if (_appSettings.Contains(DriverId))
            {
                try
                {
                    return (Guid)_appSettings[DriverId];
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        void _service_CheckBreakingRolesAsync(object sender, CheckBreakingRolesCompletedEventArgs e)
        {
            _service.CheckBreakingRolesCompleted -= _service_CheckBreakingRolesAsync;
            if (e.Error == null && e.Result.HasValue && !unloaded)
            {
                StartRide();
            }
            else
            {
                DriverNotApprooved();
            }
        }

        private void StartRide()
        {
            var rideRequest = new RideRequest { driverId = getDriverId().ToString()};
             arduinoCommunicator.rideRequestCoplete += RideRequestCoplete;
             arduinoCommunicator.MainProcdure(rideRequest);
        }

        private void RideRequestCoplete(string s)
        {
            if (s.Contains("allowed"))
            {
                NavigationService.Navigate(new Uri("/Pages/DrivingPage.xaml", UriKind.Relative));
            }
            else
            {
                DriverNotApprooved();
            }
        }

        private void DriverNotApprooved()
        {
            MessageBox.Show("Sory you don't have permission to drive now");
            enableButtons();
        }

        private async Task getCarId()
        {  
            var getCarId = new GetCarId { };
            arduinoCommunicator.GetCarIdComplete += GetCarIdComplete;
            await arduinoCommunicator.GetCarId(getCarId);
        }

        private void GetCarIdComplete(string s)
        {
            var mesageParts = s.Split('\n','\r');
            int carId = 0;
            string tmp = string.Empty;
            foreach (string part in mesageParts)
                {
                    tmp = part.Trim();
                    if (int.TryParse(tmp, out carId))
                    {
                        App.CarId = carId.ToString();
                        break;
                    }
                }
            arduinoCommunicator.GetCarIdComplete -= GetCarIdComplete;
        }

        private void disableButtons()
        {
            LoginButton.IsEnabled = false;
            StartRideButton.IsEnabled = false;
            EmergencyRideButton.IsEnabled = false;
        }

        private async Task enableButtons()
        {
            LoginButton.IsEnabled = true;

            if (getDriverId() != null && !emargencyRideAllowed)
            {
             //   checkEmergencyAllowed();
            }
            if (driverLogedIn())
            {
                StartRideButton.IsEnabled = true;
                EmergencyRideButton.IsEnabled = true;
            }
            else
            {
                StartRideButton.IsEnabled = false;
                EmergencyRideButton.IsEnabled = false;

            }
        }

        private void checkEmergencyAllowed()
        {

            Guid? driverGuid = getDriverId();
            if (driverGuid.HasValue)
            {
                var isEmergencyAllowed = new IsEmerAllowedToDrive {driverId = driverGuid.Value.ToString()};
                arduinoCommunicator.IsEmergencyyAllowedToDriveComplete += IsEmergencyyAllowedToDriveComplete;
                arduinoCommunicator.IsEmergencyAllowd(isEmergencyAllowed);
            }
        }

        private void IsEmergencyyAllowedToDriveComplete(string msg)
        {
            arduinoCommunicator.IsEmergencyyAllowedToDriveComplete -= IsEmergencyyAllowedToDriveComplete;
            EmergencyRideButton.IsEnabled = msg.Contains("Approved");
        }

        private async void EmergencyRideButton_Click(object sender, RoutedEventArgs e)
        {
            if (driverLogedIn() && !unloaded)
            {
                if (WindowsPhoneManager.checkNetworkConnection())
                {
                    _service.EmergencyDriveAsync(getDriverId().Value, App.CarId);
                    _service.EmergencyDriveCompleted += ServiceOnEmergencyDriveCompleted;
                }
                else
                {
                    arduinoEmergencyApprove();
                    disableButtons();
                    await Task.Delay(6000);
                }
            }
            else
            {
                MessageBox.Show("You must login before driving");
            }
        }

        private async Task arduinoEmergencyApprove()
        {
            string driverIdStr = getDriverId().ToString();
            if (!string.IsNullOrEmpty(driverIdStr))
            {
                var isEmergencyAllowed = new IsEmerAllowedToDrive {driverId = driverIdStr};
                arduinoCommunicator.IsEmergencyyAllowedToDriveComplete += emergancyRideAllowed;
                await arduinoCommunicator.IsEmergencyAllowd(isEmergencyAllowed);
            }
        }

        private async Task<string> getFacebookId()
        {
            if (!string.IsNullOrEmpty(App.FacebookId))
            {
                return App.FacebookId;
            }
            else
            {
                FacebookLoginUtils fbUtils = new FacebookLoginUtils();
                await fbUtils.Login();
                if (!string.IsNullOrEmpty(App.FacebookId))
                {
                    return App.FacebookId;
                }
                else
                {
                    return null;
                }
            }
        }

        private void StartEmergencyRide(Guid rideId)
        {
            _appSettings[emergancyRideId] = rideId;
            _appSettings.Save();
            StartRide();
        }

        private void ServiceOnEmergencyDriveByFacebookIdCompleted(object sender, EmergencyDriveByFacebookIdCompletedEventArgs emergencyDriveByFacebookIdCompletedEventArgs)
        {
            if (emergencyDriveByFacebookIdCompletedEventArgs.Result.HasValue && !unloaded)
            {
                StartEmergencyRide(emergencyDriveByFacebookIdCompletedEventArgs.Result.Value);
            }
        }


        private void emergancyRideAllowed(string responseMessage)
        {
            if (responseMessage.Contains("Approved"))
            {
                StartEmergencyRide(Guid.NewGuid());
            }
            else
            {
                var result = MessageBox.Show("You are not allowed to make emergency ride\n do you want to try update authorized list?", "EmergencyRideButton ride not allowed", MessageBoxButton.OKCancel);
                emargencyRideAllowed = false;
                if (result == MessageBoxResult.OK)
                {
                    if (WindowsPhoneManager.checkNetworkConnection())
                    {
                        getUpdatedAuthorizedList();
                        }
                    else
                    {
                        // 
                    }
               }
                enableButtons();
            }
            arduinoCommunicator.IsEmergencyyAllowedToDriveComplete -= emergancyRideAllowed;
        }

        private void getUpdatedAuthorizedList()
        {
                _service.GetOfflineAndEmergencyAllowedDriversAsync(App.CarId);
                _service.GetOfflineAndEmergencyAllowedDriversCompleted += updateAllowedList;
            }

        private async void updateAllowedList(object sender, GetOfflineAndEmergencyAllowedDriversCompletedEventArgs e)
        {
            _service.GetOfflineAndEmergencyAllowedDriversCompleted -= updateAllowedList;
            if (e.Error == null && e.Result.HasValue && !unloaded)
            {
                updatedAllowedLists = e.Result.Value;
                 deleteCurrentAllowedLists();
            }
        }

        private void deleteCurrentAllowedLists()
        {
            deletePermanent();

        }

        private void deletePermanent()
        {
            var deletePer = new DeletePermanentList { };
            arduinoCommunicator.deletePerListComplete += DeletePerListComplete;
            arduinoCommunicator.DeletePermanent(deletePer);
        }

        private void DeletePerListComplete(string s)
        {
            arduinoCommunicator.deletePerListComplete -= DeletePerListComplete;
            deleteEmergency();
        }

        private void deleteEmergency()
        {
            var deleteEmer = new DeleteEmergencyList {};
            arduinoCommunicator.deleteEmerListComplete += DeleteEmerListComplete;
            arduinoCommunicator.DeleteEmergency(deleteEmer);
        }

        private void DeleteEmerListComplete(string s)
        {
            arduinoCommunicator.deleteEmerListComplete -= DeleteEmerListComplete;
            updateEmergencyList();

        }

        private void updateEmergencyList()
        {
            var emergencyAllowedDrivers = updatedAllowedLists.value;

            if (emergencyAllowedDrivers.Count > 0)
            {
                Guid? driverId = getDriverId();
                if (driverId.HasValue)
                {

                    emargencyRideAllowed = emergencyAllowedDrivers.Contains(driverId.Value);
                }
                var emergencyDriverToAdd = emergencyAllowedDrivers.First();
                emergencyAllowedDrivers.Remove(emergencyDriverToAdd);
                arduinoCommunicator.AddEmerComplete += AddEmerComplete;
                updateEmergencyArduino(emergencyDriverToAdd);
            }
        }

        private void AddEmerComplete(string s)
        {
            if (!unloaded)
            {
            var emergencyAllowedDrivers = updatedAllowedLists.value;
            if (emergencyAllowedDrivers.Count > 0)
            {
                var emergencyDriverToAdd = emergencyAllowedDrivers.First();
                emergencyAllowedDrivers.Remove(emergencyDriverToAdd);
                updateEmergencyArduino(emergencyDriverToAdd);
            }
            else
            {
                arduinoCommunicator.AddEmerComplete -= AddEmerComplete;
                updatePermanentList();
            }
        }
            else
            {
                arduinoCommunicator.AddEmerComplete -= AddEmerComplete;
            }
        }

        private void updatePermanentList()
        {
            if (!unloaded)
            {
            var permenantAllowedDrivers = updatedAllowedLists.key;

            if (permenantAllowedDrivers.Count > 0)
            {
                var permenantDriverToAdd = permenantAllowedDrivers.First();
                permenantAllowedDrivers.Remove(permenantDriverToAdd);
                arduinoCommunicator.AddPerComplete += AddPerComplete;
                updatePermenantArduino(permenantDriverToAdd);
            }
        }
        }

        private void AddPerComplete(string s)
        {
            if (!unloaded)
            {
            var permenantAllowedDrivers = updatedAllowedLists.key;

            if (permenantAllowedDrivers.Count > 0)
            {
                var permenantDriverToAdd = permenantAllowedDrivers.First();
                permenantAllowedDrivers.Remove(permenantDriverToAdd);
                updatePermenantArduino(permenantDriverToAdd);
            }
            else
            {
                arduinoCommunicator.AddPerComplete -= AddPerComplete;
            }
        }
            else
            {
                arduinoCommunicator.AddPerComplete -= AddPerComplete;
            }
        }

        private void updatePermenantArduino(Guid permenantDriverToAdd)
        {
            throw new NotImplementedException();
        }

        private void updateEmergencyArduino(Guid emergencyDriverGuid)
        {
            var driverNew = new EmergencyDriver { driverId = emergencyDriverGuid.ToString(), count = "5"};
            arduinoCommunicator.AddNewEmergencyDriver(driverNew);
        }



        private void ServiceOnEmergencyDriveCompleted(object sender, EmergencyDriveCompletedEventArgs emergencyDriveCompletedEventArgs)
        {
            if (emergencyDriveCompletedEventArgs.Result.HasValue && !unloaded)
            {
                StartEmergencyRide(emergencyDriveCompletedEventArgs.Result.Value);
            }
        }

        private void PhoneApplicationPage_Unloaded(object sender, RoutedEventArgs e)
        {
            unloaded = true;
            UnloadEvents();
           
            if (arduinoCommunicator != null)
            {
                arduinoCommunicator.UnConnect();
            }
        }

        private void UnloadEvents()
        {
            if (arduinoCommunicator.getRidesComplete != null)
            {
                arduinoCommunicator.getRidesComplete -= addRidesToUpdate;
            }
            if (arduinoCommunicator.GetCarIdComplete != null)
            {
                arduinoCommunicator.GetCarIdComplete -= GetCarIdComplete;
            }
            if (arduinoCommunicator.IsEmergencyyAllowedToDriveComplete != null)
            {
                arduinoCommunicator.IsEmergencyyAllowedToDriveComplete -= IsEmergencyyAllowedToDriveComplete;
            }
            if (arduinoCommunicator.IsEmergencyyAllowedToDriveComplete != null)
            {
                arduinoCommunicator.IsEmergencyyAllowedToDriveComplete -= emergancyRideAllowed;
            }
            if (arduinoCommunicator.deletePerListComplete != null)
            {
                arduinoCommunicator.deletePerListComplete -= DeletePerListComplete;
            }
            if (arduinoCommunicator.deleteEmerListComplete != null)
            {
                arduinoCommunicator.deleteEmerListComplete -= DeleteEmerListComplete;
            }
            if (arduinoCommunicator.AddEmerComplete != null)
            {
                arduinoCommunicator.AddEmerComplete -= AddEmerComplete;
            }
            if (arduinoCommunicator.AddEmerComplete != null)
            {
                arduinoCommunicator.AddEmerComplete -= AddEmerComplete;
            }
            if (arduinoCommunicator.getRidesComplete != null)
            {
                arduinoCommunicator.getRidesComplete -= addRidesToUpdate;
            }
        }
    }
}