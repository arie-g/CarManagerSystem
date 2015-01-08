using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Microsoft.Phone.Net.NetworkInformation;

namespace CarManagerPhoneApp.ConnectorManager
{
    public static class WindowsPhoneManager
    {
        public static bool checkNetworkConnection()
        {
            var ni = NetworkInterface.NetworkInterfaceType;

            bool IsConnected = false;
            if ((ni == NetworkInterfaceType.Wireless80211) || (ni == NetworkInterfaceType.MobileBroadbandCdma) ||
                (ni == NetworkInterfaceType.MobileBroadbandGsm))
            {
                IsConnected = true && IsInternet();
            }
            else if (ni == NetworkInterfaceType.None)
            {
                IsConnected = false;
            }
            return IsConnected;
        }

        private static bool IsInternet()
        {
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            bool internet = connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            return internet;
        }
    }
}
