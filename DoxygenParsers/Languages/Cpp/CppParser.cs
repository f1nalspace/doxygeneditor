using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TSP.DoxygenEditor.Collections;
using TSP.DoxygenEditor.Languages.Doxygen;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Languages.Cpp
{
    public class CppParser : BaseParser<CppEntity>
    {
        public delegate IBaseNode GetDocumentationNodeEventHandler(BaseToken token);
        public event GetDocumentationNodeEventHandler GetDocumentationNode;

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
                n = n.Previous;
            }
            return (null);
        }

        enum SearchMode
        {
            Current,
            Next,
            Forward,
            Prev,
            Backward,
        }
        class SearchResult
        {
            public LinkedListNode<BaseToken> Node { get; }
            public CppToken Token { get; }
            public SearchResult(LinkedListNode<BaseToken> node, CppToken token)
            {
                Node = node;
                Token = token;
            }
        }
        private SearchResult Search(LinkedListNode<BaseToken> inNode, SearchMode mode, Func<CppToken, bool> matchFunc)
        {
            bool canTravel = (mode == SearchMode.Forward || mode == SearchMode.Backward);
            var n = inNode;
            do
            {
                if (n == null)
                    break;
                switch (mode)
                {
                    case SearchMode.Current:
                        break;
                    case SearchMode.Prev:
                    case SearchMode.Backward:
                        n = n.Previous;
                        break;
                    case SearchMode.Next:
                    case SearchMode.Forward:
                        n = n.Next;
                        break;
                }
                if (n != null)
                {
                    CppToken token = n.Value as CppToken;
                    if (token != null)
                    {
                        if (matchFunc(token))
                            return new SearchResult(n, token);
                    }
                }
            } while (n != null && canTravel);
            return (null);
        }
        private SearchResult Search(LinkedListStream<BaseToken> stream, SearchMode mode, params CppTokenKind[] kinds)
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
            SearchResult result = Search(stream.CurrentNode, mode, searchFunc);
            return (result);
        }
        private SearchResult Search(SearchResult inResult, SearchMode mode, params CppTokenKind[] kinds)
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
            SearchResult result = Search(inResult.Node, mode, searchFunc);
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
            SearchResult result = Search(stream.CurrentNode, mode, searchFunc);
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
                    SymbolCache.AddSource(Tag, enumValueName, new SourceSymbol(enumValueNode, enumValueToken, SymbolKind.CppMember));

                    // Skip until comma or right brace
                    var commaOrRightBraceResult = Search(stream, SearchMode.Forward, CppTokenKind.Comma, CppTokenKind.RightBrace);
                    if (commaOrRightBraceResult != null)
                    {
                        stream.Seek(commaOrRightBraceResult.Node);
                        if (commaOrRightBraceResult.Token.Kind == CppTokenKind.Comma)
                            stream.Next();
                        continue;
                    }
                    else
                    {
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

            SearchResult enumIdentResult = null;

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
                        AddParseError(pos, $"Expect identifier token, but got token kind {token?.Kind} for enum");
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
                SymbolCache.AddSource(Tag, enumIdent, new SourceSymbol(enumRootNode, enumIdentToken, SymbolKind.CppType));

                if (braceOrSemiResult.Token.Kind == CppTokenKind.LeftBrace)
                    ParseEnumValues(stream, enumRootNode);
                else
                    stream.Next();
            }
            else
            {
                var token = stream.Peek<CppToken>();
                AddParseError(pos, $"Expect identifier token, but got token kind {token?.Kind} for enum");
                return;
            }

#if false
            if (token.Kind == CppTokenKind.IdentLiteral)
            {
                string enumIdent = token.Value;
                token = stream.CurrentNode?.Value as CppToken;
                CppEntity enumRootEntity = new CppEntity(CppEntityKind.Enum, token, enumIdent)
                {
                    DocumentationNode = FindDocumentationNode(stream.CurrentNode, 1),
                };
                var enumRootNode = new CppNode(Top, enumRootEntity);
                Add(enumRootNode);
                SymbolCache.AddSource(Tag, enumIdent, new SourceSymbol(enumRootNode, token));
                stream.Next();
                token = stream.Peek<CppToken>();
                if (token == null)
                {
                    // ERROR: No more cpp tokens
                    return;
                }
                if (token.Kind == CppTokenKind.Semicolon)
                {
                    stream.Next();
                }
                else if (token.Kind == CppTokenKind.LeftBrace)
                {
                    stream.Next();
                    while (!stream.IsEOF)
                    {
                        var n = stream.CurrentNode;
                        token = n.Value as CppToken;
                        if (token == null)
                        {
                            // Skip non-cpp tokens
                            stream.Next();
                            continue;
                        }
                        if (token.Kind == CppTokenKind.RightBrace)
                        {
                            // Enum complete
                            stream.Next();
                            break;
                        }
                        else if (token.Kind == CppTokenKind.IdentLiteral)
                        {
                            // Next enum name
                            var enumValueToken = token;
                            string enumValueName = enumValueToken.Value;
                            stream.Next();

                            CppEntity enumValueEntity = new CppEntity(CppEntityKind.EnumValue, enumValueToken, enumValueName)
                            {
                                DocumentationNode = FindDocumentationNode(n, 1),
                            };
                            var enumValueNode = new CppNode(enumRootNode, enumValueEntity);
                            enumRootNode.AddChild(enumValueNode);
                            SymbolCache.AddSource(Tag, enumValueName, new SourceSymbol(enumValueNode, enumValueToken));

                            // Skip until comma or right brace
                            while (!stream.IsEOF)
                            {
                                n = stream.CurrentNode;
                                token = n.Value as CppToken;
                                if (token == null)
                                {
                                    // Skip non-cpp tokens
                                    stream.Next();
                                    continue;
                                }

                                if (token.Kind == CppTokenKind.Comma)
                                {
                                    stream.Next();
                                    break;
                                }
                                else if (token.Kind == CppTokenKind.RightBrace)
                                {
                                    break;
                                }
                                stream.Next();
                            }
                            continue;
                        }
                        stream.Next();
                    }
                }
            }
