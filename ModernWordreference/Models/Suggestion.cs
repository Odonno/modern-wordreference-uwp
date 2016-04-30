using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernWordreference.Models
{
    public class Suggestion
    {
        public string Word { get; set; }
        public string Language { get; set; }
        public int MatchValue { get; set; }
        public string LanguageImagePath { get { return $"ms-appx:///Images/Flags/{Language}.png"; } }

        public override string ToString()
        {
            return Word;
        }
    }
}
