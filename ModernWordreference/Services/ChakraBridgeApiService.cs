using ChakraBridge;
using ModernWordreference.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ModernWordreference.Services
{
    public class ChakraBridgeApiService : IApiService
    {
        private ChakraHost _host = new ChakraHost();
        private Models.JsBridgeResponse<Models.Suggestion[]> SuggestionsResponse = new Models.JsBridgeResponse<Models.Suggestion[]>();
        private Models.JsBridgeResponse<Models.TranslationResult> TranslationResponse = new Models.JsBridgeResponse<Models.TranslationResult>();

        public ChakraBridgeApiService()
        {
            // Retrieve log info
            Console.OnLog += (object sender, string e) => Debug.WriteLine(e);
        }

        public async Task InitializeAsync()
        {
            // Set js sources
            await _host.ReadAndExecuteAsync("wordreference.js");

            // Define types received from js
            CommunicationManager.RegisterType(typeof(Models.Dictionary));
            CommunicationManager.RegisterType(typeof(Models.Suggestion[]));
            CommunicationManager.RegisterType(typeof(Models.TranslationPart));
            CommunicationManager.RegisterType(typeof(Models.TranslationLine));
            CommunicationManager.RegisterType(typeof(Models.TranslationResult));

            // Receive data from js calls
            CommunicationManager.OnObjectReceived = (data) =>
            {
                if (data is Models.Suggestion[])
                {
                    SuggestionsResponse.Object = (Models.Suggestion[])data;
                    SuggestionsResponse.Done = true;
                }

                if (data is Models.TranslationResult)
                {
                    TranslationResponse.Object = (Models.TranslationResult)data;
                    TranslationResponse.Done = true;
                }
            };
        }

        public async Task<IEnumerable<Models.Suggestion>> RetrieveSuggestionsAsync(string word, Models.Dictionary dictionary)
        {
            _host.CallFunction("retrieveSuggestions", word, dictionary.Value);

            while (!SuggestionsResponse.Done)
            {
                await Task.Delay(100);
            }

            SuggestionsResponse.Done = false;
            return SuggestionsResponse.Object.Take(15);
        }

        public async Task<Models.TranslationResult> ExecuteSearchAsync(string word, Models.Dictionary dictionary)
        {
            _host.CallFunction("executeSearch", word, dictionary.Value);

            while (!TranslationResponse.Done)
            {
                await Task.Delay(250);
            }

            TranslationResponse.Object.Dictionary = dictionary;
            TranslationResponse.Done = false;
            return TranslationResponse.Object;
        }
    }
}
