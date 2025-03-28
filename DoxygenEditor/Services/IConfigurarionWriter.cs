using System;
using System.Collections.Generic;

namespace TSP.DoxygenEditor.Services
{
    public interface IConfigurarionWriter : IDisposable
    {
        void WriteString(string section, string name, string value);
        void WriteInt(string section, string name, int value);
        void WriteDouble(string section, string name, double value);
        void WriteBool(string section, string name, bool value);
        void WriteList(string section, string name, IEnumerable<string> list);
        void WriteDictionary<TValue>(string section, string name, IDictionary<string, TValue> dict) where TValue : struct;
        void WriteEnum<TEnum>(string section, string name, TEnum value) where TEnum : struct, IConvertible;
        void Save(string filePath);
    }
}
