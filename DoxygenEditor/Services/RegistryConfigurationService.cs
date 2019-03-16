using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TSP.DoxygenEditor.Services
{
    class RegistryConfigurationService : IConfigurationService
    {
        class RegistryConfiguration : AbstractConfigurationPublisher, IConfigurarionReader
        {
            private RegistryKey _rootKey;

            public RegistryConfiguration(bool isReadOnly, ConfigurationServiceConfig config) : base(isReadOnly, config)
            {
                var softwareKey = Registry.CurrentUser.OpenSubKey("Software", !isReadOnly);
                var companyKey = softwareKey.OpenSubKey(config.CompanyName, !isReadOnly);
                if (!isReadOnly)
                    companyKey = softwareKey.CreateSubKey(config.CompanyName);
                if (companyKey != null)
                {
                    _rootKey = companyKey.OpenSubKey(config.ProductName, !isReadOnly);
                    if (!isReadOnly && _rootKey == null)
                        _rootKey = companyKey.CreateSubKey(config.ProductName);
                }
            }

            protected override void ClearAll()
            {
                var names = _rootKey.GetSubKeyNames();
                foreach (var name in names)
                    _rootKey.DeleteSubKeyTree(name);
            }

            protected override void PublishWrite(WriteKind kind, string section, string name, object value)
            {
                Debug.Assert(!IsReadOnly);
                if (_rootKey != null)
                {
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
                    else
                    {
                        if (sectionKey.GetSubKeyNames().Contains(name))
                            sectionKey.DeleteSubKeyTree(name);
                        if (sectionKey.GetValue(name) != null)
                            sectionKey.DeleteValue(name);
                    }
                }
            }
            public object ReadRaw(string section, string name)
            {
                Debug.Assert(IsReadOnly);
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

            public string ReadString(string section, string name)
            {
                string result = (string)ReadRaw(section, name);
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
                Debug.Assert(IsReadOnly);
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
        public IConfigurarionReader CreateReader(ConfigurationServiceConfig config)
        {
            RegistryConfiguration result = new RegistryConfiguration(true, config);
            return (result);
        }
        public IConfigurarionWriter CreateWriter(ConfigurationServiceConfig config)
        {
            RegistryConfiguration result = new RegistryConfiguration(false, config);
            return (result);
        }
    }
}
