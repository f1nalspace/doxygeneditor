using System.Collections.Generic;

namespace TSP.DoxygenEditor.Services
{
    abstract class AbstractConfigurationPublisher
    {
        public enum WriteKind
        {
            Bool,
            Int,
            String,
            List,
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

        public abstract void Dispose();

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
        public void WriteInt(string section, string name, int value)
        {
            PushWrite(WriteKind.Int, section, name, value);
        }
        public void WriteBool(string section, string name, bool value)
        {
            PushWrite(WriteKind.Bool, section, name, value);
        }
        public void WriteList(string section, string name, IEnumerable<string> list)
        {
            PushWrite(WriteKind.List, section, name, new List<string>(list));
        }
    }
}
