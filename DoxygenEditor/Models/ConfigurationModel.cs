using DoxygenEditor.MVVM;
using DoxygenEditor.Services;
using System.Collections.Generic;

namespace DoxygenEditor.Models
{
    public class ConfigurationModel : BindableBase
    {
        const int MaxRecentFileCount = 10;
        public readonly List<string> _recentFiles = new List<string>();
        public IEnumerable<string> RecentFiles { get { return _recentFiles; } }
        public string LastOpenFilePath
        {
            get { return GetProperty(() => LastOpenFilePath); }
            set { SetProperty(() => LastOpenFilePath, value); }
        }
        public bool IsWhitespaceVisible
        {
            get { return GetProperty(() => IsWhitespaceVisible); }
            set { SetProperty(() => IsWhitespaceVisible, value); }
        }
        public void Load()
        {
            _recentFiles.Clear();
            var srv = IOCContainer.Default.Get<IConfigurationService>();
            using (var instance = srv.Create(true))
            {
                int recentFileCount = srv.ReadInt(instance, "RecentFiles", "Count", 0);
                for (int i = 0; i < recentFileCount; ++i)
                {
                    string recentFilePath = srv.ReadString(instance, "RecentFiles", $"File{i}");
                    if (!string.IsNullOrEmpty(recentFilePath))
                        _recentFiles.Add(recentFilePath);
                }
                LastOpenFilePath = srv.ReadString(instance, "Startup", "LastOpenFilePath");
                IsWhitespaceVisible = srv.ReadBool(instance, "View", "IsWhitespaceVisible", false);
            }
            RaisePropertyChanged(() => RecentFiles);
        }
        public void Save()
        {
            var srv = IOCContainer.Default.Get<IConfigurationService>();
            using (var instance = srv.Create(false))
            {
                srv.WriteInt(instance, "RecentFiles", "Count", _recentFiles.Count);
                for (int i = 0; i < _recentFiles.Count; ++i)
                {
                    string recentFilePath = _recentFiles[i];
                    srv.WriteString(instance, "RecentFiles", $"File{i}", recentFilePath);
                }
                srv.WriteString(instance, "Startup", "LastOpenFilePath", LastOpenFilePath);
                srv.WriteBool(instance, "View", "IsWhitespaceVisible", IsWhitespaceVisible);
            }
        }
        public void ClearRecentFiles()
        {
            _recentFiles.Clear();
            RaisePropertyChanged(() => RecentFiles);
        }
        public void PushRecentFiles(string filePath)
        {
            while (_recentFiles.Count >= MaxRecentFileCount)
                _recentFiles.RemoveAt(_recentFiles.Count - 1);
            if (_recentFiles.Contains(filePath))
                _recentFiles.Remove(filePath);
            _recentFiles.Insert(0, filePath);
            RaisePropertyChanged(() => RecentFiles);
        }
    }
}
