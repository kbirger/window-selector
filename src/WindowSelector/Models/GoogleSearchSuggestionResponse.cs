using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WindowSelector.Properties;

namespace WindowSelector.Models
{
    [JsonConverter(typeof(GoogleSearchSuggestionConverter))]
    public class GoogleSearchSuggestionResponse
    {

        public GoogleSearchSuggestionResponse([NotNull] string query, [NotNull] string[] suggestions, [NotNull] string[] descriptions, [NotNull] string[] urls, [NotNull]IDictionary<string, object> extras)
        {
            Array.Resize(ref descriptions, suggestions.Length);
            Array.Resize(ref urls, suggestions.Length);
            Query = query;
            Suggestions = suggestions;
            Descriptions = descriptions;
            Urls = urls;
            Extras = extras;
            LoadHints();
        }

        public string Query { get; private set; }

        public string[] Suggestions { get; private set; }

        public string[] Descriptions { get; private set; }

        public string[] Urls { get; private set; }

        public IDictionary<string, object> Extras { get; private set; }

        public string[] Hints { get; private set; }

        private void LoadHints()
        {
            object temp;
                object[] tempArray;
                if (!Extras.TryGetValue("google:suggesttype", out temp) || (tempArray = temp as object[]) == null)
                {
                    Hints = new string[Suggestions.Length];
                    return;
                }

                var strings = tempArray.Cast<string>();
            if (tempArray.Length < Suggestions.Length)
            {
                strings = strings.Union(Enumerable.Repeat("", Suggestions.Length - tempArray.Length));
            }

            Hints = strings.ToArray();

        }
    }

    public class GoogleSearchSuggestionConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var json = JArray.Load(reader);

            if (json.Count < 2)
            {
                throw new ArgumentException();
            }

            var query = json[0].ToString();
            var suggestions = (json[1] as JArray ?? new JArray());
            var descriptions = (json.Count >= 3 ? json[2] as JArray : new JArray()) ?? new JArray();
            var urls = (json.Count >= 4 ? json[3] as JArray : new JArray()) ?? new JArray();
            var extras = ((json.Count >= 5 ? json[4] as JObject : new JObject()) ?? new JObject()).ToObject<Dictionary<string, object>>();

            var resultSuggestions = suggestions.Select(v => v.Value<string>()).ToArray();
            var resultDescriptions = descriptions.Count == suggestions.Count
                ? descriptions.Select(v => v.Value<string>()).ToArray()
                : new string[suggestions.Count];
            var resultUrls = urls.Count == suggestions.Count
                ? urls.Select(v => v.Value<string>()).ToArray()
                : new string[suggestions.Count];
            
            return new GoogleSearchSuggestionResponse(query, resultSuggestions, resultDescriptions, resultUrls, extras);
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}
