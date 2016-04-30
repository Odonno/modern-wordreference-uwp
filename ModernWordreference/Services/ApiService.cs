using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ModernWordreference.Services
{
    public interface IApiService
    {
        Task InitializeAsync();
        Task<IEnumerable<Models.Suggestion>> RetrieveSuggestionsAsync(string word, Models.Dictionary dictionary);
        Task<Models.TranslationResult> ExecuteSearchAsync(string word, Models.Dictionary dictionary);
    }

    public class NativeApiService : IApiService
    {
        public Task InitializeAsync()
        {
            return new Task(() => { });
        }

        public async Task<IEnumerable<Models.Suggestion>> RetrieveSuggestionsAsync(string word, Models.Dictionary dictionary)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Accept-Language", "fr-FR, fr; q=0.8, en-US; q=0.5, en; q=0.3");
                httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
                httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "deflate");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36 Edge/13.10586");

                var response = await httpClient.GetAsync($"http://www.wordreference.com/2012/autocomplete/autocomplete.aspx?dict={dictionary.Value}&query={word}");
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();

                    // Split result to retrieve data (each result is separated by a line return)
                    var splitResult = result.Split('\n');
                    var suggestions = new List<Models.Suggestion>();

                    // Handle max number of suggestions
                    int maxResult = 15;

                    foreach (string splitObject in splitResult)
                    {
                        if (suggestions.Count >= maxResult)
                            break;

                        // Each property is separated by a tab
                        var splitProperties = splitObject.Split('\t');

                        if (string.IsNullOrWhiteSpace(splitProperties[0]) || string.IsNullOrWhiteSpace(splitProperties[1]))
                        {
                            continue;
                        }

                        var suggestion = new Models.Suggestion
                        {
                            Word = splitProperties[0],
                            Language = splitProperties[1],
                            MatchValue = int.Parse(splitProperties[2])
                        };
                        suggestions.Add(suggestion);
                    }

                    return suggestions;
                }
                return new List<Models.Suggestion>();
            }
        }

        public async Task<Models.TranslationResult> ExecuteSearchAsync(string word, Models.Dictionary dictionary)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Accept-Language", "fr-FR, fr; q=0.8, en-US; q=0.5, en; q=0.3");
                httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36 Edge/13.10586");

                var response = await httpClient.GetAsync($"http://www.wordreference.com/{dictionary.Value}/{word}");
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    var translationResult = new Models.TranslationResult(word, dictionary);

                    // Load HTML result
                    var htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(result);

                    // Parse HTML through document
                    var wrdElements = htmlDocument.DocumentNode.Descendants()
                        .Where(d => d.Attributes["class"] != null && d.Attributes["class"].Value.Contains("WRD"));

                    // If there is no translation => there is no result
                    if (wrdElements.Count() <= 0)
                        return null;

                    // Retrieve WordReference translation (default)
                    List<Models.TranslationLine> currentSection = null;
                    int i = 0;

                    foreach (var wrdElement in wrdElements)
                    {
                        var wrdSectionElements = wrdElement.ChildNodes;

                        foreach (var wrdSectionElement in wrdSectionElements)
                        {
                            if (wrdSectionElement.Name != "tr")
                                continue;

                            if (wrdSectionElement.Attributes["class"] != null && wrdSectionElement.Attributes["class"].Value == "langHeader")
                                continue;

                            // Check if it is a next section
                            if (wrdSectionElement.Attributes["class"] != null && wrdSectionElement.Attributes["class"].Value == "wrtopsection")
                            {
                                var sectionElement = wrdSectionElement.ChildNodes[0];

                                if (sectionElement.Attributes["title"] != null && sectionElement.Attributes["title"].Value == "Principal Translations")
                                    currentSection = translationResult.PrimaryTranslations;

                                if (sectionElement.Attributes["title"] != null && sectionElement.Attributes["title"].Value == "Additional Translations")
                                    currentSection = translationResult.AdditionalTranslations;

                                if (sectionElement.Attributes["title"] != null && sectionElement.Attributes["title"].Value == "Compound Forms")
                                    currentSection = translationResult.CompoundForms;

                                continue;
                            }

                            // Add a new translation line
                            var translationLine = new Models.TranslationLine();
                            string currentTranslationPart = null;

                            for (i = 0; i < wrdSectionElement.ChildNodes.Count; i++)
                            {
                                var lineElement = wrdSectionElement.ChildNodes[i];

                                if (lineElement.Attributes["class"] == null)
                                {
                                    if (lineElement.ChildNodes.Count >= 1)
                                    {
                                        // Add sense info
                                        if (currentTranslationPart == "from" && lineElement.ChildNodes[0] != null)
                                        {
                                            translationLine.From[translationLine.From.Count - 1].Sense = WebUtility.HtmlDecode(lineElement.ChildNodes[0].InnerText);
                                        }
                                        if (currentTranslationPart == "to" && lineElement.ChildNodes[0] != null)
                                        {
                                            translationLine.To[translationLine.To.Count - 1].Sense = WebUtility.HtmlDecode(lineElement.ChildNodes[0].InnerText);
                                        }
                                    }
                                }
                                else if (lineElement.Attributes["class"].Value == "FrEx")
                                {
                                    translationLine.FromExamples.Add(WebUtility.HtmlDecode(lineElement.InnerText));
                                }
                                else if (lineElement.Attributes["class"].Value == "ToEx")
                                {
                                    translationLine.ToExamples.Add(WebUtility.HtmlDecode(lineElement.InnerText));
                                }
                                else if (lineElement.Attributes["class"].Value == "FrWrd")
                                {
                                    currentTranslationPart = "from";

                                    // Add "from" info
                                    var translationPart = new Models.TranslationPart();
                                    translationPart.Word = WebUtility.HtmlDecode(lineElement.ChildNodes[0].InnerText);
                                    if (lineElement.ChildNodes.Count >= 3 && lineElement.ChildNodes[2] != null && lineElement.ChildNodes[2].ChildNodes.Count >= 1 && lineElement.ChildNodes[2].ChildNodes[0] != null)
                                    {
                                        translationPart.Type = WebUtility.HtmlDecode(lineElement.ChildNodes[2].ChildNodes[0].InnerText);
                                        if (translationPart.Type == "⇒")
                                            translationPart.Type = WebUtility.HtmlDecode(lineElement.ChildNodes[3].ChildNodes[0].InnerText);
                                    }

                                    translationLine.From.Add(translationPart);
                                }
                                else if (lineElement.Attributes["class"].Value == "ToWrd")
                                {
                                    currentTranslationPart = "to";

                                    // Add "to" info
                                    var translationPart = new Models.TranslationPart();
                                    translationPart.Word = WebUtility.HtmlDecode(lineElement.ChildNodes[0].InnerText);
                                    if (lineElement.ChildNodes.Count >= 2 && lineElement.ChildNodes[1] != null && lineElement.ChildNodes[1].ChildNodes.Count >= 1 && lineElement.ChildNodes[1].ChildNodes[0] != null)
                                    {
                                        translationPart.Type = WebUtility.HtmlDecode(lineElement.ChildNodes[1].ChildNodes[0].InnerText);
                                        if (translationPart.Type == "⇒")
                                            translationPart.Type = WebUtility.HtmlDecode(lineElement.ChildNodes[3].ChildNodes[0].InnerText);
                                    }

                                    translationLine.To.Add(translationPart);
                                }
                                else
                                {
                                    // TODO
                                }
                            }

                            currentSection.Add(translationLine);
                        }
                    }

                    // Regroup translation lines inside which do not have "from" or "to" property
                    i = translationResult.PrimaryTranslations.Count - 1;
                    while (i > 0)
                    {
                        if (translationResult.PrimaryTranslations[i].From.Count <= 0 ||
                            translationResult.PrimaryTranslations[i].To.Count <= 0)
                        {
                            // Merge translation line properties
                            translationResult.PrimaryTranslations[i - 1].From =
                                translationResult.PrimaryTranslations[i - 1].From
                                    .Concat(translationResult.PrimaryTranslations[i].From)
                                    .ToList();

                            translationResult.PrimaryTranslations[i - 1].To =
                                translationResult.PrimaryTranslations[i - 1].To
                                    .Concat(translationResult.PrimaryTranslations[i].To)
                                    .ToList();

                            translationResult.PrimaryTranslations[i - 1].FromExamples =
                                translationResult.PrimaryTranslations[i - 1].FromExamples
                                    .Concat(translationResult.PrimaryTranslations[i].FromExamples)
                                    .ToList();

                            translationResult.PrimaryTranslations[i - 1].ToExamples =
                                translationResult.PrimaryTranslations[i - 1].ToExamples
                                    .Concat(translationResult.PrimaryTranslations[i].ToExamples)
                                    .ToList();

                            // Remove useless line
                            translationResult.PrimaryTranslations.RemoveAt(i);
                        }

                        i--;
                    }

                    i = translationResult.AdditionalTranslations.Count - 1;
                    while (i > 0)
                    {
                        if (translationResult.AdditionalTranslations[i].From.Count <= 0 ||
                            translationResult.AdditionalTranslations[i].To.Count <= 0)
                        {
                            // Merge translation line properties
                            translationResult.AdditionalTranslations[i - 1].From =
                                translationResult.AdditionalTranslations[i - 1].From
                                    .Concat(translationResult.AdditionalTranslations[i].From)
                                    .ToList();

                            translationResult.AdditionalTranslations[i - 1].To =
                                translationResult.AdditionalTranslations[i - 1].To
                                    .Concat(translationResult.AdditionalTranslations[i].To)
                                    .ToList();

                            translationResult.AdditionalTranslations[i - 1].FromExamples =
                                translationResult.AdditionalTranslations[i - 1].FromExamples
                                    .Concat(translationResult.AdditionalTranslations[i].FromExamples)
                                    .ToList();

                            translationResult.AdditionalTranslations[i - 1].ToExamples =
                                translationResult.AdditionalTranslations[i - 1].ToExamples
                                    .Concat(translationResult.AdditionalTranslations[i].ToExamples)
                                    .ToList();

                            // Remove useless line
                            translationResult.AdditionalTranslations.RemoveAt(i);
                        }

                        i--;
                    }

                    i = translationResult.CompoundForms.Count - 1;
                    while (i > 0)
                    {
                        if (translationResult.CompoundForms[i].From.Count <= 0 ||
                            translationResult.CompoundForms[i].To.Count <= 0)
                        {
                            // Merge translation line properties
                            translationResult.CompoundForms[i - 1].From =
                                translationResult.CompoundForms[i - 1].From
                                    .Concat(translationResult.CompoundForms[i].From)
                                    .ToList();

                            translationResult.CompoundForms[i - 1].To =
                                translationResult.CompoundForms[i - 1].To
                                    .Concat(translationResult.CompoundForms[i].To)
                                    .ToList();

                            translationResult.CompoundForms[i - 1].FromExamples =
                                translationResult.CompoundForms[i - 1].FromExamples
                                    .Concat(translationResult.CompoundForms[i].FromExamples)
                                    .ToList();

                            translationResult.CompoundForms[i - 1].ToExamples =
                                translationResult.CompoundForms[i - 1].ToExamples
                                    .Concat(translationResult.CompoundForms[i].ToExamples)
                                    .ToList();

                            // Remove useless line
                            translationResult.CompoundForms.RemoveAt(i);
                        }

                        i--;
                    }

                    // TODO : Retrieve Collins translations


                    // TODO : Retrieve reverse WordReference translation


                    // Retrieve audio sources
                    var audioSourcesParentElement = htmlDocument.GetElementbyId("listen_widget");
                    if (audioSourcesParentElement != null)
                    {
                        for (i = 0; i < audioSourcesParentElement.ChildNodes[2].ChildNodes.Count; i++)
                        {
                            var audioSourceElement = audioSourcesParentElement.ChildNodes[2].ChildNodes[i].FirstChild;
                            translationResult.AudioSources.Add("http://www.wordreference.com" + audioSourceElement.Attributes["src"].Value);
                        }
                    }

                    // Retrieve pronunciation
                    var pronunciationElement = htmlDocument.GetElementbyId("pronWR");
                    if (pronunciationElement != null)
                    {
                        translationResult.Pronunciation = pronunciationElement.InnerText;
                    }

                    return translationResult;
                }
                return null;
            }
        }
    }
}
