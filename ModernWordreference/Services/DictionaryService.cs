using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    return "english";
                case "fr":
                    return "french";
                case "es":
                    return "spanish";
                case "it":
                    return "italian";
                case "de":
                    return "german";
                case "nl":
                    return "dutch";
                case "sv":
                    return "swedish";
                case "ru":
                    return "russian";
                case "pt":
                    return "portuguese";
                case "pl":
                    return "polish";
                case "ro":
                    return "romanian";
                case "cz":
                    return "czech";
                case "gr":
                    return "greek";
                case "tr":
                    return "turkish";
                case "zh":
                    return "chinese";
                case "ja":
                    return "japanese";
                case "ko":
                    return "korean";
                case "ar":
                    return "arabic";
                default:
                    throw new NotSupportedException();
            }
        }        
    }
}
