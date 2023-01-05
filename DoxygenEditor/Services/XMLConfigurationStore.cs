using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Xml;
using TSP.DoxygenEditor.Utils;

namespace TSP.DoxygenEditor.Services
{
    class XMLConfigurationStore : AbstractConfigurationPublisher, IConfigurarionReader, IConfigurarionWriter
    {
        private readonly string _defaultNamespace;
        private readonly string _baseName;
        private readonly IConfigurationConverter _converter;
        private XmlDocument _doc;
        private XmlNamespaceManager _nsMng;
        private XmlNode _rootNode;

        public XMLConfigurationStore(string defaultNamespace, string baseName, IConfigurationConverter converter = null) : base()
        {
            _defaultNamespace = defaultNamespace;
            _baseName = baseName;
            _converter = converter;
            _doc = null;
            _rootNode = null;
        }

        public bool Load(string filePath)
        {
            _rootNode = null;
            if (File.Exists(filePath))
            {
                try
                {
                    if (_doc == null)
                        _doc = new XmlDocument();
                    _doc.Load(filePath);
                    _nsMng = new XmlNamespaceManager(_doc.NameTable);
                    _nsMng.AddNamespace("d", _defaultNamespace);
                    XmlNode baseNode = _doc.SelectSingleNode($"d:{_baseName}", _nsMng);
                    _rootNode = baseNode;
                    return (true);
                }
                catch
                {
                    _doc.RemoveAll();
                }
            }
            return (false);
        }

