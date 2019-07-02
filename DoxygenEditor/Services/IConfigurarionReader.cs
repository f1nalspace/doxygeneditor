using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TSP.DoxygenEditor.Services
{
    public interface IConfigurarionReader : IDisposable
    {
        object ReadRaw(string section, string name);
        object ReadRaw(string section, Expression<Func<object>> nameExpression);
        string ReadString(string section, string name, string defaultValue = null);
        string ReadString(string section, Expression<Func<object>> nameExpression, string defaultValue = null);
        int ReadInt(string section, string name, int defaultValue);
        int ReadInt(string section, Expression<Func<object>> nameExpression, int defaultValue);
        double ReadDouble(string section, string name, double defaultValue);
        double ReadDouble(string section, Expression<Func<object>> nameExpression, double defaultValue);
        bool ReadBool(string section, string name, bool defaultValue);
        bool ReadBool(string section, Expression<Func<object>> nameExpression, bool defaultValue);
        IEnumerable<string> ReadList(string section, string name);
        IEnumerable<string> ReadList(string section, Expression<Func<object>> nameExpression);
        IEnumerable<KeyValuePair<string, TValue>> ReadDictionary<TValue>(string section, string name) where TValue : struct;
        IEnumerable<KeyValuePair<string, TValue>> ReadDictionary<TValue>(string section, Expression<Func<object>> nameExpression) where TValue : struct;
        bool Load(string filePath);
    }
}
