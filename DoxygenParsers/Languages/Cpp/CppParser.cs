using System;
using System.Collections.Generic;
using System.Diagnostics;
using TSP.DoxygenEditor.Collections;
using TSP.DoxygenEditor.Languages.Doxygen;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Parsers;

namespace TSP.DoxygenEditor.Languages.Cpp
{
    public class CppParser : BaseParser
    {
        public delegate BaseNode GetDocumentationNodeEventHandler(BaseToken token);
        public event GetDocumentationNodeEventHandler GetDocumentationNode;

        public CppParser()
        {
        }

        private BaseNode FindDocumentationNode(LinkedListNode<BaseToken> searchNode, int maxLineDelta)
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

            if (token.Kind == CppTokenKind.IdentLiteral)
            {
                string structIdent = token.Value;
                token = stream.CurrentNode?.Value as CppToken;
                CppEntity structEntity = new CppEntity(CppEntityType.Struct, token, structIdent)
                {
                    DocumentationNode = FindDocumentationNode(stream.CurrentNode, 1),
                };
                Add(new CppNode(Top, structEntity));
                stream.Next();
                if (token != null && (token.Kind == CppTokenKind.Semicolon || token.Kind == CppTokenKind.LeftBrace))
                {
                    stream.Next();
                }
            }

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
            if (token.Kind == CppTokenKind.ReservedKeyword)
            {
                stream.Next();
                string keyword = token.Value;
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
                    if (tok.Kind == CppTokenKind.Semicolon)
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
                    if (token != null && token.Kind == CppTokenKind.IdentLiteral)
                    {
                        string typedefIdent = token.Value;
                        CppEntity typedefEntity = new CppEntity(CppEntityType.Typedef, token, typedefIdent)
                        {
                            DocumentationNode = FindDocumentationNode(identNode, 1),
                        };
                        Add(new CppNode(Top, typedefEntity));
                    }
                }
            }
            return;
        }

        private bool ParseReservedKeyword(LinkedListStream<BaseToken> stream)
        {
            CppToken token = stream.CurrentNode.Value as CppToken;
            Debug.Assert(token.Kind == CppTokenKind.ReservedKeyword);
            string keyword = token.Value;
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
            Debug.Assert(token.Kind == CppTokenKind.LeftParen);

            token = stream.CurrentNode.Previous.Value as CppToken;
            Debug.Assert(token.Kind == CppTokenKind.IdentLiteral);
            var functionNameNode = stream.CurrentNode.Previous;
            CppToken functionNameToken = token;
            functionName = token.Value;

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

                    CppEntity functionEntity = new CppEntity(CppEntityType.Function, functionNameToken, functionName);
                    CppNode functionNode = new CppNode(Top, functionEntity);
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
            switch (token.Kind)
            {
                case CppTokenKind.ReservedKeyword:
                    return ParseReservedKeyword(stream);

                // @TODO(final): Switch to identifier mode
                case CppTokenKind.LeftParen:
                    {
                        var prevToken = node.Previous?.Value as CppToken;
                        if (prevToken != null && prevToken.Kind == CppTokenKind.IdentLiteral)
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
