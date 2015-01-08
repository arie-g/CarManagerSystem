using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using CarManagerPhoneApp.Arduino;
using CarManagerPhoneApp.CarManagerService;
using CarManagerPhoneApp.ConnectorManager;
using CarManagerPhoneApp.Data;

namespace CarManagerPhoneApp
{
    public class CarData
    {
        private const string offLineRidesFileName = "offlinerides.bin";
        private const string EmergancyRideId = "EmergancyRideId";
        private const string DriverId = "DriverID";
        private readonly CarManagerApiClient _webServices;
        private readonly IsolatedStorageSettings _appSettings = IsolatedStorageSettings.ApplicationSettings;
        private bool emergencyRide = false;
        private DriveStatistics rideStatistics = new DriveStatistics();
        private DispatcherTimer _updateDataInterval;
        public delegate void exitConfirmed();
        
        private event exitConfirmed exit;

        // Data for local use
        public string SpeedString { get; set; }
        public string RpmString { get; set; }
        public string TemperatureString { get; set; }
        public string PressureString { get; set; }
        public bool IsHighTemp { get; set; }
        public bool IsHighRpm { get; set; }

        public List<int> SpeedList { get; private set; }

        public enum eUpdateStatus
        {
            Ok,
            NotUpdated
        };

        public int Speed { get; private set; }


        public float EngineCoolantTemperature { get; private set; }

        public int EngineRpm { get; private set; }

        private ObdConector _obdConector;
        private Guid _rideId;
        private bool _connectedToObd;
        private readonly TimeSpan _prepareTime = new TimeSpan(0, 0, 5);
        private readonly TimeSpan _updateTime = new TimeSpan(0, 0, 5);
        private notConnectedDelegation warningEvent;
        //private ArduinoCommunicator arduinoCommunicator;
        //public string CarId { get; private set; }
        public CarData(notConnectedDelegation warningMessage, exitConfirmed canExit)
        {
           // arduinoCommunicator = ArduinoCommunicator.Arduino();
            DataInitialize();
            fullDriveData = new List<DrivePackage>();
            _webServices = new CarManagerApiClient();
            ObdInitialize(warningMessage);
            exit = canExit;

            StartNewRide();
        }

        //private void getCarId()
        //{
        //    if (String.IsNullOrEmpty(App.CarId))
        //    {
        //        CarId = arduinoGetCarId();
        //    }
        //}

        //private string arduinoGetCarId()
        //{
        //    return "1";
        //    //arduino get carId
        //}

        private async void StartNewRide()
        {
            if (_appSettings.Contains(DriverId))
            {
                if (_appSettings.Contains(EmergancyRideId))
                {
                    _rideId = (Guid) _appSettings[EmergancyRideId];
                    emergencyRide = true;
                    ContinueInitialize();
                }
                else
                {

                    try
                    {
                        Guid driverIdGuid = (Guid) _appSettings[DriverId];
                        startDriveTime = DateTime.Now;
                        if (WindowsPhoneManager.checkNetworkConnection())
                        {
                            _webServices.StartNewRideAsync(driverIdGuid, startDriveTime, App.CarId);
                            _webServices.StartNewRideCompleted += _webServices_StartNewRideCompleted;
                        }
                        else
                        {
                            if (await arduinoApproval())
                            {
                                _rideId = Guid.NewGuid();
                            }
                            else
                            {
                                UnConnect();
                            }
                        }
                    }
                    catch (Exception)
                    {
                        throw new Exception("No Driver Id");
                    }
                }
            }
            else
            {
                throw new Exception("No Driver Id");
            }

        }

        private Task<bool> arduinoApproval()
        {
            throw new NotImplementedException();
        }

        private void _webServices_StartNewRideCompleted(object sender, StartNewRideCompletedEventArgs e)
        {
            if (e.Error == null && e.Result.HasValue)
            {
                _rideId = e.Result.Value;
                //arduinoStartRide();
                ContinueInitialize();
            }
            else
            {
                UnConnect();
            }
        }

        //private void arduinoStartRide() ////
        //{
        //    Guid driverId = (Guid) _appSettings[DriverId];
        //    var rideRequest = new RideRequest { driverId = driverId.ToString()};
        //    ArduinoCommunicator.CreatePackage(rideRequest);
        //    arduinoCommunicator.MainProcdure(rideRequest);
        //}

        private async void ContinueInitialize()
        {
            _connectedToObd = await _obdConector.connectToElm327() == eConnectionStatus.OK;
            IntervalInitialize();
        }

        private void ObdInitialize(notConnectedDelegation warningMessage)
        {
            _obdConector = ObdConector.OBD;
            warningEvent = warningMessage;
            _obdConector.obdNotFound += warningEvent;
        }

        private void DataInitialize()
        {
            SpeedList = new List<int>();
            needToUpdate = false;
        }

        private void IntervalInitialize()
        {
            _updateDataInterval = new DispatcherTimer { Interval = _prepareTime };
            _updateDataInterval.Tick += prepareInterval_Tick;
            _updateDataInterval.Start();
        }

