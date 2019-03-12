using System.Collections.Generic;

namespace TSP.DoxygenEditor.Services
{
    abstract class BaseConfigurationInstance : IConfigurarionInstance
    {
        public class WriteEntry
        {
            public string Section { get; }
            public string Name { get; }
            public object Value { get; }
            public WriteEntry(string section, string name, object value)
            {
                Section = section;
                Name = name;
                Value = value;
            }
        }

        private readonly List<WriteEntry> _writeEntries = new List<WriteEntry>();

        protected abstract void ClearAll();
        protected abstract void PublishWrite(string section, string name, object value);

        protected bool IsReadOnly { get; }
        protected ConfigurationServiceConfig Config { get; }

        public BaseConfigurationInstance(bool isReadOnly, ConfigurationServiceConfig config)
        {
            IsReadOnly = isReadOnly;
            Config = config;
        }

        public abstract object ReadRaw(string section, string name);
        public abstract string ReadString(string section, string name);
        public abstract int ReadInt(string section, string name, int defaultValue);
        public abstract bool ReadBool(string section, string name, bool defaultValue);
        public abstract void Dispose();

        protected virtual void PublishWrites()
        {
            ClearAll();
            foreach (var writeEntry in _writeEntries)
                PublishWrite(writeEntry.Section, writeEntry.Name, writeEntry.Value);
        }

        protected void ClearWrites()
        {
            _writeEntries.Clear();
        }

        private void PushWrite(string section, string name, object value)
        {
            _writeEntries.Add(new WriteEntry(section, name, value));
        }

        public void PushString(string section, string name, string value)
        {
            PushWrite(section, name, value);
        }
        public void PushInt(string section, string name, int value)
        {
            PushWrite(section, name, value);
        }
        public void PushBool(string section, string name, bool value)
        {
            PushWrite(section, name, value);
        }

        public void BeginPublish()
        {
            ClearWrites();
        }
        public void EndPublish()
        {
            PublishWrites();
        }
    }
}
