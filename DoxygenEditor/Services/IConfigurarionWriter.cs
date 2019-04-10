using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TSP.DoxygenEditor.Services
{
    public interface IConfigurarionWriter : IDisposable
    {
        void WriteString(string section, string name, string value);
        void WriteString(string section, Expression<Func<object>> nameExpression, string value);
        void WriteInt(string section, string name, int value);
        void WriteInt(string section, Expression<Func<object>> nameExpression, int value);
        void WriteBool(string section, string name, bool value);
        void WriteBool(string section, Expression<Func<object>> nameExpression, bool value);
        void WriteList(string section, string name, IEnumerable<string> list);
        void WriteList(string section, Expression<Func<object>> nameExpression, IEnumerable<string> list);
        void WriteDictionary<TValue>(string section, string name, IDictionary<string, TValue> dict) where TValue : struct;
        void WriteDictionary<TValue>(string section, Expression<Func<object>> nameExpression, IDictionary<string, TValue> dict) where TValue : struct;
        void Save(string filePath);
    }
}
