using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace TSP.DoxygenEditor.Services
{
    class RegistryConfigurationInstance : IConfigurarionInstance
    {
        const string Company = "TSPSoftware";
        const string App = "DoxygenEditor";
        private RegistryKey _rootKey;
        private bool _readOnly;
        public RegistryConfigurationInstance(bool readOnly)
        {
            _readOnly = readOnly;
            var softwareKey = Registry.CurrentUser.OpenSubKey("Software", !readOnly);
            var companyKey = softwareKey.OpenSubKey(Company, !readOnly);
            if (!readOnly)
                companyKey = softwareKey.CreateSubKey(Company);
            if (companyKey != null)
            {
                _rootKey = companyKey.OpenSubKey(App, !readOnly);
                if (!readOnly && _rootKey == null)
                    _rootKey = companyKey.CreateSubKey(App);
            }
        }
        internal void Write(string section, string name, object value)
        {
            Debug.Assert(!_readOnly);
            if (_rootKey != null)
            {
                var sectionKey = _rootKey.OpenSubKey(section, true);
                if (sectionKey == null)
                    sectionKey = _rootKey.CreateSubKey(section);
                if (value != null)
                    sectionKey.SetValue(name, value);
                else
                    if (sectionKey.GetValue(name) != null)
                        sectionKey.DeleteValue(name);
            }
        }
        internal object Read(string section, string name)
        {
            Debug.Assert(_readOnly);
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
        internal void Clear(string section)
        {
            Debug.Assert(!_readOnly);
            if (_rootKey != null)
            {
                var sectionKey = _rootKey.OpenSubKey(section, true);
                if (sectionKey != null)
                {
                    var names = sectionKey.GetValueNames();
                    foreach (var name in names)
                        sectionKey.DeleteValue(name);
                }
            }
        }
        public void Dispose()
        {
            _rootKey?.Dispose();
        }
    }
    class RegistryConfigurationService : IConfigurationService
    {
        public IConfigurarionInstance Create(bool readOnly)
        {
            RegistryConfigurationInstance result = new RegistryConfigurationInstance(readOnly);
            return (result);
        }
        public void Clear(IConfigurarionInstance instance, string section)
        {
            var regInstance = (RegistryConfigurationInstance)instance;
            regInstance.Clear(section);
        }
        public string ReadString(IConfigurarionInstance instance, string section, string name)
        {
            var regInstance = (RegistryConfigurationInstance)instance;
            string result = (string)regInstance.Read(section, name);
            return (result);
        }
        public int ReadInt(IConfigurarionInstance instance, string section, string name, int defaultValue)
        {
            var regInstance = (RegistryConfigurationInstance)instance;
            int? value = (int?)regInstance.Read(section, name);
            if (value.HasValue)
                return (value.Value);
            return (defaultValue);
        }
        public bool ReadBool(IConfigurarionInstance instance, string section, string name, bool defaultValue)
        {
            var regInstance = (RegistryConfigurationInstance)instance;
            int? value = (int?)regInstance.Read(section, name);
            if (value.HasValue)
                return (value.Value == 1);
            return (defaultValue);
        }
        public void WriteString(IConfigurarionInstance instance, string section, string name, string value)
        {
            var regInstance = (RegistryConfigurationInstance)instance;
            regInstance.Write(section, name, value);
        }
        public void WriteInt(IConfigurarionInstance instance, string section, string name, int value)
        {
            var regInstance = (RegistryConfigurationInstance)instance;
            regInstance.Write(section, name, value);
        }
        public void WriteBool(IConfigurarionInstance instance, string section, string name, bool value)
        {
            var regInstance = (RegistryConfigurationInstance)instance;
            regInstance.Write(section, name, value ? 1 : 0);
        }
    }
}