        private void prepareInterval_Tick(object sender, EventArgs e)
        {
            _updateDataInterval.Stop();
            if (_connectedToObd)
            {
                _updateDataInterval = new DispatcherTimer { Interval = _updateTime };
                _updateDataInterval.Tick -= prepareInterval_Tick;
                _updateDataInterval.Tick += intervalUpdate_Tick;
            }
            _updateDataInterval.Start();
        }

        private async void intervalUpdate_Tick(object sender, EventArgs e)
        {
            _updateDataInterval.Stop();

            await UpdateValues();
            
            UpdateData();
            _updateDataInterval.Start();
        }

        private void UpdateData()
        {
            DrivePackage newPackage = new DrivePackage()
            {
                Speed = Speed,
                RPM = (int?)EngineRpm,
                EngineTemp = (int?)EngineCoolantTemperature,
                Time = DateTime.Now,
                RideId = _rideId
            };
            fullDriveData.Add(newPackage);
            if (WindowsPhoneManager.checkNetworkConnection())
            {
                _webServices.SendDrivePackageAsync(_rideId, newPackage);
            }
        }

        private void UpdateSpeed(int currentSpeed)
        {
            Speed = currentSpeed;
            SpeedList.Add(currentSpeed);
        }

        public async Task<eConnectionStatus> Connect()
        {
            if (_updateDataInterval != null)
            {
                _updateDataInterval.Stop();
            }
            eConnectionStatus res = await _obdConector.connectToElm327();
            _connectedToObd = res == eConnectionStatus.OK;
            if (_updateDataInterval != null)
            {
                _updateDataInterval.Start();
            }
            return res;
        }

        private Guid? getDriverId()
        {
            if (_appSettings.Contains(DriverId))
            {
                return (Guid)_appSettings[DriverId];
            }
            else
            {
                return null;
            }
        }


        public async Task UnConnect()
        {
            bool DataToSend = false;
            if (_updateDataInterval != null)
            {
                _updateDataInterval.Stop();
            }
            try
            {

                if (_obdConector != null)
                {
                    _obdConector.obdNotFound -= warningEvent;
                    _obdConector.UnConnect();
                }

                if (getDriverId().HasValue && fullDriveData.Count > 0)
                {
                    DataToSend = true;
                    rideStatistics = new DriveStatistics
                    {
                        DriverId = getDriverId().Value,
                        Data = new ObservableCollection<DrivePackage>(fullDriveData),
                        FinishDrive = DateTime.Now,
                        StartDrive = startDriveTime
                    };
                }
            }
            catch (InvalidOperationException)
            {
                rideStatistics = new DriveStatistics
                {
                    Data = new ObservableCollection<DrivePackage>(fullDriveData),
                    FinishDrive = DateTime.Now,
                    StartDrive = startDriveTime
                }; 
            }

            if (DataToSend)
            {
                if (WindowsPhoneManager.checkNetworkConnection())
                {
                    _webServices.SendRideStatisticsAsync(rideStatistics);
                    _webServices.SendRideStatisticsCompleted += _webServices_SendRideStatisticsCompleted;
                }
                else
                {
                    await saveToOfflineRides(new KeyValuePair<DriveStatistics, bool>(rideStatistics, emergencyRide));
                    exit();
                }
            }
        }

       private async void _webServices_SendRideStatisticsCompleted(object sender, SendRideStatisticsCompletedEventArgs e)
        {
            if (e.Error != null || e.Result == null)
            {
               await saveToOfflineRides(new KeyValuePair<DriveStatistics, bool>(rideStatistics, emergencyRide));
            }
            exit();
        }

       private async Task saveToOfflineRides(KeyValuePair<DriveStatistics, bool> offlineRideToSave)
        {
            Stack<KeyValuePair<DriveStatistics, bool>> oldRides = await IsolatedStorageOperations.Load<Stack<KeyValuePair<DriveStatistics, bool>>>(offLineRidesFileName);
            oldRides.Push(offlineRideToSave);
            oldRides.Save(offLineRidesFileName);

        }

        public List<DrivePackage> fullDriveData { get; set; }

        public bool IsConnected()
        {
            return _connectedToObd;
        }

        public async Task UpdateValues()
        {
            int? speedValue = await _obdConector.GetSpeedKmh();
            if (speedValue.HasValue)
                UpdateSpeed(speedValue.Value);

            int? engineCoolantTemperatureValue = await _obdConector.GetEngineTemp();
            if (engineCoolantTemperatureValue.HasValue)
                EngineCoolantTemperature = engineCoolantTemperatureValue.Value;

            int? engineRpmValue = await _obdConector.GetEngineRpm();
            if (engineRpmValue.HasValue)
                EngineRpm = engineRpmValue.Value;

            needToUpdate = true;
        }

        public DateTime startDriveTime { get; set; }

        public bool needToUpdate { get; set; }
    }
}
