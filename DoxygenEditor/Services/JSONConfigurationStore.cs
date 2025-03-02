using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using TSP.DoxygenEditor.Utils;

namespace TSP.DoxygenEditor.Services
{
    class JSONConfigurationStore : AbstractConfigurationPublisher, IConfigurarionReader, IConfigurarionWriter
    {
        private readonly string _baseName;
        private readonly IConfigurationConverter _converter;
        private JsonDocument _doc;
        private JsonElement? _rootNode;

        public JSONConfigurationStore(string baseName, IConfigurationConverter converter = null) : base()
        {
            _baseName = baseName;
            _converter = converter;
            _doc = null;
            _rootNode = null;
        }

        public Result<bool> Load(string filePath)
        {
            _doc = null;
            if (!File.Exists(filePath))
                return new Result<bool>(new FileNotFoundException($"The JSON file '{filePath}' was not found"));
            try
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                _doc = JsonDocument.Parse(bytes);
                _rootNode = _doc.RootElement;
                return new Result<bool>(true);
            }
            catch (Exception e)
            {
                _doc = null;
                _rootNode = null;
                return new Result<bool>(new IOException($"Failed to load JSON file '{filePath}'", e));
            }
        }

        public void Save(string filePath)
        {
            JsonObject root = new JsonObject();

            foreach (WriteEntry writeEntry in WriteEntries)
            {
                string section = writeEntry.Section;
                string name = writeEntry.Name;
                object value = writeEntry.Value;
                WriteKind kind = writeEntry.Kind;
                string[] s = section.Split('/');

                JsonObject curObj = root;
                for (int i = 0; i < s.Length; ++i)
                {
                    string thisSection = s[i];
                    JsonObject partObj = curObj[thisSection] as JsonObject;
                    if (partObj == null)
                    {
                        partObj = new JsonObject();
                        curObj.Add(thisSection, partObj);
                    }
                    curObj = partObj;
                }

                if (curObj == root)
                    throw new InvalidOperationException($"Cannot write entry '{writeEntry}' to root!");

                switch (kind)
                {
                    case WriteKind.Bool:
                        curObj[name] = (bool)value;
                        break;
                    case WriteKind.Int:
                        curObj[name] = (int)value;
                        break;
                    case WriteKind.Double:
                        curObj[name] = (double)value;
                        break;
                    case WriteKind.String:
                        curObj[name] = (string)value;
                        break;
                    case WriteKind.List:
                        {
                            JsonArray newArray = new JsonArray();
                            List<string> list = (List<string>)value;
                            foreach (string item in list)
                                newArray.Add(item);
                            curObj[name] = newArray;
                        }
                        break;
                    case WriteKind.Dictionary:
                        {
                            JsonObject newObj = new JsonObject();
                            IDictionary<string, object> dict = (IDictionary<string, object>)value;
                            foreach (KeyValuePair<string, object> pair in dict)
                            {
                                Type t = pair.Value.GetType();
                                if (t.IsValueType)
                                {
                                    System.Reflection.PropertyInfo[] properties = t.GetProperties();
                                    foreach (System.Reflection.PropertyInfo property in properties)
                                    {
                                        Type propType = property.DeclaringType;
                                        string propName = property.Name;
                                        object propValue = property.GetValue(pair.Value);
                                        if (_converter != null)
                                        {
                                            string convertedValue = _converter.ConvertToString(propValue);
                                            newObj[propName] = convertedValue;
                                        }
                                        else
                                        {
                                            if (typeof(string).Equals(propType))
                                            {
                                                string stringValue = (string)propValue;
                                                newObj[propName] = stringValue;
                                            }
                                            else if (typeof(double).Equals(propType))
                                            {
                                                double doubleValue = (double)propValue;
                                                newObj[propName] = doubleValue;
                                            }
                                            else if (typeof(int).Equals(propType))
                                            {
                                                int intValue = (int)propValue;
                                                newObj[propName] = intValue;
                                            }
                                            else if (typeof(bool).Equals(propType))
                                            {
                                                bool boolValue = (bool)propValue;
                                                newObj[propName] = boolValue;
                                            }
                                            else
                                                newObj[propName] = null;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
            using (var stream = File.Create(filePath))
            {
                using (Utf8JsonWriter writer = new Utf8JsonWriter(stream, new JsonWriterOptions() { Indented = true }))
                    root.WriteTo(writer, options);
            }
        }

        private JsonElement? FindElementBySection(JsonElement? root, string section)
        {
            if (root == null)
                return null;
            JsonElement cur = root.Value;
            string[] s = section.Split('/');
            for (int i = 0; i < s.Length; ++i)
            {
                string part = s[i];
                if (!cur.TryGetProperty(part, out JsonElement prop))
                    return null;
                cur = prop;
            }
            return cur;
        }

        public bool ReadBool(string section, string name, bool defaultValue)
        {
            JsonElement? sectionElement = FindElementBySection(_rootNode, section);
            if (sectionElement.HasValue &&
                sectionElement.Value.TryGetProperty(name, out JsonElement value) &&
                (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False))
                return value.GetBoolean();
            return defaultValue;
        }
        public bool ReadBool(string section, Expression<Func<object>> nameExpression, bool defaultValue)
            => ReadBool(section, ReflectionUtils.GetName(nameExpression), defaultValue);

        public int ReadInt(string section, string name, int defaultValue)
        {
            JsonElement? sectionElement = FindElementBySection(_rootNode, section);
            if (sectionElement.HasValue &&
                sectionElement.Value.TryGetProperty(name, out JsonElement value) &&
                value.ValueKind == JsonValueKind.Number)
                return value.GetInt32();
            return defaultValue;
        }
        public int ReadInt(string section, Expression<Func<object>> nameExpression, int defaultValue)
            => ReadInt(section, ReflectionUtils.GetName(nameExpression), defaultValue);

        public double ReadDouble(string section, string name, double defaultValue)
        {
            JsonElement? sectionElement = FindElementBySection(_rootNode, section);
            if (sectionElement.HasValue &&
                sectionElement.Value.TryGetProperty(name, out JsonElement value) &&
                value.ValueKind == JsonValueKind.Number)
                return value.GetDouble();
            return defaultValue;
        }
        public double ReadDouble(string section, Expression<Func<object>> nameExpression, double defaultValue)
            => ReadDouble(section, ReflectionUtils.GetName(nameExpression), defaultValue);

        public string ReadString(string section, string name, string defaultValue)
        {
            JsonElement? sectionElement = FindElementBySection(_rootNode, section);
            if (sectionElement.HasValue &&
                sectionElement.Value.TryGetProperty(name, out JsonElement value) &&
                value.ValueKind == JsonValueKind.String)
                return value.GetString();
            return defaultValue;
        }
        public string ReadString(string section, Expression<Func<object>> nameExpression, string defaultValue)
            => ReadString(section, ReflectionUtils.GetName(nameExpression), defaultValue);

        public IEnumerable<string> ReadList(string section, string name)
        {
            JsonElement? sectionElement = FindElementBySection(_rootNode, section);
            if (sectionElement.HasValue &&
                sectionElement.Value.TryGetProperty(name, out JsonElement foundElement) &&
                foundElement.ValueKind == JsonValueKind.Array)
            {
                JsonElement.ArrayEnumerator itemEnumerator = foundElement.EnumerateArray();
                foreach (JsonElement item in itemEnumerator)
                {
                    string value = item.GetString();
                    yield return value;
                }
            }
        }
        public IEnumerable<string> ReadList(string section, Expression<Func<object>> nameExpression)
            => ReadList(section, ReflectionUtils.GetName(nameExpression));

        public IEnumerable<KeyValuePair<string, TValue>> ReadDictionary<TValue>(string section, string name) where TValue : struct
        {
            Type structType = typeof(TValue);

            System.Reflection.PropertyInfo[] properties = structType.GetProperties(System.Reflection.BindingFlags.Public);

            JsonElement? sectionElement = FindElementBySection(_rootNode, section);
            if (sectionElement.HasValue &&
                sectionElement.Value.TryGetProperty(name, out JsonElement foundElement) &&
                foundElement.ValueKind == JsonValueKind.Object)
            {
                foreach (System.Reflection.PropertyInfo property in properties)
                {
                    Type propType = property.DeclaringType;
                    string propName = property.Name;
                    TValue structValue = (TValue)Activator.CreateInstance(structType);
                    if (foundElement.TryGetProperty(propName, out JsonElement foundProperty))
                    {
                        object convertedValue = null;
                        if (_converter != null)
                        {
                            string stringValue = foundProperty.GetRawText();
                            convertedValue = _converter.ConvertFromString(stringValue, propType);
                        }
                        else
                        {
                            if (typeof(string).Equals(propType) && foundProperty.ValueKind == JsonValueKind.String)
                                convertedValue = foundProperty.GetString();
                            else if (typeof(int).Equals(propType) && foundProperty.ValueKind == JsonValueKind.Number)
                                convertedValue = foundProperty.GetInt32();
                            else if (typeof(double).Equals(propType) && foundProperty.ValueKind == JsonValueKind.Number)
                                convertedValue = foundProperty.GetDouble();
                            else if (typeof(bool).Equals(propType) && (foundProperty.ValueKind == JsonValueKind.False || foundProperty.ValueKind == JsonValueKind.True))
                                convertedValue = foundProperty.GetBoolean();
                        }
                        property.SetValue(structValue, convertedValue);
                    }
                    yield return new KeyValuePair<string, TValue>(propName, structValue);
                }
            }
        }
        public IEnumerable<KeyValuePair<string, TValue>> ReadDictionary<TValue>(string section, Expression<Func<object>> nameExpression) where TValue : struct
            => ReadDictionary<TValue>(section, ReflectionUtils.GetName(nameExpression));

        #region IDisposable Support
        protected virtual void DisposeManaged()
        {
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
        ~JSONConfigurationStore()
        {
            Dispose(false);
        }
        #endregion

    }
}
