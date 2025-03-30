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

            void ProcessNode(IBaseNode node) 
            {
                if (node is CppNode cppNode)
                {
                    if (cppNode.Entity is not null && cppNode.Entity.DocumentationNode is not null)
                    {
                        switch (cppNode.Entity.Kind)
                        {
                            case CppEntityKind.FunctionDefinition:
                                if (cppNode.Value is CppFunctionDefinition funcDef)
                                    funcs.Add(cppNode.Id, funcDef);
                                break;
                            case CppEntityKind.Typedef:
                                break;
                            case CppEntityKind.FunctionTypedef:
                                break;
                            case CppEntityKind.Enum:
                                break;
                            case CppEntityKind.EnumValue:
                                break;
                            case CppEntityKind.Struct:
                                break;
                            case CppEntityKind.Class:
                                break;
                            case CppEntityKind.MacroDefinition:
                                break;
                        }
                    }
                }

                foreach (IBaseNode child in node.Children)
                    ProcessNode(child);
            }

            ProcessNode(parseResult.CppTree);

            ImmutableSortedDictionary<string, CppFunctionDefinition> sortedFuncs = funcs.ToImmutableSortedDictionary(StringComparer.InvariantCultureIgnoreCase);

            foreach (var functionPair in sortedFuncs)
                AddToLog($"Function: {functionPair.Value}");

            AddToLog($"Done");
        });
    }
}