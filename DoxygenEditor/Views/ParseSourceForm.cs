using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TSP.DoxygenEditor.Models;
using TSP.DoxygenEditor.Languages.Cpp;
using TSP.DoxygenEditor.Symbols;
using TSP.DoxygenEditor.Parsers;
using System.Collections.Generic;
using TSP.DoxygenEditor.Editor;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.TextAnalysis;
using System.Collections.Immutable;

namespace TSP.DoxygenEditor.Views
{
    internal partial class ParseSourceForm: Form
    {
        private readonly Task _parseTask;

        public ParseSourceForm(ParseSourceActionType type, IEditor editor, string source)
        {
            InitializeComponent();

            _parseTask = type switch
            {
                ParseSourceActionType.APIPrototypes => Task.Run(() => ParseAPIPrototypesAsync(editor, source)),
                _ => throw new NotSupportedException($"Parse type {type} is not supported"),
            };
        }

        private void ClearLog()
        {
            if (rtbLog.InvokeRequired)
                rtbLog.Invoke(() => ClearLog());
            else
                rtbLog.Clear();
        }

        private void AddToLog(string line)
        {
            if (rtbLog.InvokeRequired)
                rtbLog.Invoke(() => AddToLog(line));
            else
                rtbLog.Text += $"{line}{Environment.NewLine}";
        }

        private Task ParseAPIPrototypesAsync(IEditor editor, string source) => Task.Run(() =>
        {
            ISymbolTableId symbolTableId = editor as ISymbolTableId;

            WorkspaceModel workspace = new WorkspaceModel(null, WorkspaceModelVersion.Current);

            AddToLog($"Tokenize source '{editor.Name}' with symbol table id '{symbolTableId}'");

            ParseContext.TokenizeResult tokenResult = ParseContext.TokenizeCpp(source, 0, source.Length, new TextPosition(), Languages.LanguageKind.Cpp);
            List<IBaseToken> tokens = tokenResult.Tokens.ToList();
            List<TextError> errors = new List<TextError>();
            List<PerformanceItemModel> performanceItems = new List<PerformanceItemModel>();

            AddToLog($"Parse source '{editor.Name}' from {tokens.Count} tokens");
            SymbolTable symbolTable = new SymbolTable(symbolTableId);
            ParseContext.ParseTreeResult parseResult = ParseContext.ParseTokens(
                symbolTable,
                workspace,
                tokens,
                errors,
                performanceItems,
                editor,
                source
            );

            Dictionary<string, CppFunctionDefinition> funcs = new Dictionary<string, CppFunctionDefinition>();

            Dictionary<string, CppMacroDefinition> macros = new Dictionary<string, CppMacroDefinition>();

            void ProcessNode(IBaseNode node) 
            {
                if (node is CppNode cppNode && cppNode.Entity?.DocumentationNode is not null)
                {
                    if (cppNode.Value is CppFunctionDefinition funcDef)
                        funcs.TryAdd(cppNode.Id, funcDef);
                    else if(cppNode.Value is CppMacroDefinition macroDef)
                        macros.TryAdd(cppNode.Id, macroDef);
                }

                foreach (IBaseNode child in node.Children)
                    ProcessNode(child);
            }

            ProcessNode(parseResult.CppTree);

            ImmutableSortedDictionary<string, CppFunctionDefinition> sortedFuncs = funcs.ToImmutableSortedDictionary(StringComparer.InvariantCultureIgnoreCase);

            ImmutableSortedDictionary<string, CppMacroDefinition> sortedMacros = macros.ToImmutableSortedDictionary(StringComparer.InvariantCultureIgnoreCase);

            AddToLog(string.Empty);
            AddToLog("Functions:");
            AddToLog(string.Empty);
            foreach (var functionPair in sortedFuncs)
                AddToLog($"{functionPair.Value}");

            AddToLog(string.Empty);
            AddToLog("Macros:");
            AddToLog(string.Empty);
            foreach (var macroPair in sortedMacros)
                AddToLog($"{macroPair.Value}");

            AddToLog($"Done");
        });
    }
}