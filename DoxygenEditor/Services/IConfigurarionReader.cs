using System;
using System.Collections.Generic;
using TSP.DoxygenEditor.Utils;

namespace TSP.DoxygenEditor.Services
{
    public interface IConfigurarionReader : IDisposable
    {
        string ReadString(string section, string name, string defaultValue = null);
        int ReadInt(string section, string name, int defaultValue);
        double ReadDouble(string section, string name, double defaultValue);
        bool ReadBool(string section, string name, bool defaultValue);
        IEnumerable<string> ReadList(string section, string name);
        IEnumerable<KeyValuePair<string, TValue>> ReadDictionary<TValue>(string section, string name) where TValue : struct;
        TEnum ReadEnum<TEnum>(string section, string name, TEnum defaultValue) where TEnum : struct, IConvertible;
        Result<bool> Load(string filePath);
    }
}
