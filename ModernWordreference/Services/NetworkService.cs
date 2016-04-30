using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace ModernWordreference.Services
{
    public interface INetworkService
    {
        bool IsInternetAvailable { get; }
    }

    public class NetworkService : INetworkService
    {
        public bool IsInternetAvailable { get; private set; }

        public NetworkService()
        {
            GetCurrentStatus();
            NetworkInformation.NetworkStatusChanged += (object sender) =>
            {
                GetCurrentStatus();
            };
        }

        private bool GetCurrentStatus()
        {
            var internetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();

            if (internetConnectionProfile == null)
            {
                IsInternetAvailable = false;
            }
            else
            {
                var networkConnectivityLevel = internetConnectionProfile.GetNetworkConnectivityLevel();
                IsInternetAvailable = (networkConnectivityLevel == NetworkConnectivityLevel.InternetAccess);
            }

            return IsInternetAvailable;
        }
    }
}
