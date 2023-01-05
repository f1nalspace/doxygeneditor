using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using System.Xml;
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

        public bool Load(string filePath)
        {
            _doc = null;
            if (!File.Exists(filePath))
                return false;
            try
            {
                var bytes = File.ReadAllBytes(filePath);
                _doc = JsonDocument.Parse(bytes);
                _rootNode = _doc.RootElement;
                return (true);
            }
            catch
            {
                _doc = null;
                _rootNode = null;
            }
            return (false);
        }

        class WriteNode : IEnumerable<WriteNode>
        {
            public WriteNode Parent { get; }
            public string Name { get; }
            public WriteKind Kind { get; }
            public object Value { get; }
            public IEnumerable<WriteNode> Children => _children;
            private readonly List<WriteNode> _children = new List<WriteNode>();
            private readonly Dictionary<string, WriteNode> _childMap = new Dictionary<string, WriteNode>();

            public bool HasChildren => _children.Count > 0;

            public WriteNode(WriteNode parent, string name, WriteKind kind, object value)
            {
                Parent = parent;
                Name = name;
                Kind = kind;
                Value = value;
            }

            public WriteNode Add(string name, WriteKind kind, object value)
            {
                if (_childMap.ContainsKey(name))
                    throw new DuplicateNameException($"There is already a node with the key '{name}' in '{Name}'");
                WriteNode node = new WriteNode(this, name, kind, value);
                _children.Add(node);
                _childMap.Add(name, node);
                return node;
            }

            public WriteNode Get(string name)
            {
                _childMap.TryGetValue(name, out WriteNode node);
                return node;
            }

            public IEnumerator<WriteNode> GetEnumerator() => _children.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public override string ToString() => $"{Name} as [{Kind}] => {Value}";
        }

        public void Save(string filePath)
        {
            WriteNode root = new WriteNode(null, "Root", WriteKind.None, null);

            foreach (WriteEntry writeEntry in WriteEntries)
            {
                string section = writeEntry.Section;
                string name = writeEntry.Name;
                object value = writeEntry.Value;
                WriteKind kind = writeEntry.Kind;
                string[] s = section.Split('/');
                WriteNode cur = root;
                for (int i = 0; i < s.Length; ++i)
                {
                    string thisSection = s[i];
                    WriteNode sectionNode = cur.Get(thisSection);
                    if (sectionNode == null)
                        sectionNode = cur.Add(thisSection, WriteKind.None, null);
                    cur = sectionNode;
                }
                cur.Add(name, kind, value);
            }

            using var stream = File.Create(filePath);

            using Utf8JsonWriter writer = new Utf8JsonWriter(stream, new JsonWriterOptions() { Indented = true });

            writer.WriteStartObject(_baseName);

            void WriteValue(WriteKind kind, string name, object value)
            {
                switch (kind)
                {
                    case WriteKind.Bool:
                        {
                            bool boolValue = (bool)value;
                            writer.WriteBoolean(name, boolValue);
                        }
                        break;
                    case WriteKind.Int:
                        {
                            int intValue = (int)value;
                            writer.WriteNumber(name, intValue);
                        }
                        break;
                    case WriteKind.Double:
                        {
                            double doubleValue = (double)value;
                            writer.WriteNumber(name, doubleValue);
                        }
                        break;
                    case WriteKind.String:
                        {
                            string stringValue = (string)value;
                            writer.WriteString(name, stringValue);
                        }
                        break;
                    case WriteKind.List:
                        {
                            List<string> list = (List<string>)value;
                            writer.WriteStartArray(name);
                            foreach (string item in list)
                                writer.WriteStringValue(item);
                            writer.WriteEndArray();
                        }
                        break;
                    case WriteKind.Dictionary:
                        {
                            writer.WriteStartObject(name);
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
                                            WriteValue(WriteKind.String, propName, convertedValue);
                                        }
                                        else
                                        {
                                            if (typeof(string).Equals(propType))
                                            {
                                                string stringValue = (string)propValue;
                                                WriteValue(WriteKind.String, propName, stringValue);
                                            }
                                            else if (typeof(double).Equals(propType))
                                            {
                                                double doubleValue = (double)propValue;
                                                WriteValue(WriteKind.Double, propName, doubleValue);
                                            }
                                            else if (typeof(int).Equals(propType))
                                            {
                                                int intValue = (int)propValue;
                                                WriteValue(WriteKind.Int, propName, intValue);
                                            }
                                            else if (typeof(bool).Equals(propType))
                                            {
                                                bool boolValue = (bool)propValue;
                                                WriteValue(WriteKind.Bool, propName, boolValue);
                                            }
                                            else
                                                WriteValue(WriteKind.Null, propName, null);
                                        }
                                    }
                                }
                            }
                            writer.WriteEndObject();
                        }
                        break;
                    case WriteKind.Null:
                        writer.WriteNull(name);
                        break;
                    default:
                        break;
                }
            }

            void WriteChilds(IEnumerable<WriteNode> childs)
            {
                foreach (WriteNode child in childs)
                {
                    if (child.HasChildren)
                    {
                        writer.WriteStartObject(child.Name);
                        WriteChilds(child);
                        writer.WriteEndObject();
                    }
                    else
                        WriteValue(child.Kind, child.Name, child.Value);
                }
            }

            writer.WriteEndObject();

            writer.Flush();
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
