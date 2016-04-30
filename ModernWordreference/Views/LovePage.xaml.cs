using ModernWordreference.Constants;
using ModernWordreference.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace ModernWordreference.Views
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class LovePage : Page
    {
        #region Properties

        public Models.IapBuyed IapBuyed { get; set; }

        #endregion

        #region Contructor

        public LovePage()
        {
            this.InitializeComponent();

            // Set navigation system
            var systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        #endregion

        #region Events

        private async void RateApp_Click(object sender, RoutedEventArgs e)
        {
            // Rate the app
            var currentPackage = Windows.ApplicationModel.Package.Current;
            var packageFamilyName = currentPackage.Id.FamilyName;

            var uriToLaunch = "ms-windows-store:REVIEW?PFN=" + packageFamilyName;
            var uri = new Uri(uriToLaunch);
            var options = new LauncherOptions();
            options.TreatAsUntrusted = true;

            bool result = await Launcher.LaunchUriAsync(uri, options);

            if (result)
            {
                // Send telemetry
                ServiceFactory.Analytics.TrackEvent("RateApp");
            }
        }

        private async void BuyWord_Click(object sender, RoutedEventArgs e)
        {
            await BuyIapAsync("theWord");
        }

        private async void BuyPage_Click(object sender, RoutedEventArgs e)
        {
            await BuyIapAsync("thePage");
        }

        private async void BuyBook_Click(object sender, RoutedEventArgs e)
        {
            await BuyIapAsync("theBook");
        }

        #endregion

        #region Logic

        private void Initialize()
        {
            // Retrieve already purchased items
            IapBuyed = ServiceFactory.Storage.Retrieve<Models.IapBuyed>(StorageConstants.IapBuyed);
            if (IapBuyed == null)
            {
                IapBuyed = new Models.IapBuyed();
            }

            UpdateIapUI();
        }

        private async Task BuyIapAsync(string name)
        {
            // Send telemetry
            ServiceFactory.Analytics.TrackEvent("StartBuyIAP", new Dictionary<string, string> { { "iapName", name } });

#if DEBUG
            // Use simulator info
            var licenseInformation = CurrentAppSimulator.LicenseInformation;

            // Buy IAP
            var requestPurchase = await CurrentAppSimulator.RequestProductPurchaseAsync(name);
#else
            // Use release licensing info
            var licenseInformation = CurrentApp.LicenseInformation;

            // Buy IAP
            var requestPurchase = await CurrentApp.RequestProductPurchaseAsync(name);
#endif

            var productLicense = licenseInformation.ProductLicenses[name];
            if (productLicense != null && productLicense.IsActive)
            {
                // Show toast notification (thanks to support us)
                ServiceFactory.ToastNotification.SendTitleAndText("Thank you !", "Thank you to support us !", "purchase");

                // Save buyed IAP
                if (name == "theWord")
                {
                    IapBuyed.Word++;
                }
                if (name == "thePage")
                {
                    IapBuyed.Page++;
                }
                if (name == "theBook")
                {
                    IapBuyed.Book++;
                }

                UpdateIapUI();
                ServiceFactory.Storage.Save(StorageConstants.IapBuyed, IapBuyed);

                // Send telemetry
                ServiceFactory.Analytics.TrackEvent("SuccessBuyIAP", new Dictionary<string, string> { { "iapName", name } });
            }
            else
            {
                // Show toast notification (something going wrong)
                ServiceFactory.ToastNotification.SendTitleAndText("Purchase failed", "Something is going wrong with your purchase...", "purchase");
            }
        }

        #endregion

        #region UI

        private void UpdateIapUI()
        {
            if (IapBuyed.Word > 0 || IapBuyed.Page > 0 || IapBuyed.Book > 0)
            {
                var sb = new StringBuilder();

                sb.Append("You bought ");

                if (IapBuyed.Word > 0)
                {
                    sb.Append($" {IapBuyed.Word} word");
                    if (IapBuyed.Word > 1)
                        sb.Append("s");
                    if (IapBuyed.Page > 0)
                        sb.Append(",");
                }

                if (IapBuyed.Page > 0)
                {
                    sb.Append($" {IapBuyed.Page} page");
                    if (IapBuyed.Page > 1)
                        sb.Append("s");
                    if (IapBuyed.Book > 0)
                        sb.Append(",");
                }

                if (IapBuyed.Book > 0)
                {
                    sb.Append($" {IapBuyed.Book} book");
                    if (IapBuyed.Book > 1)
                        sb.Append("s");
                }

                sb.Append("! Thank you very much.");

                ThankYouText.Text = sb.ToString();;
                ThankYouText.Visibility = Visibility.Visible;
            }
            else
            {
                ThankYouText.Text = string.Empty;
                ThankYouText.Visibility = Visibility.Collapsed;
            }
        }

        #endregion
    }
}
