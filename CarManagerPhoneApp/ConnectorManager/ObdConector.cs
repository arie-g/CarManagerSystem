using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;



namespace CarManagerPhoneApp.ConnectorManager
{
    public delegate Task<eConnectionStatus> notConnectedDelegation();


    public class ObdConector
    {
        private static ObdConector m_Instance; // singelton

        private static readonly object lockKey = new object();
        public StreamSocket _socket { get; private set; }
        public DataWriter _dataWriter { get; private set; }
        public DataReader _dataReader { get; private set; }
        private DispatcherTimer _checkConnectionInterval;
        private bool _needToUpdate;


        public event notConnectedDelegation obdNotFound;

        private Dictionary<ePid, string> k_ObdPID = new Dictionary<ePid, string> {
           {ePid.speed,"01 0D\r"},
           {ePid.engineCoolantTemperature,"01 05\r"},
           {ePid.engineRPM,"01 0C\r"},
        };

        private ObdConector()
        {
            initializeConnectionInterval();
        }

        private void initializeConnectionInterval()
        {
            _checkConnectionInterval = new DispatcherTimer();
            _checkConnectionInterval.Tick += _checkConnectionInterval_Tick;
            _checkConnectionInterval.Interval = new TimeSpan(0,0,10);
            _checkConnectionInterval.Start();
        }

        private async void _checkConnectionInterval_Tick(object sender, EventArgs e)
        {
            _checkConnectionInterval.Stop();
            if (!isConnected())
            {
               await UpdateNotConnectedBluetooth();
            }
            _checkConnectionInterval.Start();
        }

