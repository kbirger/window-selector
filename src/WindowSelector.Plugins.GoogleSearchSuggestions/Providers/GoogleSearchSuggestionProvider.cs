﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json;
using WindowSelector.Common.Interfaces;
using WindowSelector.Common.ViewModels;
using WindowSelector.Plugins.GoogleSearchSuggestions.Models;
using WindowSelector.Plugins.GoogleSearchSuggestions.ViewModels;

namespace WindowSelector.Plugins.GoogleSearchSuggestions.Providers
{
    public class GoogleSearchSuggestionProvider : IWindowResultProvider
    {
        private readonly Func<GoogleSearchSuggestion, GoogleSuggestionWindowResult> _resultFactoryFunc;
        public GoogleSearchSuggestionProvider(Func<GoogleSearchSuggestion, GoogleSuggestionWindowResult> resultFactoryFunc)
        {
            _resultFactoryFunc = resultFactoryFunc;
        }

        public async Task<IEnumerable<WindowResult>> GetResultsAsync(string keyword, string query, CancellationToken cancellationToken, SearchResultsWriter writer)
        {
            writer.AddResults(new List<WindowResult> { _resultFactoryFunc(new GoogleSearchSuggestion(query, "", null, "QUERY")) });

            var responseString = 
                await
                    ("http://suggestqueries.google.com/complete/search?client=chrome&hl=en&q=" + query).GetStringAsync(
                        cancellationToken);
            var response = JsonConvert.DeserializeObject<GoogleSearchSuggestionResponse>(responseString);
            List<WindowResult> results = new List<WindowResult>(response.Suggestions.Length);
            for (int i = 0; i < response.Suggestions.Length; i++)
            {
                results.Add(
                    _resultFactoryFunc(
                        new GoogleSearchSuggestion(
                            response.Suggestions[i], 
                            response.Descriptions[i],
                            response.Urls[i], response.Hints[i])));
            }
            writer.AddResults(results);
            return results;
        }

        public bool SupportsKeyword(string keyword)
        {
            return keyword == "?";
        }
    }
}
