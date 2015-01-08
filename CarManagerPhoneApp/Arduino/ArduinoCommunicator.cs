using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Windows.Networking;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using CarManagerPhoneApp.CarManagerService;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Reactive;
using Newtonsoft.Json;

namespace CarManagerPhoneApp.Arduino
{
    /// <summary>
    /// Class to control the bluetooth connection to the Arduino.
    /// </summary>

    public class ArduinoCommunicator
    {
        private const string arduinoDisplay = "4A30";
        private static ArduinoCommunicator _arduinoInstance;
        private string arduinoMsg = string.Empty;
        private ConnectionManager arduinoConnector { get; set; }
        private readonly Mutex mutexLocker = new Mutex();
        private static readonly object locker = new object();

        public Action<string> GetCarIdComplete { get; set; }
        public Action<string> AddPerComplete { get; set; }
        public Action<string> AddEmerComplete { get; set; }
        public Action<string> rideRequestCoplete { get; set; }
        public Action<string> getRidesComplete { get; set; }
        public Action<string> deletePerListComplete { get; set; }
        public Action<string> deleteEmerListComplete { get; set; }
        public Action<string> IsEmergencyyAllowedToDriveComplete { get; set; }

        public static readonly string startFix = OperationToSend.StartMessage.Substring(0, 1);
        public static readonly string endFix = OperationToSend.EndMessage.Substring(0, 1);
       public static readonly string addPerCommand = OperationToSend.PermanentDriverInitString.Substring(0, 2); 
       public static readonly string addEmCommand = OperationToSend.EmergencyDriverInitString.Substring(0, 2);
       public static readonly string rideRequestCommand = OperationToSend.RideRequestInitString.Substring(0, 2);
       public static readonly string getRidesCommand = OperationToSend.GetRidesInitString.Substring(0, 2); 
       public static readonly string getCarIdCommand = OperationToSend.GetCarIdInitString.Substring(0, 2);
       public static readonly string deletePerListCommand = OperationToSend.DelPerListInitString.Substring(0, 2); 
       public static readonly string deleteEmerListCommand = OperationToSend.DelEmerListInitString.Substring(0, 2);
       public static readonly string IsEmergencyyAllowedToDriveCommand = OperationToSend.IsEmerAllowedInitString.Substring(0, 2);
       public static readonly string BlankCommand = OperationToSend.PerCountInitString.Substring(0, 2);
       private static bool terminated; 

        public static ArduinoCommunicator Arduino()
        {

            if (_arduinoInstance == null)
            {
                lock (locker)
                {
                    if (_arduinoInstance == null)
                    {
                        _arduinoInstance = new ArduinoCommunicator();
                    }
                }
            }
            terminated = false;
            return _arduinoInstance;
        }

        public void UnConnect()
        {
            terminated = true;
            arduinoConnector.Terminate();
        }

        public bool IsConnected()
        {
            return arduinoConnector.IsConnected();
        }
        private ArduinoCommunicator()
        {
            arduinoConnector = new ConnectionManager();
            arduinoConnector.Initialize();
            arduinoConnector.MessageReceived += arduinoConnector_MessageReceived;
        }

        private void arduinoConnector_MessageReceived(string message)
        {
        
            string recievedMsg = string.Empty;
            arduinoMsg += message;
            if (arduinoMsg.Contains(endFix))
            {
                int startIndex = 0;
                arduinoMsg = arduinoMsg.Replace(startFix, string.Empty);
                recievedMsg = arduinoMsg.Substring(0, arduinoMsg.IndexOf(endFix));
                arduinoMsg = arduinoMsg.Replace(endFix, string.Empty);
                arduinoMsg = arduinoMsg.Substring(recievedMsg.Length);
            }
            arduinoMsg = arduinoMsg.Trim();
            if (!String.IsNullOrEmpty(recievedMsg))
            {

                if (recievedMsg.Contains(BlankCommand))
                {
                }
                else
                {
                    UnConnect();

                    if (recievedMsg.Contains(addPerCommand))
                    {
                        if (AddPerComplete != null)
                        {
                            AddPerComplete(recievedMsg);
                        }
                    }
                    else if (recievedMsg.Contains(addEmCommand))
                    {
                        if (AddEmerComplete != null)
                        {
                            AddEmerComplete(recievedMsg);
                        }
                    }
                    else if (recievedMsg.Contains(rideRequestCommand))
                    {
                        if (rideRequestCoplete != null)
                        {
                            rideRequestCoplete(recievedMsg);
                        }
                    }
                    else if (recievedMsg.Contains(IsEmergencyyAllowedToDriveCommand))
                    {
                        if (IsEmergencyyAllowedToDriveComplete != null)
                        {
                            IsEmergencyyAllowedToDriveComplete(recievedMsg);
                        }
                    }
                    else if (recievedMsg.Contains(getRidesCommand))
                    {
                        if (getRidesComplete != null)
                        {
                            getRidesComplete(recievedMsg);
                        }

                    }
                    else if (recievedMsg.Contains(getCarIdCommand))
                    {
                        if (GetCarIdComplete != null)
                        {
                            GetCarIdComplete(recievedMsg);
                        }
                    }
                    else if (recievedMsg.Contains(deletePerListCommand))
                    {
                        if (deletePerListComplete != null)
                        {
                            deletePerListComplete(recievedMsg);
                        }
                    }
                    else if (recievedMsg.Contains(deleteEmerListCommand))
                    {
                        if (deleteEmerListComplete != null)
                        {
                            deleteEmerListComplete(recievedMsg);
                        }
                    }
                    else
                    {

                    }
                }
                arduinoMsg = string.Empty;
            }
        }

