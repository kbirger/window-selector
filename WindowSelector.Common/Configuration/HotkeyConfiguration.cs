using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Newtonsoft.Json;
using WindowSelector.Common.Properties;

namespace WindowSelector.Common.Configuration
{
    public class HotkeyConfigurationConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override bool CanRead => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            HotkeyConfiguration config = (HotkeyConfiguration)value;
            foreach (var setting in config)
            {

                writer.WritePropertyName(setting.HotkeyName);
                serializer.Serialize(writer, setting.Hotkey);
            }
            writer.WriteEndObject();
        }
    }
    [JsonConverter(typeof(HotkeyConfigurationConverter))]
    public class HotkeyConfiguration : IEnumerable<HotkeyConfiguration.HotkeyItem>
    {
        private readonly DirtyManager _dirtyManager;
        private readonly IDictionary<string, HotkeySetting> _hotkeys;
        public HotkeyConfiguration(string propertyName)
            : this(propertyName, GetDefaultDict())
        {
        }

        public HotkeyConfiguration(string propertyName, IDictionary<string, HotkeySetting> hotkeys)
        {
            _dirtyManager = new DirtyManager(propertyName);
            _hotkeys = hotkeys ?? GetDefaultDict();
        }

        public HotkeyConfiguration(string propertyName, HotkeyConfiguration original)
            : this(propertyName, new Dictionary<string, HotkeySetting>(original._hotkeys)) { }

        public IEnumerable<string> GetDirty() => _dirtyManager.GetDirty();
        public void ResetDirty() => _dirtyManager.ResetDirty();

        public HotkeySetting this[string hotkeyName]
        {
            get
            {
                if(_hotkeys.ContainsKey(hotkeyName))
                {
                    return _hotkeys[hotkeyName];
                }
                return new HotkeySetting(System.Windows.Input.Key.None, System.Windows.Input.ModifierKeys.None);
            }
            set
            {
                _hotkeys[hotkeyName] = value;
                _dirtyManager.SetDirty(hotkeyName);
            }
        }
        public IEnumerator<HotkeyItem> GetEnumerator()
        {
            foreach(var dirty in _hotkeys)
            {
                yield return new HotkeyItem(this, dirty.Key);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var dirty in _hotkeys)
            {
                yield return new HotkeyItem(this, dirty.Key);
            }
        }

        public class HotkeyItem : INotifyPropertyChanged
        {
            private readonly HotkeyConfiguration _parent;

            public HotkeyItem(HotkeyConfiguration parent, string hotkeyName)
            {
                _parent = parent;
                HotkeyName = hotkeyName;
            }

            public HotkeySetting Hotkey
            {
                get
                {
                    return _parent[HotkeyName];
                }
                set
                {
                    if(_parent[HotkeyName] != value)
                    {
                        _parent[HotkeyName] = value;
                        OnPropertyChanged();
                    }
                }
            }

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public string HotkeyName { get; private set; }

            public event PropertyChangedEventHandler PropertyChanged;
        }
        private static IDictionary<string, HotkeySetting> GetDefaultDict()
        {
            return new Dictionary<string, HotkeySetting>
            {
                {"activation_key", new HotkeySetting(Key.OemTilde, ModifierKeys.Windows)},
                {"debug_key", new HotkeySetting(Key.OemTilde, ModifierKeys.Windows)},
                {"minimize_key", new HotkeySetting(Key.Down, ModifierKeys.Alt)},
                {"close_key", new HotkeySetting(Key.Delete, ModifierKeys.Alt)},
            };
        }
    }
}
