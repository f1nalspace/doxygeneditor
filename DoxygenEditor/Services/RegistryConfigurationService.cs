using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TSP.DoxygenEditor.Services
{
    class RegistryConfigurationInstance : BaseConfigurationInstance
    {
        private RegistryKey _rootKey;

        public RegistryConfigurationInstance(bool isReadOnly, ConfigurationServiceConfig config) : base(isReadOnly, config)
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

        protected override void PublishWrite(string section, string name, object value)
        {
            Debug.Assert(!IsReadOnly);
            if (_rootKey != null)
            {
                var sectionKey = _rootKey.OpenSubKey(section, true);
                if (sectionKey == null)
                    sectionKey = _rootKey.CreateSubKey(section);
                if (value != null)
                {
                    Type valueType = value.GetType();
                    if (typeof(bool).Equals(valueType))
                        sectionKey.SetValue(name, (bool)value ? 1 : 0);
                    else
                        sectionKey.SetValue(name, value);
                }
                else
                    if (sectionKey.GetValue(name) != null)
                    sectionKey.DeleteValue(name);
            }
        }
        public override object ReadRaw(string section, string name)
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

        public override string ReadString(string section, string name)
        {
            string result = (string)ReadRaw(section, name);
            return (result);
        }
        public override int ReadInt(string section, string name, int defaultValue)
        {
            int? value = (int?)ReadRaw(section, name);
            if (value.HasValue)
                return (value.Value);
            return (defaultValue);
        }
        public override bool ReadBool(string section, string name, bool defaultValue)
        {
            int? value = (int?)ReadRaw(section, name);
            if (value.HasValue)
                return (value.Value == 1);
            return (defaultValue);
        }

        public override void Dispose()
        {
            _rootKey?.Dispose();
        }
    }
    class RegistryConfigurationService : IConfigurationService
    {
        public IConfigurarionInstance Create(bool readOnly, ConfigurationServiceConfig config)
        {
            RegistryConfigurationInstance result = new RegistryConfigurationInstance(readOnly, config);
            return (result);
        }
    }
}
