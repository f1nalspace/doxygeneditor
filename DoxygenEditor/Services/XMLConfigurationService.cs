using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TSP.DoxygenEditor.Extensions;

namespace TSP.DoxygenEditor.Services
{
    class XMLConfigurationService : IConfigurationService
    {
        class XMLConfigurationInstance : BaseConfigurationInstance
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

            protected override void PublishWrite(string section, string name, object value)
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
                XmlNode valueNode = sectionNode.SelectSingleNode(name);
                if (value != null)
                {
                    if (valueNode == null)
                    {
                        valueNode = _doc.CreateElement(name);
                        sectionNode.AppendChild(valueNode);
                    }
                    Type valueType = value.GetType();
                    if (typeof(string).Equals(valueType))
                        valueNode.InnerText = (string)value;
                    else if (typeof(int).Equals(valueType))
                        valueNode.InnerText = $"{(int)value}";
                    else if (typeof(bool).Equals(valueType))
                        valueNode.InnerText = (bool)value ? "true" : "false";
                    else
                        valueNode.InnerText = value.ToString();
                }
                else
                {
                    if (valueNode != null)
                        sectionNode.RemoveChild(valueNode);
                }
            }

            public override object ReadRaw(string section, string name)
            {
                Debug.Assert(IsReadOnly);
                if (_rootNode != null)
                {
                    var nameNode = _rootNode.SelectSingleNode($"{section}/{name}");
                    return (nameNode?.InnerText);
                }
                return (null);
            }

            public override bool ReadBool(string section, string name, bool defaultValue)
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

            public override int ReadInt(string section, string name, int defaultValue)
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

            public override string ReadString(string section, string name)
            {
                string rawValue = ReadRaw(section, name) as string;
                return (rawValue);
            }

            public override void Dispose()
            {
            }

        }

        public IConfigurarionInstance Create(bool readOnly, ConfigurationServiceConfig config)
        {
            IConfigurarionInstance result = new XMLConfigurationInstance(readOnly, config);
            return (result);
        }
    }
}
