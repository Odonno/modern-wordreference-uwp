﻿using ModernWordreference.Constants;
using ModernWordreference.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Pour plus d"informations sur le modèle d"élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace ModernWordreference.Views
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d"un frame.
    /// </summary>
    public sealed partial class SelectDictionaryPage : Page
    {
        #region Fields

        private readonly ResourceLoader _resourceLoader = ResourceLoader.GetForCurrentView("Resources");
        private bool _initializing;

        #endregion

        #region Properties

        public IEnumerable<Models.Dictionary> AllDictionaries { get; set; }
        public IEnumerable<Models.Dictionary> RecommendedDictionaries { get; set; }
        public IEnumerable<Models.Dictionary> DefaultDictionaries { get; set; }
        public Models.Dictionary SelectedDictionary { get; set; }
        public string Search { get; set; }

        #endregion

        #region Constructor

        public SelectDictionaryPage()
        {
            this.InitializeComponent();

            // Set navigation system
            var systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            // Initialize page
            Initialize();
        }

        #endregion

        #region Events

        private void ChangeDictionary_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_initializing)
                return;

            var list = sender as ListView;
            var dictionary = list?.SelectedItem as Models.Dictionary;

            // Check if we have really change of dictionary
            if (dictionary != null && 
                (SelectedDictionary.From != dictionary.From ||
                SelectedDictionary.To != dictionary.To))
            {
                SelectedDictionary = dictionary;
                DictionariesList.SelectedItem = SelectedDictionary;

                // Save selection
                ServiceFactory.Storage.Save(StorageConstants.CurrentDictionary, SelectedDictionary);

                // Send telemetry
                ServiceFactory.Analytics.TrackEvent("SelectDictionary", new Dictionary<string, string>
                {
                    { "From", SelectedDictionary.From },
                    { "To", SelectedDictionary.To }
                });

                // Go back
                Frame.GoBack();
            }
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            Search = SearchTextBox.Text;
            UpdateDictionariesUI();
        }

        #endregion

        #region Logic

        private void Initialize()
        {
            _initializing = true;

            // Set dictionary list (all)
            AllDictionaries = ServiceFactory.Dictionary.GetAll();

            // Set dictionary list (recommended)
            RecommendedDictionaries = new List<Models.Dictionary>
            {
                ServiceFactory.Dictionary.Get("en", "fr"),
                ServiceFactory.Dictionary.Get("fr", "en")
            };

            // Set dictionary list (default)
            DefaultDictionaries = ServiceFactory.Dictionary.GetAll()
                .Except(RecommendedDictionaries);

            // Refresh UI
            UpdateDictionariesUI();

            _initializing = false;
        }

        #endregion

        #region UI

        private void UpdateDictionariesUI()
        {
            _initializing = true;

            Func<Models.Dictionary, string, bool> dictionaryContainsFromLanguage =
                (Models.Dictionary d, string s) => d.FromLanguage.ToLower().Contains(s.ToLower());

            Func<Models.Dictionary, string, bool> dictionaryContainsToLanguage =
                (Models.Dictionary d, string s) => d.ToLanguage.ToLower().Contains(s.ToLower());

            Func<IEnumerable<IGrouping<string, Models.Dictionary>>, Models.Dictionary, bool> groupContainsDictionary =
                (IEnumerable<IGrouping<string, Models.Dictionary>> groupList, Models.Dictionary d) => groupList.Any(g => g.Any(d2 => d == d2));

            // Set group key in each dictionary
            var searchGroupList = AllDictionaries
                .Where(dictionary => !string.IsNullOrWhiteSpace(Search) &&
                                     (dictionaryContainsFromLanguage(dictionary, Search) ||
                                     dictionaryContainsToLanguage(dictionary, Search)))
                .GroupBy(_ => _resourceLoader.GetString("SearchedDictionaries"));

            var recommendGroupList = RecommendedDictionaries
                .Where(dictionary => !groupContainsDictionary(searchGroupList, dictionary))
                .GroupBy(_ => _resourceLoader.GetString("Recommended"));

            var defaultGroupList = DefaultDictionaries
                .Where(dictionary => !groupContainsDictionary(searchGroupList, dictionary))
                .GroupBy(_ => _resourceLoader.GetString("AllDictionaries"));

            // Fill collection view source with data
            DictionariesSource.Source = searchGroupList
                .Concat(recommendGroupList)
                .Concat(defaultGroupList);

            // Retrieve currently selected dictionary
            var savedDictionary = ServiceFactory.Storage.Retrieve<Models.Dictionary>(StorageConstants.CurrentDictionary);
            SelectedDictionary = ServiceFactory.Dictionary.Get(savedDictionary.From, savedDictionary.To);
            DictionariesList.SelectedItem = SelectedDictionary;

            _initializing = false;
        }

        #endregion        
    }
}
