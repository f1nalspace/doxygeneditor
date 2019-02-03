using System;

namespace DoxygenEditor.Services
{
    public interface IConfigurarionInstance : IDisposable
    {
    }
    public interface IConfigurationService
    {
        IConfigurarionInstance Create(bool readOnly);
        void Clear(IConfigurarionInstance instance, string section);
        void WriteString(IConfigurarionInstance instance, string section, string name, string value);
        void WriteInt(IConfigurarionInstance instance, string section, string name, int value);
        void WriteBool(IConfigurarionInstance instance, string section, string name, bool value);
        string ReadString(IConfigurarionInstance instance, string section, string name);
        int ReadInt(IConfigurarionInstance instance, string section, string name, int defaultValue);
        bool ReadBool(IConfigurarionInstance instance, string section, string name, bool defaultValue);
    }
}
