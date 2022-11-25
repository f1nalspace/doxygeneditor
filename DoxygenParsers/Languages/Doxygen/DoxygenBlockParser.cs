using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TSP.DoxygenEditor.Collections;
using TSP.DoxygenEditor.Languages.Utils;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.Symbols;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Languages.Doxygen
{
    public class DoxygenBlockParser : BaseParser<DoxygenBlockEntity, DoxygenToken>
    {
        public static HashSet<DoxygenBlockEntityKind> ShowChildrensSet = new HashSet<DoxygenBlockEntityKind>()
        {
            DoxygenBlockEntityKind.Page,
            DoxygenBlockEntityKind.Section,
            DoxygenBlockEntityKind.SubSection,
            DoxygenBlockEntityKind.SubSubSection,
        };

        public string Source { get; }

        public DoxygenBlockParser(ISymbolTableId id, string source) : base(id)
        {
            Source = source;
        }

        private DoxygenBlockNode PushEntity(DoxygenBlockEntity newEntity)
        {
            if (newEntity.Kind == DoxygenBlockEntityKind.BlockSingle || newEntity.Kind == DoxygenBlockEntityKind.BlockMulti)
            {
                if (Top != null && Top.Entity.Kind == DoxygenBlockEntityKind.Group)
                    Pop();
                DoxygenBlockNode blockNode = new DoxygenBlockNode(Top, newEntity);
                Push(blockNode);
                return (blockNode);
            }

            DoxygenBlockNode itemNode = new DoxygenBlockNode(Top, newEntity);
            Push(itemNode);
            return (itemNode);
        }

        private void ParseText(LinkedListStream<IBaseToken> stream, IBaseNode contentNode)
        {
            DoxygenToken nextToken = stream.Peek<DoxygenToken>();
            Debug.Assert(nextToken.Kind == DoxygenTokenKind.TextStart);
            TextPosition textStart = nextToken.Position;
            TextPosition textEnd = textStart;
            stream.Next();
            while (!stream.IsEOF)
            {
                DoxygenToken t = stream.Peek<DoxygenToken>();
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
                    DoxygenBlockNode textNode = new DoxygenBlockNode(contentNode, new DoxygenBlockEntity(DoxygenBlockEntityKind.Text, new TextRange(textStart, textLen)));
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

            DoxygenSyntax.CommandRule rule = DoxygenSyntax.GetCommandRule(commandName);
            if (rule != null)
            {
                if (rule.Kind == DoxygenSyntax.CommandKind.EndCommandBlock)
                {
                    IEntityBaseNode<DoxygenBlockEntity> t = Top;
                    if (t == null)
                    {
                        AddError(commandToken.Position, $"Unterminated starting command block in command '{commandName}'", typeName, commandName);
                        return (false);
                    }
                    if (t.Entity.Kind != DoxygenBlockEntityKind.BlockCommand)
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
                    IEntityBaseNode<DoxygenBlockEntity> t = Top;
                    if (t != null)
                    {
                        if (t.Entity.Kind == DoxygenBlockEntityKind.Paragraph ||
                            t.Entity.Kind == DoxygenBlockEntityKind.Section ||
                            t.Entity.Kind == DoxygenBlockEntityKind.SubSection ||
                            t.Entity.Kind == DoxygenBlockEntityKind.SubSubSection)
                        {
                            Pop();
                        }
                    }
                }

                DoxygenBlockEntity commandEntity = null;
                IEntityBaseNode<DoxygenBlockEntity> commandNode = null;
                if (rule.EntityKind != DoxygenBlockEntityKind.None)
                {
                    commandEntity = new DoxygenBlockEntity(rule.EntityKind, commandToken.Range);
                    commandEntity.Id = commandName;
                    commandNode = new DoxygenBlockNode(Top, commandEntity);
                    if (rule.IsPush)
                        Push(commandNode);
                    else
                        Add(commandNode);
                }

                foreach (DoxygenSyntax.ArgumentRule arg in rule.Args)
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
                    DoxygenBlockEntity.Parameter nameParam = commandEntity.FindParameterByName("name", "id");
                    DoxygenBlockEntity.Parameter textParam = commandEntity.FindParameterByName("text", "title", "caption");
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
                        string symbolDisplayName = textParam?.Value;
                        Debug.Assert(commandNode != null);
                        if (rule.Kind == DoxygenSyntax.CommandKind.Section)
                        {
                            SourceSymbolKind kind = SourceSymbolKind.DoxygenSection;
                            if ("page".Equals(commandName) || "mainpage".Equals(commandName))
                                kind = SourceSymbolKind.DoxygenPage;
                            LocalSymbolTable.AddSource(new SourceSymbol(nameParam.Token.Lang, kind, symbolName, symbolDisplayName, nameParam.Token.Range, commandNode));
                        }
                        else if ("ref".Equals(commandName) || "refitem".Equals(commandName))
                        {
                            string referenceValue = nameParam.Value;
                            TextPosition startPos = new TextPosition(0, nameParam.Token.Position.Line, nameParam.Token.Position.Column);
                            using (TextStream referenceTextStream = new BasicTextStream(referenceValue, referenceValue.Length, startPos))
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
                                        TextRange refRange = referenceTextStream.LexemeRange;
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
                                        TextRange symbolRange = new TextRange(new TextPosition(nameParam.Token.Position.Index + refRange.Position.Index, refRange.Position.Line, refRange.Position.Column), refRange.Length);
                                        LocalSymbolTable.AddReference(new ReferenceSymbol(nameParam.Token.Lang, referenceTarget, singleRereference, symbolRange, commandNode));
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
                            LocalSymbolTable.AddReference(new ReferenceSymbol(nameParam.Token.Lang, ReferenceSymbolKind.DoxygenPage, symbolName, nameParam.Token.Range, commandNode));
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

        private ParseTokenResult ParseSingleBlock(LinkedListStream<IBaseToken> stream)
        {
            // @NOTE(final) Single block = auto-brief

            IBaseToken blockToken = stream.Peek();
            DoxygenBlockEntity blockEntity = new DoxygenBlockEntity(DoxygenBlockEntityKind.BlockSingle, blockToken);
            PushEntity(blockEntity);
            stream.Next();

            IBaseToken endToken = null;
            IBaseToken briefToken = DoxygenTokenPool.Make(DoxygenTokenKind.Command, blockToken.Range, true);

            DoxygenBlockEntity briefEntity = new DoxygenBlockEntity(DoxygenBlockEntityKind.Brief, briefToken);
            DoxygenBlockNode briefNode = PushEntity(briefEntity);

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

            return (ParseTokenResult.AlreadyAdvanced);
        }

        private void CloseParagraphOrSection(IBaseNode contentRoot)
        {
            if (Top != null)
            {
                // @TODO(final): Proper assert here!
                Pop();
            }
        }
        private void CloseEverythingUntil(DoxygenBlockEntityKind kind)
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

        protected override ParseTokenResult ParseToken(LinkedListStream<IBaseToken> stream)
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
                            DoxygenBlockEntity blockEntity = new DoxygenBlockEntity(DoxygenBlockEntityKind.BlockMulti, doxyToken);
                            PushEntity(blockEntity);
                            stream.Next();
                            return (ParseTokenResult.AlreadyAdvanced);
                        }

                    case DoxygenTokenKind.DoxyBlockEnd:
                        {
                            CloseEverythingUntil(DoxygenBlockEntityKind.BlockMulti);
                            Debug.Assert(Top != null);
                            DoxygenBlockEntity rootEntity = (DoxygenBlockEntity)Top.Entity;
                            Debug.Assert(rootEntity.Kind == DoxygenBlockEntityKind.BlockMulti);
                            Pop();
                            rootEntity.EndRange = doxyToken.Range;
                            stream.Next();
                            return (ParseTokenResult.AlreadyAdvanced);
                        }

                    default:
                        {
                            ParseBlockContent(stream, Top);
                            return (ParseTokenResult.AlreadyAdvanced);
                        }
                }
            }
            else
                return (ParseTokenResult.ReadNext);
        }

        public override void Finished(IEnumerable<IBaseToken> tokens)
        {
            DoxygenBlockSymbolResolver resolver = new DoxygenBlockSymbolResolver(LocalSymbolTable);
            resolver.ResolveTokens(tokens.Where(t => typeof(DoxygenToken).Equals(t.GetType())).Select(t => t as DoxygenToken));
        }
    }
}
