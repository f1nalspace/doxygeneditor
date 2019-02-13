using System;
using System.Collections.Generic;
using System.Diagnostics;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Lexers.Cpp;
using TSP.DoxygenEditor.Lexers.Doxygen;
using TSP.DoxygenEditor.Lists;
using TSP.DoxygenEditor.Parsers.Doxygen;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Parsers.Cpp
{
    class CppTree : BaseTree
    {
        public delegate BaseNode GetDocumentationNodeEventHandler(TextRange range);
        public event GetDocumentationNodeEventHandler GetDocumentationNode;

        public CppTree()
        {
        }

        private BaseNode FindDocumentationNode(LinkedListNode<BaseToken> searchNode, int maxLineDelta)
        {
            BaseToken searchToken = searchNode.Value;
            int start = searchToken.Index;
            int startLine = GetLine(start);
            if (startLine == -1) return (null);
            LinkedListNode<BaseToken> n = searchNode;
            while (n != null)
            {
                DoxygenToken doxyToken = n.Value as DoxygenToken;
                if (doxyToken != null && doxyToken.Type == DoxygenTokenType.BlockEnd)
                {
                    int end = n.Value.End;
                    int endLine = GetLine(end);
                    if (endLine == -1) break;
                    int lineDelta = startLine - endLine;
                    if (lineDelta > 0 && lineDelta <= maxLineDelta)
                    {
                        int endIndex = n.Value.Index + 1;
                        var foundNode = GetDocumentationNode?.Invoke(doxyToken);
                        return (foundNode);
                    }
                    break;
                }
                n = n.Previous;
            }
            return (null);
        }

        private void ParseStruct(LinkedListStream<BaseToken> stream)
        {
            CppToken token = stream.CurrentNode?.Value as CppToken;
            if (token == null)
            {
                // @TODO(final): "struct" / "union" without any cpp token after
                return;
            }

            // There are multiple ways to define a struct in C
            // - typedef struct { int a; } foo;
            // - typedef struct foo { int a; } sFoo; (prefered style)
            // - struct { int a; }; (Anonymous struct as a member)
            // - struct foo { int a; };

            return;
        }

        private void ParseTypedef(LinkedListStream<BaseToken> stream)
        {
            CppToken token = stream.CurrentNode?.Value as CppToken;
            if (token == null)
            {
                // @TODO(final): "typedef" without any cpp token after
                return;
            }
            if (token.Type == CppTokenType.ReservedKeyword)
            {
                stream.Next();
                string keyword = GetText(token);
                switch (keyword)
                {
                    case "union":
                    case "struct":
                        ParseStruct(stream);
                        break;
                }
            }
            else
            {
                // Normal typedef
                var n = stream.CurrentNode;
                LinkedListNode<BaseToken> semicolonNode = null;
                while (n != null)
                {
                    CppToken tok = stream.CurrentNode.Value as CppToken;
                    if (tok == null) break;
                    if (tok.Type == CppTokenType.Semicolon)
                    {
                        semicolonNode = stream.CurrentNode;
                        stream.Next();
                        break;
                    }
                    n = n.Next;
                    stream.Next();
                }
                if (semicolonNode != null && semicolonNode.Previous != null)
                {
                    var identNode = semicolonNode.Previous;
                    token = identNode.Value as CppToken;
                    if (token != null && token.Type == CppTokenType.Identifier)
                    {
                        string typedefIdent = GetText(token);
                        CppEntity typedefEntity = new CppEntity(CppEntityType.Typedef, token, typedefIdent)
                        {
                            DocumentationNode = FindDocumentationNode(identNode, 1),
                        };
                        Add(new CppNode(Root, typedefEntity));
                    }
                }
            }
            return;
        }

        private bool ParseReservedKeyword(LinkedListStream<BaseToken> stream)
        {
            CppToken token = stream.CurrentNode.Value as CppToken;
            Debug.Assert(token.Type == CppTokenType.ReservedKeyword);
            string keyword = GetText(token);
            stream.Next();
            switch (keyword)
            {
                case "typedef":
                    ParseTypedef(stream);
                    break;

                case "struct":
                case "union":
                case "class":
                    ParseStruct(stream);
                    break;
            }
            return (true);
        }

        private bool ParseFunction(LinkedListStream<BaseToken> stream)
        {
            string functionName = null;
            CppToken token = stream.CurrentNode.Value as CppToken;
            Debug.Assert(token.Type == CppTokenType.LeftParen);

            token = stream.CurrentNode.Previous.Value as CppToken;
            Debug.Assert(token.Type == CppTokenType.Identifier);
            var functionNameNode = stream.CurrentNode.Previous;
            CppToken functionNameToken = token;
            functionName = GetText(functionNameToken);

            // Read function prefix stuff (storage identifiers, return value)
            LinkedListNode<BaseToken> n = stream.CurrentNode.Previous.Previous;
            List<CppToken> funcTokens = new List<CppToken>();
            while (n != null)
            {
                CppToken ntok = n.Value as CppToken;
                if (ntok != null && ((ntok.Type == CppTokenType.Identifier || ntok.Type == CppTokenType.ReservedKeyword || ntok.Type == CppTokenType.TypeKeyword) || (ntok.Type == CppTokenType.OpMul || ntok.Type == CppTokenType.OpLess || ntok.Type == CppTokenType.OpGreater || ntok.Type == CppTokenType.LeftBracket || ntok.Type == CppTokenType.RightBracket)))
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
                    if (token.Type == CppTokenType.LeftParen)
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
                    else if (token.Type == CppTokenType.RightParen)
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
                    if (token == null || token.Type != CppTokenType.Semicolon)
                    {
                        return (true);
                    }
                    Debug.Assert(token.Type == CppTokenType.Semicolon);
                    stream.Next();

                    CppEntity functionEntity = new CppEntity(CppEntityType.Function, functionNameToken, functionName);
                    CppNode functionNode = new CppNode(Root, functionEntity);
                    Add(functionNode);

                    // @NOTE(final): Comment block are expected on the previous line always
                    functionEntity.DocumentationNode = FindDocumentationNode(functionNameNode, 1);
                }
                return (true);
            }
            else
            {
                return (false);
            }
        }

        public override bool ParseToken(LinkedListStream<BaseToken> stream)
        {
            var node = stream.CurrentNode;
            var token = node.Value as CppToken;
            if (token == null) return (false);
            switch (token.Type)
            {
                case CppTokenType.ReservedKeyword:
                    return ParseReservedKeyword(stream);

                // @TODO(final): Switch to identifier mode
                case CppTokenType.LeftParen:
                    {
                        var prevToken = node.Previous?.Value as CppToken;
                        if (prevToken != null && prevToken.Type == CppTokenType.Identifier)
                        {
                            return ParseFunction(stream);
                        }
                        return (false);
                    }
            }
            return (false);
        }
    }
}
