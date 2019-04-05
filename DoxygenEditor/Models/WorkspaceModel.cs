using TSP.DoxygenEditor.Services;
using System.Collections.Generic;
using System.Diagnostics;

namespace TSP.DoxygenEditor.Models
{
    public class WorkspaceModel
    {
        const string DefaultWorkspaceNamespace = "https://tspsoftware.net/doxygeneditor/workspace/";
        const int MaxRecentFileCount = 10;

        public readonly List<string> _recentFiles = new List<string>();
        public IEnumerable<string> RecentFiles => _recentFiles;

        public readonly List<string> _lastOpenedFiles = new List<string>();
        public IEnumerable<string> LastOpenedFiles => _lastOpenedFiles;
        public int LastOpenedFileCount => _lastOpenedFiles.Count;

        public bool IsWhitespaceVisible { get; set; }

        public string FilePath { get; set; }

        public WorkspaceModel(string filePath)
        {
            FilePath = filePath;
        }

        public void Overwrite(WorkspaceModel other)
        {
            FilePath = other.FilePath;
            IsWhitespaceVisible = other.IsWhitespaceVisible;
            _recentFiles.Clear();
            _recentFiles.AddRange(other.RecentFiles);
            _lastOpenedFiles.Clear();
            _lastOpenedFiles.AddRange(other.LastOpenedFiles);
        }

        public bool Load(string filePath)
        {
            FilePath = filePath;
            _recentFiles.Clear();
            _lastOpenedFiles.Clear();
            bool result = true;
            using (IConfigurarionReader instance = new XMLConfigurationStore(DefaultWorkspaceNamespace, "Workspace"))
            {
                if (!instance.Load(filePath))
                    result = false;
                IsWhitespaceVisible = instance.ReadBool("View", "IsWhitespaceVisible", false);
                _recentFiles.AddRange(instance.ReadList("History", "RecentFiles"));
                _lastOpenedFiles.AddRange(instance.ReadList("History", "LastOpenedFiles"));
            }
            return (result);
        }
        public void Save()
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(FilePath));
            using (IConfigurarionWriter instance = new XMLConfigurationStore(DefaultWorkspaceNamespace, "Workspace"))
            {
                instance.WriteBool("View", "IsWhitespaceVisible", IsWhitespaceVisible);

                instance.WriteList("History", "RecentFiles", _recentFiles);
                instance.WriteList("History", "LastOpenedFiles", _lastOpenedFiles);

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
