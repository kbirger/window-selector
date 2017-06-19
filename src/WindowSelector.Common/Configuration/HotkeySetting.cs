using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WindowSelector.Common.Configuration
{
    /// <summary>
    /// Represents a hotkey combination
    /// </summary>
    public class HotkeySetting
    {
        [JsonConstructor]
        public HotkeySetting(Key key, ModifierKeys modifiers)
        {
            Key = key;
            Modifiers = modifiers;
        }
        public Key Key { get; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ModifierKeys Modifiers { get; }


        public override bool Equals(object obj)
        {
            HotkeySetting other = obj as HotkeySetting;
            if(other == null)
            {
                return false;
            }
            return Key == other.Key && Modifiers == other.Modifiers;
        }

        public override int GetHashCode()
        {
            return (int)Key ^ (int)Modifiers;
        }

    }
}