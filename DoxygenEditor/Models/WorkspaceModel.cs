using TSP.DoxygenEditor.Services;
using System.Collections.Generic;
using TSP.DoxygenEditor.Solid;

namespace TSP.DoxygenEditor.Models
{
    public class WorkspaceModel
    {
        const string CompanyName = "TSPSoftware";
        const string ProductName = "DoxygenEditor";
        private static readonly ConfigurationServiceConfig ProductConfig = new ConfigurationServiceConfig(CompanyName, ProductName);

        const int MaxRecentFileCount = 10;

        public readonly List<string> _recentFiles = new List<string>();
        public IEnumerable<string> RecentFiles => _recentFiles;

        public readonly List<string> _lastOpenedFiles = new List<string>();
        public IEnumerable<string> LastOpenedFiles => _lastOpenedFiles;
        public int LastOpenedFileCount => _lastOpenedFiles.Count;

        public bool IsWhitespaceVisible { get; set; }
        public bool RestoreLastOpenedFiles { get; set; }

        private IConfigurationService ConfigService { get; }

        public WorkspaceModel(IConfigurationService configService)
        {
            ConfigService = configService;
        }

        public void Load()
        {
            _recentFiles.Clear();
            _lastOpenedFiles.Clear();
            using (var instance = ConfigService.CreateReader(ProductConfig))
            {
                int recentFileCount = instance.ReadInt("RecentFiles", "Count", 0);
                for (int i = 0; i < recentFileCount; ++i)
                {
                    string recentFilePath = instance.ReadString("RecentFiles", $"File{i}");
                    if (!string.IsNullOrEmpty(recentFilePath))
                        _recentFiles.Add(recentFilePath);
                }
                int lastOpenFileCount = instance.ReadInt("LastOpenedFiles", "Count", 0);
                for (int i = 0; i < lastOpenFileCount; ++i)
                {
                    string lastOpenFilePath = instance.ReadString("LastOpenedFiles", $"File{i}");
                    if (!string.IsNullOrEmpty(lastOpenFilePath))
                        _lastOpenedFiles.Add(lastOpenFilePath);
                }
                IsWhitespaceVisible = instance.ReadBool("View", "IsWhitespaceVisible", false);
                RestoreLastOpenedFiles = instance.ReadBool("Startup", "RestoreLastOpenedFiles", false);
            }
        }
        public void Save()
        {
            using (var instance = ConfigService.CreateWriter(ProductConfig))
            {
                instance.BeginPublish();

                instance.PushInt("RecentFiles", "Count", _recentFiles.Count);
                for (int i = 0; i < _recentFiles.Count; ++i)
                {
                    string recentFilePath = _recentFiles[i];
                    instance.PushString("RecentFiles", $"File{i}", recentFilePath);
                }

                instance.PushInt("LastOpenedFiles", "Count", _lastOpenedFiles.Count);
                for (int i = 0; i < _lastOpenedFiles.Count; ++i)
                {
                    string lastOpenedFilePath = _lastOpenedFiles[i];
                    instance.PushString("LastOpenedFiles", $"File{i}", lastOpenedFilePath);
                }

                instance.PushBool("View", "IsWhitespaceVisible", IsWhitespaceVisible);

                instance.PushBool("Startup", "RestoreLastOpenedFiles", RestoreLastOpenedFiles);

                instance.EndPublish();
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
        public void UpdateLastOpenedFiles(IEnumerable<string> files)
        {
            _lastOpenedFiles.Clear();
            _lastOpenedFiles.AddRange(files);
        }
    }
}
