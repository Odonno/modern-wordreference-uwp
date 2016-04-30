using Microsoft.Services.Store.Engagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Profile;

namespace ModernWordreference.Services
{
    public interface IFeatureToggleService
    {
        bool IsNotificationBackgroundTasksEnabled();
        bool UseFeedbackHubApp();
        bool IsRunningWindowsMobile();
    }

    public class FeatureToggleService : IFeatureToggleService
    {
        // Deployment Toggles
        public bool IsNotificationBackgroundTasksEnabled()
        {
            return true;
        }

        // Runtime Toggles
        public bool UseFeedbackHubApp()
        {
            return Feedback.IsSupported;
        }

        public bool IsRunningWindowsMobile()
        {
            return AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile";
        }

        // Permission Toggles

        // User settings Toggles

        // Experiment Toggles
    }
}
