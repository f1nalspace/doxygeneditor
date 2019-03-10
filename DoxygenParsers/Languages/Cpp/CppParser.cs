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
    public class CppParser : BaseParser<CppEntity>
    {
        public delegate IBaseNode GetDocumentationNodeEventHandler(BaseToken token);
        public event GetDocumentationNodeEventHandler GetDocumentationNode;

        public class CppConfiguration
        {
            public bool FunctionCallSymbolsEnabled { get; set; } = true;
            public bool SkipFunctionBlocks { get; set; } = false;
        }

        public CppConfiguration Configuration { get; } = new CppConfiguration();

        public CppParser(object tag) : base(tag)
        {
        }

        private IBaseNode FindDocumentationNode(LinkedListNode<BaseToken> searchNode, int maxLineDelta)
        {
            BaseToken searchToken = searchNode.Value;
            int start = searchToken.Index;
            int startLine = searchToken.Position.Line;
            int minEndLine = startLine - maxLineDelta;
            LinkedListNode<BaseToken> n = searchNode;
            while (n != null)
            {
                BaseToken baseToke = n.Value;
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

        private SearchResult<CppToken> Search(LinkedListStream<BaseToken> stream, SearchMode mode, params CppTokenKind[] kinds)
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
        private bool IsToken(LinkedListStream<BaseToken> stream, SearchMode mode, params CppTokenKind[] kinds)
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

        private void ParseEnumValues(LinkedListStream<BaseToken> stream, CppNode rootNode)
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
                    string enumValueName = enumValueToken.Value;
                    stream.Next();

                    CppEntity enumValueEntity = new CppEntity(CppEntityKind.EnumValue, enumValueToken, enumValueName)
                    {
                        DocumentationNode = FindDocumentationNode(identResult.Node, 1),
                    };
                    var enumValueNode = new CppNode(rootNode, enumValueEntity);
                    rootNode.AddChild(enumValueNode);
                    SymbolCache.AddSource(Tag, enumValueName, new SourceSymbol(enumValueNode, enumValueToken, SourceSymbolKind.CppMember));

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
                            AddParseError(equalsResult.Token.Position, $"Expect assignment token, but got token '{stream.Peek()}' for enum member '{enumValueName}'", "Enum", enumValueName);
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
                            AddParseError(tok.Position, $"Unexpected token '{tok.Kind}'", "EnumValue");
                        break;
                    }
                }
                stream.Next();
            }
        }

        private void ParseEnum(LinkedListStream<BaseToken> stream, TextPosition pos)
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
                        AddParseError(pos, $"Expect identifier token, but got token kind {token?.Kind} for enum", "enum");
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

            if (enumIdentResult != null)
            {
                var braceOrSemiResult = Search(stream, SearchMode.Current, CppTokenKind.LeftBrace, CppTokenKind.Semicolon);
                if (braceOrSemiResult == null)
                    return;
                var enumIdentToken = enumIdentResult.Token;
                string enumIdent = enumIdentToken.Value;
                CppEntityKind kind = braceOrSemiResult.Token.Kind == CppTokenKind.Semicolon ? CppEntityKind.ForwardEnum : CppEntityKind.Enum;
                CppEntity enumRootEntity = new CppEntity(kind, enumIdentToken, enumIdent)
                {
                    DocumentationNode = FindDocumentationNode(enumIdentResult.Node, 1),
                };
                CppNode enumRootNode = new CppNode(Top, enumRootEntity);
                Add(enumRootNode);
                SymbolCache.AddSource(Tag, enumIdent, new SourceSymbol(enumRootNode, enumIdentToken, SourceSymbolKind.CppEnum));

                if (braceOrSemiResult.Token.Kind == CppTokenKind.LeftBrace)
                    ParseEnumValues(stream, enumRootNode);
                else
                    stream.Next();
            }
            else
            {
                var token = stream.Peek<CppToken>();
                AddParseError(pos, $"Expect identifier token, but got token kind {token?.Kind} for enum", "enum");
                return;
            }
        }

        private void ParseStruct(LinkedListStream<BaseToken> stream)
        {
            CppToken structKeywordToken = stream.Peek<CppToken>();
            Debug.Assert(structKeywordToken.Kind == CppTokenKind.ReservedKeyword && ("struct".Equals(structKeywordToken.Value) || "union".Equals(structKeywordToken.Value)));

            var typedefResult = Search<CppToken>(stream.CurrentNode, SearchMode.Prev, (t) => t.Kind == CppTokenKind.ReservedKeyword && "typedef".Equals(t.Value));
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
                SymbolCache.AddSource(Tag, structIdent, new SourceSymbol(structNode, identToken, SourceSymbolKind.CppStruct));
            }

            // @TODO(final): Parse struct members
        }

        private void ParseTypedef(LinkedListStream<BaseToken> stream)
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
                    SymbolCache.AddSource(Tag, typedefIdent, new SourceSymbol(typedefNode, identToken, SourceSymbolKind.CppType));
                }
            }
        }

        private bool ParsePreprocessor(LinkedListStream<BaseToken> stream)
        {
            CppToken token = stream.Peek<CppToken>();
            Debug.Assert(token.Kind == CppTokenKind.Raute);
            stream.Next();

            var identResult = Search(stream, SearchMode.Current, CppTokenKind.IdentLiteral);
            if (identResult != null)
            {
                var identToken = identResult.Token;
                string ident = identToken.Value;
                stream.Next();

                if ("define".Equals(ident))
                {
                    var nameResult = Search(stream, SearchMode.Current, CppTokenKind.IdentLiteral);
                    if (nameResult != null)
                    {
                        var defineNameToken = nameResult.Token;
                        string defineName = defineNameToken.Value;
                        stream.Next();

                        var defineValueResult = Search(stream, SearchMode.Current, CppTokenKind.IdentLiteral);
                        if (defineValueResult != null)
                            stream.Next();

                        CppEntity defineKeyEntity = new CppEntity(CppEntityKind.Define, defineNameToken, defineName)
                        {
                            DocumentationNode = FindDocumentationNode(nameResult.Node, 1),
                            Value = defineValueResult?.Token.Value
                        };
                        CppNode defineNode = new CppNode(Top, defineKeyEntity);
                        Add(defineNode);
                        SymbolCache.AddSource(Tag, defineName, new SourceSymbol(defineNode, defineNameToken, SourceSymbolKind.CppDefine));
                    }
                    else
                        AddParseError(identToken.Position, $"Processor define has no identifier!", "Define");
                }
            }
            return (true);
        }

        private bool ParseReservedKeyword(LinkedListStream<BaseToken> stream)
        {
            CppToken keywordToken = stream.Peek<CppToken>();
            Debug.Assert(keywordToken.Kind == CppTokenKind.ReservedKeyword);
            string keyword = keywordToken.Value;
            switch (keyword)
            {
                case "typedef":
                    ParseTypedef(stream);
                    return (true);

                case "struct":
                case "union":
                case "class":
                    ParseStruct(stream);
                    return (true);

                case "enum":
                    ParseEnum(stream, keywordToken.Position);
                    return (true);

                default:
                    return (false);
            }
        }

        private List<CppToken> ParseType(LinkedListStream<BaseToken> stream)
        {
            List<CppToken> result = new List<CppToken>();
            return (result);
        }

        private bool ParseFunction(LinkedListStream<BaseToken> stream)
        {
            var functionIdentNode = stream.CurrentNode;
            CppToken functionIdentToken = functionIdentNode.Value as CppToken;
            Debug.Assert(functionIdentToken.Kind == CppTokenKind.IdentLiteral);
            var functionName = functionIdentToken.Value;

            CppTokenKind[] funcCallKinds =
            {
                CppTokenKind.EqOp,
                CppTokenKind.Comma,
                CppTokenKind.LeftParen,
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
            CppEntityKind kind = CppEntityKind.FunctionCall;

            // Skip ident
            stream.Next();

            // Skip parameters
            CppToken openParenToken = stream.Peek<CppToken>();
            Debug.Assert(openParenToken.Kind == CppTokenKind.LeftParen);
            stream.Next();
            var closeParenResult = Search(stream, SearchMode.Forward, CppTokenKind.RightParen);
            if (closeParenResult == null)
            {
                AddParseError(openParenToken.Position, $"Unterminated function '{functionName}'", "Function", functionName);
                return (true);
            }
            stream.Seek(closeParenResult.Node);
            stream.Next();

            if (funcCallResult == null)
            {
                var endingTokenResult = Search(stream, SearchMode.Current, CppTokenKind.Semicolon, CppTokenKind.LeftBrace);
                if (endingTokenResult != null)
                {
                    stream.Next();
                    if (endingTokenResult.Token.Kind == CppTokenKind.LeftBrace)
                    {
                        kind = CppEntityKind.FunctionBody;
                        if (Configuration.SkipFunctionBlocks)
                            return (true);
                    }
                    else
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
                SymbolCache.AddSource(Tag, functionName, new SourceSymbol(functionNode, functionIdentToken, SourceSymbolKind.CppFunctionDefinition));
            else if (kind == CppEntityKind.FunctionBody)
                SymbolCache.AddSource(Tag, functionName, new SourceSymbol(functionNode, functionIdentToken, SourceSymbolKind.CppFunctionBody));
            else if (Configuration.FunctionCallSymbolsEnabled)
                SymbolCache.AddReference(Tag, functionName, new ReferenceSymbol(functionNode, functionIdentToken, ReferenceSymbolKind.CppFunction));

            return (true);
        }

        public override bool ParseToken(LinkedListStream<BaseToken> stream)
        {
            var token = stream.Peek<CppToken>();
            if (token == null) return (false);
            switch (token.Kind)
            {
                case CppTokenKind.Raute:
                    return ParsePreprocessor(stream);

                case CppTokenKind.ReservedKeyword:
                    return ParseReservedKeyword(stream);

                case CppTokenKind.IdentLiteral:
                    {
                        if (IsToken(stream, SearchMode.Next, CppTokenKind.LeftParen))
                            return ParseFunction(stream);
                        return (false);
                    }
            }
            return (false);
        }
    }
}
