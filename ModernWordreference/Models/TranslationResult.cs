using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernWordreference.Models
{
    public class TranslationPart
    {
        public string Word { get; set; }
        public string Type { get; set; }
        public string Sense { get; set; }
    }

    public class TranslationLine
    {
        public List<TranslationPart> From { get; set; } = new List<TranslationPart>();
        public List<TranslationPart> To { get; set; } = new List<TranslationPart>();
        public List<string> FromExamples { get; set; } = new List<string>();
        public List<string> ToExamples { get; set; } = new List<string>();
    }

    public class TranslationResult
    {
        public string OriginalWord { get; set; }
        public Dictionary Dictionary { get; set; }
        public List<TranslationLine> PrimaryTranslations { get; set; } = new List<TranslationLine>();
        public List<TranslationLine> AdditionalTranslations { get; set; } = new List<TranslationLine>();
        public List<TranslationLine> CompoundForms { get; set; } = new List<TranslationLine>();
        public List<string> AudioSources { get; set; } = new List<string>();
        public string Pronunciation { get; set; }

        public TranslationResult(string originalWord, Dictionary dictionary)
        {
            OriginalWord = originalWord;
            Dictionary = dictionary;
        }
    }
}
