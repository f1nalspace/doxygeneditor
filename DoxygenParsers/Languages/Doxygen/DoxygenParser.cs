using System;
using System.Collections.Generic;
using System.Diagnostics;
using TSP.DoxygenEditor.Collections;
using TSP.DoxygenEditor.Languages.Utils;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.Symbols;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Languages.Doxygen
{
    public class DoxygenParser : BaseParser<DoxygenEntity, DoxygenToken>
    {
        public static HashSet<DoxygenEntityKind> ShowChildrensSet = new HashSet<DoxygenEntityKind>()
        {
            DoxygenEntityKind.Page,
            DoxygenEntityKind.Section,
            DoxygenEntityKind.SubSection,
            DoxygenEntityKind.SubSubSection,
        };

        public string Source { get; }

        public DoxygenParser(object tag, string source) : base(tag)
        {
            Source = source;
        }

        private DoxygenNode PushEntity(DoxygenEntity newEntity)
        {
            if (newEntity.Kind == DoxygenEntityKind.BlockSingle || newEntity.Kind == DoxygenEntityKind.BlockMulti)
            {
                if (Top != null)
                {
                    Debug.Assert(Top.Entity.Kind == DoxygenEntityKind.Group);
                    Pop();
                }
                DoxygenNode blockNode = new DoxygenNode(Top, newEntity);
                Push(blockNode);
                return (blockNode);
            }

            DoxygenNode itemNode = new DoxygenNode(Top, newEntity);
            Push(itemNode);
            return (itemNode);
        }

        private void ParseText(LinkedListStream<IBaseToken> stream, IBaseNode contentNode)
        {
            var nextToken = stream.Peek<DoxygenToken>();
            Debug.Assert(nextToken.Kind == DoxygenTokenKind.TextStart);
            TextPosition textStart = nextToken.Position;
            TextPosition textEnd = textStart;
            stream.Next();
            while (!stream.IsEOF)
            {
                var t = stream.Peek<DoxygenToken>();
                if (t == null)
                    break;
                if (t.Kind == DoxygenTokenKind.TextEnd)
                {
                    textEnd = t.Position;
                    stream.Next();
                    break;
                }
                stream.Next();
            }
            if (contentNode != null)
            {
                int textLen = textEnd.Index - textStart.Index;
                string text = Source.Substring(textStart.Index, textLen).Trim();
                if (text.Length > 0)
                {
                    var textNode = new DoxygenNode(contentNode, new DoxygenEntity(DoxygenEntityKind.Text, new TextRange(textStart, textLen)));
                    textNode.Entity.Value = text;
                    textNode.Entity.EndRange = new TextRange(textEnd, 0);
                    contentNode.AddChild(textNode);
                }
            }
        }

        private bool ParseCommand(LinkedListStream<IBaseToken> stream, IBaseNode contentRoot)
        {
            // @NOTE(final): This must always return true, due to the fact that the stream is advanced at least once
            DoxygenToken commandToken = stream.Peek<DoxygenToken>();
            Debug.Assert(commandToken != null && commandToken.Kind == DoxygenTokenKind.Command);

            string commandName = commandToken.Value.Substring(1);
            stream.Next();

            string typeName = "Command";

            var rule = DoxygenSyntax.GetCommandRule(commandName);
            if (rule != null)
            {
                if (rule.Kind == DoxygenSyntax.CommandKind.EndCommandBlock)
                {
                    var t = Top;
                    if (t == null)
                    {
                        AddError(commandToken.Position, $"Unterminated starting command block in command '{commandName}'", typeName, commandName);
                        return (false);
                    }
                    if (t.Entity.Kind != DoxygenEntityKind.BlockCommand)
                    {
                        AddError(commandToken.Position, $"Expect starting command block, but found '{t.Entity.Kind}' in command '{commandName}'", typeName, commandName);
                        return (false);
                    }
                    Pop();
                }

                // Paragraph or section command starts or command block starts -> Close previous paragraph or sectioning command
                if (rule.Kind == DoxygenSyntax.CommandKind.Paragraph ||
                    rule.Kind == DoxygenSyntax.CommandKind.Section ||
                    rule.Kind == DoxygenSyntax.CommandKind.StartCommandBlock)
                {
                    var t = Top;
                    if (t != null)
                    {
                        if (t.Entity.Kind == DoxygenEntityKind.Paragraph ||
                            t.Entity.Kind == DoxygenEntityKind.Section ||
                            t.Entity.Kind == DoxygenEntityKind.SubSection ||
                            t.Entity.Kind == DoxygenEntityKind.SubSubSection)
                        {
                            Pop();
                        }
                    }
                }

                DoxygenEntity commandEntity = null;
                IEntityBaseNode<DoxygenEntity> commandNode = null;
                if (rule.EntityKind != DoxygenEntityKind.None)
                {
                    commandEntity = new DoxygenEntity(rule.EntityKind, commandToken.Range);
                    commandEntity.Id = commandName;
                    commandNode = new DoxygenNode(Top, commandEntity);
                    if (rule.IsPush)
                        Push(commandNode);
                    else
                        Add(commandNode);
                }

                foreach (var arg in rule.Args)
                {
                    DoxygenToken argToken = stream.Peek<DoxygenToken>();
                    if (argToken == null)
                        break;
                    DoxygenTokenKind expectedTokenKind = DoxygenSyntax.ArgumentToTokenKindMap.ContainsKey(arg.Kind) ? DoxygenSyntax.ArgumentToTokenKindMap[arg.Kind] : DoxygenTokenKind.Invalid;
                    if (expectedTokenKind == DoxygenTokenKind.Invalid)
                        break;
                    if (expectedTokenKind != argToken.Kind)
                    {
                        AddError(argToken.Position, $"Expect argument token '{expectedTokenKind}', but got '{argToken.Kind}'", typeName, commandName);
                        break;
                    }
                    if (commandNode != null)
                    {
                        string paramName = arg.Name;
                        string paramValue = argToken.Value;
                        commandNode.Entity.AddParameter(argToken, paramName, paramValue);
                    }
                    stream.Next();
                }

                if (commandEntity != null)
                {
                    // Get name and text parameter (Some commands, have different names and text parameters, so there is a variable list of strings)
                    var nameParam = commandEntity.FindParameterByName("name", "id");
                    var textParam = commandEntity.FindParameterByName("text", "title", "caption");
                    if (nameParam == null || string.IsNullOrWhiteSpace(nameParam.Value))
                    {
                        if (rule.Kind == DoxygenSyntax.CommandKind.Section)
                        {
                            if (!"mainpage".Equals(commandName))
                                AddError(commandToken.Position, $"Missing identifier mapping for command '{commandName}'", typeName, commandName);
                        }
                    }

                    if (nameParam != null && !string.IsNullOrWhiteSpace(nameParam.Value))
                    {
                        string symbolName = nameParam.Value;
                        Debug.Assert(commandNode != null);
                        if (rule.Kind == DoxygenSyntax.CommandKind.Section)
                        {
                            SourceSymbolKind kind = SourceSymbolKind.DoxygenSection;
                            if ("page".Equals(commandName) || "mainpage".Equals(commandName))
                                kind = SourceSymbolKind.DoxygenPage;
                            SymbolTable.AddSource(new SourceSymbol(kind, symbolName, nameParam.Token.Range, commandNode));
                        }
                        else if ("ref".Equals(commandName) || "refitem".Equals(commandName))
                        {
                            string referenceValue = nameParam.Value;
                            TextPosition startPos = new TextPosition(0, nameParam.Token.Position.Line, nameParam.Token.Position.Column);
                            using (TextStream referenceTextStream = new BasicTextStream(referenceValue, startPos, referenceValue.Length))
                            {
                                ReferenceSymbolKind referenceTarget = ReferenceSymbolKind.Any;
                                while (!referenceTextStream.IsEOF)
                                {
                                    char first = referenceTextStream.Peek();
                                    char second = referenceTextStream.Peek(1);
                                    char third = referenceTextStream.Peek(2);
                                    if (SyntaxUtils.IsIdentStart(first))
                                    {
                                        referenceTextStream.StartLexeme();
                                        while (!referenceTextStream.IsEOF)
                                        {
                                            if (!SyntaxUtils.IsIdentPart(referenceTextStream.Peek()))
                                                break;
                                            referenceTextStream.AdvanceColumn();
                                        }
                                        var refRange = referenceTextStream.LexemeRange;
                                        string singleRereference = referenceTextStream.GetSourceText(refRange.Index, refRange.Length);
                                        if (referenceTextStream.Peek() == '(')
                                        {
                                            referenceTarget = ReferenceSymbolKind.CppFunction;
                                            referenceTextStream.AdvanceColumn();
                                            while (!referenceTextStream.IsEOF)
                                            {
                                                if (referenceTextStream.Peek() == ')')
                                                    break;
                                                referenceTextStream.AdvanceColumn();
                                            }
                                        }
                                        var symbolRange = new TextRange(new TextPosition(nameParam.Token.Position.Index + refRange.Position.Index, refRange.Position.Line, refRange.Position.Column), refRange.Length);
                                        SymbolTable.AddReference(new ReferenceSymbol(referenceTarget, singleRereference, symbolRange, commandNode));
                                    }
                                    else if (first == '#' || first == '.')
                                    {
                                        referenceTarget = ReferenceSymbolKind.CppMember;
                                        referenceTextStream.AdvanceColumn();
                                    }
                                    else if (first == ':' || second == ':')
                                    {
                                        referenceTarget = ReferenceSymbolKind.CppMember;
                                        referenceTextStream.AdvanceColumns(2);
                                    }
                                    else break;
                                }
                            }
                        }
                        else if ("subpage".Equals(commandName))
                            SymbolTable.AddReference(new ReferenceSymbol(ReferenceSymbolKind.DoxygenPage, symbolName, nameParam.Token.Range, commandNode));
                    }
                }
                ParseBlockContent(stream, commandNode);
            }
            else
            {
                AddError(commandToken.Position, $"No parse rule for command '{commandName}' found", "Command", commandName);
            }
            return (true);
        }

        private bool ParseSingleBlock(LinkedListStream<IBaseToken> stream)
        {
            // @NOTE(final) Single block = auto-brief

            IBaseToken blockToken = stream.Peek();
            DoxygenEntity blockEntity = new DoxygenEntity(DoxygenEntityKind.BlockSingle, blockToken);
            PushEntity(blockEntity);
            stream.Next();

            IBaseToken endToken = null;
            IBaseToken briefToken = DoxygenTokenPool.Make(DoxygenTokenKind.Command, blockToken.Range, true);

            DoxygenEntity briefEntity = new DoxygenEntity(DoxygenEntityKind.Brief, briefToken);
            DoxygenNode briefNode = PushEntity(briefEntity);

            while (!stream.IsEOF)
            {
                IBaseToken token = stream.Peek();
                DoxygenToken doxyToken = token as DoxygenToken;
                if (doxyToken != null && doxyToken.Kind == DoxygenTokenKind.DoxyBlockEnd)
                {
                    endToken = token;
                    stream.Next();
                    break;
                }
                if (!ParseBlockContent(stream, briefNode))
                    break;
                Debug.Assert(stream.CurrentValue != token);
            }

            Pop(); // Pop brief

            Pop(); // Pop block

            if (endToken != null)
                blockEntity.EndRange = endToken.Range;

            return (true);
        }

        private void CloseParagraphOrSection(IBaseNode contentRoot)
        {
            if (Top != null)
            {
                // @TODO(final): Proper assert here!
                Pop();
            }
        }
        private void CloseEverythingUntil(DoxygenEntityKind kind)
        {
            while (Top != null)
            {
                if (Top.Entity.Kind == kind)
                    break;
                else
                    Pop();
            }
        }

        private bool ParseBlockContent(LinkedListStream<IBaseToken> stream, IBaseNode contentRoot)
        {
            IBaseToken token = stream.Peek();
            if (typeof(DoxygenToken).Equals(token.GetType()))
            {
                DoxygenToken doxyToken = (DoxygenToken)token;
                switch (doxyToken.Kind)
                {
                    case DoxygenTokenKind.EmptyLine:
                        CloseParagraphOrSection(contentRoot);
                        return (true);

                    case DoxygenTokenKind.Command:
                        return ParseCommand(stream, contentRoot);

                    case DoxygenTokenKind.InvalidCommand:
                        string commandName = doxyToken.Value.Substring(1);
                        AddError(doxyToken.Position, $"Unknown doxygen command '{commandName}'", "Command", commandName);
                        stream.Next();
                        return (true);

                    case DoxygenTokenKind.TextStart:
                        ParseText(stream, contentRoot);
                        return (true);

                    default:
                        stream.Next();
                        return (true);
                }
            }
            else
                return (false);
        }

        public override bool ParseToken(LinkedListStream<IBaseToken> stream)
        {
            IBaseToken token = stream.Peek();
            if (typeof(DoxygenToken).Equals(token.GetType()))
            {
                DoxygenToken doxyToken = (DoxygenToken)token;
                switch (doxyToken.Kind)
                {
                    case DoxygenTokenKind.DoxyBlockStartSingle:
                        return ParseSingleBlock(stream);

                    case DoxygenTokenKind.DoxyBlockStartMulti:
                        {
                            DoxygenEntity blockEntity = new DoxygenEntity(DoxygenEntityKind.BlockMulti, doxyToken);
                            PushEntity(blockEntity);
                            stream.Next();
                            return (true);
                        }

                    case DoxygenTokenKind.DoxyBlockEnd:
                        {
                            CloseEverythingUntil(DoxygenEntityKind.BlockMulti);
                            Debug.Assert(Top != null);
                            DoxygenEntity rootEntity = (DoxygenEntity)Top.Entity;
                            Debug.Assert(rootEntity.Kind == DoxygenEntityKind.BlockMulti);
                            Pop();
                            rootEntity.EndRange = doxyToken.Range;
                            stream.Next();
                            return (true);
                        }

                    default:
                        {
                            ParseBlockContent(stream, Top);
                            return (true);
                        }
                }
            }
            else
                return (false);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
