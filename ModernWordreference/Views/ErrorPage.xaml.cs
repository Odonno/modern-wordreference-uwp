using ModernWordreference.Constants;
using ModernWordreference.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
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
    public sealed partial class ErrorPage : Page
    {
        #region Fields

        private readonly ResourceLoader _resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        #endregion

        #region Properties

        public Models.TranslationResult LastTranslation { get; set; }

        #endregion

        #region Constructor

        public ErrorPage()
        {
            this.InitializeComponent();

            // Set navigation system
            var systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;

            // Back to the app when there is a network connection now
            NetworkInformation.NetworkStatusChanged += HandleBackToApp;

            // Show last translation
            ShowLastTranslation();
        }

        #endregion

        #region Logic

        private async void HandleBackToApp(object sender)
        {
            if (ServiceFactory.Network.IsInternetAvailable)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher
                    .RunAsync(CoreDispatcherPriority.High, () =>
                    {
                        NetworkInformation.NetworkStatusChanged -= HandleBackToApp;
                        Frame.GoBack();
                    });
            }

        }

        private async void ShowLastTranslation()
        {
            // Retrieve last saved translation
            LastTranslation = await ServiceFactory.Storage.RetrieveFileAsync<Models.TranslationResult>(StorageConstants.LastTranslation);
            if (LastTranslation != null)
            {
                UpdateLastTranslationUI();
            }
        }

        #endregion

        #region UI

        private void UpdateLastTranslationUI()
        {
            // Set group key in each translation line
            var primaryTranslationsGroup = LastTranslation.PrimaryTranslations
                .GroupBy(_ => _resourceLoader.GetString("PrimaryTranslations"));
            var additionalTranslationsGroup = LastTranslation.AdditionalTranslations
                .GroupBy(_ => _resourceLoader.GetString("AdditionalTranslations"));
            var compoundFormsTranslationsGroup = LastTranslation.CompoundForms
                .GroupBy(_ => _resourceLoader.GetString("CompoundForms"));

            // Fill collection view source with data
            TranslationResultSource.Source = primaryTranslationsGroup
                .Concat(additionalTranslationsGroup)
                .Concat(compoundFormsTranslationsGroup);

            // Refresh UI info too
            WordTranslatedText.Text = LastTranslation.OriginalWord;
            if (LastTranslation.Pronunciation != null)
                PronunciationText.Text = LastTranslation.Pronunciation;
            else
                PronunciationText.Text = string.Empty;

            // Set visibility
            TranslationInfoGrid.Visibility = Visibility.Visible;
            TranslationResultGrid.Visibility = Visibility.Visible;
            DontBeAfraidText.Visibility = Visibility.Visible;
        }

        #endregion
    }
}