        public void Save(string filePath)
        {
            if (_doc == null)
            {
                _doc = new XmlDocument();
                _nsMng = new XmlNamespaceManager(_doc.NameTable);
                _nsMng.AddNamespace("d", _defaultNamespace);
            }
            if (_doc.HasChildNodes)
                _doc.RemoveAll();
            XmlNode baseNode = _doc.CreateElement(_baseName, _defaultNamespace);
            _doc.AppendChild(baseNode);
            _rootNode = baseNode;
            foreach (WriteEntry writeEntry in WriteEntries)
            {
                string section = writeEntry.Section;
                string name = writeEntry.Name;
                object value = writeEntry.Value;
                WriteKind kind = writeEntry.Kind;
                XmlNode sectionNode = null;
                XmlNode rootSectionNode = _rootNode;
                string[] s = section.Split('/');
                for (int i = 0; i < s.Length; ++i)
                {
                    string thisSection = s[i];
                    XmlNode thisNode = rootSectionNode.SelectSingleNode($"d:{thisSection}", _nsMng);
                    if (thisNode == null)
                    {
                        thisNode = _doc.CreateElement(thisSection, _defaultNamespace);
                        rootSectionNode.AppendChild(thisNode);
                        rootSectionNode = thisNode;
                    }
                    else
                        rootSectionNode = thisNode;
                    sectionNode = thisNode;
                }
                Debug.Assert(sectionNode != null);
                XmlNode nameNode = sectionNode.SelectSingleNode($"d:{name}", _nsMng);
                if (value != null)
                {
                    if (nameNode == null)
                    {
                        nameNode = _doc.CreateElement(name, _defaultNamespace);
                        sectionNode.AppendChild(nameNode);
                    }
                    switch (kind)
                    {
                        case WriteKind.Bool:
                            nameNode.InnerText = (bool)value ? "true" : "false";
                            break;
                        case WriteKind.Int:
                            nameNode.InnerText = $"{(int)value}";
                            break;
                        case WriteKind.Double:
                            double d = (double)value;
                            nameNode.InnerText = d.ToString(CultureInfo.InvariantCulture);
                            break;
                        case WriteKind.String:
                            nameNode.InnerText = (string)value;
                            break;
                        case WriteKind.List:
                            {
                                List<string> list = (List<string>)value;
                                nameNode.RemoveAll();
                                foreach (string item in list)
                                {
                                    XmlNode itemNode = _doc.CreateElement("Item", _defaultNamespace);
                                    itemNode.InnerText = item;
                                    nameNode.AppendChild(itemNode);
                                }
                            }
                            break;
                        case WriteKind.Dictionary:
                            {
                                IDictionary<string, object> dict = (IDictionary<string, object>)value;
                                nameNode.RemoveAll();
                                foreach (KeyValuePair<string, object> pair in dict)
                                {
                                    XmlNode itemNode = _doc.CreateElement(pair.Key, _defaultNamespace);
                                    Type t = pair.Value.GetType();
                                    if (t.IsValueType)
                                    {
                                        System.Reflection.PropertyInfo[] properties = t.GetProperties();
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
                                            {
                                                XmlNode propNode = _doc.CreateElement(propName, _defaultNamespace);
                                                propNode.InnerText = convertedValue;
                                                itemNode.AppendChild(propNode);
                                            }
                                        }
                                    }

                                    nameNode.AppendChild(itemNode);
                                }
                            }
                            break;
                        default:
                            nameNode.InnerText = value.ToString();
                            break;
                    }
                }
                else
                {
                    if (nameNode != null)
                        sectionNode.RemoveChild(nameNode);
                }
            }

            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                _doc.Save(writer);
            }
        }

        private object ReadRaw(string section, string name)
        {
            if (_rootNode != null)
            {
                XmlNode nameNode = _rootNode.SelectSingleNode($"d:{section}/d:{name}", _nsMng);
                return (nameNode?.InnerText);
            }
            return (null);
        }
        private object ReadRaw(string section, Expression<Func<object>> nameExpression)
            => ReadRaw(section, ReflectionUtils.GetName(nameExpression));

        public bool ReadBool(string section, string name, bool defaultValue)
        {
            string rawValue = ReadRaw(section, name) as string;
            if (rawValue != null)
            {
                bool result;
                if (bool.TryParse(rawValue, out result))
                    return (result);
            }
            return (defaultValue);
        }
        public bool ReadBool(string section, Expression<Func<object>> nameExpression, bool defaultValue)
        {
            return ReadBool(section, ReflectionUtils.GetName(nameExpression), defaultValue);
        }

        public int ReadInt(string section, string name, int defaultValue)
        {
            string rawValue = ReadRaw(section, name) as string;
            if (rawValue != null)
            {
                int result;
                if (int.TryParse(rawValue, out result))
                    return (result);
            }
            return (defaultValue);
        }
        public int ReadInt(string section, Expression<Func<object>> nameExpression, int defaultValue)
        {
            return ReadInt(section, ReflectionUtils.GetName(nameExpression), defaultValue);
        }

        public double ReadDouble(string section, string name, double defaultValue)
        {
            string rawValue = ReadRaw(section, name) as string;
            if (rawValue != null)
            {
                double result;
                if (double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                    return (result);
            }
            return (defaultValue);
        }
        public double ReadDouble(string section, Expression<Func<object>> nameExpression, double defaultValue)
        {
            return ReadDouble(section, ReflectionUtils.GetName(nameExpression), defaultValue);
        }

        public string ReadString(string section, string name, string defaultValue)
        {
            string rawValue = ReadRaw(section, name) as string;
            if (rawValue == null)
                rawValue = defaultValue;
            return (rawValue);
        }
        public string ReadString(string section, Expression<Func<object>> nameExpression, string defaultValue)
        {
            return ReadString(section, ReflectionUtils.GetName(nameExpression), defaultValue);
        }

        public IEnumerable<string> ReadList(string section, string name)
        {
            if (_rootNode != null)
            {
                XmlNode sectionNode = null;
                XmlNode currentNode = _rootNode;
                string[] s = section.Split('/');
                for (int i = 0; i < s.Length; ++i)
                {
                    string thisSection = s[i];
                    XmlNode thisNode = currentNode.SelectSingleNode($"d:{thisSection}", _nsMng);
                    if (thisNode == null)
                    {
                        sectionNode = null;
                        break;
                    }
                    sectionNode = currentNode = thisNode;
                }

                XmlNode nameNode = sectionNode?.SelectSingleNode($"d:{name}", _nsMng);
                if (nameNode != null)
                {
                    XmlNodeList itemsNodeList = nameNode.SelectNodes("d:Item", _nsMng);
                    foreach (XmlNode itemNode in itemsNodeList)
                    {
                        string item = itemNode.InnerText;
                        yield return item;
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
            if (_rootNode != null)
            {
                XmlNode nameNode = _rootNode.SelectSingleNode($"d:{section}/d:{name}", _nsMng);
                if (nameNode != null)
                {
                    XmlNodeList itemsNodeList = nameNode.SelectNodes("d:Item", _nsMng);
                    foreach (XmlNode itemNode in itemsNodeList)
                    {
                        string key = itemNode.Attributes.GetNamedItem("d:Key")?.Value;
                        if (!string.IsNullOrWhiteSpace(key))
                        {
                            Type structType = typeof(TValue);
                            TValue structValue = (TValue)Activator.CreateInstance(structType);
                            System.Reflection.PropertyInfo[] properties = structType.GetProperties(System.Reflection.BindingFlags.Public);
                            foreach (System.Reflection.PropertyInfo property in properties)
                            {
                                Type propType = property.DeclaringType;
                                string propName = property.Name;
                                XmlNode propNode = itemNode.SelectSingleNode($"d:{propName}", _nsMng);
                                if (propNode != null)
                                {
                                    string stringValue = propNode.InnerText;
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
                            }
                            yield return new KeyValuePair<string, TValue>(key, structValue);
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
        ~XMLConfigurationStore()
        {
            Dispose(false);
        }
        #endregion

    }
}
