using System;
using System.Collections.Generic;
using TSP.DoxygenEditor.Languages.Utils;

namespace TSP.DoxygenEditor.Languages.Doxygen
{
    public static class DoxygenSyntax
    {
        public static HashSet<char> MultiLineDocChars = new HashSet<char>() { '!', '*' };
        public static HashSet<char> SingleLineDocChars = new HashSet<char>() { '!', '/' };

        public enum ArgumentKind
        {
            None = 0,
            // Single character
            Char,
            // Identifier ([a-zA-Z_][a-zA-Z0-9_]*)
            Identifier,
            // Single word, until any whitespace character ([^\s]+)
            SingleWord,
            // Quoted word
            QuotedString,
            // Header file (With ot without quotes or sharp brackets)
            HeaderFile,
            // Header file name (With quotes or sharp brackets)
            HeaderName,
            // One cross-reference to class, method, variable, file or url
            // Two names joined by either :: or # are understood as referring to a class and one of its members. One of several overloaded methods or constructors may be selected by including a parenthesized list of argument types after the method name.
            // Or a reference to a doxygen element (section, subsection, etc.)
            SingleObjectReference,
            // One or more cross-references to classes, methods, variables, files or urls
            // Two names joined by either :: or # are understood as referring to a class and one of its members. One of several overloaded methods or constructors may be selected by including a parenthesized list of argument types after the method name.
            MultipleObjectReference,
            // (refid) -> Extends until the end of the line
            UntilEndOfLine,
            // Extends until the prefix plus the length of the prefix
            PrefixToPostfix,
            // <sizeindication>=<size>
            Size,
            // {simple} -> Extends until the end of line, but allows multiple commands
            ComplexLine,
            // {complex} -> Extends until the next paragraph or a section indicator or same command was found
            ComplexBlock,
        }

        public static readonly Dictionary<ArgumentKind, DoxygenTokenKind> ArgumentToTokenKindMap = new Dictionary<ArgumentKind, DoxygenTokenKind>()
        {
            { ArgumentKind.Identifier, DoxygenTokenKind.ArgumentIdent },
            { ArgumentKind.SingleObjectReference, DoxygenTokenKind.ArgumentIdent },
            { ArgumentKind.MultipleObjectReference, DoxygenTokenKind.ArgumentIdent },
            { ArgumentKind.SingleWord, DoxygenTokenKind.ArgumentCaption },
            { ArgumentKind.PrefixToPostfix, DoxygenTokenKind.ArgumentCaption },
            { ArgumentKind.QuotedString, DoxygenTokenKind.ArgumentText },
            { ArgumentKind.UntilEndOfLine, DoxygenTokenKind.ArgumentText },
            { ArgumentKind.HeaderFile, DoxygenTokenKind.ArgumentFile },
            { ArgumentKind.HeaderName, DoxygenTokenKind.ArgumentFile },
        };

        public enum ArgumentRepeat
        {
            /// <summary>No limit set</summary>
            Ignore = -1,
            /// <summary>0 or 1</summary>
            ZeroOrOne = 0,
            /// <summary>0 or 1+</summary>
            ZeroOrMore,
            /// <summary>1+</summary>
            OneOrMore,
            /// <summary>1</summary>
            One,
        }

        [Flags]
        public enum ArgumentFlags
        {
            None = 0,
            DirectlyAfterCommand = 1 << 0,
        }

        public class ArgumentRule
        {
            public ArgumentKind Kind { get; }
            public string Name { get; }
            public string Prefix { get; }
            public string Postfix { get; }
            public ArgumentFlags Flags { get; }
            public ArgumentRepeat Repeat { get; private set; }
            public bool IsRequired => (Repeat == ArgumentRepeat.One) || (Repeat == ArgumentRepeat.OneOrMore);
            public bool IsOptional => (Repeat == ArgumentRepeat.ZeroOrOne) || (Repeat == ArgumentRepeat.ZeroOrMore);
            public ArgumentRule(ArgumentKind kind, string name, string prefix = "", string postfix = "", ArgumentFlags flags = ArgumentFlags.None)
            {
                Kind = kind;
                Name = name;
                Prefix = prefix;
                Postfix = postfix;
                Flags = flags;
                Repeat = ArgumentRepeat.Ignore;
            }
            public ArgumentRule AtLeastOnce()
            {
                ArgumentRule result = this;
                result.Repeat = ArgumentRepeat.OneOrMore;
                return (result);
            }
            public ArgumentRule Many()
            {
                ArgumentRule result = this;
                result.Repeat = ArgumentRepeat.ZeroOrMore;
                return (result);
            }
            public ArgumentRule Optional()
            {
                ArgumentRule result = this;
                result.Repeat = ArgumentRepeat.ZeroOrOne;
                return (result);
            }
            public ArgumentRule Required()
            {
                ArgumentRule result = this;
                result.Repeat = ArgumentRepeat.One;
                return (result);
            }
            public override string ToString()
            {
                return $"'{Name}' as {Kind}, Repeat {Repeat}";
            }
        }

