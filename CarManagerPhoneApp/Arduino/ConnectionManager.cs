using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace CarManagerPhoneApp.Arduino
{
    public class ConnectionManager
    {
        private readonly object lockKey = new object();
        /// <summary>
        /// Socket used to communicate with Arduino.
        /// </summary>
        private StreamSocket socket;

        /// <summary>
        /// DataWriter used to send commands easily.
        /// </summary>
        private DataWriter dataWriter;

        /// <summary>
        /// DataReader used to receive messages easily.
        /// </summary>
        private DataReader dataReader;

        /// <summary>
        /// Thread used to keep reading data from socket.
        /// </summary>
        private BackgroundWorker dataReadWorker;

        /// <summary>
        /// Delegate used by event handler.
        /// </summary>
        /// <param name="message">The message received.</param>
        public delegate void MessageReceivedHandler(string message);

        /// <summary>
        /// Event fired when a new message is received from Arduino.
        /// </summary>
        public event MessageReceivedHandler MessageReceived;
        private bool connectingProccesss = false;
        private bool connected = false;

        /// <summary>
        /// Initialize the manager, should be called in OnNavigatedTo of main page.
        /// </summary>
        public void Initialize()
        {
            socket = new StreamSocket();
            dataReadWorker = new BackgroundWorker();
            dataReadWorker.WorkerSupportsCancellation = true;
            dataReadWorker.DoWork += new DoWorkEventHandler(ReceiveMessages);
        }

        /// <summary>
        /// Finalize the connection manager, should be called in OnNavigatedFrom of main page.
        /// </summary>
        public void Terminate()
        {
            connected = false;
            dataReadWorker.DoWork -= new DoWorkEventHandler(ReceiveMessages);

            if (socket != null)
            {
                socket.Dispose();
                socket = null;
            }
            if (dataReadWorker != null)
            {
                dataReadWorker.CancelAsync();
                
            }
        }

        /// <summary>
        /// Connect to the given host device.
        /// </summary>
        /// <param name="deviceHostName">The host device name.</param>
        public async Task Connect(HostName deviceHostName)
        {
            try
            {
                if (!connectingProccesss && !connected)
                {
                    if (socket == null)
                    {
                        Initialize();
                    }
                    if (socket != null)
                    {
                        connectingProccesss = true;
                        await socket.ConnectAsync(deviceHostName, "1");
                        dataReader = new DataReader(socket.InputStream);
                        dataReadWorker.RunWorkerAsync();
                        dataWriter = new DataWriter(socket.OutputStream);
                        connected = dataWriter != null;
                        connectingProccesss = false;
                    }
                }
            }
            catch (Exception ex)
            {
                socket = null;
                dataReader = null;
                dataWriter = null;
                dataReadWorker.CancelAsync();
                throw new Exception("imubilizer Can't be connected");
            }
        }



        //private async void ReceiveMessages2(object sender, DoWorkEventArgs e)
        //{
        //    try
        //    {
        //        const string startCode = "START";
        //        const string endCode = "END";
        //        string msgRead = string.Empty;
        //        string msgSend, tmpMsg;
        //        while (true)
        //        {

        //            while (!(msgRead.Contains(startCode) || msgRead.Contains(endCode)))
        //            {
        //                msgRead += await ReadFromArduino();
        //            }
        //            while (!msgRead.Contains(endCode))
        //            {
        //                msgRead += await ReadFromArduino();
        //            }
        //            //int startIndex = msgRead.IndexOf(startCode) + startCode.Length;
        //            int endIndex = msgRead.IndexOf(endCode) ;
        //            if (endIndex > 0 && endIndex < msgRead.Length - endCode.Length)
        //            {
        //                msgSend = msgRead.Substring(0, endIndex);
        //                msgRead = msgRead.Substring(endIndex + endCode.Length);
        //            }
        //            else if (endIndex > 0)
        //            {
        //                msgSend = msgRead.Substring(0, endIndex);
        //                msgRead = string.Empty;
        //            }
        //            else
        //            {
        //                msgSend = null;
        //                msgRead = null;
        //            }

        //            MessageReceived(msgSend);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex.Message);
        //    }

        //}

        //private async Task<string> ReadFromArduino()
        //{
        //    uint sizeFieldCount = await dataReader.LoadAsync(1);
        //    if (sizeFieldCount != 1)
        //    {
        //        // The underlying socket was closed before we were able to read the whole data. 
        //        return null;
        //    }

        //    uint messageLength = dataReader.ReadByte();
        //    uint actualMessageLength = await dataReader.LoadAsync(messageLength);
        //    if (messageLength != actualMessageLength)
        //    {
        //        // The underlying socket was closed before we were able to read the whole data. 
        //        return null;
        //    }

        //    return dataReader.ReadString(actualMessageLength);
        //}

        /// <summary>
        /// Receive messages from the Arduino through bluetooth.
        /// </summary>
        private async void ReceiveMessages(object sender, DoWorkEventArgs e)
        {
            //try
            //{
            //    while (true)
            //    {
            //        await readFromArduino();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.Message);
            //}

        }

        private async Task<string> readFromArduino()
        {
            // Read first byte (length of the subsequent message, 255 or less). 
            uint sizeFieldCount = await dataReader.LoadAsync(1);
            if (sizeFieldCount != 1)
            {
                // The underlying socket was closed before we were able to read the whole data. 
                return string.Empty;
            }

            // Read the message. 
            uint messageLength = dataReader.ReadByte();
            uint actualMessageLength = await dataReader.LoadAsync(messageLength);
            if (messageLength != actualMessageLength)
            {
                // The underlying socket was closed before we were able to read the whole data. 
                return string.Empty;
            }
            // Read the message and process it.
            string msg = dataReader.ReadString(actualMessageLength);
            MessageReceived(msg);
            return msg;
        }



        /// <summary>
        /// Send command to the Arduino through bluetooth.
        /// </summary>
        /// <param name="command">The sent command.</param>
        /// <returns>The number of bytes sent</returns>
        public async Task<string> SendCommand(string command)
        {
                uint sentCommandSize = 0;
                if (dataWriter != null)
                {
                    uint commandSize = dataWriter.MeasureString(command);
                    dataWriter.WriteByte((byte) commandSize);
                    sentCommandSize = dataWriter.WriteString(command);
                    await dataWriter.StoreAsync();
                    await socket.OutputStream.FlushAsync();
                    return await readFromArduino();
                }
                return string.Empty;
        }

        public bool IsConnected()
        {
            return connected;
        }
    }
}