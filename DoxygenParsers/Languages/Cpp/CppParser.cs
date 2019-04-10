using System;
using System.Collections.Generic;
using System.Diagnostics;
using TSP.DoxygenEditor.Collections;
using TSP.DoxygenEditor.Languages.Doxygen;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.Symbols;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Languages.Cpp
{
    public class CppParser : BaseParser<CppEntity, CppToken>
    {
        public delegate IBaseNode GetDocumentationNodeEventHandler(IBaseToken token);
        public event GetDocumentationNodeEventHandler GetDocumentationNode;

        public class CppConfiguration
        {
            public bool ExcludeFunctionBodies { get; set; } = false;
            public bool ExcludeFunctionCallSymbols { get; set; } = false;
            public bool ExcludeFunctionBodySymbols { get; set; } = false;
        }

        public CppConfiguration Configuration { get; }

        public CppParser(ISymbolTableId id, CppConfiguration configuration) : base(id)
        {
            Configuration = configuration;
        }

        private IBaseNode FindDocumentationNode(LinkedListNode<IBaseToken> searchNode, int maxLineDelta)
        {
            IBaseToken searchToken = searchNode.Value;
            int start = searchToken.Index;
            int startLine = searchToken.Position.Line;
            int minEndLine = startLine - maxLineDelta;
            LinkedListNode<IBaseToken> n = searchNode;
            while (n != null)
            {
                IBaseToken baseToke = n.Value;
                if (baseToke.Position.Line >= minEndLine)
                {
                    DoxygenToken doxyToken = baseToke as DoxygenToken;
                    if (doxyToken != null)
                    {
                        if (doxyToken.Kind == DoxygenTokenKind.DoxyBlockStartSingle || doxyToken.Kind == DoxygenTokenKind.DoxyBlockEnd)
                        {
                            var foundNode = GetDocumentationNode?.Invoke(baseToke);
                            return (foundNode);
                        }
                    }
                }
                else
                    break;
                n = n.Previous;
            }
            return (null);
        }

        private SearchResult<CppToken> Search(LinkedListStream<IBaseToken> stream, SearchMode mode, params CppTokenKind[] kinds)
        {
            var searchFunc = new Func<CppToken, bool>((token) =>
            {
                foreach (CppTokenKind kind in kinds)
                {
                    if (token.Kind == kind)
                        return (true);
                }
                return (false);
            });
            SearchResult<CppToken> result = Search(stream.CurrentNode, mode, searchFunc);
            return (result);
        }
        private SearchResult<CppToken> Search(SearchResult<CppToken> inResult, SearchMode mode, params CppTokenKind[] kinds)
        {
            var searchFunc = new Func<CppToken, bool>((token) =>
            {
                foreach (CppTokenKind kind in kinds)
                {
                    if (token.Kind == kind)
                        return (true);
                }
                return (false);
            });
            SearchResult<CppToken> result = Search(inResult.Node, mode, searchFunc);
            return (result);
        }
        private bool IsToken(LinkedListStream<IBaseToken> stream, SearchMode mode, params CppTokenKind[] kinds)
        {
            var searchFunc = new Func<CppToken, bool>((token) =>
            {
                foreach (CppTokenKind kind in kinds)
                {
                    if (token.Kind == kind)
                        return (true);
                }
                return (false);
            });
            SearchResult<CppToken> result = Search(stream.CurrentNode, mode, searchFunc);
            return (result != null);

        }

        private void ParseEnumValues(LinkedListStream<IBaseToken> stream, CppNode rootNode)
        {
            CppToken leftBraceToken = stream.Peek<CppToken>();
            Debug.Assert(leftBraceToken.Kind == CppTokenKind.LeftBrace);
            stream.Next();
            while (!stream.IsEOF)
            {
                var rightBraceResult = Search(stream, SearchMode.Current, CppTokenKind.RightBrace);
                if (rightBraceResult != null)
                {
                    // Enum complete
                    stream.Next();
                    break;
                }
                var identResult = Search(stream, SearchMode.Current, CppTokenKind.IdentLiteral);
                if (identResult != null)
                {
                    stream.Seek(identResult.Node);

                    // Enum value
                    var enumValueToken = identResult.Token;
                    enumValueToken.Kind = CppTokenKind.MemberIdent;
                    string enumValueName = enumValueToken.Value;
                    stream.Next();

                    CppEntity enumValueEntity = new CppEntity(CppEntityKind.EnumValue, enumValueToken, enumValueName)
                    {
                        DocumentationNode = FindDocumentationNode(identResult.Node, 1),
                    };
                    CppNode enumValueNode = new CppNode(rootNode, enumValueEntity);
                    rootNode.AddChild(enumValueNode);
                    SymbolTable.AddSource(new SourceSymbol(enumValueToken.Lang, SourceSymbolKind.CppMember, enumValueName, enumValueToken.Range, enumValueNode));

                    var equalsResult = Search(stream, SearchMode.Current, CppTokenKind.EqOp);
                    if (equalsResult != null)
                    {
                        stream.Next();

                        // Skip until comma or right brace
                        var tmpResult = Search(stream, SearchMode.Forward, CppTokenKind.Comma, CppTokenKind.RightBrace);
                        if (tmpResult != null)
                            stream.Seek(tmpResult.Node);
                        else
                        {
                            AddError(equalsResult.Token.Position, $"Expect assignment token, but got token '{stream.Peek()}' for enum member '{enumValueName}'", "Enum", enumValueName);
                            break;
                        }
                    }

                    var commaOrRightBraceResult = Search(stream, SearchMode.Current, CppTokenKind.Comma, CppTokenKind.RightBrace);
                    if (commaOrRightBraceResult != null)
                    {
                        stream.Seek(commaOrRightBraceResult.Node);
                        if (commaOrRightBraceResult.Token.Kind == CppTokenKind.Comma)
                            stream.Next();
                        continue;
                    }
                    else
                    {
                        var tok = stream.Peek<CppToken>();
                        if (tok != null)
                            AddError(tok.Position, $"Unexpected token '{tok.Kind}'", "EnumValue");
                        break;
                    }
                }
                stream.Next();
            }
        }

        private void ParseEnum(LinkedListStream<IBaseToken> stream, TextPosition pos)
        {
            CppToken enumBaseToken = stream.Peek<CppToken>();
            Debug.Assert(enumBaseToken.Kind == CppTokenKind.ReservedKeyword && "enum".Equals(enumBaseToken.Value));
            stream.Next();

            SearchResult<CppToken> enumIdentResult = null;

            var classKeywordResult = Search(stream, SearchMode.Current, CppTokenKind.ReservedKeyword);
            if (classKeywordResult != null)
            {
                string keyword = classKeywordResult.Token.Value;
                if ("class".Equals(keyword))
                {
                    // enum class
                    stream.Next();
                    enumIdentResult = Search(stream, SearchMode.Current, CppTokenKind.IdentLiteral);
                    if (enumIdentResult == null)
                    {
                        var token = stream.Peek<CppToken>();
                        AddError(pos, $"Expect identifier token, but got token kind {token?.Kind} for enum", "enum");
                        return;
                    }
                }
            }

            if (enumIdentResult == null)
            {
                // Ident after enum
                var identAfterEnumResult = Search(stream, SearchMode.Current, CppTokenKind.IdentLiteral);
                if (identAfterEnumResult != null)
                {
                    stream.Next();
                    enumIdentResult = identAfterEnumResult;
                }
            }

            var braceOrSemiResult = Search(stream, SearchMode.Current, CppTokenKind.LeftBrace, CppTokenKind.Semicolon);
            if (braceOrSemiResult == null)
                return;

            CppEntityKind enumKind = braceOrSemiResult.Token.Kind == CppTokenKind.Semicolon ? CppEntityKind.ForwardEnum : CppEntityKind.Enum;
            CppNode enumRootNode = new CppNode(Top, null);

            // Enum values
            if (braceOrSemiResult.Token.Kind == CppTokenKind.LeftBrace)
                ParseEnumValues(stream, enumRootNode);
            else
                stream.Next();

            // Enum ident at the end (C-style)
            if (enumIdentResult == null)
            {
                enumIdentResult = Search(stream, SearchMode.Current, CppTokenKind.IdentLiteral);
                if (enumIdentResult != null)
                    stream.Next();
            }

            // Add enum only when we had a ident
            if (enumIdentResult != null)
            {
                CppToken enumIdentToken = enumIdentResult.Token;
                string enumIdent = enumIdentToken.Value;
                CppEntity enumRootEntity = new CppEntity(enumKind, enumIdentToken, enumIdent)
                {
                    DocumentationNode = FindDocumentationNode(enumIdentResult.Node, 1),
                };
                enumRootNode.Entity = enumRootEntity;
                Add(enumRootNode);
                SymbolTable.AddSource(new SourceSymbol(enumIdentToken.Lang, SourceSymbolKind.CppEnum, enumIdent, enumIdentToken.Range, enumRootNode));
            }
        }

        private void ParseStruct(LinkedListStream<IBaseToken> stream)
        {
            CppToken structKeywordToken = stream.Peek<CppToken>();
            Debug.Assert(structKeywordToken.Kind == CppTokenKind.ReservedKeyword && ("struct".Equals(structKeywordToken.Value) || "union".Equals(structKeywordToken.Value)));

            var typedefResult = Search(stream.CurrentNode, SearchMode.Prev, (t) => t.Kind == CppTokenKind.ReservedKeyword && "typedef".Equals(t.Value));
            bool isTypedef = typedefResult != null;

            stream.Next();

            // There are multiple ways to define a struct in C
            // A = typedef struct { int a; } foo;
            // B = typedef struct foo { int a; } sFoo; (prefered style)
            // C = struct { int a; }; (Anonymous struct without a name)
            // D = struct { int a; } memberName; (Anonymous struct named as a member)
            // E = struct foo { int a; }; (C++ style)

            // B/E
            var identTokenResult = Search(stream, SearchMode.Current, CppTokenKind.IdentLiteral);
            if (identTokenResult != null)
            {
                var identToken = identTokenResult.Token;
                string structIdent = identToken.Value;
                stream.Next();

                CppEntityKind kind = CppEntityKind.Struct;

                var terminatorTokenResult = Search(stream, SearchMode.Current, CppTokenKind.Semicolon, CppTokenKind.LeftBrace);
                if (terminatorTokenResult != null)
                {
                    var terminatorToken = terminatorTokenResult.Token;
                    if (terminatorToken.Kind == CppTokenKind.Semicolon)
                        kind = CppEntityKind.ForwardStruct;
                    stream.Next();
                }
                CppEntity structEntity = new CppEntity(kind, identToken, structIdent)
                {
                    DocumentationNode = FindDocumentationNode(identTokenResult.Node, 1),
                };
                CppNode structNode = new CppNode(Top, structEntity);
                Add(structNode);
                SymbolTable.AddSource(new SourceSymbol(identToken.Lang, SourceSymbolKind.CppStruct, structIdent, identToken.Range, structNode));
            }

            // @TODO(final): Parse struct members
        }

        private void ParseClass(LinkedListStream<IBaseToken> stream)
        {
            CppToken classKeywordToken = stream.Peek<CppToken>();
            Debug.Assert(classKeywordToken.Kind == CppTokenKind.ReservedKeyword && "class".Equals(classKeywordToken.Value));
            stream.Next();
            var identTokenResult = Search(stream, SearchMode.Current, CppTokenKind.IdentLiteral);
            if (identTokenResult != null)
            {
                var identToken = identTokenResult.Token;
                string classIdent = identToken.Value;
                stream.Next();

                CppEntityKind kind = CppEntityKind.Class;

                CppEntity classEntity = new CppEntity(kind, identToken, classIdent)
                {
                    DocumentationNode = FindDocumentationNode(identTokenResult.Node, 1),
                };
                CppNode classNode = new CppNode(Top, classEntity);
                Add(classNode);
                SymbolTable.AddSource(new SourceSymbol(identToken.Lang, SourceSymbolKind.CppClass, classIdent, identToken.Range, classNode));

                // @TODO(final): Parse class members
            }
        }

        private void ParseTypedef(LinkedListStream<IBaseToken> stream)
        {
            CppToken typedefToken = stream.Peek<CppToken>();
            Debug.Assert(typedefToken.Kind == CppTokenKind.ReservedKeyword && "typedef".Equals(typedefToken.Value));
            stream.Next();

            var reservedKeywordResult = Search(stream, SearchMode.Current, CppTokenKind.ReservedKeyword);
            if (reservedKeywordResult != null)
            {
                var reservedKeywordToken = reservedKeywordResult.Token;
                string keyword = reservedKeywordToken.Value;
                switch (keyword)
                {
                    case "union":
                    case "struct":
                        ParseStruct(stream);
                        return;

                    case "class":
                        ParseClass(stream);
                        return;

                    case "enum":
                        ParseEnum(stream, reservedKeywordToken.Position);
                        return;
                }
            }

            // Normal typedef
            CppEntityKind kind = CppEntityKind.Typedef;
            var semicolonResult = Search(stream, SearchMode.Forward, CppTokenKind.Semicolon);
            if (semicolonResult != null)
            {
                stream.Seek(semicolonResult.Node);
                stream.Next();

                SearchResult<CppToken> identResult = null;
                // @TODO(final): Support for array typedef, such as: typedef int myArray[16 + 3];
                var prevResult = Search(semicolonResult, SearchMode.Prev, CppTokenKind.RightParen, CppTokenKind.IdentLiteral);
                if (prevResult != null)
                {
                    if (prevResult.Token.Kind == CppTokenKind.RightParen)
                    {
                        // Function typedef
                        var leftParenResult = Search(prevResult, SearchMode.Backward, CppTokenKind.LeftParen);
                        if (leftParenResult != null)
                        {
                            var rightParentResult = Search(leftParenResult, SearchMode.Prev, CppTokenKind.RightParen);
                            if (rightParentResult != null)
                            {
                                leftParenResult = Search(rightParentResult, SearchMode.Backward, CppTokenKind.LeftParen);
                                if (leftParenResult != null)
                                {
                                    identResult = Search(leftParenResult, SearchMode.Next, CppTokenKind.IdentLiteral);
                                    kind = CppEntityKind.FunctionTypedef;
                                }
                            }
                            else
                            {
                                // Typedef on define
                                // #define MY_TYPEDEF(name)
                                // typedef MY_TYPEDEF(my_typedef);
                                //Debug.WriteLine("Right paren not found!");
                            }
                        }
                    }
                    else
                    {
                        // Type typedef
                        Debug.Assert(prevResult.Token.Kind == CppTokenKind.IdentLiteral);
                        identResult = prevResult;
                    }
                }

                if (identResult != null)
                {
                    var identToken = identResult.Token;
                    string typedefIdent = identToken.Value;
                    CppEntity typedefEntity = new CppEntity(kind, identToken, typedefIdent)
                    {
                        DocumentationNode = FindDocumentationNode(identResult.Node, 1),
                    };
                    var typedefNode = new CppNode(Top, typedefEntity);
                    Add(typedefNode);
                    SymbolTable.AddSource(new SourceSymbol(identToken.Lang, SourceSymbolKind.CppType, typedefIdent, identToken.Range));
                }
            }
        }

        enum PreprocessorMacroKind
        {
            Source,
            MacroMatch,
            MacroUsage
        }

        private void AddPreprocessorDefine(CppToken token, LinkedListNode<IBaseToken> node, CppEntityKind entityKind, PreprocessorMacroKind macroKind)
        {
            CppEntity defineKeyEntity = new CppEntity(entityKind, token, token.Value)
            {
                DocumentationNode = FindDocumentationNode(node, 1),
                Value = token.Value
            };
            CppNode defineNode = new CppNode(Top, defineKeyEntity);
            Add(defineNode);
            if (macroKind == PreprocessorMacroKind.Source)
                SymbolTable.AddSource(new SourceSymbol(token.Lang, SourceSymbolKind.CppMacro, token.Value, token.Range, defineNode));
            else
            {
                ReferenceSymbolKind referenceKind;
                if (macroKind == PreprocessorMacroKind.MacroMatch)
                    referenceKind = ReferenceSymbolKind.CppMacroMatch;
                else
                    referenceKind = ReferenceSymbolKind.CppMacroUsage;
                SymbolTable.AddReference(new ReferenceSymbol(token.Lang, referenceKind, token.Value, token.Range, defineNode));
            }
        }

        private ParseTokenResult ParsePreprocessor(LinkedListStream<IBaseToken> stream)
        {
            CppToken token = stream.Peek<CppToken>();
            Debug.Assert(token.Kind == CppTokenKind.PreprocessorStart);
            stream.Next();
            var keywordResult = Search(stream, SearchMode.Current, CppTokenKind.PreprocessorKeyword);
            if (keywordResult != null)
            {
                var keywordToken = keywordResult.Token;
                stream.Next();
                if ("define".Equals(keywordResult.Token.Value))
                {
                    var defineResult = Search(stream, SearchMode.Current, CppTokenKind.PreprocessorDefineSource);
                    if (defineResult != null)
                    {
                        var defineNameToken = defineResult.Token;
                        string defineName = string.Intern(defineNameToken.Value);
                        stream.Next();

                        var defineValueResult = Search(stream, SearchMode.Current, CppTokenKind.IdentLiteral);
                        if (defineValueResult != null)
                            stream.Next();

                        CppEntity defineKeyEntity = new CppEntity(CppEntityKind.MacroDefinition, defineNameToken, defineName)
                        {
                            DocumentationNode = FindDocumentationNode(defineResult.Node, 1),
                            Value = defineValueResult?.Token.Value
                        };
                        CppNode defineNode = new CppNode(Top, defineKeyEntity);
                        Add(defineNode);
                        SymbolTable.AddSource(new SourceSymbol(defineNameToken.Lang, SourceSymbolKind.CppMacro, defineName, defineNameToken.Range, defineNode));
                    }
                    else
                        AddError(keywordToken.Position, $"Processor define has no identifier!", "Define");
                }
            }
            return (ParseTokenResult.AlreadyAdvanced);
        }

        private ParseTokenResult ParseReservedKeyword(LinkedListStream<IBaseToken> stream)
        {
            CppToken keywordToken = stream.Peek<CppToken>();
            Debug.Assert(keywordToken.Kind == CppTokenKind.ReservedKeyword);
            string keyword = keywordToken.Value;
            switch (keyword)
            {
                case "typedef":
                    ParseTypedef(stream);
                    return (ParseTokenResult.AlreadyAdvanced);

                case "struct":
                case "union":
                    ParseStruct(stream);
                    return (ParseTokenResult.AlreadyAdvanced);

                case "class":
                    ParseClass(stream);
                    return (ParseTokenResult.AlreadyAdvanced);

                case "enum":
                    ParseEnum(stream, keywordToken.Position);
                    return (ParseTokenResult.AlreadyAdvanced);

                default:
                    return (ParseTokenResult.ReadNext);
            }
        }

        private List<CppToken> ParseType(LinkedListStream<IBaseToken> stream)
        {
            List<CppToken> result = new List<CppToken>();
            return (result);
        }

        private bool ParseFunction(LinkedListStream<IBaseToken> stream)
        {
            var functionIdentNode = stream.CurrentNode;
            CppToken functionIdentToken = functionIdentNode.Value as CppToken;
            Debug.Assert(functionIdentToken.Kind == CppTokenKind.IdentLiteral);
            var functionName = functionIdentToken.Value;

            HashSet<CppTokenKind> funcBeforeKinds = new HashSet<CppTokenKind>()
            {
                CppTokenKind.IdentLiteral,
                CppTokenKind.ReservedKeyword,
                CppTokenKind.GlobalTypeKeyword,
                CppTokenKind.UserTypeIdent,
                CppTokenKind.PreprocessorDefineUsage, // This wont happen?
            };

#if false
            CppTokenKind[] funcCallKinds =
            {
                CppTokenKind.EqOp,
                CppTokenKind.Comma,
                CppTokenKind.LeftParen,
                CppTokenKind.LeftBrace,
                CppTokenKind.LeftBracket,
                CppTokenKind.AddOp,
                CppTokenKind.AddAssign,
                CppTokenKind.SubOp,
                CppTokenKind.SubAssign,
                CppTokenKind.DivOp,
                CppTokenKind.DivAssign,
                CppTokenKind.MulAssign,
                CppTokenKind.XorOp,
                CppTokenKind.XorAssign,
                CppTokenKind.LeftShiftOp,
                CppTokenKind.LeftShiftAssign,
                CppTokenKind.RightShiftOp,
                CppTokenKind.RightShiftAssign,
            };

            var funcCallResult = Search(stream, SearchMode.Prev, funcCallKinds);
#endif

            List<CppToken> prevTokens = new List<CppToken>();
            var prevNode = stream.CurrentNode.Previous;
            while (prevNode != null)
            {
                if (!(prevNode.Value is CppToken))
                    break;
                CppToken cppToken = (CppToken)prevNode.Value;
                if (!funcBeforeKinds.Contains(cppToken.Kind))
                    break;
                prevTokens.Add(cppToken);
                prevNode = prevNode.Previous;
            }

            // Skip ident
            stream.Next();

            // Skip parameters
            CppToken openParenToken = stream.Peek<CppToken>();
            Debug.Assert(openParenToken.Kind == CppTokenKind.LeftParen);
            stream.Next();

            // @TODO(final): These wont work, when arguments are filled in from another function calls!
            var closeParenResult = Search(stream, SearchMode.Forward, CppTokenKind.RightParen);
            if (closeParenResult == null)
            {
                AddError(openParenToken.Position, $"Unterminated function '{functionName}'", "Function", functionName);
                return (true);
            }
            stream.Seek(closeParenResult.Node);
            stream.Next();

            CppEntityKind kind = CppEntityKind.FunctionCall;

            var endingTokenResult = Search(stream, SearchMode.Current, CppTokenKind.Semicolon, CppTokenKind.LeftBrace);
            if (endingTokenResult != null)
            {
                stream.Next();
                if (endingTokenResult.Token.Kind == CppTokenKind.LeftBrace)
                {
                    kind = CppEntityKind.FunctionBody;
                    if (Configuration.ExcludeFunctionBodies)
                        return (true);
                }
                else
                {
                    kind = CppEntityKind.FunctionDefinition;
                }
            }

            CppEntity functionEntity = new CppEntity(kind, functionIdentToken, functionName)
            {
                DocumentationNode = FindDocumentationNode(functionIdentNode, 1),
            };
            CppNode functionNode = new CppNode(Top, functionEntity);
            Add(functionNode);

            if (kind == CppEntityKind.FunctionDefinition)
                SymbolTable.AddSource(new SourceSymbol(functionIdentToken.Lang, SourceSymbolKind.CppFunctionDefinition, functionName, functionIdentToken.Range, functionNode));
            else if (kind == CppEntityKind.FunctionBody && !Configuration.ExcludeFunctionBodySymbols)
                SymbolTable.AddSource(new SourceSymbol(functionIdentToken.Lang, SourceSymbolKind.CppFunctionBody, functionName, functionIdentToken.Range, functionNode));
            else if (kind == CppEntityKind.FunctionCall && !Configuration.ExcludeFunctionCallSymbols)
                SymbolTable.AddReference(new ReferenceSymbol(functionIdentToken.Lang, ReferenceSymbolKind.CppFunction, functionName, functionIdentToken.Range, functionNode));

            return (true);
        }

        public override ParseTokenResult ParseToken(LinkedListStream<IBaseToken> stream)
        {
            var token = stream.Peek<CppToken>();
            if (token == null) return (ParseTokenResult.ReadNext);
            var node = stream.CurrentNode;
            switch (token.Kind)
            {
                case CppTokenKind.PreprocessorStart:
                    {
                        return ParsePreprocessor(stream);
                    }

                case CppTokenKind.PreprocessorDefineSource:
                    {
                        AddPreprocessorDefine(token, node, CppEntityKind.MacroDefinition, PreprocessorMacroKind.Source);
                        stream.Next();
                        return (ParseTokenResult.AlreadyAdvanced);
                    }

                case CppTokenKind.PreprocessorDefineMatch:
                    {
                        AddPreprocessorDefine(token, node, CppEntityKind.MacroMatch, PreprocessorMacroKind.MacroMatch);
                        stream.Next();
                        return (ParseTokenResult.AlreadyAdvanced);
                    }

                case CppTokenKind.ReservedKeyword:
                    return ParseReservedKeyword(stream);
            }
            return (ParseTokenResult.ReadNext);
        }

        public override void Finished(IEnumerable<IBaseToken> tokens)
        {
            LinkedListStream<IBaseToken> tokenStream;

            tokenStream = new LinkedListStream<IBaseToken>(tokens);
            while (!tokenStream.IsEOF)
            {
                CppToken token = tokenStream.Peek<CppToken>();
                if (token != null)
                {
                    if (token.Kind == CppTokenKind.IdentLiteral)
                    {
                        string intern = string.IsInterned(token.Value);
                        string value = intern ?? token.Value;
                        SourceSymbol sourceSymbol = SymbolTable.GetSource(value);
                        if (sourceSymbol != null)
                        {
                            ReferenceSymbolKind refKind = ReferenceSymbolKind.Any;
                            if (sourceSymbol.Kind == SourceSymbolKind.CppMacro)
                            {
                                token.Kind = CppTokenKind.PreprocessorDefineUsage;
                                refKind = ReferenceSymbolKind.CppMacroUsage;
                            }
                            else if (sourceSymbol.Kind == SourceSymbolKind.CppFunctionDefinition || sourceSymbol.Kind == SourceSymbolKind.CppFunctionBody)
                            {
                                token.Kind = CppTokenKind.FunctionIdent;
                                refKind = ReferenceSymbolKind.CppFunction;
                            }
                            else if (
                                sourceSymbol.Kind == SourceSymbolKind.CppType ||
                                sourceSymbol.Kind == SourceSymbolKind.CppStruct ||
                                sourceSymbol.Kind == SourceSymbolKind.CppClass ||
                                sourceSymbol.Kind == SourceSymbolKind.CppEnum)
                            {
                                token.Kind = CppTokenKind.UserTypeIdent;
                                refKind = ReferenceSymbolKind.CppType;
                            }
                            else if (sourceSymbol.Kind == SourceSymbolKind.CppMember)
                            {
                                token.Kind = CppTokenKind.MemberIdent;
                                refKind = ReferenceSymbolKind.CppMember;
                            }
                            SymbolTable.AddReference(new ReferenceSymbol(token.Lang, refKind, token.Value, token.Range, null));
                        }
                    }
                }
                tokenStream.Next();
            }

            //
            // Parse functions
            //
            tokenStream = new LinkedListStream<IBaseToken>(tokens);
            while (!tokenStream.IsEOF)
            {
                CppToken token = tokenStream.Peek<CppToken>();
                bool skipNext = false;
                if (token != null)
                {
                    switch (token.Kind)
                    {
                        case CppTokenKind.IdentLiteral:
                            {
                                if (IsToken(tokenStream, SearchMode.Next, CppTokenKind.LeftParen))
                                    skipNext = ParseFunction(tokenStream);
                            }
                            break;
                    }
                }
                if (!skipNext)
                    tokenStream.Next();
            }
        }
    }
}
