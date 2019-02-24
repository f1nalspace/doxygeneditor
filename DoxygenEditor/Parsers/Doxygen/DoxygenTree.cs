using System;
using System.Collections.Generic;
using System.Diagnostics;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Lexers.Doxygen;
using TSP.DoxygenEditor.Lists;

namespace TSP.DoxygenEditor.Parsers.Doxygen
{
    class DoxygenTree : BaseTree
    {
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

        public static HashSet<DoxygenEntityType> ShowChildrensSet = new HashSet<DoxygenEntityType>()
        {
            DoxygenEntityType.Page,
            DoxygenEntityType.Section,
            DoxygenEntityType.SubSection,
            DoxygenEntityType.SubSubSection,
        };


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

        private DoxygenNode PushEntity(DoxygenEntity newEntity)
        {
            if (newEntity.Type == DoxygenEntityType.BlockSingle || newEntity.Type == DoxygenEntityType.BlockMulti)
            {
                if (Root != null)
                    Pop();
                DoxygenNode blockNode = new DoxygenNode(Root, newEntity);
                Push(blockNode);
                return (blockNode);
            }

            BaseNode parentNode = Root;
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
        }

        private bool ParseCommand(LinkedListStream<BaseToken> stream)
        {
            // @NOTE(final): This must always return true, due to the fact that the stream is advanced at least once
            DoxygenToken commandToken = stream.Peek<DoxygenToken>();
            Debug.Assert(commandToken != null && commandToken.Type == DoxygenTokenType.Command);

            string command = GetText(commandToken);
            command = command.Substring(1);

            stream.Next();

            if (commandRulesMap.ContainsKey(command))
            {
                ParseCommandRule config = commandRulesMap[command];

                string name = null;
                if (config.Flags.HasFlag(ParseCommandFlags.RequiresName))
                {
                    DoxygenToken next = stream.Peek<DoxygenToken>();
                    if (next == null || (next.Type != DoxygenTokenType.Name))
                        return (false);

                    name = GetText(next);
                    stream.Next<DoxygenToken>();
                }

                string caption = null;
                if (config.Flags.HasFlag(ParseCommandFlags.AllowCaption))
                {
                    DoxygenToken captionToken = stream.Peek() as DoxygenToken;
                    if (captionToken != null && captionToken.Type == DoxygenTokenType.Caption)
                    {
                        caption = GetText(captionToken);
                        stream.Next();
                    }
                }

                DoxygenEntity newEntity = new DoxygenEntity(config.TargetType, name, commandToken)
                {
                    Caption = caption,
                };

                if (config.IsPush)
                {
                    PushEntity(newEntity);
                }
                else
                {
                    Debug.Assert(Root != null);
                    Add(new DoxygenNode(Root, newEntity));
                }
            }
            return (true);
        }

        private bool ParseSingleBlock(LinkedListStream<BaseToken> stream)
        {
            BaseToken blockToken = stream.Peek();
            DoxygenEntity blockEntity = new DoxygenEntity(DoxygenEntityType.BlockSingle, null, blockToken);
            PushEntity(blockEntity);
            stream.Next();

            BaseToken endToken = null;
            BaseToken briefToken = new DoxygenToken(DoxygenTokenType.Command, blockToken.Index, blockToken.Length, true);

            var briefEntity = new DoxygenEntity(DoxygenEntityType.Brief, null, briefToken);
            PushEntity(briefEntity);
            while (!stream.IsEOF)
            {
                BaseToken token = stream.Peek();
                DoxygenToken doxyToken = token as DoxygenToken;
                if (doxyToken != null && doxyToken.Type == DoxygenTokenType.BlockEnd)
                {
                    endToken = token;
                    stream.Next();
                    break;
                }

                DoxygenNode foreignNode = null;
                if (doxyToken != null)
                {
                    switch (doxyToken.Type)
                    {
                        case DoxygenTokenType.Command:
                            ParseCommand(stream);
                            break;

                        default:
                            foreignNode = new DoxygenNode(Root, new DoxygenEntity(DoxygenEntityType.Foreign, null, token));
                            break;
                    }
                }
                else
                    foreignNode = new DoxygenNode(Root, new DoxygenEntity(DoxygenEntityType.Foreign, null, token));
                if (foreignNode != null)
                    Add(foreignNode);
                stream.Next();
            }
            Pop(); // Pop brief

            if (endToken != null)
                blockEntity.EndRange = endToken;

            Pop(); // Pop block

            return (true);
        }

        public override bool ParseToken(LinkedListStream<BaseToken> stream)
        {
            BaseToken token = stream.Peek();
            if (typeof(DoxygenToken).Equals(token.GetType()))
            {
                DoxygenToken doxyToken = (DoxygenToken)token;
                switch (doxyToken.Type)
                {
                    case DoxygenTokenType.Command:
                        return ParseCommand(stream);

                    case DoxygenTokenType.BlockStartSingle:
                        return ParseSingleBlock(stream);

                    case DoxygenTokenType.BlockStartMulti:
                        {
                            DoxygenEntity blockEntity = new DoxygenEntity(DoxygenEntityType.BlockMulti, null, doxyToken);
                            PushEntity(blockEntity);
                            stream.Next();
                            return (true);
                        }

                    case DoxygenTokenType.BlockEnd:
                        {
                            // Pop everything until block
                            while (Root != null)
                            {
                                DoxygenEntity e = (DoxygenEntity)Root.Entity;
                                if (e.Type == DoxygenEntityType.BlockMulti)
                                {
                                    break;
                                }
                                else
                                    Pop();
                            }
                            Debug.Assert(Root != null);
                            DoxygenEntity rootEntity = (DoxygenEntity)Root.Entity;
                            Debug.Assert(rootEntity.Type == DoxygenEntityType.BlockMulti);
                            Pop();

                            rootEntity.EndRange = doxyToken;

                            stream.Next();
                            return (true);
                        }

                    default:
                        {
                            stream.Next();
                            return (!doxyToken.IsEOF);
                        }
                }
            }
            else
            {
                return (false);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
