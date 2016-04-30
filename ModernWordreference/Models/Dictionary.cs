using ModernWordreference.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernWordreference.Models
{
    public class Dictionary
    {
        public string From { get; set; }
        public string FromLanguage { get; set; }
        public string FromImagePath { get { return $"ms-appx:///Images/Flags/{From}.png"; } }
        public string To { get; set; }
        public string ToLanguage { get; set; }
        public string ToImagePath { get { return $"ms-appx:///Images/Flags/{To}.png"; } }
        public string Value { get { return $"{From}{To}"; } }
        public bool Active { get; set; }

        public Dictionary(string from, string to, bool active = true)
        {
            From = from;
            FromLanguage = ServiceFactory.Dictionary.GetLanguage(from);
            To = to;
            ToLanguage = ServiceFactory.Dictionary.GetLanguage(to);
            Active = active;
        }
    }
}
