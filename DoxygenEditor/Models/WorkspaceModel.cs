using TSP.DoxygenEditor.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.IO;
using TSP.DoxygenEditor.Utils;

namespace TSP.DoxygenEditor.Models
{
    public class WorkspaceModel
    {
        const string DefaultWorkspaceNamespace = "https://tspsoftware.net/doxygeneditor/workspace/";

        public string FilePath { get; private set; }

        public interface IWorkspaceOptions<T>
        {
            void Assign(T other);
            void Load(IConfigurarionReader reader);
            void Save(IConfigurarionWriter writer);
        }

        public class ViewOptions : IWorkspaceOptions<ViewOptions>
        {
            const string SectionName = "View";

            public bool IsWhitespaceVisible { get; internal set; } = false;
            public double TreeSplitterDistance { get; internal set; } = 0.25;

            public void Assign(ViewOptions other)
            {
                IsWhitespaceVisible = other.IsWhitespaceVisible;
                TreeSplitterDistance = other.TreeSplitterDistance;
            }
            public void Load(IConfigurarionReader reader)
            {
                IsWhitespaceVisible = reader.ReadBool(SectionName, nameof(IsWhitespaceVisible), false);
                TreeSplitterDistance = reader.ReadDouble(SectionName, nameof(TreeSplitterDistance), 0.25);
            }
            public void Save(IConfigurarionWriter writer)
            {
                writer.WriteBool(SectionName, nameof(IsWhitespaceVisible), IsWhitespaceVisible);
                writer.WriteDouble(SectionName, nameof(TreeSplitterDistance), TreeSplitterDistance);
            }
        }

        public interface IBuildOptions<T>
        {
            void Assign(T other);
            void Load(IConfigurarionReader reader);
            void Save(IConfigurarionWriter writer);
        }

        public class BuildOptions : IBuildOptions<BuildOptions>
        {
            const string SectionName = "Build";

            public bool OpenInBrowser { get; internal set; } = true;
            public string PathToDoxygen { get; internal set; }
            public string BaseDirectory { get; internal set; }
            public string ConfigFile { get; internal set; }

            public void Assign(BuildOptions other)
            {
                OpenInBrowser = other.OpenInBrowser;
                PathToDoxygen = other.PathToDoxygen;
                BaseDirectory = other.BaseDirectory;
                ConfigFile = other.ConfigFile;
            }
            public void Load(IConfigurarionReader reader)
            {
                OpenInBrowser = reader.ReadBool(SectionName, nameof(OpenInBrowser), true);
                PathToDoxygen = reader.ReadString(SectionName, nameof(PathToDoxygen));
                BaseDirectory = reader.ReadString(SectionName, nameof(BaseDirectory));
                ConfigFile = reader.ReadString(SectionName, nameof(ConfigFile));
            }
            public void Save(IConfigurarionWriter writer)
            {
                writer.WriteBool(SectionName, nameof(OpenInBrowser), OpenInBrowser);
                writer.WriteString(SectionName, nameof(PathToDoxygen), PathToDoxygen);
                writer.WriteString(SectionName, nameof(BaseDirectory), BaseDirectory);
                writer.WriteString(SectionName, nameof(ConfigFile), ConfigFile);
            }
        }

        public class HistoryOptions : IWorkspaceOptions<HistoryOptions>
        {
            const string SectionName = "History";
            const int MaxRecentFileCount = 10;

            public readonly List<string> _recentFiles = new List<string>();
            public IEnumerable<string> RecentFiles => _recentFiles;

            public readonly List<string> _lastOpenedFiles = new List<string>();
            public IEnumerable<string> LastOpenedFiles => _lastOpenedFiles;
            public int LastOpenedFileCount => _lastOpenedFiles.Count;

