using TSP.DoxygenEditor.Services;

namespace TSP.DoxygenEditor.Models
{
    public class GlobalConfigModel
    {
        const string BaseName = "GlobalConfig";

        private readonly string _companyName;
        private readonly string _appName;

        public string WorkspacePath { get; set; }
        public bool RestoreLastOpenedFiles { get; set; }

        public GlobalConfigModel(string companyName, string appName)
        {
            _companyName = companyName;
            _appName = appName;
        }

        public void Load()
        {
            using (IConfigurarionReader instance = new RegistryConfigurationStore(BaseName))
            {
                instance.Load($"{_companyName}/{_appName}");
                RestoreLastOpenedFiles = instance.ReadBool("Startup", "RestoreLastOpenedFiles", false);
                WorkspacePath = instance.ReadString("Workspace", "DefaultPath");
            }
        }

        public void Save()
        {
            using (IConfigurarionWriter instance = new RegistryConfigurationStore(BaseName))
            {
                instance.WriteBool("Startup", "RestoreLastOpenedFiles", RestoreLastOpenedFiles);
                instance.WriteString("Workspace", "DefaultPath", WorkspacePath);
                instance.Save($"{_companyName }/{_appName}");
            }
        }
    }
}
