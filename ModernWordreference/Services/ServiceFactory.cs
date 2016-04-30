using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernWordreference.Services
{
    public static class ServiceFactory
    {
        private static IAnalyticsService _analytics;
        public static IAnalyticsService Analytics
        {
            get { return _analytics ?? (_analytics = new AnalyticsService()); }
        }

        private static IApiService _api;
        public static IApiService Api
        {
            get
            {
                if (_api == null)
                    _api = new NativeApiService();
                return _api;
            }
        }

        private static IDictionaryService _dictionary;
        public static IDictionaryService Dictionary
        {
            get
            {
                if (_dictionary == null)
                {
                    _dictionary = new DictionaryService();
                    _dictionary.Initialize();
                }
                return _dictionary;
            }
        }

        private static IFeatureToggleService _featureToggle;
        public static IFeatureToggleService FeatureToggle
        {
            get { return _featureToggle ?? (_featureToggle = new FeatureToggleService()); }
        }

        private static INetworkService _network;
        public static INetworkService Network
        {
            get { return _network ?? (_network = new NetworkService()); }
        }

        private static IRoamingStorageService _storage;
        public static IRoamingStorageService Storage
        {
            get { return _storage ?? (_storage = new RoamingStorageService()); }
        }

        private static IToastNotificationService _toastNotification;
        public static IToastNotificationService ToastNotification
        {
            get { return _toastNotification ?? (_toastNotification = new ToastNotificationService()); }
        }
    }
}