        public async Task connectToArduino()
        {
            if (!terminated)
            {
                try
                {
                    PeerFinder.AlternateIdentities["Bluetooth:PAIRED"] = "";

                    var foundServices = await PeerFinder.FindAllPeersAsync();
                    if (foundServices.Any(u => u.DisplayName.Contains(arduinoDisplay)))
                    {
                        var pairedDevice = foundServices.First(u => u.DisplayName.Contains(arduinoDisplay));
                        await arduinoConnector.Connect(pairedDevice.HostName);
                    }
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }
        }

        public bool IsPermanentAllowedToDrive()
        {
            bool result = false;

            return result;
        }

        public bool IsEmergencyyAllowedToDrive()
        {
            bool result = false;

            return result;
        }

        public async Task AddNewPermanentDriver(PermanentDriver driver)
        {

            string json = CreatePackage(driver);

            await SendCommand(json);
        }

        public async Task AddNewEmergencyDriver(EmergencyDriver driver)
        {
            string json = CreatePackage(driver);
            await SendCommand(json);
        }

        private async Task<string> SendPush(string json)
        {
            if (!terminated)
            {
                if (!IsConnected())
                {
                    return string.Empty;
                }
                arduinoConnector.SendCommand(json);
            }
            return string.Empty;
        }

        private async Task SendCommand(string json)
        {
            if (!terminated)
            {
                try
                {
                    if (!IsConnected())
                    {
                        await connectToArduino();
                    }
                    arduinoConnector.SendCommand(json);
                    for (int i = 0; i < 2; i++)
                    {
                        await Task.Delay(5000);
                        PushBlank();
                    }
                }
                catch (Exception ex)
                {
                    ExitWithMsg(ex.Message);
                }
            }
        }

        private void ExitWithMsg(string msg)
        {
            if (msg.ToLower().Contains("imubilizer"))
            {
                MessageBox.Show("imubilizer problem - please connect the customer service", "Imubilizer Error",
                    MessageBoxButton.OK);
            }
            else
            {
                MessageBox.Show("Please connect the customer service", "Critical Error",
                    MessageBoxButton.OK);
            }
            Application.Current.Terminate();

        }

        public async Task DeletePermanent(DeletePermanentList list)
        {
            string json = CreatePackage(list);
            await SendCommand(json);
        }

        public async Task PushBlank()
        {
            var permanentDriversCount = new GetPermaNum { };
            GetPermaNum(permanentDriversCount);
        }

        public List<Ride> ConvertToRideList(string rides)
        {
            var deserializedObject = JsonConvert.DeserializeObject<RootObject>(rides);

            List<Ride> rideList = new List<Ride>();
            foreach (var ride in deserializedObject.rides)
            {
                Ride currentRide = new Ride();
                currentRide.CarID = new Guid(ride.cId);
                currentRide.DriverID = new Guid(ride.dId);
                currentRide.StartDrive = Convert.ToDateTime(ride.time);

                rideList.Add(currentRide);
            }
            return rideList;
        }

        public async Task GetPermaNum(GetPermaNum permanentDriversCount)
        {
            string json = CreatePackage(permanentDriversCount);
            await SendPush(json);
        }

        public async Task DeleteEmergency(DeleteEmergencyList list)
        {
            string json = CreatePackage(list);
            await SendCommand(json);
        }

        //public string AddNewRide(string id ,string carId , string date,string time)
        public async Task AddNewRide(NewRide ride)
        {
            string json = CreatePackage(ride);
            await SendCommand(json);
        }

        public async Task IsEmergencyAllowd(IsEmerAllowedToDrive isEmerAllowed)
        {
            string json = CreatePackage(isEmerAllowed);
             await SendCommand(json);
        }

        public async Task GetRidesList(GetRidesList ridesList)
        {
            string json = CreatePackage(ridesList);
            await SendCommand(json);
        }

        public async Task MainProcdure(RideRequest rideRequest)
        {
            string json = CreatePackage(rideRequest);
            await SendCommand(json);
        }

        public static string CreatePackage(object driver)
        {
            string json = JsonConvert.SerializeObject(driver);
            json = "&&&" + json + "$$$";
            return json;
        }
        public  async Task  GetCarId(GetCarId getCarId)
        {
            string json = CreatePackage(getCarId);
            await SendCommand(json);
        }
    }
}
