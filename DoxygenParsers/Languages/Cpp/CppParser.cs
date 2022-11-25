using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public GetDocumentationNodeEventHandler GetDocumentationNode;

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
                            IBaseNode foundNode = GetDocumentationNode?.Invoke(baseToke);
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
            Func<CppToken, bool> searchFunc = new Func<CppToken, bool>((token) =>
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
            Func<CppToken, bool> searchFunc = new Func<CppToken, bool>((token) =>
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
            Func<CppToken, bool> searchFunc = new Func<CppToken, bool>((token) =>
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
                SearchResult<CppToken> rightBraceResult = Search(stream, SearchMode.Current, CppTokenKind.RightBrace);
                if (rightBraceResult != null)
                {
                    // Enum complete
                    stream.Next();
                    break;
                }
                SearchResult<CppToken> identResult = Search(stream, SearchMode.Current, CppTokenKind.IdentLiteral);
                if (identResult != null)
                {
                    stream.Seek(identResult.Node);

                    // Enum value
                    CppToken enumValueToken = identResult.Token;
                    enumValueToken.Kind = CppTokenKind.MemberIdent;
                    string enumValueName = enumValueToken.Value;
                    stream.Next();

                    CppEntity enumValueEntity = new CppEntity(CppEntityKind.EnumValue, enumValueToken, enumValueName)
                    {
                        DocumentationNode = FindDocumentationNode(identResult.Node, 1),
                    };
                    CppNode enumValueNode = new CppNode(rootNode, enumValueEntity);
                    rootNode.AddChild(enumValueNode);
                    LocalSymbolTable.AddSource(new SourceSymbol(enumValueToken.Lang, SourceSymbolKind.CppMember, enumValueName, enumValueToken.Range, enumValueNode));

                    SearchResult<CppToken> equalsResult = Search(stream, SearchMode.Current, CppTokenKind.EqOp);
                    if (equalsResult != null)
                    {
                        stream.Next();

                        // Skip until comma or right brace
                        SearchResult<CppToken> tmpResult = Search(stream, SearchMode.Forward, CppTokenKind.Comma, CppTokenKind.RightBrace);
                        if (tmpResult != null)
                            stream.Seek(tmpResult.Node);
                        else
                        {
                            AddError(equalsResult.Token.Position, $"Expect assignment token, but got token '{stream.Peek()}' for enum member '{enumValueName}'", "Enum", enumValueName);
                            break;
                        }
                    }

                    SearchResult<CppToken> commaOrRightBraceResult = Search(stream, SearchMode.Current, CppTokenKind.Comma, CppTokenKind.RightBrace);
                    if (commaOrRightBraceResult != null)
                    {
                        stream.Seek(commaOrRightBraceResult.Node);
                        if (commaOrRightBraceResult.Token.Kind == CppTokenKind.Comma)
                            stream.Next();
                        continue;
                    }
                    else
                    {
                        CppToken tok = stream.Peek<CppToken>();
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

            SearchResult<CppToken> classKeywordResult = Search(stream, SearchMode.Current, CppTokenKind.ReservedKeyword);
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
                        CppToken token = stream.Peek<CppToken>();
                        AddError(pos, $"Expect identifier token, but got token kind {token?.Kind} for enum", "enum");
                        return;
                    }
                }
            }

            if (enumIdentResult == null)
            {
                // Ident after enum
                SearchResult<CppToken> identAfterEnumResult = Search(stream, SearchMode.Current, CppTokenKind.IdentLiteral);
                if (identAfterEnumResult != null)
                {
                    stream.Next();
                    enumIdentResult = identAfterEnumResult;
                }
            }

            SearchResult<CppToken> braceOrSemiResult = Search(stream, SearchMode.Current, CppTokenKind.LeftBrace, CppTokenKind.Semicolon);
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
                LocalSymbolTable.AddSource(new SourceSymbol(enumIdentToken.Lang, SourceSymbolKind.CppEnum, enumIdent, enumIdentToken.Range, enumRootNode));
            }
        }

        private void ParseStruct(LinkedListStream<IBaseToken> stream)
        {
            CppToken structKeywordToken = stream.Peek<CppToken>();
            Debug.Assert(structKeywordToken.Kind == CppTokenKind.ReservedKeyword && ("struct".Equals(structKeywordToken.Value) || "union".Equals(structKeywordToken.Value)));

            SearchResult<CppToken> typedefResult = Search(stream.CurrentNode, SearchMode.Prev, (t) => t.Kind == CppTokenKind.ReservedKeyword && "typedef".Equals(t.Value));
            bool isTypedef = typedefResult != null;

            stream.Next();

            // There are multiple ways to define a struct in C
            // A = typedef struct { int a; } foo;
            // B = typedef struct foo { int a; } sFoo; (prefered style)
            // C = struct { int a; }; (Anonymous struct without a name)
            // D = struct { int a; } memberName; (Anonymous struct named as a member)
            // E = struct foo { int a; }; (C++ style)

            // B/E
            SearchResult<CppToken> identTokenResult = Search(stream, SearchMode.Current, CppTokenKind.IdentLiteral);
            if (identTokenResult != null)
            {
                CppToken identToken = identTokenResult.Token;
                string structIdent = identToken.Value;
                stream.Next();

                CppEntityKind kind = CppEntityKind.Struct;

                SearchResult<CppToken> terminatorTokenResult = Search(stream, SearchMode.Current, CppTokenKind.Semicolon, CppTokenKind.LeftBrace);
                if (terminatorTokenResult != null)
                {
                    CppToken terminatorToken = terminatorTokenResult.Token;
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
                LocalSymbolTable.AddSource(new SourceSymbol(identToken.Lang, SourceSymbolKind.CppStruct, structIdent, identToken.Range, structNode));
            }

            // @TODO(final): Parse struct members
        }

        private void ParseClass(LinkedListStream<IBaseToken> stream)
        {
            CppToken classKeywordToken = stream.Peek<CppToken>();
            Debug.Assert(classKeywordToken.Kind == CppTokenKind.ReservedKeyword && "class".Equals(classKeywordToken.Value));
            stream.Next();
            SearchResult<CppToken> identTokenResult = Search(stream, SearchMode.Current, CppTokenKind.IdentLiteral);
            if (identTokenResult != null)
            {
                CppToken identToken = identTokenResult.Token;
                string classIdent = identToken.Value;
                stream.Next();

                CppEntityKind kind = CppEntityKind.Class;

                CppEntity classEntity = new CppEntity(kind, identToken, classIdent)
                {
                    DocumentationNode = FindDocumentationNode(identTokenResult.Node, 1),
                };
                CppNode classNode = new CppNode(Top, classEntity);
                Add(classNode);
                LocalSymbolTable.AddSource(new SourceSymbol(identToken.Lang, SourceSymbolKind.CppClass, classIdent, identToken.Range, classNode));

                // @TODO(final): Parse class members
            }
        }

        private void ParseTypedef(LinkedListStream<IBaseToken> stream)
        {
            CppToken typedefToken = stream.Peek<CppToken>();
            Debug.Assert(typedefToken.Kind == CppTokenKind.ReservedKeyword && "typedef".Equals(typedefToken.Value));
            stream.Next();

            SearchResult<CppToken> reservedKeywordResult = Search(stream, SearchMode.Current, CppTokenKind.ReservedKeyword);
            if (reservedKeywordResult != null)
            {
                CppToken reservedKeywordToken = reservedKeywordResult.Token;
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
            SearchResult<CppToken> semicolonResult = Search(stream, SearchMode.Forward, CppTokenKind.Semicolon);
            if (semicolonResult != null)
            {
                stream.Seek(semicolonResult.Node);
                stream.Next();

                SearchResult<CppToken> identResult = null;

                // @TODO(final): Support for array typedef, such as: typedef int myArray[16 + 3];

                SearchResult<CppToken> prevResult = Search(semicolonResult, SearchMode.Prev, CppTokenKind.RightParen, CppTokenKind.IdentLiteral);
                if (prevResult != null)
                {
                    if (prevResult.Token.Kind == CppTokenKind.RightParen)
                    {
                        // Function typedef
                        SearchResult<CppToken> leftParenResult = Search(prevResult, SearchMode.Backward, CppTokenKind.LeftParen);
                        if (leftParenResult != null)
                        {
                            SearchResult<CppToken> rightParentResult = Search(leftParenResult, SearchMode.Prev, CppTokenKind.RightParen);
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
                                // @TODO(final): Support for special typedef based on macros - or expand macros entirely
                                //
                                // Examples:
                                // #define MY_TYPEDEF(name)
                                // typedef MY_TYPEDEF(my_typedef);
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
                    CppToken identToken = identResult.Token;
                    string typedefIdent = identToken.Value;
                    CppEntity typedefEntity = new CppEntity(kind, identToken, typedefIdent)
                    {
                        DocumentationNode = FindDocumentationNode(identResult.Node, 1),
                    };
                    CppNode typedefNode = new CppNode(Top, typedefEntity);
                    Add(typedefNode);
                    LocalSymbolTable.AddSource(new SourceSymbol(identToken.Lang, SourceSymbolKind.CppType, typedefIdent, identToken.Range));
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
                LocalSymbolTable.AddSource(new SourceSymbol(token.Lang, SourceSymbolKind.CppMacro, token.Value, token.Range, defineNode));
            else
            {
                ReferenceSymbolKind referenceKind;
                if (macroKind == PreprocessorMacroKind.MacroMatch)
                    referenceKind = ReferenceSymbolKind.CppMacroMatch;
                else
                    referenceKind = ReferenceSymbolKind.CppMacroUsage;
                LocalSymbolTable.AddReference(new ReferenceSymbol(token.Lang, referenceKind, token.Value, token.Range, defineNode));
            }
        }

        private ParseTokenResult ParsePreprocessor(LinkedListStream<IBaseToken> stream)
        {
            CppToken token = stream.Peek<CppToken>();
            Debug.Assert(token.Kind == CppTokenKind.PreprocessorStart);
            stream.Next();

            while (!stream.IsEOF)
            {
                token = stream.Peek<CppToken>();
                if (token == null) return (ParseTokenResult.AlreadyAdvanced);
                if (token.Kind == CppTokenKind.PreprocessorDefineMatch)
                    AddPreprocessorDefine(token, stream.CurrentNode, CppEntityKind.MacroMatch, PreprocessorMacroKind.MacroMatch);
                else if (token.Kind == CppTokenKind.PreprocessorDefineSource || token.Kind == CppTokenKind.PreprocessorFunctionSource)
                    AddPreprocessorDefine(token, stream.CurrentNode, CppEntityKind.MacroDefinition, PreprocessorMacroKind.Source);
                else if (token.Kind == CppTokenKind.PreprocessorEnd)
                    break;
                stream.Next();
            }

            CppToken endToken = stream.Peek<CppToken>();
            Debug.Assert(endToken.Kind == CppTokenKind.PreprocessorEnd);
            stream.Next();

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

        private IEnumerable<CppToken> ParseType(LinkedListStream<IBaseToken> stream)
        {
            List<CppToken> result = new List<CppToken>();
            bool allowStars = false;
            while (!stream.IsEOF)
            {
                CppToken token = stream.Peek<CppToken>();
                if (token == null) break;

                if (allowStars)
                {
                    if (token.Kind == CppTokenKind.MulOp || token.Kind == CppTokenKind.AndOp)
                    {
                        result.Add(token);
                        stream.Next();
                        continue;
                    }
                    else allowStars = false;
                }

                if (token.Kind == CppTokenKind.ReservedKeyword)
                {
                    if ("const".Equals(token.Value) || "volatile".Equals(token.Value) || "void".Equals(token.Value))
                    {
                        result.Add(token);
                        if ("void".Equals(token.Value))
                        {
                            stream.Next();
                            allowStars = true;
                            continue;
                        }
                    }
                    else break;
                }
                else if (token.Kind == CppTokenKind.Ellipsis)
                    result.Add(token);
                else if (token.Kind == CppTokenKind.UserTypeIdent || token.Kind == CppTokenKind.GlobalTypeKeyword || token.Kind == CppTokenKind.PreprocessorDefineUsage)
                {
                    result.Add(token);
                    stream.Next();
                    allowStars = true;
                    continue;
                }
                else
                    break;
                stream.Next();
            }
            return (result);
        }

        private ParseTokenResult ParseFunction(LinkedListStream<IBaseToken> stream)
        {
            LinkedListNode<IBaseToken> functionIdentNode = stream.CurrentNode;
            CppToken functionIdentToken = functionIdentNode.Value as CppToken;
            Debug.Assert(functionIdentToken.Kind == CppTokenKind.IdentLiteral);
            string functionName = functionIdentToken.Value;
            stream.Next();

            CppToken openParenToken = stream.Peek<CppToken>();
            Debug.Assert(openParenToken.Kind == CppTokenKind.LeftParen);
            stream.Next();

            // Function definitions
            // ----------------------------------------------------------------------------------------
            // void foo();
            // void _foo();
            // void* foo();
            // void *foo();
            // void * foo();
            // int foo();
            // const int foo();
            // static void foo();
            // static inline void foo();
            // static inline size_t foo();
            // static inline size_t bar(int a, char c, void *ptr);
            // static inline size_t bar(short *arr[] myArray, size_t count);
            // void variadic(int a, int b, ...);
            // my_custom_inline bar(int a, int b, ...);

            // Function calls
            // ----------------------------------------------------------------------------------------
            // foo()
            // _foo();
            // void *f = _foo();
            // int f = foo(4);
            // const int f = foo(4);
            // size_t f = bar(4, 'A', nullptr);
            // size_t f = bar((short **)myArray, 10);
            // float x += computeSomething(2, 4.0f);
            // float x += computeSomething(2, 4.0f) + otherComputation(42.3, 2.0);
            //
            // Function calls inside function arguments
            // ----------------------------------------------------------------------------------------
            // otherFunc(_foo());
            // moreFuncs(42, otherFunc(32, foo()), 'B')

            HashSet<CppTokenKind> allowedBefore = new HashSet<CppTokenKind>()
            {
                // C pointer operator
                CppTokenKind.MulOp,

                // C++ reference operator
                CppTokenKind.AndOp,

                // Template shit
                CppTokenKind.LessThanOp,
                CppTokenKind.GreaterThanOp,
            };

            HashSet<CppTokenKind> notAllowedBefore = new HashSet<CppTokenKind>()
            {
                CppTokenKind.SingleLineComment,
                CppTokenKind.SingleLineCommentDoc,
                CppTokenKind.PreprocessorEnd,
                CppTokenKind.EqOp,
                CppTokenKind.OrOp,
                CppTokenKind.XorOp,
                CppTokenKind.AddOp,
                CppTokenKind.SubOp,
                CppTokenKind.DivOp,
                CppTokenKind.ModOp,
                CppTokenKind.LeftParen,
                CppTokenKind.RightParen,
                CppTokenKind.LeftBrace,
                CppTokenKind.RightBrace,
                CppTokenKind.Ellipsis,
                CppTokenKind.ExclationMark,
                CppTokenKind.QuestionMark,
                CppTokenKind.Dot,
                CppTokenKind.Backslash,
                CppTokenKind.Tilde,
                CppTokenKind.Semicolon,
                CppTokenKind.Comma,
                CppTokenKind.Colon,
            };

            List<CppToken> beforeTokens = new List<CppToken>();
            LinkedListNode<IBaseToken> beforeNode = functionIdentNode.Previous;
            while (beforeNode != null)
            {
                CppToken tok = beforeNode.Value as CppToken;
                if (tok == null) break;
                if (!allowedBefore.Contains(tok.Kind))
                {
                    if (tok.Kind >= CppTokenKind.RightShiftAssign && tok.Kind <= CppTokenKind.LogicalNotEqualsOp)
                        break;
                    if (notAllowedBefore.Contains(tok.Kind))
                        break;
                }
                beforeTokens.Add(tok);
                beforeNode = beforeNode.Previous;
            }

            //
            // Skip any parameters
            //
            Stack<CppToken> parenStack = new Stack<CppToken>();
            parenStack.Push(openParenToken);
            while (!stream.IsEOF && parenStack.Count > 0)
            {
                CppToken argToken = stream.Peek<CppToken>();
                if (argToken == null)
                    break;
                if (argToken.Kind == CppTokenKind.LeftParen)
                {
                    parenStack.Push(argToken);
                    stream.Next();
                    continue;
                }
                else if (argToken.Kind == CppTokenKind.RightParen)
                {
                    parenStack.Pop();
                    stream.Next();
                    continue;
                }
                else if (argToken.Kind == CppTokenKind.LeftBrace || argToken.Kind == CppTokenKind.RightBrace)
                {
                    AddError(argToken.Position, $"Braces inside function arguments are not supported yet!", "Function", functionName);
                    return (ParseTokenResult.AlreadyAdvanced);
                }
                Debug.Assert(parenStack.Count > 0);
                stream.Next();
            }
            if (parenStack.Count > 0)
            {
                CppToken t = parenStack.Peek();
                AddError(t.Position, $"Unterminated function parenthesis for token '{t}'!", "Function", functionName);
                return (ParseTokenResult.AlreadyAdvanced);
            }

            functionIdentToken.Kind = CppTokenKind.FunctionIdent;

            CppEntityKind kind = CppEntityKind.FunctionCall;
            SearchResult<CppToken> endingTokenResult = Search(stream, SearchMode.Current, CppTokenKind.LeftBrace, CppTokenKind.Semicolon);
            if (endingTokenResult != null)
            {
                stream.Next();
                if (endingTokenResult.Token.Kind == CppTokenKind.LeftBrace)
                {
                    kind = CppEntityKind.FunctionBody;
                    if (Configuration.ExcludeFunctionBodies)
                        return (ParseTokenResult.AlreadyAdvanced);
                }
                else
                {
                    Debug.Assert(endingTokenResult.Token.Kind == CppTokenKind.Semicolon);
                    if (beforeTokens.Count > 0)
                        kind = CppEntityKind.FunctionDefinition;
                }
            }

            CppEntity functionEntity = new CppEntity(kind, functionIdentToken, functionName)
            {
                DocumentationNode = FindDocumentationNode(functionIdentNode, 1),
            };
            CppNode functionNode = new CppNode(Top, functionEntity);
            Add(functionNode);

            if (kind == CppEntityKind.FunctionCall && !Configuration.ExcludeFunctionCallSymbols)
                LocalSymbolTable.AddReference(new ReferenceSymbol(functionIdentToken.Lang, ReferenceSymbolKind.CppFunction, functionName, functionIdentToken.Range, functionNode));
            else if (kind == CppEntityKind.FunctionBody && !Configuration.ExcludeFunctionBodySymbols)
                LocalSymbolTable.AddSource(new SourceSymbol(functionIdentToken.Lang, SourceSymbolKind.CppFunctionBody, functionName, functionIdentToken.Range, functionNode));
            else if (kind == CppEntityKind.FunctionDefinition)
                LocalSymbolTable.AddSource(new SourceSymbol(functionIdentToken.Lang, SourceSymbolKind.CppFunctionDefinition, functionName, functionIdentToken.Range, functionNode));

            return (ParseTokenResult.AlreadyAdvanced);
        }

        private static readonly HashSet<CppTokenKind> _filteredTokenKinds = new HashSet<CppTokenKind>()
        {
            CppTokenKind.MultiLineComment,
            CppTokenKind.MultiLineCommentDoc,
            CppTokenKind.SingleLineComment,
            CppTokenKind.SingleLineCommentDoc,
        };
        public override IEnumerable<IBaseToken> FilterTokens(IEnumerable<IBaseToken> tokens)
        {
            List<IBaseToken> result = new List<IBaseToken>();
            result.AddRange(tokens.Where(t => !(t is CppToken && _filteredTokenKinds.Contains(((CppToken)t).Kind))));
            return (result);
        }

        protected override ParseTokenResult ParseToken(string source,LinkedListStream<IBaseToken> stream)
        {
            CppToken token = stream.Peek<CppToken>();
            if (token == null) return (ParseTokenResult.ReadNext);
            LinkedListNode<IBaseToken> node = stream.CurrentNode;
            switch (token.Kind)
            {
                case CppTokenKind.PreprocessorStart:
                    {
                        return ParsePreprocessor(stream);
                    }

                case CppTokenKind.ReservedKeyword:
                    return ParseReservedKeyword(stream);

                case CppTokenKind.IdentLiteral:
                    {
                        if (IsToken(stream, SearchMode.Next, CppTokenKind.LeftParen))
                            return ParseFunction(stream);
                    }
                    break;
            }
            return (ParseTokenResult.ReadNext);
        }

        private void SkipUntil(LinkedListStream<IBaseToken> tokenStream, params CppTokenKind[] kinds)
        {
            while (!tokenStream.IsEOF)
            {
                CppToken token = tokenStream.Peek<CppToken>();
                if (token == null)
                    break;
                if (kinds.Contains(token.Kind))
                    break;
                tokenStream.Next();
            }
        }

        public override void Finished(IEnumerable<IBaseToken> tokens)
        {
            CppSymbolResolver resolver = new CppSymbolResolver(LocalSymbolTable);
            resolver.ResolveTokens(tokens.Where(t => typeof(CppToken).Equals(t.GetType())).Select(t => t as CppToken));
        }
    }
}
