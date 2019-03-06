using System;
using System.Collections.Generic;
using System.Diagnostics;
using TSP.DoxygenEditor.Collections;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Languages.Doxygen
{
    public class DoxygenParser : BaseParser<DoxygenEntity>
    {
        public static HashSet<DoxygenEntityKind> ShowChildrensSet = new HashSet<DoxygenEntityKind>()
        {
            DoxygenEntityKind.Page,
            DoxygenEntityKind.Section,
            DoxygenEntityKind.SubSection,
            DoxygenEntityKind.SubSubSection,
        };

        public string Source { get; }

        public DoxygenParser(string source)
        {
            Source = source;
        }

#if false
        [Flags]
        enum ParseCommandFlags
        {
            None = 0,
            RequiresName = 1 << 0,
            AllowCaption = 1 << 1,
        }

        class ParseCommandRule
        {
            public DoxygenEntityType TargetType { get; }
            public bool IsPush { get; }
            public ParseCommandFlags Flags { get; }
            public ParseCommandRule(DoxygenEntityType targetType, bool isPush, ParseCommandFlags flags = ParseCommandFlags.RequiresName)
            {
                TargetType = targetType;
                IsPush = isPush;
                Flags = flags;
            }
        }

        private static readonly Dictionary<string, ParseCommandRule> commandRulesMap = new Dictionary<string, ParseCommandRule>()
        {
            { "page", new ParseCommandRule(DoxygenEntityType.Page, isPush: true, flags: ParseCommandFlags.RequiresName | ParseCommandFlags.AllowCaption) },
            { "mainpage", new ParseCommandRule(DoxygenEntityType.Page, isPush: true, flags: ParseCommandFlags.None) },

            { "section", new ParseCommandRule(DoxygenEntityType.Section, isPush: true, flags: ParseCommandFlags.RequiresName | ParseCommandFlags.AllowCaption) },
            { "subsection", new ParseCommandRule(DoxygenEntityType.SubSection,isPush: true, flags: ParseCommandFlags.RequiresName | ParseCommandFlags.AllowCaption) },
            { "subsubsection", new ParseCommandRule(DoxygenEntityType.SubSubSection, isPush: true, flags: ParseCommandFlags.RequiresName | ParseCommandFlags.AllowCaption) },

            { "subpage", new ParseCommandRule(DoxygenEntityType.SubPage, isPush: false, flags: ParseCommandFlags.RequiresName | ParseCommandFlags.AllowCaption) },
            { "ref", new ParseCommandRule(DoxygenEntityType.Ref, isPush: false, flags: ParseCommandFlags.RequiresName | ParseCommandFlags.AllowCaption) },
            { "brief", new ParseCommandRule(DoxygenEntityType.Brief, isPush: false, flags: ParseCommandFlags.AllowCaption) },
        };
#endif

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

#if false
            BaseNode parentNode = Top;
            while (parentNode != null)
            {
                DoxygenNode doxyParentNode = parentNode as DoxygenNode;
                if (doxyParentNode == null)
                    break;
                DoxygenEntity parentEntity = (DoxygenEntity)doxyParentNode.Entity;
                int typeDiff = (int)newEntity.Type - (int)parentEntity.Type;
                if (typeDiff <= 0)
                {
                    Pop();
                    parentNode = parentNode.Parent;
                    if (typeDiff == 0)
                        break;
                }
                else break;
            }
            DoxygenNode newNode = new DoxygenNode(parentNode, newEntity);
            Push(newNode);
            return (newNode);
#endif
        }

        private void ParseText(LinkedListStream<BaseToken> stream, IBaseNode contentNode)
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

        private bool ParseCommand(LinkedListStream<BaseToken> stream, IBaseNode contentRoot)
        {
            // @NOTE(final): This must always return true, due to the fact that the stream is advanced at least once
            DoxygenToken commandToken = stream.Peek<DoxygenToken>();
            Debug.Assert(commandToken != null && commandToken.Kind == DoxygenTokenKind.Command);

            string commandName = commandToken.Value.Substring(1);
            stream.Next();

            var rule = DoxygenSyntax.GetCommandRule(commandName);
            if (rule != null)
            {
                if (rule.Kind == DoxygenSyntax.CommandKind.EndCommandBlock)
                {
                    var t = Top;
                    if (t == null)
                    {
                        AddParseError(commandToken.Position, $"Unterminated starting command block in command '{commandName}'");
                        return (false);
                    }
                    if (t.Entity.Kind != DoxygenEntityKind.BlockCommand)
                    {
                        AddParseError(commandToken.Position, $"Expect starting command block, but found '{t.Entity.Kind}' in command '{commandName}'");
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
                    commandEntity = new DoxygenEntity(rule.EntityKind, commandToken);
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
                        AddParseError(argToken.Position, $"Expect argument token '{expectedTokenKind}', but got '{argToken.Kind}'");
                        break;
                    }
                    if (commandNode != null)
                    {
                        string paramName = arg.Name;
                        string paramValue = argToken.Value;
                        commandNode.Entity.AddParameter(paramName, paramValue);
                    }
                    stream.Next();
                }

                if (commandEntity != null)
                {
                    string name = commandEntity.GetParameterValue("name", "id");
                    string text = commandEntity.GetParameterValue("text", "title", "caption");
                    if (!string.IsNullOrWhiteSpace(name))
                        commandEntity.Id = name;
                    if (!string.IsNullOrWhiteSpace(text))
                        commandEntity.Value = text;
                }
                ParseBlockContent(stream, commandNode);
            }
            return (true);
        }

        private bool ParseSingleBlock(LinkedListStream<BaseToken> stream)
        {
            // @NOTE(final) Single block = auto-brief

            BaseToken blockToken = stream.Peek();
            DoxygenEntity blockEntity = new DoxygenEntity(DoxygenEntityKind.BlockSingle, blockToken);
            PushEntity(blockEntity);
            stream.Next();

            BaseToken endToken = null;
            BaseToken briefToken = DoxygenTokenPool.Make(DoxygenTokenKind.Command, blockToken, true);

            var briefEntity = new DoxygenEntity(DoxygenEntityKind.Brief, briefToken);
            var briefNode = PushEntity(briefEntity);

            while (!stream.IsEOF)
            {
                BaseToken token = stream.Peek();
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
                blockEntity.EndRange = endToken;

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

        private bool ParseBlockContent(LinkedListStream<BaseToken> stream, IBaseNode contentRoot)
        {
            BaseToken token = stream.Peek();
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

        public override bool ParseToken(LinkedListStream<BaseToken> stream)
        {
            BaseToken token = stream.Peek();
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
                            rootEntity.EndRange = doxyToken;
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
