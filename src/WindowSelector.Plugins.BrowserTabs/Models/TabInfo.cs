using Newtonsoft.Json;

namespace WindowSelector.Plugins.BrowserTabs.Models
{
    public class TabInfo
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("windowId")]
        public int WindowId { get; set; }
        [JsonProperty("favIconUrl")]
        public string FavIconUrl { get; set; }

        [JsonProperty("thumb")]
        public string Thumb { get; set; }
    }
}
