using System.Diagnostics;
using System.Net;
using WindowSelector.Common.ViewModels;
using WindowSelector.Plugins.GoogleSearchSuggestions.Models;

namespace WindowSelector.Plugins.GoogleSearchSuggestions.ViewModels
{
    public class GoogleSuggestionWindowResult : WindowResult
    {
        private GoogleSearchSuggestion _value;

        public GoogleSuggestionWindowResult(GoogleSearchSuggestion suggestion)
        {
            if (suggestion.IsQuery)
            {
                DisplayText = "Search for: " + suggestion.Term;
                Details = "https://www.google.com/search?q=" + WebUtility.UrlEncode(suggestion.Term);
                Label = Details;
            }
            else if (string.IsNullOrWhiteSpace(suggestion.Url))
            {
                DisplayText = "Visit: " + suggestion.Description;
                Details = suggestion.Term;
                Label = Details;
            }
            else
            {
                DisplayText = "Visit : " + (!string.IsNullOrWhiteSpace(suggestion.Description) ? suggestion.Description : suggestion.Url);
                Details = suggestion.Url;
                Label = Details;
            }
            _value = suggestion;
        }

        private string _link = null;
        public override void Select(bool centerWindow)
        {
            Process.Start(Details);
        }

        public override object Value
        {
            get { return _value; }
        }

        public override void Close()
        {
            
        }

        public override void Minimize()
        {
            
        }

        public override void Whitelist()
        {
            
        }

        public override void Blacklist()
        {
            
        }

        public override void UnWhitelist()
        {
            
        }

        public override void UnBlacklist()
        {
            
        }
    }
}
