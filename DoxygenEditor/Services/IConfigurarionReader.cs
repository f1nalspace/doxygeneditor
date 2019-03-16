using System;
using System.Collections.Generic;

namespace TSP.DoxygenEditor.Services
{
    public interface IConfigurarionReader : IDisposable
    {
        object ReadRaw(string section, string name);
        string ReadString(string section, string name, string defaultValue = null);
        IEnumerable<string> ReadList(string section, string name);
        int ReadInt(string section, string name, int defaultValue);
        bool ReadBool(string section, string name, bool defaultValue);
        void Load(string filePath);
    }
}
