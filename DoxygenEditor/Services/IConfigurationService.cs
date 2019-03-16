using System;

namespace TSP.DoxygenEditor.Services
{
    public class ConfigurationServiceConfig {
        public string CompanyName { get; }
        public string ProductName { get; }
        public ConfigurationServiceConfig(string companyName, string productName)
        {
            CompanyName = companyName;
            ProductName = productName;
            if (string.IsNullOrWhiteSpace(CompanyName))
                throw new Exception("Company name is required!");
            if (string.IsNullOrWhiteSpace(ProductName))
                throw new Exception("Product name is required!");
        }
    }
    public interface IConfigurarionReader : IDisposable
    {
        object ReadRaw(string section, string name);
        string ReadString(string section, string name);
        int ReadInt(string section, string name, int defaultValue);
        bool ReadBool(string section, string name, bool defaultValue);
    }
    public interface IConfigurarionWriter : IDisposable
    {
        void PushString(string section, string name, string value);
        void PushInt(string section, string name, int value);
        void PushBool(string section, string name, bool value);
        void BeginPublish();
        void EndPublish();
    }
    public interface IConfigurationService
    {
        IConfigurarionReader CreateReader(ConfigurationServiceConfig config);
        IConfigurarionWriter CreateWriter(ConfigurationServiceConfig config);
    }
}
