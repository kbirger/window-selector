using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WindowSelector.Signalr
{
    public interface IChromeExtension
    {
        Task QueryTabs(string substring);

        Task SetTab(int windowId, int tabId, WindowUpdateInfo updateInfo = null, bool centerWindow = false);

        Task CloseTabs(int tabId);

        Task CloseTabs(int[] tabIds);
    }

    public sealed class WindowUpdateInfo
    {
        [JsonProperty("left")]
        public int? Left { get; set; }
        [JsonProperty("top")]
        public int? Top { get; set; }
        [JsonProperty("width")]
        public int? Width { get; set; }
        [JsonProperty("height")]
        public int? Height { get; set; }

        [JsonProperty("focused")]
        public bool? Focused { get; set; }

        [JsonProperty("drawAttention")]
        public bool? DrawAttention { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("state")]
        public WindowState State { get; set; }
    }

    public enum WindowState
    {
        normal, minimized, maximized, fullscreen, docked
    }
}