        public enum CommandKind
        {
            /// <summary>No kind</summary>
            None = -1,
            /// <summary>Is a basic command</summary>
            Basic = 0,
            /// <summary>Is a paragraph command</summary>
            Paragraph,
            /// <summary>Is a section command</summary>
            Section,
            /// <summary>Is a visual command</summary>
            VisualEnhancement,
            /// <summary>Is a start block</summary>
            StartCommandBlock,
            /// <summary>Is a end block</summary>
            EndCommandBlock,
            /// <summary>Escape character</summary>
            Escape,
        }

        public abstract class CommandRule
        {
            public CommandKind Kind { get; }
            public DoxygenEntityKind EntityKind { get; }
            public IEnumerable<ArgumentRule> Args { get; }
            public bool IsPush
            {
                get
                {
                    bool result = (Kind == CommandKind.StartCommandBlock) ||
                        (Kind == CommandKind.Section) ||
                        (Kind == CommandKind.Paragraph);
                    return (result);
                }
            }
            public CommandRule(CommandKind kind, DoxygenEntityKind entityKind, IEnumerable<ArgumentRule> args)
            {
                Kind = kind;
                EntityKind = entityKind;
                Args = args;
            }
        }
        public class BasicCommandRule : CommandRule
        {
            public BasicCommandRule(params ArgumentRule[] args) : base(CommandKind.Basic, DoxygenEntityKind.Basic, args)
            {
            }
        }
        public class VisualEnhancementCommandRule : CommandRule
        {
            public VisualEnhancementCommandRule(params ArgumentRule[] args) : base(CommandKind.VisualEnhancement, DoxygenEntityKind.VisualEnhancement, args)
            {
            }
        }
        public class ParagraphCommandRule : CommandRule
        {
            public ParagraphCommandRule(params ArgumentRule[] args) : base(CommandKind.Paragraph, DoxygenEntityKind.Paragraph, args)
            {
            }
        }
        public class SectionCommandRule : CommandRule
        {
            public SectionCommandRule(DoxygenEntityKind entityKind, params ArgumentRule[] args) : base(CommandKind.Section, entityKind, args)
            {
            }
        }
        public class EscapeCommandRule : CommandRule
        {
            public EscapeCommandRule(params ArgumentRule[] args) : base(CommandKind.Escape, DoxygenEntityKind.None, args)
            {
            }
        }
        public class StartBlockCommandRule : CommandRule
        {
            public StartBlockCommandRule(params ArgumentRule[] args) : base(CommandKind.StartCommandBlock, DoxygenEntityKind.BlockCommand, args)
            {
            }
        }
        public class EndBlockCommandRule : CommandRule
        {
            public HashSet<string> StartCommandNames { get; }
            public EndBlockCommandRule(string[] startCommandNames, params ArgumentRule[] args) : base(CommandKind.EndCommandBlock, DoxygenEntityKind.None, args)
            {
                StartCommandNames = new HashSet<string>(startCommandNames);
            }
            public EndBlockCommandRule(string startCommandName, params ArgumentRule[] args) : this(new[] { startCommandName }, args)
            {
            }
        }

