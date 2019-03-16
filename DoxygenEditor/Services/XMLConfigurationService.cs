using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using TSP.DoxygenEditor.Extensions;

namespace TSP.DoxygenEditor.Services
{
    class XMLConfigurationService : IConfigurationService
    {
        class XMLConfigurationInstance : AbstractConfigurationPublisher, IConfigurarionReader
        {
            private readonly XmlDocument _doc;
            private XmlNode _rootNode;
            private readonly string _profilePath;

            const string Filename = "settings.xml";
            string SettingsFilePath => Path.Combine(_profilePath, Config.CompanyName, Config.ProductName, Filename);

            public XMLConfigurationInstance(bool isReadOnly, ConfigurationServiceConfig config) : base(isReadOnly, config)
            {
                _profilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                _doc = new XmlDocument();
                string filePath = SettingsFilePath;
                if (File.Exists(filePath))
                {
                    try
                    {
                        _doc.Load(filePath);
                    }
                    catch
                    {
                        _doc.RemoveAll();
                    }
                }

                _rootNode = null;

                XmlNode companyNode = _doc.SelectSingleNode($"/{config.CompanyName}");
                XmlNode productNode = null;
                if (companyNode != null)
                    productNode = companyNode.SelectSingleNode($"{config.ProductName}");
                if (!IsReadOnly)
                {
                    if (companyNode == null)
                    {
                        companyNode = _doc.CreateElement(config.CompanyName);
                        _doc.AppendChild(companyNode);
                    }
                    if (productNode == null)
                    {
                        productNode = _doc.CreateElement(config.ProductName);
                        companyNode.AppendChild(productNode);
                    }
                }
                _rootNode = productNode;
            }

            protected override void PublishWrites()
            {
                
                base.PublishWrites();
                if (!IsReadOnly)
                {
                    DirectoryInfo userDir = new DirectoryInfo(_profilePath);
                    Debug.Assert(userDir.Exists);
                    DirectoryInfo companyDir = userDir.GetDirectory(Config.CompanyName);
                    if (companyDir == null)
                    {
                        companyDir = new DirectoryInfo(Path.Combine(userDir.FullName, Config.CompanyName));
                        companyDir.Create();
                    }
                    DirectoryInfo productDir = companyDir.GetDirectory(Config.ProductName);
                    if (productDir == null)
                    {
                        productDir = new DirectoryInfo(Path.Combine(companyDir.FullName, Config.ProductName));
                        productDir.Create();
                    }
                    string filePath = SettingsFilePath;
                    using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                    {
                        _doc.Save(writer);
                    }
                }
            }

            protected override void ClearAll()
            {
                if (_rootNode != null)
                    _rootNode.RemoveAll();
            }

            protected override void PublishWrite(WriteKind kind, string section, string name, object value)
            {
                Debug.Assert(!IsReadOnly);
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

            public object ReadRaw(string section, string name)
            {
                Debug.Assert(IsReadOnly);
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
                Debug.Assert(IsReadOnly);
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

        public IConfigurarionReader CreateReader(ConfigurationServiceConfig config)
        {
            IConfigurarionReader result = new XMLConfigurationInstance(true, config);
            return (result);
        }
        public IConfigurarionWriter CreateWriter(ConfigurationServiceConfig config)
        {
            IConfigurarionWriter result = new XMLConfigurationInstance(false, config);
            return (result);
        }
    }
}