#endif
        }

        private void ParseStruct(LinkedListStream<BaseToken> stream)
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

            // B
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
                SymbolCache.AddSource(Tag, structIdent, new SourceSymbol(structNode, identToken, SymbolKind.CppType));
            }
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
                        break;

                    case "enum":
                        ParseEnum(stream, reservedKeywordToken.Position);
                        break;
                }
            }
            else
            {
                // Normal typedef
                CppEntityKind kind = CppEntityKind.Typedef;
                var semicolonResult = Search(stream, SearchMode.Forward, CppTokenKind.Semicolon);
                if (semicolonResult != null)
                {
                    stream.Seek(semicolonResult.Node);
                    stream.Next();

                    SearchResult identResult = null;
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
                                    Debug.WriteLine("Right paren not found!");
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
                        SymbolCache.AddSource(Tag, typedefIdent, new SourceSymbol(typedefNode, identToken, SymbolKind.CppType));
                    }
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
                            Value = defineValueResult?.Token.Value
                        };
                        CppNode defineNode = new CppNode(Top, defineKeyEntity);
                        Add(defineNode);
                        SymbolCache.AddSource(Tag, defineName, new SourceSymbol(defineNode, defineNameToken, SymbolKind.CppDefine));
                    }
                    else
                        AddParseError(identToken.Position, $"Processor define has no identifier!");
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
            return (false);
#if false
            string functionName = null;
            CppToken token = stream.CurrentNode.Value as CppToken;
            Debug.Assert(token.Kind == CppTokenKind.LeftParen);

            token = stream.CurrentNode.Previous.Value as CppToken;
            Debug.Assert(token.Kind == CppTokenKind.IdentLiteral);
            var functionNameNode = stream.CurrentNode.Previous;
            CppToken functionNameToken = token;
            functionName = token.Value;

            // Skip function defines
            if (stream.CurrentNode.Previous.Previous != null)
            {
                if ("define".Equals(stream.CurrentNode.Previous.Previous.Value.Value))
                {
                    if (stream.CurrentNode.Previous.Previous.Previous != null)
                    {
                        if (stream.CurrentNode.Previous.Previous.Previous.Value.Value == "#")
                        {
                            return (false);
                        }
                    }
                }
            }

            // Read function prefix stuff (storage identifiers, return value)
            LinkedListNode<BaseToken> n = stream.CurrentNode.Previous.Previous;
            List<CppToken> funcTokens = new List<CppToken>();
            while (n != null)
            {
                CppToken ntok = n.Value as CppToken;
                if (ntok != null && ((ntok.Kind == CppTokenKind.IdentLiteral || ntok.Kind == CppTokenKind.ReservedKeyword || ntok.Kind == CppTokenKind.TypeKeyword) || (ntok.Kind == CppTokenKind.MulOp || ntok.Kind == CppTokenKind.LessThanOp || ntok.Kind == CppTokenKind.GreaterThanOp || ntok.Kind == CppTokenKind.LeftBracket || ntok.Kind == CppTokenKind.RightBracket)))
                    funcTokens.Add(ntok);
                else
                    break;
                n = n.Previous;
            }
            funcTokens.Reverse();

            if (funcTokens.Count > 0)
            {
                // Arguments
                List<CppToken> argumentTokens = new List<CppToken>();
                stream.Next();
                int expectedRightParenCount = 1;
                bool validFunction = false;
                while (!stream.IsEOF)
                {
                    token = stream.Peek<CppToken>();
                    if (token == null) return (true);
                    if (token.Kind == CppTokenKind.LeftParen)
                    {
                        if (expectedRightParenCount > 0)
                        {
                            ++expectedRightParenCount;
                        }
                        else
                        {
                            stream.Next();
                            break;
                        }
                    }
                    else if (token.Kind == CppTokenKind.RightParen)
                    {
                        --expectedRightParenCount;
                        if (expectedRightParenCount == 0)
                        {
                            validFunction = true;
                            stream.Next();
                            break;
                        }
                    }
                    argumentTokens.Add(token);
                    stream.Next();
                }

                if (validFunction)
                {
                    token = stream.Peek<CppToken>();
                    if (token == null || token.Kind != CppTokenKind.Semicolon)
                    {
                        return (true);
                    }
                    Debug.Assert(token.Kind == CppTokenKind.Semicolon);
                    stream.Next();

                    CppEntity functionEntity = new CppEntity(CppEntityKind.Function, functionNameToken, functionName);
                    CppNode functionNode = new CppNode(Top, functionEntity);
                    Add(functionNode);
                    SymbolCache.AddSource(Tag, functionName, new SourceSymbol(functionNode, functionNameToken));

                    // @NOTE(final): Comment block are expected on the previous line always
                    functionEntity.DocumentationNode = FindDocumentationNode(functionNameNode, 1);
                }
                return (true);
            }
            else
            {
                return (false);
            }
#endif
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
