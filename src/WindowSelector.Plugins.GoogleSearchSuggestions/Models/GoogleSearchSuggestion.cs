using System;

namespace WindowSelector.Plugins.GoogleSearchSuggestions.Models
{
    public class GoogleSearchSuggestion
    {
        public string Term { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string Hint { get; set; }

        public GoogleSearchSuggestion(string term, string description, string url, string hint)
        {
            Term = term;
            Description = description;
            Url = url;
            if (!string.IsNullOrWhiteSpace(hint))
            {
                Hint = hint;
            }
            else
            {
                Hint = Uri.IsWellFormedUriString(Term, UriKind.Absolute) ? "NAVIGATION" : "QUERY";
            }
        }

        public bool IsQuery { get { return Hint == "QUERY"; } }
    }
}