        public static ObdConector OBD
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (lockKey)
                    {
                        if (m_Instance == null)
                        {
                            m_Instance = new ObdConector();
                        }
                    }
                }
                return m_Instance;
            }
        }

        public enum ePid
        {
            speed = 0,
            engineCoolantTemperature = 1,
            engineRPM = 2
        };

        private void initializeDataRW()
        {
            if (_socket != null)
            {
                _needToUpdate = true;
                _dataWriter = new DataWriter(_socket.OutputStream);
                _dataReader = new DataReader(_socket.InputStream);
            }
        }

        public async Task<eConnectionStatus> connectToElm327()
        {
            eConnectionStatus resStatus = eConnectionStatus.NotConnected;
            try
            {
                PeerFinder.AlternateIdentities["Bluetooth:SDP"] = "{00001101-0000-1000-8000-00805F9B34FB}";
      
                var foundServices = await PeerFinder.FindAllPeersAsync();

                if (!foundServices.Any(u => u.DisplayName.Contains("OBD")))
                {
                    _socket = null;
                    resStatus = eConnectionStatus.NotConnected;
                }
                else
                {
                    var selectedDevice = foundServices.FirstOrDefault(u=> u.DisplayName.Contains("OBD"));
                    if (selectedDevice != null)
                    {
                        _socket = new StreamSocket();
                         _socket.Dispose();
                        _socket = new StreamSocket();

                        await _socket.ConnectAsync(selectedDevice.HostName, "16");
                        initializeDataRW();

                        await initializeElm327();
                    }
                    if (_socket != null)
                    {
                        resStatus = eConnectionStatus.OK;
                    }
                    else
                    {
                        resStatus = eConnectionStatus.NotConnected;
                    }
                }
            }
            catch (Exception ex)
                    {
                        if ((uint) ex.HResult == 0x8007048F)
                        {
                            MessageBox.Show("Please turn on the bluetooth in the phone");
                            Debug.WriteLine("Bluetooth off");
                        }
                        Debug.WriteLine("Exception {0}", ex.Message);
                        _socket = null;
                    }

            return resStatus;
        }

        private async Task<eConnectionStatus> UpdateNotConnectedBluetooth()
        {
            if (obdNotFound != null)
                return await obdNotFound();
            return eConnectionStatus.NotConnected;
        }

        public async Task<int?> GetSpeedKmh()
        {
            if (_needToUpdate)
            {
                int? retVal = null;
                if (!isConnected())
                {
                    await UpdateNotConnectedBluetooth();
                    return null;
                }
                try
                {
                    Debug.WriteLine("Speed: ");
                    Task<string> getDataFromOBD = getValue(ePid.speed);
                    return convertToSpeed(await getDataFromOBD);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Exception in Speed: {0}\n {1}", e.ToString(), e.Message);
                }
                return retVal;
            }
            else
            {
                return null;
            }
        }

        private int? convertToSpeed(string i_Data)
        {
            int? retVal = null;
            string crop = null;
            if (i_Data != null && i_Data.Length >= 2)
            {
                int indexBegin = i_Data.Length > 2 ? i_Data.Length - 2 : 0;
                crop = i_Data.Substring(indexBegin, 2);
                try
                {
                    retVal = (int?)Convert.ToInt32(crop, 16);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error converting {0} to value\n {1}", crop, ex.Message);
                }
            }
            return retVal;
        }

        public bool isConnected()
        {
            return _socket != null;
        }

        public async Task<int?> GetEngineTemp()
        {
            if (!isConnected())
            {
                await UpdateNotConnectedBluetooth();
                return null;
            }
            Debug.WriteLine("Engine Temp");
            Task<string> getDataFromOBD = getValue(ePid.engineCoolantTemperature);
            return convertToEngineTemp(await getDataFromOBD);
        }

        private int? convertToEngineTemp(string i_Data)
        {
            string crop = null;
            int? retVal = null;
            if (i_Data != null)
            {
                int indexBegin = i_Data.Length > 2 ? i_Data.Length - 2 : 0;
                crop = i_Data.Substring(indexBegin, 2);
                try
                {
                    retVal = (int?)Convert.ToInt32(crop, 16) - 40;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error converting {0} to value\n {1}", crop, ex.Message);
                }
            }
            return retVal;

        }
        public async Task<int?> GetEngineRpm()
        {
            if (_needToUpdate)
            {
                if (!isConnected())
                {
                    await UpdateNotConnectedBluetooth();
                    return null;
                }
                Debug.WriteLine("Engine Rpm");
                Task<string> getDataFromOBD = getValue(ePid.engineRPM);
                int? retVal = convertToRPM(await getDataFromOBD);
                return retVal;
            }
            return null;
        }

        private int? convertToRPM(string i_Data)
        {
            if (_needToUpdate)
            {
                int? retVal = null;
                string crop = i_Data;

                if (i_Data != null)
                {
                    int indexBeginFirst = i_Data.Length > 4 ? i_Data.Length - 4 : 0;
                    int indexBeginSecond = i_Data.Length > 2 ? i_Data.Length - 2 : 2;
                    int data1 = Convert.ToInt32((crop.Substring(indexBeginFirst, 2)).PadLeft(2, '0'), 16);
                    int data2 = Convert.ToInt32((crop.Substring(indexBeginSecond, 2)).PadLeft(2, '0'), 16);
                    try
                    {
                        retVal = (int?) ((data1*256) + data2)/4;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error converting {0} to value\n {1}", crop, ex.Message);
                    }
                }
                return retVal;
            }
            else
            {
                return null;
            }
        }

        private async Task<eConnectionStatus> initializeElm327()
        {
            uint maxBytes = 255;
            String Message;
            const int k_DelayBetweenMessages = 2;
            const int k_DelayReciveInput = 500;


            await Task.Delay(k_DelayBetweenMessages);
            _dataWriter.WriteString("atz\r");
            await _dataWriter.StoreAsync();

            await Task.Delay(k_DelayBetweenMessages);
            _dataReader.InputStreamOptions = InputStreamOptions.Partial;
            maxBytes = await _dataReader.LoadAsync(maxBytes);  
            Message = _dataReader.ReadString(maxBytes);

            await Task.Delay(k_DelayBetweenMessages);
            _dataWriter.WriteString("ate0\r");
            await _dataWriter.StoreAsync();

            await Task.Delay(k_DelayBetweenMessages);
            maxBytes = 255;
            _dataReader.InputStreamOptions = InputStreamOptions.Partial;
            maxBytes = await _dataReader.LoadAsync(maxBytes);  
            Message = _dataReader.ReadString(maxBytes);

            await Task.Delay(k_DelayReciveInput);
            _dataWriter.WriteString("atl0\r");
            await _dataWriter.StoreAsync();
            await Task.Delay(k_DelayBetweenMessages);
            maxBytes = 255;
            _dataReader.InputStreamOptions = InputStreamOptions.Partial;
            maxBytes = await _dataReader.LoadAsync(maxBytes);  
            Message = _dataReader.ReadString(maxBytes);

            await Task.Delay(k_DelayBetweenMessages);
            _dataWriter.WriteString("ath0\r");
            await _dataWriter.StoreAsync();

            await Task.Delay(k_DelayBetweenMessages);
            maxBytes = 255;
            _dataReader.InputStreamOptions = InputStreamOptions.Partial;
            maxBytes = await _dataReader.LoadAsync(maxBytes); 
            Message = _dataReader.ReadString(maxBytes);

            await Task.Delay(k_DelayBetweenMessages);
            _dataWriter.WriteString("atsp 3\r");
            await _dataWriter.StoreAsync();
            await Task.Delay(k_DelayBetweenMessages);
            _dataReader.InputStreamOptions = InputStreamOptions.Partial;
            maxBytes = await _dataReader.LoadAsync(maxBytes); 
            Message = _dataReader.ReadString(maxBytes);

            return eConnectionStatus.OK;
        }

        private async Task<string> getValue(ePid i_Pid)
        {
            const string errorMessage = "NODATA";
            uint maxBytes = 255;
            string code = k_ObdPID[i_Pid].Substring(3, 2);
            _dataWriter.WriteString(k_ObdPID[i_Pid]); 
            await _dataWriter.StoreAsync();
            String message = string.Empty;

            do
            {
                _dataReader.InputStreamOptions = InputStreamOptions.Partial;
                maxBytes = await _dataReader.LoadAsync(maxBytes);
                message += _dataReader.ReadString(maxBytes);
            } while (!message.Contains('>'));

            if (message.Contains(code))
            {
                int index = message.IndexOf(code);
                message = message.Substring(index + 2);
                message = message.Replace("\r", string.Empty);
                message = message.Replace(">", string.Empty);
                message = message.Replace(" ", string.Empty);
                if (message.Contains(errorMessage))
                    message = null;
            }
            else
            {
                Debug.WriteLine("BAD INPUT OF {0} IS: {1}", i_Pid, message);
                message = null;
            }
            Debug.WriteLine("Time: {0}, PinCode: {1}, Result: {2}", DateTime.Now, k_ObdPID[i_Pid], message);
            return message;
        }

        public void UnConnect()
        {
            _checkConnectionInterval.Stop();
            _needToUpdate = false;
            _socket = null;
            _dataReader = null;
            _dataWriter = null;
        }
    }
    public enum eConnectionStatus
    {
        OK,
        NotConnected
    };

}


