using Microsoft.Win32;
using System.Collections.Generic;
using System.Diagnostics;

namespace TSP.DoxygenEditor.Services
{
    class RegistryConfigurationStore : AbstractConfigurationPublisher, IConfigurarionReader, IConfigurarionWriter
    {
        private RegistryKey _rootKey;
        private readonly string _baseName;

        public RegistryConfigurationStore(string baseName) : base()
        {
            _baseName = baseName;
        }

        public bool Load(string filePath)
        {
            var softwareKey = Registry.CurrentUser.OpenSubKey("Software", false);
            string[] names = filePath.Split('/');
            _rootKey = null;
            var curKey = softwareKey;
            for (int i = 0; i < names.Length; ++i)
            {
                var newKey = curKey.OpenSubKey(names[i], false);
                if (newKey == null)
                {
                    _rootKey = null;
                    break;
                }
                _rootKey = newKey;
                curKey = newKey;
            }
            return (_rootKey != null);
        }

        public void Save(string filePath)
        {
            // Create keys
            var softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
            string[] names = filePath.Split('/');
            _rootKey = null;
            var curKey = softwareKey;
            for (int i = 0; i < names.Length; ++i)
            {
                var newKey = curKey.OpenSubKey(names[i], true);
                if (newKey == null)
                    newKey = curKey.CreateSubKey(names[i]);
                _rootKey = newKey;
                curKey = newKey;
            }
            Debug.Assert(_rootKey != null);

            // Clear registry
            var existingKeyNames = _rootKey.GetSubKeyNames();
            foreach (var existingKeyName in existingKeyNames)
                _rootKey.DeleteSubKeyTree(existingKeyName);

            foreach (var writeEntry in WriteEntries)
            {
                string section = writeEntry.Section;
                string name = writeEntry.Name;
                object value = writeEntry.Value;
                WriteKind kind = writeEntry.Kind;
                var sectionKey = _rootKey.OpenSubKey(section, true);
                if (sectionKey == null)
                    sectionKey = _rootKey.CreateSubKey(section);
                if (value != null)
                {
                    switch (kind)
                    {
                        case WriteKind.Bool:
                            sectionKey.SetValue(name, (bool)value ? 1 : 0);
                            break;
                        case WriteKind.Int:
                            sectionKey.SetValue(name, (int)value);
                            break;
                        case WriteKind.String:
                            sectionKey.SetValue(name, (string)value);
                            break;
                        case WriteKind.List:
                            {
                                var list = (List<string>)value;
                                var listKey = sectionKey.OpenSubKey(name, true);
                                sectionKey.DeleteSubKeyTree(name);
                                listKey = sectionKey.CreateSubKey(name);
                                listKey.SetValue("Count", list.Count);
                                for (int i = 0; i < list.Count; ++i)
                                    listKey.SetValue("Item", list[i]);
                            }
                            break;
                        default:
                            sectionKey.SetValue(name, value.ToString());
                            break;
                    }
                }
            }
        }

        public object ReadRaw(string section, string name)
        {
            if (_rootKey != null)
            {
                var sectionKey = _rootKey.OpenSubKey(section, false);
                if (sectionKey != null)
                {
                    object value = sectionKey.GetValue(name);
                    return (value);
                }
            }
            return (null);
        }

        public string ReadString(string section, string name, string defaultValue)
        {
            string result = (string)ReadRaw(section, name);
            if (result == null)
                result = defaultValue;
            return (result);
        }
        public int ReadInt(string section, string name, int defaultValue)
        {
            int? value = (int?)ReadRaw(section, name);
            if (value.HasValue)
                return (value.Value);
            return (defaultValue);
        }
        public bool ReadBool(string section, string name, bool defaultValue)
        {
            int? value = (int?)ReadRaw(section, name);
            if (value.HasValue)
                return (value.Value == 1);
            return (defaultValue);
        }
        public IEnumerable<string> ReadList(string section, string name)
        {
            if (_rootKey != null)
            {
                var sectionKey = _rootKey.OpenSubKey(section, false);
                if (sectionKey != null)
                {
                    var listKey = sectionKey.OpenSubKey(name, false);
                    if (listKey != null)
                    {
                        int? count = (int?)listKey.GetValue("Count");
                        if (count.HasValue)
                        {
                            for (int i = 0; i < count.Value; ++i)
                            {
                                string item = (string)listKey.GetValue("Item" + i);
                                yield return item;
                            }
                        }
                    }
                }
            }
        }

        public override void Dispose()
        {
            _rootKey?.Dispose();
        }
    }
}