            public void Assign(HistoryOptions other)
            {
                _recentFiles.Clear();
                _recentFiles.AddRange(other.RecentFiles);
                _lastOpenedFiles.Clear();
                _lastOpenedFiles.AddRange(other.LastOpenedFiles);
            }
            public void Load(IConfigurarionReader reader)
            {
                _recentFiles.Clear();
                _recentFiles.AddRange(reader.ReadList(SectionName, nameof(RecentFiles)));
                _lastOpenedFiles.Clear();
                _lastOpenedFiles.AddRange(reader.ReadList(SectionName, nameof(LastOpenedFiles)));
            }
            public void Save(IConfigurarionWriter writer)
            {
                writer.WriteList(SectionName, nameof(RecentFiles), _recentFiles);
                writer.WriteList(SectionName, nameof(LastOpenedFiles), _lastOpenedFiles);
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

        public class ParserCppOptions : IWorkspaceOptions<ParserCppOptions>
        {
            const string SectionName = "Parser/Cpp";
            public bool ExcludeFunctionBodies { get; internal set; } = false;
            public bool ExcludeFunctionBodySymbols { get; internal set; } = false;
            public bool ExcludeFunctionCallSymbols { get; internal set; } = false;

            public void Assign(ParserCppOptions other)
            {
                ExcludeFunctionBodies = other.ExcludeFunctionBodies;
                ExcludeFunctionBodySymbols = other.ExcludeFunctionBodySymbols;
                ExcludeFunctionCallSymbols = other.ExcludeFunctionCallSymbols;
            }

            public void Load(IConfigurarionReader reader)
            {
                ExcludeFunctionBodies = reader.ReadBool(SectionName, nameof(ExcludeFunctionBodies), false);
                ExcludeFunctionBodySymbols = reader.ReadBool(SectionName, nameof(ExcludeFunctionBodySymbols), false);
                ExcludeFunctionCallSymbols = reader.ReadBool(SectionName, nameof(ExcludeFunctionCallSymbols), false);
            }

            public void Save(IConfigurarionWriter writer)
            {
                writer.WriteBool(SectionName, nameof(ExcludeFunctionBodies), ExcludeFunctionBodies);
                writer.WriteBool(SectionName, nameof(ExcludeFunctionBodySymbols), ExcludeFunctionBodySymbols);
                writer.WriteBool(SectionName, nameof(ExcludeFunctionCallSymbols), ExcludeFunctionCallSymbols);
            }
        }

        public class ValidationCppOptions : IWorkspaceOptions<ValidationCppOptions>
        {
            const string SectionName = "Validation/Cpp";

            public bool ExcludePreprocessorMatch { get; internal set; }
            public bool ExcludePreprocessorUsage { get; internal set; }
            public bool RequireDoxygenReference { get; internal set; }
            public bool ValidateFunctionDefinitions { get; internal set; }

            public ImmutableArray<string> SkipFunctionPatterns { get; internal set; }
            public IEnumerable<Regex> SkipFunctionRexes
            {
                get
                {
                    if (_skipFunctionRexes.Length != SkipFunctionPatterns.Length)
                    {
                        List<Regex> list = new List<Regex>();
                        foreach (var pattern in SkipFunctionPatterns)
                            list.Add(new Regex(pattern, RegexOptions.Compiled));
                        _skipFunctionRexes = list.ToImmutableArray();
                    }
                    return _skipFunctionRexes;
                }
            }
            private ImmutableArray<Regex> _skipFunctionRexes = ImmutableArray<Regex>.Empty;

            public IEnumerable<Regex> CheckFunctionRexes
            {
                get
                {
                    if (_checkFunctionRexes.Length != CheckFunctionPatterns.Length)
                    {
                        List<Regex> list = new List<Regex>();
                        foreach (var pattern in CheckFunctionPatterns)
                            list.Add(new Regex(pattern, RegexOptions.Compiled));
                        _checkFunctionRexes = list.ToImmutableArray();
                    }
                    return _checkFunctionRexes;
                }
            }
            private ImmutableArray<Regex> _checkFunctionRexes = ImmutableArray<Regex>.Empty;
            public ImmutableArray<string> CheckFunctionPatterns { get; internal set; }

            public ValidationCppOptions()
            {
                ExcludePreprocessorMatch = false;
                ExcludePreprocessorUsage  = false;
                RequireDoxygenReference = true;
                ValidateFunctionDefinitions = true;
                SkipFunctionPatterns = new[] { "fplAtomic[a-zA-Z0-9_]+", "fpl__[a-zA-Z0-9_]+" }.ToImmutableArray();
                CheckFunctionPatterns = new[] { "fpl[A-Z][a-zA-Z0-9_]+", "FPL_[A-Z][A-Z0-9_]+" }.ToImmutableArray();
            }

            public void Assign(ValidationCppOptions other)
            {
                ExcludePreprocessorMatch = other.ExcludePreprocessorMatch;
                ExcludePreprocessorUsage = other.ExcludePreprocessorUsage;
                RequireDoxygenReference = other.RequireDoxygenReference;
                ValidateFunctionDefinitions = other.ValidateFunctionDefinitions;
                SkipFunctionPatterns = other.SkipFunctionPatterns;
                CheckFunctionPatterns = other.CheckFunctionPatterns;
            }

            public void Load(IConfigurarionReader reader)
            {
                ExcludePreprocessorMatch = reader.ReadBool(SectionName, nameof(ExcludePreprocessorMatch), false);
                ExcludePreprocessorUsage = reader.ReadBool(SectionName, nameof(ExcludePreprocessorUsage), false);
                RequireDoxygenReference = reader.ReadBool(SectionName, nameof(RequireDoxygenReference), true);
                ValidateFunctionDefinitions = reader.ReadBool(SectionName, nameof(ValidateFunctionDefinitions), true);
                SkipFunctionPatterns = reader.ReadList(SectionName, nameof(SkipFunctionPatterns)).ToImmutableArray();
                CheckFunctionPatterns = reader.ReadList(SectionName, nameof(CheckFunctionPatterns)).ToImmutableArray();
            }

            public void Save(IConfigurarionWriter writer)
            {
                writer.WriteBool(SectionName, nameof(ExcludePreprocessorMatch), ExcludePreprocessorMatch);
                writer.WriteBool(SectionName, nameof(ExcludePreprocessorUsage), ExcludePreprocessorUsage);
                writer.WriteBool(SectionName, nameof(RequireDoxygenReference), RequireDoxygenReference);
                writer.WriteBool(SectionName, nameof(ValidateFunctionDefinitions), ValidateFunctionDefinitions);
                writer.WriteList(SectionName, nameof(SkipFunctionPatterns), SkipFunctionPatterns);
                writer.WriteList(SectionName, nameof(CheckFunctionPatterns), CheckFunctionPatterns);
            }
        }

        public ViewOptions View { get; }
        public HistoryOptions History { get; }
        public ParserCppOptions ParserCpp { get; }
        public ValidationCppOptions ValidationCpp { get; }
        public BuildOptions Build { get; }

        public WorkspaceModel(string filePath)
        {
            FilePath = filePath;
            View = new ViewOptions();
            History = new HistoryOptions();
            ParserCpp = new ParserCppOptions();
            ValidationCpp = new ValidationCppOptions();
            Build = new BuildOptions();
        }

        public void Assign(WorkspaceModel other)
        {
            FilePath = other.FilePath;
            View.Assign(other.View);
            History.Assign(other.History);
            ParserCpp.Assign(other.ParserCpp);
            ValidationCpp.Assign(other.ValidationCpp);
            Build.Assign(other.Build);
        }

        public static Result<WorkspaceModel> Load(string filePath)
        {
            using (IConfigurarionReader reader = new JSONConfigurationStore("Workspace", new DefaultConfigurationConverter()))
            {
                Result<bool> loadRes = reader.Load(filePath);
                if (!loadRes.Success)
                    return new Result<WorkspaceModel>(loadRes.Error);
                WorkspaceModel result = new WorkspaceModel(filePath);
                result.View.Load(reader);
                result.History.Load(reader);
                result.ParserCpp.Load(reader);
                result.ValidationCpp.Load(reader);
                result.Build.Load(reader);
                return new Result<WorkspaceModel>(result);
            }
        }

        public void Save()
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(FilePath));
            using (IConfigurarionWriter writer = new JSONConfigurationStore("Workspace", new DefaultConfigurationConverter()))
            {
                View.Save(writer);
                History.Save(writer);
                ParserCpp.Save(writer);
                ValidationCpp.Save(writer);
                Build.Save(writer);
                writer.Save(FilePath);
            }
        }
        public void SaveAs(string filePath)
        {
            FilePath = filePath;
            Save();
        }

    }
}
