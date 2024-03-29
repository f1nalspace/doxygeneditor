﻿using TSP.DoxygenEditor.Services;
using System.Collections.Generic;
using System.Diagnostics;

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
                IsWhitespaceVisible = reader.ReadBool(SectionName, () => IsWhitespaceVisible, false);
                TreeSplitterDistance = reader.ReadDouble(SectionName, () => TreeSplitterDistance, 0.25);
            }
            public void Save(IConfigurarionWriter writer)
            {
                writer.WriteBool(SectionName, () => IsWhitespaceVisible, IsWhitespaceVisible);
                writer.WriteDouble(SectionName, () => TreeSplitterDistance, TreeSplitterDistance);
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
                OpenInBrowser = reader.ReadBool(SectionName, () => OpenInBrowser, true);
                PathToDoxygen = reader.ReadString(SectionName, () => PathToDoxygen);
                BaseDirectory = reader.ReadString(SectionName, () => BaseDirectory);
                ConfigFile = reader.ReadString(SectionName, () => ConfigFile);
            }
            public void Save(IConfigurarionWriter writer)
            {
                writer.WriteBool(SectionName, () => OpenInBrowser, OpenInBrowser);
                writer.WriteString(SectionName, () => PathToDoxygen, PathToDoxygen);
                writer.WriteString(SectionName, () => BaseDirectory, BaseDirectory);
                writer.WriteString(SectionName, () => ConfigFile, ConfigFile);
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
                _recentFiles.AddRange(reader.ReadList(SectionName, () => RecentFiles));
                _lastOpenedFiles.Clear();
                _lastOpenedFiles.AddRange(reader.ReadList(SectionName, () => LastOpenedFiles));
            }
            public void Save(IConfigurarionWriter writer)
            {
                writer.WriteList(SectionName, () => RecentFiles, _recentFiles);
                writer.WriteList(SectionName, () => LastOpenedFiles, _lastOpenedFiles);
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
                ExcludeFunctionBodies = reader.ReadBool(SectionName, () => ExcludeFunctionBodies, false);
                ExcludeFunctionBodySymbols = reader.ReadBool(SectionName, () => ExcludeFunctionBodySymbols, false);
                ExcludeFunctionCallSymbols = reader.ReadBool(SectionName, () => ExcludeFunctionCallSymbols, false);
            }
            public void Save(IConfigurarionWriter writer)
            {
                writer.WriteBool(SectionName, () => ExcludeFunctionBodies, ExcludeFunctionBodies);
                writer.WriteBool(SectionName, () => ExcludeFunctionBodySymbols, ExcludeFunctionBodySymbols);
                writer.WriteBool(SectionName, () => ExcludeFunctionCallSymbols, ExcludeFunctionCallSymbols);
            }
        }

        public class ValidationCppOptions : IWorkspaceOptions<ValidationCppOptions>
        {
            const string SectionName = "Validation/Cpp";
            public bool ExcludePreprocessorMatch { get; internal set; } = false;
            public bool ExcludePreprocessorUsage { get; internal set; } = false;
            public bool RequireDoxygenReference { get; internal set; } = true;
            public void Assign(ValidationCppOptions other)
            {
                ExcludePreprocessorMatch = other.ExcludePreprocessorMatch;
                ExcludePreprocessorUsage = other.ExcludePreprocessorUsage;
                RequireDoxygenReference = other.RequireDoxygenReference;
            }

            public void Load(IConfigurarionReader reader)
            {
                ExcludePreprocessorMatch = reader.ReadBool(SectionName, () => ExcludePreprocessorMatch, false);
                ExcludePreprocessorUsage = reader.ReadBool(SectionName, () => ExcludePreprocessorUsage, false);
                RequireDoxygenReference = reader.ReadBool(SectionName, () => RequireDoxygenReference, true);
            }

            public void Save(IConfigurarionWriter writer)
            {
                writer.WriteBool(SectionName, () => ExcludePreprocessorMatch, ExcludePreprocessorMatch);
                writer.WriteBool(SectionName, () => ExcludePreprocessorUsage, ExcludePreprocessorUsage);
                writer.WriteBool(SectionName, () => RequireDoxygenReference, RequireDoxygenReference);
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

        public static WorkspaceModel Load(string filePath)
        {
            WorkspaceModel result = new WorkspaceModel(filePath);
            using (IConfigurarionReader reader = new XMLConfigurationStore(DefaultWorkspaceNamespace, "Workspace", new DefaultConfigurationConverter()))
            {
                if (!reader.Load(filePath))
                    result = null;
                result.View.Load(reader);
                result.History.Load(reader);
                result.ParserCpp.Load(reader);
                result.ValidationCpp.Load(reader);
                result.Build.Load(reader);
            }
            return (result);
        }

        public void Save()
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(FilePath));
            using (IConfigurarionWriter writer = new XMLConfigurationStore(DefaultWorkspaceNamespace, "Workspace", new DefaultConfigurationConverter()))
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
