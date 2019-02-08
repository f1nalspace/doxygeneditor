using TSP.DoxygenEditor.Services;
using System.Collections.Generic;

namespace TSP.DoxygenEditor.Models
{
    public class ConfigurationModel
    {
        const int MaxRecentFileCount = 10;
        public readonly List<string> _recentFiles = new List<string>();
        public IEnumerable<string> RecentFiles { get { return _recentFiles; } }
        public string LastOpenFilePath { get; set; }
        public bool IsWhitespaceVisible { get; set; }
        public void Load(IConfigurationService service)
        {
            _recentFiles.Clear();
            using (var instance = service.Create(true))
            {
                int recentFileCount = service.ReadInt(instance, "RecentFiles", "Count", 0);
                for (int i = 0; i < recentFileCount; ++i)
                {
                    string recentFilePath = service.ReadString(instance, "RecentFiles", $"File{i}");
                    if (!string.IsNullOrEmpty(recentFilePath))
                        _recentFiles.Add(recentFilePath);
                }
                LastOpenFilePath = service.ReadString(instance, "Startup", "LastOpenFilePath");
                IsWhitespaceVisible = service.ReadBool(instance, "View", "IsWhitespaceVisible", false);
            }
        }
        public void Save(IConfigurationService service)
        {
            using (var instance = service.Create(false))
            {
                service.WriteInt(instance, "RecentFiles", "Count", _recentFiles.Count);
                for (int i = 0; i < _recentFiles.Count; ++i)
                {
                    string recentFilePath = _recentFiles[i];
                    service.WriteString(instance, "RecentFiles", $"File{i}", recentFilePath);
                }
                service.WriteString(instance, "Startup", "LastOpenFilePath", LastOpenFilePath);
                service.WriteBool(instance, "View", "IsWhitespaceVisible", IsWhitespaceVisible);
            }
        }
        public void ClearRecentFiles()
        {
            _recentFiles.Clear();
        }
        public void PushRecentFiles(string filePath)
        {
            while (_recentFiles.Count >= MaxRecentFileCount)
                _recentFiles.RemoveAt(_recentFiles.Count - 1);
            if (_recentFiles.Contains(filePath))
                _recentFiles.Remove(filePath);
            _recentFiles.Insert(0, filePath);
        }
    }
}
