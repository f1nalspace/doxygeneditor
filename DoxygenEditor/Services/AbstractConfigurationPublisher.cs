using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TSP.DoxygenEditor.Utils;

namespace TSP.DoxygenEditor.Services
{
    abstract class AbstractConfigurationPublisher
    {
        public enum WriteKind
        {
            None = 0,
            Bool,
            Int,
            Double,
            String,
            List,
            Dictionary,
        }

        public struct WriteEntry
        {
            public WriteKind Kind { get; }
            public string Section { get; }
            public string Name { get; }
            public object Value { get; }
            public WriteEntry(WriteKind kind, string section, string name, object value)
            {
                Kind = kind;
                Section = section;
                Name = name;
                Value = value;
            }
        }

        private readonly List<WriteEntry> _writeEntries = new List<WriteEntry>();
        public IEnumerable<WriteEntry> WriteEntries => _writeEntries;

        public AbstractConfigurationPublisher()
        {
        }

        protected void ClearWrites()
        {
            _writeEntries.Clear();
        }

        private void PushWrite(WriteKind kind, string section, string name, object value)
        {
            _writeEntries.Add(new WriteEntry(kind, section, name, value));
        }

        public void WriteString(string section, string name, string value)
        {
            PushWrite(WriteKind.String, section, name, value);
        }
        public void WriteString(string section, Expression<Func<object>> nameExpression, string value)
        {
            WriteString(section, ReflectionUtils.GetName(nameExpression), value);
        }

        public void WriteInt(string section, string name, int value)
        {
            PushWrite(WriteKind.Int, section, name, value);
        }
        public void WriteInt(string section, Expression<Func<object>> nameExpression, int value)
        {
            WriteInt(section, ReflectionUtils.GetName(nameExpression), value);
        }

        public void WriteDouble(string section, string name, double value)
        {
            PushWrite(WriteKind.Double, section, name, value);
        }
        public void WriteDouble(string section, Expression<Func<object>> nameExpression, double value)
        {
            WriteDouble(section, ReflectionUtils.GetName(nameExpression), value);
        }

        public void WriteBool(string section, string name, bool value)
        {
            PushWrite(WriteKind.Bool, section, name, value);
        }
        public void WriteBool(string section, Expression<Func<object>> nameExpression, bool value)
        {
            WriteBool(section, ReflectionUtils.GetName(nameExpression), value);
        }

        public void WriteList(string section, string name, IEnumerable<string> list)
        {
            PushWrite(WriteKind.List, section, name, new List<string>(list));
        }
        public void WriteList(string section, Expression<Func<object>> nameExpression, IEnumerable<string> list)
        {
            WriteList(section, ReflectionUtils.GetName(nameExpression), new List<string>(list));
        }
        public void WriteDictionary<TValue>(string section, string name, IDictionary<string, TValue> dict) where TValue : struct
        {
            Dictionary<string, object> outDict = new Dictionary<string, object>();
            foreach (KeyValuePair<string, TValue> pair in dict)
                outDict[pair.Key] =  pair.Value;
            PushWrite(WriteKind.Dictionary, section, name, outDict);
        }
        public void WriteDictionary<TValue>(string section, Expression<Func<object>> nameExpression, IDictionary<string, TValue> dict) where TValue : struct
        {
            WriteDictionary(section, ReflectionUtils.GetName(nameExpression), dict);
        }
    }
}
