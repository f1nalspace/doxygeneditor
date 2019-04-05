using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace TSP.DoxygenEditor.Services
{
    class XMLConfigurationStore : AbstractConfigurationPublisher, IConfigurarionReader, IConfigurarionWriter
    {
        private readonly string _defaultNamespace;
        private readonly string _baseName;
        private XmlDocument _doc;
        private XmlNamespaceManager _nsMng;
        private XmlNode _rootNode;

        public XMLConfigurationStore(string defaultNamespace, string baseName) : base()
        {
            _defaultNamespace = defaultNamespace;
            _baseName = baseName;
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
            foreach (var writeEntry in WriteEntries)
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
                        case WriteKind.String:
                            nameNode.InnerText = (string)value;
                            break;
                        case WriteKind.List:
                            {
                                var list = (List<string>)value;
                                nameNode.RemoveAll();
                                foreach (var item in list)
                                {
                                    XmlNode itemNode = _doc.CreateElement("Item", _defaultNamespace);
                                    itemNode.InnerText = item;
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

        public object ReadRaw(string section, string name)
        {
            if (_rootNode != null)
            {
                var nameNode = _rootNode.SelectSingleNode($"d:{section}/d:{name}", _nsMng);
                return (nameNode?.InnerText);
            }
            return (null);
        }

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

        public string ReadString(string section, string name, string defaultValue)
        {
            string rawValue = ReadRaw(section, name) as string;
            if (rawValue == null)
                rawValue = defaultValue;
            return (rawValue);
        }

        public IEnumerable<string> ReadList(string section, string name)
        {
            if (_rootNode != null)
            {
                var nameNode = _rootNode.SelectSingleNode($"d:{section}/d:{name}", _nsMng);
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

        public override void Dispose()
        {
        }

    }
}
