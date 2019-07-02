using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using TSP.DoxygenEditor.Utils;

namespace TSP.DoxygenEditor.Services
{
    class RegistryConfigurationStore : AbstractConfigurationPublisher, IConfigurarionReader, IConfigurarionWriter
    {
        private RegistryKey _rootKey;
        private readonly string _baseName;
        private readonly IConfigurationConverter _converter;

        public RegistryConfigurationStore(string baseName, IConfigurationConverter converter = null) : base()
        {
            _baseName = baseName;
            _converter = converter;
        }

        public bool Load(string filePath)
        {
            RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", false);
            string[] names = filePath.Split('/');
            _rootKey = null;
            RegistryKey curKey = softwareKey;
            for (int i = 0; i < names.Length; ++i)
            {
                RegistryKey newKey = curKey.OpenSubKey(names[i], false);
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
            RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
            string[] names = filePath.Split('/');
            _rootKey = null;
            RegistryKey curKey = softwareKey;
            for (int i = 0; i < names.Length; ++i)
            {
                RegistryKey newKey = curKey.OpenSubKey(names[i], true);
                if (newKey == null)
                    newKey = curKey.CreateSubKey(names[i]);
                _rootKey = newKey;
                curKey = newKey;
            }
            Debug.Assert(_rootKey != null);

            // Clear registry
            string[] existingKeyNames = _rootKey.GetSubKeyNames();
            foreach (string existingKeyName in existingKeyNames)
                _rootKey.DeleteSubKeyTree(existingKeyName);

            foreach (WriteEntry writeEntry in WriteEntries)
            {
                string section = writeEntry.Section;
                string name = writeEntry.Name;
                object value = writeEntry.Value;
                WriteKind kind = writeEntry.Kind;
                RegistryKey sectionKey = _rootKey.OpenSubKey(section, true);
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
                                List<string> list = (List<string>)value;
                                RegistryKey listKey = sectionKey.OpenSubKey(name, true);
                                sectionKey.DeleteSubKeyTree(name);
                                listKey = sectionKey.CreateSubKey(name);
                                listKey.SetValue("Count", list.Count);
                                for (int i = 0; i < list.Count; ++i)
                                    listKey.SetValue("Item", list[i]);
                            }
                            break;
                        case WriteKind.Dictionary:
                            {
                                IDictionary<string, object> dict = (IDictionary<string, object>)value;
                                RegistryKey listKey = sectionKey.OpenSubKey(name, true);
                                sectionKey.DeleteSubKeyTree(name);
                                listKey = sectionKey.CreateSubKey(name);

                                foreach (KeyValuePair<string, object> pair in dict)
                                {
                                    Type valueType = pair.Value.GetType();
                                    if (valueType.IsValueType)
                                    {
                                        System.Reflection.PropertyInfo[] properties = valueType.GetProperties(System.Reflection.BindingFlags.Public);
                                        foreach (System.Reflection.PropertyInfo property in properties)
                                        {
                                            Type propType = property.DeclaringType;
                                            string propName = property.Name;
                                            object propValue = property.GetValue(pair.Value);
                                            string convertedValue = null;
                                            if (_converter != null)
                                                convertedValue = _converter.ConvertToString(propValue);
                                            else
                                            {
                                                if (typeof(string).Equals(propType))
                                                    convertedValue = (string)propValue;
                                            }
                                            if (!string.IsNullOrEmpty(convertedValue))
                                                listKey.SetValue(propName, convertedValue);
                                        }
                                    }
                                }
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
                RegistryKey sectionKey = _rootKey.OpenSubKey(section, false);
                if (sectionKey != null)
                {
                    object value = sectionKey.GetValue(name);
                    return (value);
                }
            }
            return (null);
        }
        public object ReadRaw(string section, Expression<Func<object>> nameExpression)
        {
            return ReadRaw(section, ReflectionUtils.GetName(nameExpression));
        }

        public string ReadString(string section, string name, string defaultValue)
        {
            string result = (string)ReadRaw(section, name);
            if (result == null)
                result = defaultValue;
            return (result);
        }
        public string ReadString(string section, Expression<Func<object>> nameExpression, string defaultValue)
        {
            return ReadString(section, ReflectionUtils.GetName(nameExpression), defaultValue);
        }

        public int ReadInt(string section, string name, int defaultValue)
        {
            int? value = (int?)ReadRaw(section, name);
            if (value.HasValue)
                return (value.Value);
            return (defaultValue);
        }
        public int ReadInt(string section, Expression<Func<object>> nameExpression, int defaultValue)
        {
            return ReadInt(section, ReflectionUtils.GetName(nameExpression), defaultValue);
        }

        public double ReadDouble(string section, string name, double defaultValue)
        {
            double? value = (double?)ReadRaw(section, name);
            if (value.HasValue)
                return (value.Value);
            return (defaultValue);
        }
        public double ReadDouble(string section, Expression<Func<object>> nameExpression, double defaultValue)
        {
            return ReadDouble(section, ReflectionUtils.GetName(nameExpression), defaultValue);
        }

        public bool ReadBool(string section, string name, bool defaultValue)
        {
            int? value = (int?)ReadRaw(section, name);
            if (value.HasValue)
                return (value.Value == 1);
            return (defaultValue);
        }
        public bool ReadBool(string section, Expression<Func<object>> nameExpression, bool defaultValue)
        {
            return ReadBool(section, ReflectionUtils.GetName(nameExpression), defaultValue);
        }
        public IEnumerable<string> ReadList(string section, string name)
        {
            if (_rootKey != null)
            {
                RegistryKey sectionKey = _rootKey.OpenSubKey(section, false);
                if (sectionKey != null)
                {
                    RegistryKey listKey = sectionKey.OpenSubKey(name, false);
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
        public IEnumerable<string> ReadList(string section, Expression<Func<object>> nameExpression)
        {
            return ReadList(section, ReflectionUtils.GetName(nameExpression));
        }

        public IEnumerable<KeyValuePair<string, TValue>> ReadDictionary<TValue>(string section, string name) where TValue : struct
        {
            if (_rootKey != null)
            {
                RegistryKey sectionKey = _rootKey.OpenSubKey(section, false);
                if (sectionKey != null)
                {
                    RegistryKey listKey = sectionKey.OpenSubKey(name, false);
                    if (listKey != null)
                    {
                        int? count = (int?)listKey.GetValue("Count");
                        if (count.HasValue)
                        {
                            for (int i = 0; i < count.Value; ++i)
                            {
                                string key = (string)listKey.GetValue("Key" + i);
                                if (!string.IsNullOrWhiteSpace(key))
                                {
                                    Type structType = typeof(TValue);
                                    TValue structValue = (TValue)Activator.CreateInstance(structType);
                                    System.Reflection.PropertyInfo[] properties = structType.GetProperties(System.Reflection.BindingFlags.Public);
                                    foreach (System.Reflection.PropertyInfo property in properties)
                                    {
                                        Type propType = property.DeclaringType;
                                        string propName = property.Name;
                                        string stringValue = (string)listKey.GetValue("V" + propName + i);
                                        object convertedValue = null;
                                        if (_converter != null)
                                            convertedValue = _converter.ConvertFromString(stringValue, propType);
                                        else
                                        {
                                            if (typeof(string).Equals(propType))
                                                convertedValue = stringValue;
                                            else if (typeof(int).Equals(propType))
                                                convertedValue = int.Parse(stringValue);
                                            else if (typeof(bool).Equals(propType))
                                                convertedValue = "true".Equals(stringValue, StringComparison.InvariantCultureIgnoreCase);
                                        }
                                        property.SetValue(structValue, convertedValue);
                                    }
                                    yield return new KeyValuePair<string, TValue>(key, structValue);
                                }
                            }
                        }
                    }
                }
            }
        }
        public IEnumerable<KeyValuePair<string, TValue>> ReadDictionary<TValue>(string section, Expression<Func<object>> nameExpression) where TValue : struct
        {
            return ReadDictionary<TValue>(section, ReflectionUtils.GetName(nameExpression));
        }

        #region IDisposable Support
        protected virtual void DisposeManaged()
        {
            _rootKey?.Dispose();
        }
        protected virtual void DisposeUnmanaged()
        {
        }
        private void Dispose(bool disposing)
        {
            if (disposing)
                DisposeManaged();
            DisposeUnmanaged();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~RegistryConfigurationStore()
        {
            Dispose(false);
        }
        #endregion
    }
}
