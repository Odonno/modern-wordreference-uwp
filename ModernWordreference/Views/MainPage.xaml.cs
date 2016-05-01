using Microsoft.Services.Store.Engagement;
using ModernWordreference.Constants;
using ModernWordreference.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Networking.Connectivity;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ModernWordreference.Views
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Fields

        private readonly ResourceLoader _resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        #endregion

        #region Properties

        public Models.Dictionary CurrentDictionary { get; set; }
        public Models.TranslationResult LastTranslation { get; set; }
        public bool NeedSpecificKeyboard
        {
            get
            {
                string from = CurrentDictionary.From;
                return (from == "ru" ||
                    from == "gr" ||
                    from == "zh" ||
                    from == "ja" ||
                    from == "ko" ||
                    from == "ar");
            }
        }

        #endregion

        #region Constructor

        public MainPage()
        {
            this.InitializeComponent();

            // Set navigation system
            var systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            systemNavigationManager.BackRequested += (object sender, BackRequestedEventArgs e) =>
            {
                if (Frame.CanGoBack)
                {
                    e.Handled = true;
                    Frame.GoBack();
                }
            };

            // Initialize page
            InitializeAsync();

            // Initialize Api Service
            ServiceFactory.Api.InitializeAsync();
        }

        #endregion

        #region Events

        private void About_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AboutPage));
        }

        private async void Feedback_Click(object sender, RoutedEventArgs e)
        {
            // Send telemetry
            ServiceFactory.Analytics.TrackEvent("OpenFeedbackApp");

            // Open feedback hub app
            bool executed = await Feedback.LaunchFeedbackAsync();

            if (executed)
            {
                // Feedback app opened
            }
        }

        private void Love_Click(object sender, RoutedEventArgs e)
        {
            // Send telemetry
            ServiceFactory.Analytics.TrackEvent("OpenLoveModal");

            Frame.Navigate(typeof(LovePage));
        }

        private void SelectDictionary_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SelectDictionaryPage));
        }

        private void SwitchDictionary_Click(object sender, RoutedEventArgs e)
        {
            SwitchDictionary();
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteSearchAsync(WordTextBox.Text);
        }

        private void PlayAudio_Click(object sender, RoutedEventArgs e)
        {
            // Play audio
            Audio.Play();

            // Send telemetry
            ServiceFactory.Analytics.TrackEvent("ListenPronunciation", new Dictionary<string, string> {
                { "From", LastTranslation.Dictionary.From },
                { "To", LastTranslation.Dictionary.To },
                { "Word", LastTranslation.OriginalWord }
            });
        }

        private async void Search_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                // Update search button UI
                UpdateSearchButtonUI();

                // Retrieve suggestions
                if (string.IsNullOrWhiteSpace(WordTextBox.Text) || WordTextBox.Text.Length <= 1)
                    return;

                WordTextBox.ItemsSource = await ServiceFactory.Api.RetrieveSuggestionsAsync(WordTextBox.Text, CurrentDictionary);
            }
        }

        private async void Search_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                var suggestion = args.ChosenSuggestion as Models.Suggestion;
                await ExecuteSearchAsync(suggestion.Word);
            }
            else
            {
                await ExecuteSearchAsync(WordTextBox.Text);
            }
        }

        private void Search_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            // Select suggestion
            var suggestion = args.SelectedItem as Models.Suggestion;
            if (suggestion == null)
            {
                // Suggestion does not exist...
                return;
            }

            // Invert dictionary if the searched word is from another language
            if (suggestion.Language != CurrentDictionary.From)
            {
                SwitchDictionary();
            }

            // Update text
            WordTextBox.Text = suggestion.Word;

            // Send telemetry
            ServiceFactory.Analytics.TrackEvent("SelectSuggestion", new Dictionary<string, string> {
                { "From", LastTranslation.Dictionary.From },
                { "To", LastTranslation.Dictionary.To },
                { "Suggestion", WordTextBox.Text }
            });
        }

        private void CloseInfo_Click(object sender, RoutedEventArgs e)
        {
            InfoPanel.Visibility = Visibility.Collapsed;
        }

        #endregion        

        #region Logic

        private async void InitializeAsync()
        {
            // Handle error when there is no network
            HandleNetworkErrorAsync();
            NetworkInformation.NetworkStatusChanged += HandleNetworkErrorAsync;

            // Set feedback button visibility
            if (!ServiceFactory.FeatureToggle.UseFeedbackHubApp())
            {
                FeedbackButton.Visibility = Visibility.Collapsed;
            }

            // Retrieve current dictionary
            CurrentDictionary = ServiceFactory.Storage.Retrieve<Models.Dictionary>(StorageConstants.CurrentDictionary);
            if (CurrentDictionary == null)
            {
                CurrentDictionary = ServiceFactory.Dictionary.Get("en", "fr");
                ServiceFactory.Storage.Save(StorageConstants.CurrentDictionary, CurrentDictionary);
            }
            UpdateDictionaryUI();

            // Update keyboard required UI
            UpdateKeyboardRequiredUI();

            // Retrieve last saved translation
            LastTranslation = await ServiceFactory.Storage.RetrieveFileAsync<Models.TranslationResult>(StorageConstants.LastTranslation);
            if (LastTranslation != null)
            {
                UpdateLastTranslationUI();
                LoadAudio();
            }

            // Update search button UI
            UpdateSearchButtonUI();
        }

        private async void HandleNetworkErrorAsync(object sender = null)
        {
            if (!ServiceFactory.Network.IsInternetAvailable)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher
                    .RunAsync(CoreDispatcherPriority.High, () =>
                    {
                        Frame.Navigate(typeof(ErrorPage));
                    });
            }
        }

        private void SwitchDictionary()
        {
            CurrentDictionary = ServiceFactory.Dictionary.Get(CurrentDictionary.To, CurrentDictionary.From);
            UpdateDictionaryUI();

            // Send telemetry
            ServiceFactory.Analytics.TrackEvent("SwitchDictionary", new Dictionary<string, string> {
                { "From", CurrentDictionary.To },
                { "To", CurrentDictionary.From },
                { "NewFrom", CurrentDictionary.From },
                { "NewTo", CurrentDictionary.To }
            });
        }

        private async Task ExecuteSearchAsync(string word)
        {
            // Do not execute search if we already searched this word
            if (LastTranslation != null
                && word.Trim() == LastTranslation.OriginalWord.Trim()
                && LastTranslation.Dictionary.From == CurrentDictionary.From
                && LastTranslation.Dictionary.To == CurrentDictionary.To)
                return;

            // Start progress bar
            UpdateProgressBarUI(true);

            // Execute search
            var searchResult = await ServiceFactory.Api.ExecuteSearchAsync(word, CurrentDictionary);
            if (searchResult == null)
            {
                // Show toast notification that nothing was found
                ServiceFactory.ToastNotification.SendText(_resourceLoader.GetString("WordDoesNotExist"), "search");
                ServiceFactory.Analytics.TrackEvent("NotFound");

                // End progress bar
                UpdateProgressBarUI(false);

                return;
            }

            // Save last translation
            LastTranslation = searchResult;

            // Load audio
            LoadAudio();

            // Refresh UI
            UpdateLastTranslationUI();

            // Reset text search
            WordTextBox.Text = string.Empty;

            // Remove suggestions list
            WordTextBox.ItemsSource = new List<string>();

            // End progress bar
            UpdateProgressBarUI(false);

            // Save result
            await ServiceFactory.Storage.SaveFileAsync(StorageConstants.LastTranslation, LastTranslation);

            // Send telemetry
            var properties = new Dictionary<string, string> {
                { "From", LastTranslation.Dictionary.From },
                { "To", LastTranslation.Dictionary.To },
                { "Word", LastTranslation.OriginalWord }
            };
            var metrics = new Dictionary<string, double> {
                { "PrincipalTranslations", LastTranslation.PrimaryTranslations.Count },
                { "AdditionalTranslations", LastTranslation.AdditionalTranslations.Count },
                { "CompoundForms", LastTranslation.CompoundForms.Count }
            };
            ServiceFactory.Analytics.TrackEvent("TranslationDone", properties, metrics);
        }

        private void LoadAudio()
        {
            if (LastTranslation.AudioSources.Count > 0)
            {
                // Reload audio element
                Audio.Source = new Uri(LastTranslation.AudioSources[0]);
            }

            UpdateAudioUI();
        }

        #endregion

        #region UI

        private void UpdateDictionaryUI()
        {
            FromImage.Source = new BitmapImage(new Uri(CurrentDictionary.FromImagePath));
            ToImage.Source = new BitmapImage(new Uri(CurrentDictionary.ToImagePath));
        }

        private void UpdateSearchButtonUI()
        {
            SearchButton.IsEnabled = (!string.IsNullOrWhiteSpace(WordTextBox.Text) && WordTextBox.Text.Length > 1);
        }

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
            NoTranslationPanel.Visibility = Visibility.Collapsed;
        }

        private void UpdateAudioUI()
        {
            AudioButton.Visibility = (LastTranslation.AudioSources.Count > 0) ?
                Visibility.Visible :
                Visibility.Collapsed;
        }

        private void UpdateProgressBarUI(bool show)
        {
            ProgressBar.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            ProgressBar.IsIndeterminate = show;
        }

        private void UpdateKeyboardRequiredUI()
        {
            if (NeedSpecificKeyboard)
            {
                KeyboardRequiredText.Text = string.Format(_resourceLoader.GetString("KeyboardRequired"), CurrentDictionary.FromLanguage);
                InfoPanel.Visibility = Visibility.Visible;
            }
            else
            {
                InfoPanel.Visibility = Visibility.Collapsed;
            }
        }

        #endregion
    }
}
