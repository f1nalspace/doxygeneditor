using System;
using System.Collections.Generic;
using System.Linq;
using TSP.DoxygenEditor.Editor;
using TSP.DoxygenEditor.Services;

namespace TSP.DoxygenEditor.Models
{
    public class GlobalConfigModel
    {
        const string BaseName = "GlobalConfig";

        private readonly string _companyName;
        private readonly string _appName;

        public class FileExtensions
        {
            private readonly List<string> _extensions = new List<string>();
            public IEnumerable<string> Extensions => _extensions;
            public string Name { get; }
            public EditorFileType FileType { get; }
            public FileExtensions(string name, EditorFileType fileType, params string[] extensions)
            {
                Name = name;
                FileType = fileType;
                _extensions.AddRange(extensions);
            }

            public bool HasExtension(string fileExt)
            {
                bool result = _extensions.Count(c => string.Equals(c, fileExt)) > 0;
                return (result);
            }
        }

        public string WorkspacePath { get; set; }
        private readonly List<FileExtensions> _supportedFileExtensions = new List<FileExtensions>();
        public IEnumerable<FileExtensions> SupportedFileExtensions => _supportedFileExtensions;

        public GlobalConfigModel(string companyName, string appName)
        {
            _companyName = companyName;
            _appName = appName;
            _supportedFileExtensions.Add(new FileExtensions("Doxygen documentation files", EditorFileType.DoxyDocs, ".docs"));
            _supportedFileExtensions.Add(new FileExtensions("Doxygen configuration files", EditorFileType.DoxyConfig, ".doxygen", ".doxy"));
            _supportedFileExtensions.Add(new FileExtensions("C/C++ files", EditorFileType.Cpp, ".h", ".c", ".hpp", ".cpp"));
        }

        public void Load()
        {
            using (IConfigurarionReader instance = new RegistryConfigurationStore(BaseName))
            {
                instance.Load($"{_companyName}/{_appName}");
                WorkspacePath = instance.ReadString("Workspace", "DefaultPath");
                // @TODO(final): Read supported file extensions
            }
        }

        public void Save()
        {
            using (IConfigurarionWriter instance = new RegistryConfigurationStore(BaseName))
            {
                instance.WriteString("Workspace", "DefaultPath", WorkspacePath);
                instance.Save($"{_companyName }/{_appName}");
                // @TODO(final): Write supported file extensions
            }
        }

        public EditorFileType GetFileTypeByExtension(string fileExt)
        {
            foreach (FileExtensions entry in _supportedFileExtensions)
            {
                if (entry.HasExtension(fileExt))
                    return (entry.FileType);
            }
            return (EditorFileType.Unknown);
        }
    }
}
