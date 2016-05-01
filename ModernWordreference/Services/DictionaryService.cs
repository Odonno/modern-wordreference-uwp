using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace ModernWordreference.Services
{
    public interface IDictionaryService
    {
        void Initialize();
        List<Models.Dictionary> GetAll();
        Models.Dictionary Get(string from, string to);
        string GetLanguage(string countryCode);
    }

    public class DictionaryService : IDictionaryService
    {
        private readonly ResourceLoader _resourceLoader = ResourceLoader.GetForCurrentView("LanguageResources");
        private List<Models.Dictionary> _dictionaries;

        public DictionaryService()
        {            
        }

        public void Initialize()
        {
            _dictionaries = new List<Models.Dictionary>
            {
                new Models.Dictionary("en", "es"),
                new Models.Dictionary("es", "en"),

                new Models.Dictionary("en", "fr"),
                new Models.Dictionary("fr", "en"),

                new Models.Dictionary("en", "it"),
                new Models.Dictionary("it", "en"),

                new Models.Dictionary("en", "de"),
                new Models.Dictionary("de", "en"),

                new Models.Dictionary("en", "nl"),
                new Models.Dictionary("nl", "en"),

                new Models.Dictionary("en", "sv"),
                new Models.Dictionary("sv", "en"),

                new Models.Dictionary("en", "ru"),
                new Models.Dictionary("ru", "en"),

                new Models.Dictionary("en", "pt"),
                new Models.Dictionary("pt", "en"),

                new Models.Dictionary("en", "pl"),
                new Models.Dictionary("pl", "en"),

                new Models.Dictionary("en", "ro"),
                new Models.Dictionary("ro", "en"),

                new Models.Dictionary("en", "cz"),
                new Models.Dictionary("cz", "en"),

                new Models.Dictionary("en", "gr"),
                new Models.Dictionary("gr", "en"),

                new Models.Dictionary("en", "tr"),
                new Models.Dictionary("tr", "en"),

                new Models.Dictionary("en", "zh"),
                new Models.Dictionary("zh", "en"),

                new Models.Dictionary("en", "ja"),
                new Models.Dictionary("ja", "en"),

                new Models.Dictionary("en", "ko"),
                new Models.Dictionary("ko", "en"),

                new Models.Dictionary("en", "ar"),
                new Models.Dictionary("ar", "en"),

                new Models.Dictionary("fr", "es", false),
                new Models.Dictionary("es", "fr", false),

                new Models.Dictionary("es", "pt", false),
                new Models.Dictionary("pt", "es", false)
            };
        }

        public List<Models.Dictionary> GetAll()
        {
            return _dictionaries.Where(d => d.Active).ToList();
        }

        public Models.Dictionary Get(string from, string to)
        {
            return _dictionaries.FirstOrDefault(d => d.From == from && d.To == to);
        }

        public string GetLanguage(string countryCode)
        {
            switch (countryCode)
            {
                case "en":
                    return _resourceLoader.GetString("english");
                case "fr":
                    return _resourceLoader.GetString("french");
                case "es":
                    return _resourceLoader.GetString("spanish");
                case "it":
                    return _resourceLoader.GetString("italian");
                case "de":
                    return _resourceLoader.GetString("german");
                case "nl":
                    return _resourceLoader.GetString("dutch");
                case "sv":
                    return _resourceLoader.GetString("swedish");
                case "ru":
                    return _resourceLoader.GetString("russian");
                case "pt":
                    return _resourceLoader.GetString("portuguese");
                case "pl":
                    return _resourceLoader.GetString("polish");
                case "ro":
                    return _resourceLoader.GetString("romanian");
                case "cz":
                    return _resourceLoader.GetString("czech");
                case "gr":
                    return _resourceLoader.GetString("greek");
                case "tr":
                    return _resourceLoader.GetString("turkish");
                case "zh":
                    return _resourceLoader.GetString("chinese");
                case "ja":
                    return _resourceLoader.GetString("japanese");
                case "ko":
                    return _resourceLoader.GetString("korean");
                case "ar":
                    return _resourceLoader.GetString("arabic");
                default:
                    throw new NotSupportedException();
            }
        }        
    }
}