        /// <summary>
        /// This command rules are based on http://www.doxygen.nl/manual/commands.html
        /// </summary>
        private static readonly Dictionary<string, CommandRule> CommandRules = new Dictionary<string, CommandRule>()
        {
            { "a", new VisualEnhancementCommandRule(new ArgumentRule(ArgumentKind.SingleWord, "word").Required()) },
            { "addindex", new BasicCommandRule(new ArgumentRule(ArgumentKind.UntilEndOfLine, "text").Required()) },
            { "addtogroup", new BasicCommandRule(new ArgumentRule(ArgumentKind.Identifier, "name").Required(), new ArgumentRule(ArgumentKind.UntilEndOfLine, "title").Optional() ) },
            { "anchor", new BasicCommandRule(new ArgumentRule(ArgumentKind.Identifier, "word").Required() ) },
            { "arg", new BasicCommandRule(new ArgumentRule(ArgumentKind.ComplexLine, "item-description").Required() ) },
            { "attention", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.ComplexBlock, "attention text").Required() ) },
            { "author", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.ComplexBlock, "list of authors").Required() ) },
            { "b", new VisualEnhancementCommandRule(new ArgumentRule(ArgumentKind.SingleWord, "word").Required()) },
            { "brief", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.ComplexBlock, "brief description").Required()) },
            { "bug", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.ComplexBlock, "bug description").Required()) },
            { "c", new VisualEnhancementCommandRule(new ArgumentRule(ArgumentKind.SingleWord, "word").Required()) },
            { "callergraph", new BasicCommandRule() },
            { "callgraph", new BasicCommandRule() },
            { "category", new BasicCommandRule(new ArgumentRule(ArgumentKind.Identifier, "name").Required(), new ArgumentRule(ArgumentKind.HeaderFile, "header-file").Optional(), new ArgumentRule(ArgumentKind.HeaderName, "header-name").Optional()) },
            { "cite", new BasicCommandRule(new ArgumentRule(ArgumentKind.Identifier, "label").Required()) },
            { "class", new BasicCommandRule(new ArgumentRule(ArgumentKind.Identifier, "name").Required(), new ArgumentRule(ArgumentKind.HeaderFile, "header-file").Optional(), new ArgumentRule(ArgumentKind.HeaderName, "header-name").Optional()) },
            { "code", new StartBlockCommandRule(new ArgumentRule(ArgumentKind.PrefixToPostfix, "code-type", "{", "}", ArgumentFlags.DirectlyAfterCommand).Optional() ) },
            { "cond", new StartBlockCommandRule(new ArgumentRule(ArgumentKind.UntilEndOfLine, "section-label").Required()) },
            { "copybrief", new BasicCommandRule(new ArgumentRule(ArgumentKind.SingleObjectReference, "link-object").Required()) },
            { "copydetails", new BasicCommandRule(new ArgumentRule(ArgumentKind.SingleObjectReference, "link-object").Required()) },
            { "copydoc", new BasicCommandRule(new ArgumentRule(ArgumentKind.SingleObjectReference, "link-object").Required()) },
            { "copyright", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.ComplexBlock, "copyright description").Required()) },
            { "date", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.ComplexBlock, "date description").Required()) },
            { "def", new BasicCommandRule(new ArgumentRule(ArgumentKind.Identifier, "name").Required()) },
            { "defgroup", new BasicCommandRule(new ArgumentRule(ArgumentKind.Identifier, "name").Required(), new ArgumentRule(ArgumentKind.UntilEndOfLine, "group title").Optional()) },
            { "deprecated", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.ComplexBlock, "description").Required()) },
            { "details", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.ComplexBlock, "detailed description").Required()) },
            { "diafile", new BasicCommandRule(new ArgumentRule(ArgumentKind.HeaderFile, "file").Required(), new ArgumentRule(ArgumentKind.QuotedString, "caption").Optional(), new ArgumentRule(ArgumentKind.Size, "sizeindication").Optional()) },
            { "dir", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.QuotedString, "path fragment").Optional()) },
            { "docbookonly", new StartBlockCommandRule() },
            { "dontinclude", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.HeaderFile, "file-name").Required()) },
            { "dot", new StartBlockCommandRule(new ArgumentRule(ArgumentKind.PrefixToPostfix, "caption", "\"", "\"").Optional(), new ArgumentRule(ArgumentKind.Size, "sizeindication").Optional() ) },
            { "else", new StartBlockCommandRule() },
            { "elseif", new StartBlockCommandRule(new ArgumentRule(ArgumentKind.UntilEndOfLine, "section-label").Required() ) },
            { "emoji", new VisualEnhancementCommandRule(new ArgumentRule(ArgumentKind.QuotedString, "name").Required()) },
            { "endcode", new EndBlockCommandRule("code") },
            { "endcond", new EndBlockCommandRule("cond") },
            { "enddocbookonly", new EndBlockCommandRule("docbookonly") },
            { "enddot", new EndBlockCommandRule("dot") },
            { "endhtmlonly", new EndBlockCommandRule("htmlonly") },
            { "endif", new EndBlockCommandRule(new[]{"if", "ifnot", "else", "elseif" }) },
            { "endinternal", new EndBlockCommandRule("internal") },
            { "endlatexonly", new EndBlockCommandRule("latexonly") },
            { "endlink", new EndBlockCommandRule("link") },
            { "endmanonly", new EndBlockCommandRule("manonly") },
            { "endmsc", new EndBlockCommandRule("msc") },
            { "endparblock", new EndBlockCommandRule("parblock") },
            { "endrtfonly", new EndBlockCommandRule("rtfonly") },
            { "endsecreflist", new EndBlockCommandRule("secreflist") },
            { "endverbatim", new EndBlockCommandRule("verbatim") },
            { "enduml", new EndBlockCommandRule("startuml") },
            { "endxmlonly", new EndBlockCommandRule("xmlonly") },
            { "enum", new BasicCommandRule(new ArgumentRule(ArgumentKind.SingleObjectReference, "name").Required()) },
            { "example", new BasicCommandRule(new ArgumentRule(ArgumentKind.PrefixToPostfix, "lineno", "{", "}", ArgumentFlags.DirectlyAfterCommand).Optional(), new ArgumentRule(ArgumentKind.HeaderFile, "file-name").Required()) },
            { "exception", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.SingleObjectReference, "exception-object"), new ArgumentRule(ArgumentKind.ComplexBlock, "exception description")) },
            { "extends", new BasicCommandRule(new ArgumentRule(ArgumentKind.SingleObjectReference, "name").Required()) },
            { "f$", new BasicCommandRule() },
            { "f[", new StartBlockCommandRule() },
            { "f]", new EndBlockCommandRule("f[") },
            { "f{", new StartBlockCommandRule(new ArgumentRule(ArgumentKind.PrefixToPostfix, "environment", "", "}").Required(), new ArgumentRule(ArgumentKind.Char, "{").Required()) },
            { "f}", new EndBlockCommandRule("f{") },
            { "file", new BasicCommandRule(new ArgumentRule(ArgumentKind.HeaderFile, "name").Required()) },
            { "fn", new BasicCommandRule(new ArgumentRule(ArgumentKind.UntilEndOfLine, "function declaration").Required()) },
            { "headerfile", new BasicCommandRule(new ArgumentRule(ArgumentKind.HeaderFile, "header-file").Required(), new ArgumentRule(ArgumentKind.HeaderName, "header-name").Optional()) },
            { "hidecallergraph", new BasicCommandRule() },
            { "hidecallgraph", new BasicCommandRule() },
            { "hiderefby", new BasicCommandRule() },
            { "hiderefs", new BasicCommandRule() },
            { "htmlinclude", new BasicCommandRule(new ArgumentRule(ArgumentKind.PrefixToPostfix, "block", "\"", "\"").Optional(), new ArgumentRule(ArgumentKind.UntilEndOfLine, "file-name").Required()) },
            { "htmlonly", new StartBlockCommandRule(new ArgumentRule(ArgumentKind.PrefixToPostfix, "block", "\"", "\"").Optional()) },
            { "idlexcept", new BasicCommandRule(new ArgumentRule(ArgumentKind.Identifier, "name").Required()) },
            { "if", new StartBlockCommandRule(new ArgumentRule(ArgumentKind.UntilEndOfLine, "section-label").Required()) },
            { "ifnot", new StartBlockCommandRule(new ArgumentRule(ArgumentKind.UntilEndOfLine, "section-label").Required()) },
            { "image", new BasicCommandRule(new ArgumentRule(ArgumentKind.PrefixToPostfix, "option", "{", "}").Optional(), new ArgumentRule(ArgumentKind.Identifier, "format").Required(), new ArgumentRule(ArgumentKind.HeaderFile, "file").Required(), new ArgumentRule(ArgumentKind.QuotedString, "caption").Optional(), new ArgumentRule(ArgumentKind.Size, "sizeindication").Optional()) },
            { "implements", new BasicCommandRule(new ArgumentRule(ArgumentKind.SingleObjectReference, "name").Required()) },
            { "include", new BasicCommandRule(new ArgumentRule(ArgumentKind.PrefixToPostfix, "lineno_or_doc", "{", "}", ArgumentFlags.DirectlyAfterCommand).Optional(), new ArgumentRule(ArgumentKind.HeaderFile, "file-name").Required()) },
            { "includedoc", new BasicCommandRule(new ArgumentRule(ArgumentKind.HeaderFile, "file-name").Required()) },
            { "ingroup", new BasicCommandRule(new ArgumentRule(ArgumentKind.Identifier, "groupname").AtLeastOnce()) },
            { "internal", new StartBlockCommandRule() },
            { "invariant", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.ComplexBlock, "description of invariant").Required()) },
            { "interface", new BasicCommandRule(new ArgumentRule(ArgumentKind.SingleObjectReference, "name").Required(), new ArgumentRule(ArgumentKind.HeaderFile, "header-file").Optional(), new ArgumentRule(ArgumentKind.HeaderName, "header-name").Optional()) },
            { "latexinclude", new BasicCommandRule(new ArgumentRule(ArgumentKind.HeaderFile, "file-name").Required()) },
            { "latexonly", new StartBlockCommandRule() },
            { "line", new BasicCommandRule(new ArgumentRule(ArgumentKind.UntilEndOfLine, "pattern").Required()) },
            { "link", new StartBlockCommandRule(new ArgumentRule(ArgumentKind.SingleObjectReference, "link-object").Required()) },
            { "mainpage", new SectionCommandRule(DoxygenEntityKind.Page, new ArgumentRule(ArgumentKind.UntilEndOfLine, "title").Optional()) },
            { "manonly", new StartBlockCommandRule() },
            { "memberof", new BasicCommandRule(new ArgumentRule(ArgumentKind.SingleObjectReference, "name").Required()) },
            { "msc", new StartBlockCommandRule(new ArgumentRule(ArgumentKind.QuotedString, "caption").Optional(), new ArgumentRule(ArgumentKind.Size, "sizeindication").Optional()) },
            { "mscfile", new BasicCommandRule(new ArgumentRule(ArgumentKind.HeaderFile, "file").Required(), new ArgumentRule(ArgumentKind.QuotedString, "caption").Optional(), new ArgumentRule(ArgumentKind.Size, "sizeindication").Optional()) },
            { "n", new VisualEnhancementCommandRule() },
            { "name", new BasicCommandRule(new ArgumentRule(ArgumentKind.UntilEndOfLine, "header").Optional()) },
            { "namespace", new BasicCommandRule(new ArgumentRule(ArgumentKind.UntilEndOfLine, "name").Optional()) },
            { "nosubgrouping", new BasicCommandRule() },
            { "note", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.ComplexBlock, "text").Required()) },
            { "overload", new BasicCommandRule(new ArgumentRule(ArgumentKind.UntilEndOfLine, "function declaration").Optional()) },
            { "package", new BasicCommandRule(new ArgumentRule(ArgumentKind.Identifier, "name").Required()) },
            { "page", new SectionCommandRule(DoxygenEntityKind.Page, new ArgumentRule(ArgumentKind.Identifier, "name").Required(), new ArgumentRule(ArgumentKind.UntilEndOfLine, "title").Optional()) },
            { "par", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.UntilEndOfLine, "paragraph title").Optional(), new ArgumentRule(ArgumentKind.ComplexBlock, "paragraph").Required()) },
            { "paragraph", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.Identifier, "paragraph-name").Required(), new ArgumentRule(ArgumentKind.UntilEndOfLine, "paragraph title").Required()) },
            { "param", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.Identifier, "dir", "[", "]", ArgumentFlags.DirectlyAfterCommand).Optional(), new ArgumentRule(ArgumentKind.Identifier, "parameter-name").Required(), new ArgumentRule(ArgumentKind.ComplexLine, "parameter description").Required()) },
            { "parblock", new StartBlockCommandRule() },
            { "post", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.ComplexBlock, "description of the postcondition").Required()) },
            { "pre", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.ComplexBlock, "description of the precondition").Required()) },
            { "private", new BasicCommandRule() },
            { "privatesection", new BasicCommandRule() },
            { "property", new BasicCommandRule(new ArgumentRule(ArgumentKind.UntilEndOfLine, "qualified property name").Required()) },
            { "protected", new BasicCommandRule() },
            { "protectedsection", new BasicCommandRule() },
            { "protocol", new BasicCommandRule(new ArgumentRule(ArgumentKind.SingleObjectReference, "name").Required(), new ArgumentRule(ArgumentKind.HeaderFile, "header-file").Optional(), new ArgumentRule(ArgumentKind.HeaderName, "header-name").Optional()) },
            { "public", new BasicCommandRule() },
            { "publicsection", new BasicCommandRule() },
            { "pure", new BasicCommandRule() },
            { "ref", new BasicCommandRule(new ArgumentRule(ArgumentKind.SingleObjectReference, "name").Required(), new ArgumentRule(ArgumentKind.QuotedString, "text").Optional()) },
            { "refitem", new BasicCommandRule(new ArgumentRule(ArgumentKind.SingleObjectReference, "name").Required()) },
            { "relates", new BasicCommandRule(new ArgumentRule(ArgumentKind.SingleObjectReference, "name").Required()) },
            { "relatesalso", new BasicCommandRule(new ArgumentRule(ArgumentKind.SingleObjectReference, "name").Required()) },
            { "remark", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.ComplexBlock, "remark text").Required()) },
            { "return", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.ComplexBlock, "description of the return value").Required()) },
            { "retval", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.Identifier, "return value").Required(), new ArgumentRule(ArgumentKind.ComplexBlock, "description").Required()) },
            { "rtfonly", new StartBlockCommandRule() },
            { "sa", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.ComplexLine, "references").AtLeastOnce()) },
            { "secreflist", new StartBlockCommandRule() },
            { "section", new SectionCommandRule(DoxygenEntityKind.Section, new ArgumentRule(ArgumentKind.Identifier, "name").Required(), new ArgumentRule(ArgumentKind.UntilEndOfLine, "title").Required()) },
            { "showinitializer", new BasicCommandRule() },
            { "showrefby", new BasicCommandRule() },
            { "showrefs", new BasicCommandRule() },
            { "since", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.ComplexBlock, "text").Required()) },
            { "skip", new BasicCommandRule(new ArgumentRule(ArgumentKind.UntilEndOfLine, "pattern").Required()) },
            { "skipline", new BasicCommandRule(new ArgumentRule(ArgumentKind.UntilEndOfLine, "pattern").Required()) },
            { "snippet", new BasicCommandRule(new ArgumentRule(ArgumentKind.PrefixToPostfix, "lineno_or_doc", "{", "}", ArgumentFlags.DirectlyAfterCommand).Optional(), new ArgumentRule(ArgumentKind.HeaderFile, "file-name").Required(), new ArgumentRule(ArgumentKind.UntilEndOfLine, "block_id").Required()) },
            { "snippetdoc", new BasicCommandRule(new ArgumentRule(ArgumentKind.HeaderFile, "file-name").Required(), new ArgumentRule(ArgumentKind.UntilEndOfLine, "block_id").Required()) },
            { "snippetlineno", new BasicCommandRule(new ArgumentRule(ArgumentKind.HeaderFile, "file-name").Required(), new ArgumentRule(ArgumentKind.UntilEndOfLine, "block_id").Required()) },
            { "startuml", new StartBlockCommandRule(new ArgumentRule(ArgumentKind.PrefixToPostfix, "file", "{", "}").Optional(), new ArgumentRule(ArgumentKind.QuotedString, "caption").Optional(), new ArgumentRule(ArgumentKind.Size, "sizeindication").Optional()) },
            { "struct", new BasicCommandRule(new ArgumentRule(ArgumentKind.Identifier, "name").Optional(), new ArgumentRule(ArgumentKind.HeaderFile, "header-file").Optional(), new ArgumentRule(ArgumentKind.HeaderName, "header-name").Optional()) },
            { "subpage", new BasicCommandRule(new ArgumentRule(ArgumentKind.Identifier, "name").Required(), new ArgumentRule(ArgumentKind.UntilEndOfLine, "text").Optional()) },
            { "subsection", new SectionCommandRule(DoxygenEntityKind.Section, new ArgumentRule(ArgumentKind.Identifier, "name").Required(), new ArgumentRule(ArgumentKind.UntilEndOfLine, "title").Required()) },
            { "subsubsection", new SectionCommandRule(DoxygenEntityKind.Section, new ArgumentRule(ArgumentKind.Identifier, "name").Required(), new ArgumentRule(ArgumentKind.UntilEndOfLine, "title").Required()) },
            { "tableofcontents", new BasicCommandRule(new ArgumentRule(ArgumentKind.PrefixToPostfix, "option", "{", "}", ArgumentFlags.DirectlyAfterCommand).Optional()) },
            { "test", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.ComplexBlock, "paragraph describing a test case").Required()) },
            { "throw", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.SingleObjectReference, "exception-object").Required(), new ArgumentRule(ArgumentKind.ComplexBlock, "exception description").Required()) },
            { "todo", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.ComplexBlock, "paragraph describing what is to be done").Required()) },
            { "tparam", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.Identifier, "template-parameter-name").Required(), new ArgumentRule(ArgumentKind.ComplexLine, "description").Required()) },
            { "typedef", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.UntilEndOfLine, "typedef declaration").Required()) },
            { "union", new BasicCommandRule(new ArgumentRule(ArgumentKind.Identifier, "name").Optional(), new ArgumentRule(ArgumentKind.HeaderFile, "header-file").Optional(), new ArgumentRule(ArgumentKind.HeaderName, "header-name").Optional()) },
            { "until", new BasicCommandRule(new ArgumentRule(ArgumentKind.UntilEndOfLine, "pattern").Required()) },
            { "var", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.UntilEndOfLine, "variable declaration").Required()) },
            { "verbatim", new StartBlockCommandRule() },
            { "verbinclude", new BasicCommandRule(new ArgumentRule(ArgumentKind.HeaderFile, "file-name").Required()) },
            { "version", new BasicCommandRule(new ArgumentRule(ArgumentKind.ComplexBlock, "version number").Required()) },
            { "vhdlflow", new BasicCommandRule(new ArgumentRule(ArgumentKind.UntilEndOfLine, "title for the flow chart").Optional()) },
            { "warning", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.ComplexBlock, "warning message").Required()) },
            { "weakgroup", new BasicCommandRule(new ArgumentRule(ArgumentKind.Identifier, "name").Required(), new ArgumentRule(ArgumentKind.UntilEndOfLine, "title").Optional()) },
            { "xmlonly", new StartBlockCommandRule() },
            { "xrefitem", new ParagraphCommandRule(new ArgumentRule(ArgumentKind.Identifier, "key").Required(), new ArgumentRule(ArgumentKind.QuotedString, "heading").Required(), new ArgumentRule(ArgumentKind.QuotedString, "list title").Required(), new ArgumentRule(ArgumentKind.ComplexBlock, "text").Required()) },
            { "$", new EscapeCommandRule() },
            { "@", new EscapeCommandRule() },
            { "\\", new EscapeCommandRule() },
            { "~", new BasicCommandRule(new ArgumentRule(ArgumentKind.Identifier, "LanguageId", "", "", ArgumentFlags.DirectlyAfterCommand).Required()) },
            { "<", new EscapeCommandRule() },
            { "=", new EscapeCommandRule() },
            { ">", new EscapeCommandRule() },
            { "#", new EscapeCommandRule() },
            { "\"", new EscapeCommandRule() },
            { "::", new EscapeCommandRule() },
            { "|", new EscapeCommandRule() },
            { "--", new EscapeCommandRule() },
            { "---", new EscapeCommandRule() },
        };

        public static HashSet<char> SpecialCommandStartChars = new HashSet<char>()
        {
            '$','@','\\','~','<','=','>','#','"',':','|','-','{','}'
        };

        private static readonly Dictionary<string, CommandRule> EquivalentCommandMap = new Dictionary<string, CommandRule>()
        {
            { "e", CommandRules["a"] },
            { "em", CommandRules["a"] },
            { "li", CommandRules["arg"] },
            { "authors", CommandRules["author"] },
            { "p", CommandRules["c"] },
            { "related", CommandRules["relates"] },
            { "relatedalso", CommandRules["relatesalso"] },
            { "remarks", CommandRules["remark"] },
            { "result", CommandRules["return"] },
            { "returns", CommandRules["return"] },
            { "see", CommandRules["sa"] },
            { "short", CommandRules["brief"] },
            { "throws", CommandRules["throw"] },
        };

        public static CommandRule GetCommandRule(string commandName)
        {
            if (EquivalentCommandMap.ContainsKey(commandName))
                return EquivalentCommandMap[commandName];
            else
                return CommandRules.ContainsKey(commandName) ? CommandRules[commandName] : null;
        }

        public static bool IsCommandBegin(char c)
        {
            bool result = c == '@' || c == '\\';
            return (result);
        }
        public static bool IsCommandIdentStart(char c)
        {
            bool result = SyntaxUtils.IsIdentStart(c) || (c == '{' || c == '}');
            return (result);
        }
        public static bool IsCommandIdent(char c)
        {
            bool result = SyntaxUtils.IsIdentPart(c);
            return (result);
        }
    }
}
