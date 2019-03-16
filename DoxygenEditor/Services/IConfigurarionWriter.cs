using System;
using System.Collections.Generic;

namespace TSP.DoxygenEditor.Services
{
    public interface IConfigurarionWriter : IDisposable
    {
        void WriteString(string section, string name, string value);
        void WriteInt(string section, string name, int value);
        void WriteBool(string section, string name, bool value);
        void WriteList(string section, string name, IEnumerable<string> list);
        void Save(string filePath);
    }
}
