using TSP.DoxygenEditor.Services;
using System.Collections.Generic;
using System.Diagnostics;

namespace TSP.DoxygenEditor.Models
{
    public class WorkspaceModel
    {
        const int MaxRecentFileCount = 10;

        public readonly List<string> _recentFiles = new List<string>();
        public IEnumerable<string> RecentFiles => _recentFiles;

        public readonly List<string> _lastOpenedFiles = new List<string>();
        public IEnumerable<string> LastOpenedFiles => _lastOpenedFiles;
        public int LastOpenedFileCount => _lastOpenedFiles.Count;

        public bool IsWhitespaceVisible { get; set; }

        private readonly List<string> _includeDirectories = new List<string>();
        public ICollection<string> IncludeDirectories => _includeDirectories;

        public string IncludeFilter { get; set; }
        private const string DefaultIncludeFilter = ".h .hpp";

        public string FilePath { get; set; }

        public WorkspaceModel(string filePath)
        {
            FilePath = filePath;
            IncludeFilter = DefaultIncludeFilter;
        }

        public void Overwrite(WorkspaceModel other)
        {
            FilePath = other.FilePath;
            IsWhitespaceVisible = other.IsWhitespaceVisible;
            _recentFiles.Clear();
            _recentFiles.AddRange(other.RecentFiles);
            _lastOpenedFiles.Clear();
            _lastOpenedFiles.AddRange(other.LastOpenedFiles);
            _includeDirectories.Clear();
            _includeDirectories.AddRange(other.IncludeDirectories);
        }

        public void Load(string filePath)
        {
            FilePath = filePath;
            _recentFiles.Clear();
            _lastOpenedFiles.Clear();
            _includeDirectories.Clear();
            using (IConfigurarionReader instance = new XMLConfigurationStore("Workspace"))
            {
                instance.Load(filePath);

                IsWhitespaceVisible = instance.ReadBool("View", "IsWhitespaceVisible", false);

                _recentFiles.AddRange(instance.ReadList("History", "RecentFiles"));
                _lastOpenedFiles.AddRange(instance.ReadList("History", "LastOpenedFiles"));

                _includeDirectories.AddRange(instance.ReadList("Sources", "IncludeDirectories"));
                IncludeFilter = instance.ReadString("Sources", "IncludeFilter", DefaultIncludeFilter);
            }
        }
        public void Save()
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(FilePath));
            using (IConfigurarionWriter instance = new XMLConfigurationStore("Workspace"))
            {
                instance.WriteBool("View", "IsWhitespaceVisible", IsWhitespaceVisible);

                instance.WriteList("History", "RecentFiles", _recentFiles);
                instance.WriteList("History", "LastOpenedFiles", _lastOpenedFiles);

                instance.WriteList("Sources", "IncludeDirectories", _includeDirectories);
                instance.WriteString("Sources", "IncludeFilter", IncludeFilter);

                instance.Save(FilePath);
            }
        }
        public void SaveAs(string filePath)
        {
            FilePath = filePath;
            Save();
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
