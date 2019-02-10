using System;
using System.Collections.Generic;
using System.Diagnostics;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Lexers.Cpp;
using TSP.DoxygenEditor.Lexers.Doxygen;
using TSP.DoxygenEditor.Lists;

namespace TSP.DoxygenEditor.Parsers.Doxygen
{
    class DoxygenTree : BaseTree
    {
        [Flags]
        enum CommandFlags
        {
            None = 0,
            RequiresIdent = 1 << 0,
            AllowCaption = 1 << 1,
        }

        class CommandRule
        {
            public DoxygenEntityType TargetType { get; }
            public bool IsPush { get; }
            public CommandFlags Flags { get; }
            public CommandRule(DoxygenEntityType targetType, bool isPush, CommandFlags flags = CommandFlags.RequiresIdent)
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


        private static readonly Dictionary<string, CommandRule> commandRulesMap = new Dictionary<string, CommandRule>()
        {
            { "page", new CommandRule(DoxygenEntityType.Page, isPush: true, flags: CommandFlags.RequiresIdent | CommandFlags.AllowCaption) },
            { "mainpage", new CommandRule(DoxygenEntityType.Page, isPush: true, flags: CommandFlags.None) },

            { "section", new CommandRule(DoxygenEntityType.Section, isPush: true, flags: CommandFlags.RequiresIdent | CommandFlags.AllowCaption) },
            { "subsection", new CommandRule(DoxygenEntityType.SubSection,isPush: true, flags: CommandFlags.RequiresIdent | CommandFlags.AllowCaption) },
            { "subsubsection", new CommandRule(DoxygenEntityType.SubSubSection, isPush: true, flags: CommandFlags.RequiresIdent | CommandFlags.AllowCaption) },

            { "subpage", new CommandRule(DoxygenEntityType.SubPage, isPush: false, flags: CommandFlags.RequiresIdent | CommandFlags.AllowCaption) },
            { "ref", new CommandRule(DoxygenEntityType.Ref, isPush: false, flags: CommandFlags.RequiresIdent | CommandFlags.AllowCaption) },
        };

        private void PushEntity(DoxygenEntity newEntity)
        {
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
            Push(new DoxygenNode(parentNode, newEntity));
        }

        private bool ParseCommand(LinkedListStream<BaseToken> stream)
        {
            DoxygenToken commandToken = stream.Peek<DoxygenToken>();
            Debug.Assert(commandToken != null && commandToken.Type == DoxygenTokenType.Command);

            string command = GetText(commandToken);
            command = command.Substring(1);

            stream.Next();

            if (commandRulesMap.ContainsKey(command))
            {
                CommandRule config = commandRulesMap[command];

                string ident = null;
                if (config.Flags.HasFlag(CommandFlags.RequiresIdent))
                {
                    DoxygenToken next = stream.Peek<DoxygenToken>();
                    if (next == null || (next.Type != DoxygenTokenType.Ident))
                        return (false);
                    ident = GetText(next);
                    stream.Next<DoxygenToken>();
                }

                string caption = null;
                if (config.Flags.HasFlag(CommandFlags.AllowCaption))
                {
                    DoxygenToken next = stream.Peek<DoxygenToken>();
                    if (next != null && next.Type == DoxygenTokenType.Caption)
                    {
                        caption = GetText(next);
                        stream.Next<DoxygenToken>();
                    }
                }

                DoxygenEntity newEntity = new DoxygenEntity(config.TargetType, ident, commandToken)
                {
                    Caption = caption,
                };

                if (config.IsPush)
                {
                    PushEntity(newEntity);
                }
                else
                {
                    // @FIXME(final): Root is null on a @ref element, which should not happen in any case
                    BaseNode parentNode = Root;
                    Debug.Assert(parentNode != null);
                    Add(new DoxygenNode(parentNode, newEntity));
                }
                return (true);
            }
            else
                return (false);
        }

        private bool ParseSourceDeclaration(LinkedListStream<BaseToken> stream)
        {
            DoxygenToken blockEndToken = stream.Peek<DoxygenToken>();
            Debug.Assert(blockEndToken != null && blockEndToken.Type == DoxygenTokenType.BlockEnd);
            stream.Next();

            CppToken cppToken = stream.Peek<CppToken>();
            if (cppToken != null)
            {
                return (true);
            }

            return (false);
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

#if false
                    case DoxygenTokenType.BlockStart:
                        {
                            LinkedListNode<BaseToken> n = stream.CurrentNode;
                            DoxygenEntity blockEntity = new DoxygenEntity(DoxygenEntityType.Block, null, doxyToken);
                            DoxygenNode newNode = new DoxygenNode(Root, blockEntity);
                            PushEntity(blockEntity);
                            stream.Next();
                            return (true);
                        }
#endif

                    case DoxygenTokenType.BlockEnd:
                        {
#if false
                            DoxygenNode n = (DoxygenNode)Root;
                            Debug.Assert(n != null);
                            DoxygenEntity e = (DoxygenEntity)n.Entity;
                            Pop();
#endif
                            return ParseSourceDeclaration(stream);
                        }

                    default:
                        break;
                }
            }
            return (false);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
