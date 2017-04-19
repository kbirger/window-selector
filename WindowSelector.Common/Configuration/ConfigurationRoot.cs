using System.Collections.Generic;
using System.IO;
using System.Linq;
using SystemInterface.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WindowSelector.Common.Configuration
{
    public class ConfigurationRoot
    {
        [JsonConstructor]
        public ConfigurationRoot(IDictionary<string, HotkeySetting> hotkeys)
        {
            _hotkeys = new HotkeyConfiguration("Hotkeys", hotkeys);
            _dirtyManager = new DirtyManager();
            _blacklist = new DirtyHashSet(_dirtyManager.GetDirtyHandle("Blacklist"));
            _whitelist = new DirtyHashSet(_dirtyManager.GetDirtyHandle("Whitelist"));
        }

        public ConfigurationRoot(ConfigurationRoot original)
        {
            _dirtyManager = new DirtyManager();
            _blacklist = new DirtyHashSet(_dirtyManager.GetDirtyHandle("Blacklist"), original.Blacklist);
            _whitelist = new DirtyHashSet(_dirtyManager.GetDirtyHandle("Whitelist"), original.Whitelist);
            _hotkeys = new HotkeyConfiguration("Hotkeys", original.Hotkeys);

            _dirtyManager.ResetDirty();
            _hotkeys.ResetDirty();
        }
        public ConfigurationRoot()
            : this(new Dictionary<string, HotkeySetting>())
        {}

        public HotkeyConfiguration Hotkeys => _hotkeys;

        /// <summary>
        /// Gets the value representing list of items to be excluded from results
        /// </summary>
        public ICollection<string> Blacklist => _blacklist;

        /// <summary>
        /// Gets the value representing list of items to be included in the results even if other criteria don't match
        /// </summary>
        /// <remarks>For Window results, this is tied to the window style</remarks>
        public ICollection<string> Whitelist => _whitelist;

        

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                NullValueHandling = NullValueHandling.Include,
                Formatting = Formatting.Indented
            };

        private readonly DirtyHashSet _blacklist;
        private readonly DirtyHashSet _whitelist;
        private readonly DirtyManager _dirtyManager;
        private readonly HotkeyConfiguration _hotkeys;

        /// <summary>
        /// Saves the current configuration state using the given file system abstraction
        /// </summary>
        /// <param name="fileSystem"></param>
        /// <returns></returns>
        public string[] Save(IFile fileSystem)
        {
            var dirtyItems = _dirtyManager.GetDirty()
                             .Union(_hotkeys.GetDirty())
                             .ToArray();

            var serializer = JsonSerializer.CreateDefault(Settings);
            using (IStreamWriter file = fileSystem.CreateText("config.json"))
            {
                serializer.Serialize(file.StreamWriterInstance, this);
            }
            _dirtyManager.ResetDirty();
            _hotkeys.ResetDirty();
            return dirtyItems;
        }

        /// <summary>
        /// Loads configuration using given filesystem abstraction
        /// </summary>
        /// <param name="fileSystem"></param>
        /// <returns></returns>
        public static ConfigurationRoot Load(IFile fileSystem)
        {
            try
            {
                var serializer = JsonSerializer.CreateDefault(Settings);
                using (IStreamReader file = fileSystem.OpenText("config.json"))
                using (JsonReader reader = new JsonTextReader(file.StreamReaderInstance))
                {
                    var ret = serializer.Deserialize<ConfigurationRoot>(reader);
                    ret._dirtyManager.ResetDirty();
                    ret._hotkeys.ResetDirty();
                    return ret;
                }
            }
            catch (FileNotFoundException)
            {
                var ret = new ConfigurationRoot();
                ret.Save(fileSystem);
                return ret;
            }
            catch (JsonSerializationException)
            {
                try
                {
                    fileSystem.Delete("config.json");
                }
                catch
                {
                }
                var ret = new ConfigurationRoot();
                ret.Save(fileSystem);
                return ret;
            }
        }
    }
}



