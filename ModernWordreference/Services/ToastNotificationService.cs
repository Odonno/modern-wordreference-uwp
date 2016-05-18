using NotificationsExtensions.Toasts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace ModernWordreference.Services
{
    public interface IToastNotificationService
    {
        void SendText(string text, string tag = null);
        void SendTitleAndText(string title, string text, string tag = null);
    }

    public class ToastNotificationService : IToastNotificationService
    {
        public void SendText(string text, string tag = null)
        {
            // Create notification content
            var content = new ToastContent
            {
                Visual = new ToastVisual
                {
                    BodyTextLine1 = new ToastText { Text = text }
                },
                Duration = ToastDuration.Short
            };

            Show(content, tag, null, true);
        }

        public void SendTitleAndText(string title, string text, string tag = null)
        {
            // Create notification content
            var content = new ToastContent
            {
                Visual = new ToastVisual
                {
                    TitleText = new ToastText { Text = title },
                    BodyTextLine1 = new ToastText { Text = text }
                },
                Duration = ToastDuration.Short
            };

            Show(content, tag, null, true);
        }

        private void Show(ToastContent content, string tag = null, string group = null, bool shouldBeRemoved = false)
        {
            // Generate WinRT notification
            var xmlDocument = content.GetXml();
            var toast = new ToastNotification(xmlDocument);

            if (tag != null)
                toast.Tag = tag;
            if (group != null)
                toast.Group = group;
            if (shouldBeRemoved)
                toast.ExpirationTime = DateTimeOffset.Now.AddSeconds(5);

            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}
