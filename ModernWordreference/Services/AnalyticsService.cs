using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernWordreference.Services
{
    public interface IAnalyticsService
    {
        void TrackEvent(string eventName);
        void TrackEvent(string eventName, Dictionary<string, string> properties);
        void TrackEvent(string eventName, Dictionary<string, string> properties, Dictionary<string, double> metrics);
    }

    public class AnalyticsService : IAnalyticsService
    {
        private TelemetryClient _client;

        public AnalyticsService()
        {
            _client = new TelemetryClient();
        }

        public void TrackEvent(string eventName)
        {
            _client.TrackEvent(eventName);
        }

        public void TrackEvent(string eventName, Dictionary<string, string> properties)
        {
            _client.TrackEvent(eventName, properties);
        }

        public void TrackEvent(string eventName, Dictionary<string, string> properties, Dictionary<string, double> metrics)
        {
            _client.TrackEvent(eventName, properties, metrics);
        }
    }
}
