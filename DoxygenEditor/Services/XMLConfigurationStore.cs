using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace TSP.DoxygenEditor.Services
{
    class XMLConfigurationStore : AbstractConfigurationPublisher, IConfigurarionReader, IConfigurarionWriter
    {
        private readonly XmlDocument _doc;
        private XmlNode _rootNode;
        private readonly string _baseName;

        public XMLConfigurationStore(string baseName) : base()
        {
            _doc = new XmlDocument();
            _rootNode = null;
            _baseName = baseName;
        }

        public void Load(string filePath)
        {
            _rootNode = null;
            if (File.Exists(filePath))
            {
                try
                {
                    _doc.Load(filePath);
                    XmlNode baseNode = _doc.SelectSingleNode($"/{_baseName}");
                    _rootNode = baseNode;
                }
                catch
                {
                    _doc.RemoveAll();
                }
            }
        }

        public void Save(string filePath)
        {
            if (_doc.HasChildNodes)
                _doc.RemoveAll();
            XmlNode baseNode = _doc.CreateElement(_baseName);
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
                    XmlNode thisNode = rootSectionNode.SelectSingleNode(thisSection);
                    if (thisNode == null)
                    {
                        thisNode = _doc.CreateElement(thisSection);
                        rootSectionNode.AppendChild(thisNode);
                        rootSectionNode = thisNode;
                    }
                    sectionNode = thisNode;
                }
                Debug.Assert(sectionNode != null);
                XmlNode nameNode = sectionNode.SelectSingleNode(name);
                if (value != null)
                {
                    if (nameNode == null)
                    {
                        nameNode = _doc.CreateElement(name);
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
                                    XmlNode itemNode = _doc.CreateElement("Item");
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
                var nameNode = _rootNode.SelectSingleNode($"{section}/{name}");
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

        public string ReadString(string section, string name)
        {
            string rawValue = ReadRaw(section, name) as string;
            return (rawValue);
        }

        public IEnumerable<string> ReadList(string section, string name)
        {
            if (_rootNode != null)
            {
                var nameNode = _rootNode.SelectSingleNode($"{section}/{name}");
                if (nameNode != null)
                {
                    XmlNodeList itemsNodeList = nameNode.SelectNodes("Item");
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
