using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DoxygenEditor.Lexer
{
    class DoxygenLexer : IDoxygenLexer
    {
        private readonly ScintillaNET.Scintilla _editor;

        [Flags]
        enum DoxyStyle : int
        {
            None = -1,
            Default = 0,
            DoxyMultiLineComment,
            DoxySingleLineComment,
            DoxyCommandIdentifier,
            DoxyCommandName,
            DoxyCommandCaption,
            DoxyCodeType,
            HtmlTagOp,
            HtmlTagIdent,
            HtmlAttrName,
            HtmlAttrOp,
            HtmlAttrQuote,
            HtmlAttrValue,
            C99Keyword,
            C99SingleLineComment,
            C99DataType,
            C99MacroGeneral,
            C99MacroInclude,
        }

        class StyleCommand
        {
            public int Start { get; }
            public int Length { get; }
            public int Style { get; }
            public StyleCommand(int start, int length, int style)
            {
                Start = start;
                Length = length;
                Style = style;
            }
        }

        class StyleCommands
        {
            private readonly List<StyleCommand> _styles = new List<StyleCommand>();
            public IEnumerable<StyleCommand> Styles { get { return _styles; } }
            public void Add(StyleCommand style)
            {
                _styles.Add(style);
            }
        }

        class Range
        {
            private readonly ScintillaNET.Scintilla _editor;
            public int Start { get; }
            public int Length { get; }
            public string Text
            {
                get
                {
                    return _editor.GetTextRange(Start, Length);
                }
            }

            public int End
            {
                get
                {
                    if (Length > 0)
                        return Start + (Length - 1);
                    return Start;
                }
            }

            public Range(ScintillaNET.Scintilla editor, int start, int len)
            {
                _editor = editor;
                Start = start;
                Length = len;
            }

            public bool IntersectsWith(Range other)
            {
                if (End < other.Start)
                    return (false);
                if (Start > other.End)
                    return (false);
                return (true);
            }

            public void PushStyle(StyleCommands commands, int style)
            {
                commands.Add(new StyleCommand(Start, Length, style));
            }
            public void PushStyle(StyleCommands commands, DoxyStyle style)
            {
                PushStyle(commands, (int)style);
            }

            public override string ToString()
            {
                return $"{Start} to {End}";
            }
        }

        class RangeGroup
        {
            private readonly List<Range> _ranges = new List<Range>();
            public IList<Range> Ranges { get { return _ranges; } }
            public void Add(Range range)
            {
                _ranges.Add(range);
            }

            public override string ToString()
            {
                if (_ranges.Count > 0)
                    return _ranges[0].ToString();
                else
                    return null;
            }
        }

        public DoxygenLexer(ScintillaNET.Scintilla editor)
        {
            _editor = editor;
        }

        private ScintillaNET.Style GetStyle(DoxyStyle style)
        {
            int idx = (int)style;
            return _editor.Styles[idx];
        }

        private readonly static string BLANKS = "(?:(?!\\n)\\s+)";
        private readonly static string IDENTIFIER = "[\\w\\.\\-]";

        private readonly Regex _rexDoxyMultiLineComment = new Regex("\\/\\*\\!(.*?)\\*\\/", RegexOptions.Singleline | RegexOptions.Compiled);
        private readonly Regex _rexDoxySingleLineComment = new Regex("\\/\\/([^\n]+)?", RegexOptions.Singleline | RegexOptions.Compiled);
        private readonly Regex _rexDoxyCommand = new Regex($"(?:(@ref){BLANKS}?({IDENTIFIER}+(?:\\(\\))?)?)|(?:(@subpage){BLANKS}?({IDENTIFIER}+)?{BLANKS}?(\\\"[^\"]+\\\")?)|(?:(@code)({"\\{([^}]+)\\}"})?)|(?:(@{IDENTIFIER}+){BLANKS}?({IDENTIFIER}+)?{BLANKS}?((?:\"([^\"]+)\")|(?:[^\\n^@]+\\n))?)", RegexOptions.Compiled);
        private readonly Regex _rexHtmlTag = new Regex($"(</?)({IDENTIFIER}+)((?:\\s+{IDENTIFIER}+(?:\\s*=\\s*(?:\".*?\"|'.*?'|[\\^'\" >\\s]+))?)+\\s*|\\s*)(/?>)", RegexOptions.Compiled);
        private readonly Regex _rexHtmlAttribs = new Regex($"({IDENTIFIER}+)(\\=)?((?:(\")([^\"]+)(\"))|(?:(')([^']+)(')))?", RegexOptions.Compiled);
        private readonly Regex _rexCodeBlock = new Regex("@code(\\{[.\\w]+\\})?(?:[\\n\\r]+)?(.+?(?=@endcode))", RegexOptions.Compiled | RegexOptions.Singleline);
        private readonly Regex _rexC99SingleLineComment = new Regex("\\/\\/([^\n]+)?", RegexOptions.Singleline | RegexOptions.Compiled);
        private readonly Regex _rexC99MacroGeneral = new Regex("(#.*?)(?<!\\\\)\\n", RegexOptions.Singleline | RegexOptions.Compiled);
        private readonly Regex _rexC99MacroInclude = new Regex("(#\\s*)include\\s(\\\".*\\\"|\\<.*\\>)", RegexOptions.Compiled);

        private static HashSet<string> CKeywords = new HashSet<string>{
            "auto",
            "break",
            "case",
            "char",
            "const",
            "continue",
            "default",
            "do",
            "double",
            "else",
            "enum",
            "extern",
            "float",
            "for",
            "goto",
            "if",
            "inline",
            "int",
            "long",
            "register",
            "restrict",
            "return",
            "short",
            "signed",
            "sizeof",
            "static",
            "struct",
            "switch",
            "typedef",
            "union",
            "unsigned",
            "void",
            "volatile",
            "while",
            "_Bool",
            "bool",
            "_Complex",
            "complex",
            "_Imaginary",
            "imaginary",
            "_Alignas",
            "alignas",
            "_Alignof",
            "alignof",
        };
        private Regex _cKeywordRex;
        private Regex C99KeywordRex
        {
            get
            {
                if (_cKeywordRex == null)
                {
                    StringBuilder pattern = new StringBuilder();
                    pattern.Append("\\b(");
                    pattern.Append(string.Join("|", CKeywords));
                    pattern.Append(")\\b");
                    _cKeywordRex = new Regex(pattern.ToString(), RegexOptions.Compiled);
                }
                return (_cKeywordRex);
            }
        }

        private static HashSet<string> CSimpleDataTypes = new HashSet<string>
        {
            "unsigned char",
            "signed char",
            "uint8_t",
            "int8_t",
            "char",

            "unsigned short",
            "signed short",
            "uint16_t",
            "int16_t",
            "short",

            "unsigned int",
            "signed int",
            "uint32_t",
            "int32_t",
            "int",

            "unsigned long long",
            "signed long long",
            "unsigned long",
            "signed long",
            "long long",
            "uint64_t",
            "int64_t",
            "long",

            "uintptr_t",
            "intptr_t",

            "float",
            "double",

            "ssize_t",
            "size_t",
        };
        private Regex _c99SimpleDataTypeRex;
        private Regex C99SimpleDataTypeRex
        {
            get
            {
                if (_c99SimpleDataTypeRex == null)
                {
                    StringBuilder pattern = new StringBuilder();
                    pattern.Append("\\b(");
                    pattern.Append(string.Join("|", CSimpleDataTypes));
                    pattern.Append(")\\b");
                    _c99SimpleDataTypeRex = new Regex(pattern.ToString(), RegexOptions.Compiled);
                }
                return (_c99SimpleDataTypeRex);
            }
        }


        public void Init()
        {
            var defaultStyle = GetStyle(DoxyStyle.Default);
            defaultStyle.ForeColor = Color.Black;
            defaultStyle.Bold = false;

            var doxyBlockCommentStyle = GetStyle(DoxyStyle.DoxyMultiLineComment);
            doxyBlockCommentStyle.ForeColor = Color.Purple;
            doxyBlockCommentStyle.Bold = false;

            var doxySingleLineCommentStyle = GetStyle(DoxyStyle.DoxySingleLineComment);
            doxySingleLineCommentStyle.ForeColor = Color.Gray;

            var doxyCommandIdentStyle = GetStyle(DoxyStyle.DoxyCommandIdentifier);
            doxyCommandIdentStyle.ForeColor = Color.Red;
            doxyCommandIdentStyle.Bold = true;

            var doxyCommandNameStyle = GetStyle(DoxyStyle.DoxyCommandName);
            doxyCommandNameStyle.ForeColor = Color.Blue;
            doxyCommandNameStyle.Bold = true;

            var doxyCommandCaptionStyle = GetStyle(DoxyStyle.DoxyCommandCaption);
            doxyCommandCaptionStyle.ForeColor = Color.Green;
            doxyCommandCaptionStyle.Bold = false;

            var doxyCodeTypeStyle = GetStyle(DoxyStyle.DoxyCodeType);
            doxyCodeTypeStyle.ForeColor = Color.DarkOrange;
            doxyCodeTypeStyle.Bold = true;

            var htmlTagOpStyle = GetStyle(DoxyStyle.HtmlTagOp);
            htmlTagOpStyle.ForeColor = Color.Blue;
            htmlTagOpStyle.Bold = false;

            var htmlTagIdentStyle = GetStyle(DoxyStyle.HtmlTagIdent);
            htmlTagIdentStyle.ForeColor = Color.Blue;
            htmlTagIdentStyle.Bold = false;

            var htmlAttrNameStyle = GetStyle(DoxyStyle.HtmlAttrName);
            htmlAttrNameStyle.ForeColor = Color.Red;
            htmlAttrNameStyle.Bold = false;

            var htmlAttrValueStyle = GetStyle(DoxyStyle.HtmlAttrValue);
            htmlAttrValueStyle.ForeColor = Color.BlueViolet;
            htmlAttrValueStyle.Bold = true;

            var htmlAttrOpStyle = GetStyle(DoxyStyle.HtmlAttrOp);
            htmlAttrOpStyle.ForeColor = Color.Black;
            htmlAttrOpStyle.Bold = false;

            var htmlAttrQuoteStyle = GetStyle(DoxyStyle.HtmlAttrQuote);
            htmlAttrQuoteStyle.ForeColor = Color.BlueViolet;
            htmlAttrQuoteStyle.Bold = true;

            var c99keywordStyle = GetStyle(DoxyStyle.C99Keyword);
            c99keywordStyle.ForeColor = Color.Blue;
            c99keywordStyle.Bold = false;

            var c99SimpleDataTypeStyle = GetStyle(DoxyStyle.C99DataType);
            c99SimpleDataTypeStyle.ForeColor = Color.BlueViolet;
            c99SimpleDataTypeStyle.Bold = false;

            var c99SingleLineCommentStyle = GetStyle(DoxyStyle.C99SingleLineComment);
            c99SingleLineCommentStyle.ForeColor = Color.Green;
            c99SingleLineCommentStyle.Bold = false;

            var c99MacroGeneralStyle = GetStyle(DoxyStyle.C99MacroGeneral);
            c99MacroGeneralStyle.ForeColor = Color.DimGray;
            c99MacroGeneralStyle.Bold = true;

            var c99MacroIncludeStyle = GetStyle(DoxyStyle.C99MacroInclude);
            c99MacroIncludeStyle.ForeColor = Color.Red;
            c99MacroIncludeStyle.Bold = true;
        }

        private IEnumerable<Range> MatchRanges(int startPos, int length, Regex rex)
        {
            List<Range> result = new List<Range>();
            string text = _editor.GetTextRange(startPos, length);
            MatchCollection matches = rex.Matches(text);
            foreach (Match match in matches)
                result.Add(new Range(_editor, startPos + match.Index, match.Length));
            return (result);
        }
        private IEnumerable<Range> MatchRanges(Range range, Regex rex)
        {
            var result = MatchRanges(range.Start, range.Length, rex);
            return (result);
        }
        private IEnumerable<RangeGroup> MatchGroupRanges(int startPos, int length, Regex rex)
        {
            List<RangeGroup> result = new List<RangeGroup>();
            string text = _editor.GetTextRange(startPos, length);
            MatchCollection matches = rex.Matches(text);
            foreach (Match match in matches)
            {
                RangeGroup rangeGrp = new RangeGroup();
                foreach (Group group in match.Groups)
                    rangeGrp.Add(new Range(_editor, startPos + group.Index, group.Length));
                result.Add(rangeGrp);
            }
            return (result);
        }
        private IEnumerable<RangeGroup> MatchGroupRanges(Range range, Regex rex)
        {
            var result = MatchGroupRanges(range.Start, range.Length, rex);
            return (result);
        }

        private void StyleHtmlBlock(StyleCommands commands, Range blockRange)
        {
            var tagGroups = MatchGroupRanges(blockRange, _rexHtmlTag);
            foreach (var tagGroup in tagGroups)
            {
                tagGroup.Ranges[1].PushStyle(commands, DoxyStyle.HtmlTagOp);
                tagGroup.Ranges[2].PushStyle(commands, DoxyStyle.HtmlTagIdent);
                var attrRange = tagGroup.Ranges[3];
                var attrGroups = MatchGroupRanges(attrRange, _rexHtmlAttribs);
                foreach (var attrGroup in attrGroups)
                {
                    attrGroup.Ranges[1].PushStyle(commands, DoxyStyle.HtmlAttrName);
                    attrGroup.Ranges[2].PushStyle(commands, DoxyStyle.HtmlAttrOp);
                    attrGroup.Ranges[4].PushStyle(commands, DoxyStyle.HtmlAttrQuote);
                    attrGroup.Ranges[5].PushStyle(commands, DoxyStyle.HtmlAttrValue);
                    attrGroup.Ranges[6].PushStyle(commands, DoxyStyle.HtmlAttrQuote);
                }
                tagGroup.Ranges[4].PushStyle(commands, DoxyStyle.HtmlTagOp);
            }
        }

        private void StyleCodeBlock(StyleCommands commands, Range codeRange, string codeType)
        {
            if ("{.c}".Equals(codeType, StringComparison.InvariantCultureIgnoreCase))
            {
                var keywords = MatchRanges(codeRange, C99KeywordRex);
                foreach (var keyword in keywords)
                    keyword.PushStyle(commands, DoxyStyle.C99Keyword);

                var simpleDataTypes = MatchRanges(codeRange, C99SimpleDataTypeRex);
                foreach (var simpleDataType in simpleDataTypes)
                    simpleDataType.PushStyle(commands, DoxyStyle.C99DataType);

                var macros = MatchGroupRanges(codeRange, _rexC99MacroGeneral);
                foreach (var macro in macros)
                {
                    var macroRange = macro.Ranges[1];
                    macroRange.PushStyle(commands, DoxyStyle.C99MacroGeneral);

                    var includes = MatchGroupRanges(macroRange, _rexC99MacroInclude);
                    foreach (var include in includes)
                        include.Ranges[2].PushStyle(commands, DoxyStyle.C99MacroInclude);
                }

                var singleLineComments = MatchRanges(codeRange, _rexC99SingleLineComment);
                foreach (var singleLineComment in singleLineComments)
                    singleLineComment.PushStyle(commands, DoxyStyle.C99SingleLineComment);
            }
        }

        private void StyleDoxygenBlock(StyleCommands commands, Range blockRange)
        {
            StyleHtmlBlock(commands, blockRange);
            var commandGroups = MatchGroupRanges(blockRange, _rexDoxyCommand);
            foreach (var commandGroup in commandGroups)
            {
                string commandName = commandGroup.Ranges[0].Text;
                if (commandName.StartsWith("@ref"))
                {
                    commandGroup.Ranges[1].PushStyle(commands, DoxyStyle.DoxyCommandIdentifier);
                    commandGroup.Ranges[2].PushStyle(commands, DoxyStyle.DoxyCommandName);
                }
                else if (commandName.StartsWith("@subpage"))
                {
                    commandGroup.Ranges[3].PushStyle(commands, DoxyStyle.DoxyCommandIdentifier);
                    commandGroup.Ranges[4].PushStyle(commands, DoxyStyle.DoxyCommandName);
                    commandGroup.Ranges[5].PushStyle(commands, DoxyStyle.DoxyCommandCaption);
                }
                else if (commandName.StartsWith("@code"))
                {
                    commandGroup.Ranges[6].PushStyle(commands, DoxyStyle.DoxyCommandIdentifier);
                    commandGroup.Ranges[7].PushStyle(commands, DoxyStyle.DoxyCodeType);
                }
                else if (commandName.StartsWith("@note") || commandName.StartsWith("@warning") || commandName.StartsWith("@attention") || commandName.StartsWith("@headerfile"))
                {
                    commandGroup.Ranges[9].PushStyle(commands, DoxyStyle.DoxyCommandIdentifier);
                    commandGroup.Ranges[10].PushStyle(commands, DoxyStyle.DoxyCommandCaption);
                }
                else
                {
                    commandGroup.Ranges[9].PushStyle(commands, DoxyStyle.DoxyCommandIdentifier);
                    commandGroup.Ranges[10].PushStyle(commands, DoxyStyle.DoxyCommandName);
                    commandGroup.Ranges[11].PushStyle(commands, DoxyStyle.DoxyCommandCaption);
                }
                var codeBlocks = MatchGroupRanges(blockRange, _rexCodeBlock);
                foreach (var codeBlock in codeBlocks)
                {
                    string codeType = codeBlock.Ranges[1].Text.Trim();
                    StyleCodeBlock(commands, codeBlock.Ranges[2], codeType);
                }
                
            }
            
        }

        private void StyleBlocks(StyleCommands commands, IEnumerable<Range> blockRanges)
        {
            foreach (var blockRange in blockRanges)
            {
                blockRange.PushStyle(commands, DoxyStyle.Default);

                // Style begin of comment block
                Range beginRange = new Range(_editor, blockRange.Start, "/*!".Length);
                beginRange.PushStyle(commands, DoxyStyle.DoxyMultiLineComment);

                // Clear style of inner range
                Range innerRange = new Range(_editor, blockRange.Start + "/*!".Length, blockRange.Length - "/*!*/".Length);
                innerRange.PushStyle(commands, DoxyStyle.Default);
                StyleDoxygenBlock(commands, innerRange);

                // Style end of comment block
                Range endRange = new Range(_editor, blockRange.End - 1, "*/".Length);
                endRange.PushStyle(commands, DoxyStyle.DoxyMultiLineComment);
            }
        }

        private IEnumerable<Range> GetOutsideBlocks(IEnumerable<Range> insideBlocks)
        {
            List<Range> result = new List<Range>();
            if (insideBlocks.Count() == 0)
                result.Add(new Range(_editor, 0, _editor.TextLength));
            else
            {
                // First
                var firstRange = insideBlocks.First();
                int currentStart = 0;
                int currentEnd = firstRange.Start;
                if (currentEnd > currentStart)
                {
                    int l = (currentEnd - currentStart) + 1;
                    result.Add(new Range(_editor, 0, l));
                }

                // @TODO(final): Implement this properly

                foreach (var range in insideBlocks)
                {
                    if (currentEnd == -1)
                    {
                    }
                }
            }
            return (result);
        }

        public void Style(int rangeStartPos, int rangeEndPos)
        {
            Debug.Assert(_editor.TextLength > 0);
            Debug.Assert(_editor.Lines.Count > 0);

            StyleCommands commands = new StyleCommands();

            Range range = new Range(_editor, rangeStartPos, rangeEndPos - rangeStartPos);
            range.PushStyle(commands, DoxyStyle.Default);

            int startLineIndex = _editor.LineFromPosition(rangeStartPos);
            int endLineIndex = _editor.LineFromPosition(rangeEndPos);
            int startLinePos = _editor.Lines[startLineIndex].Position;
            int endLinePos = _editor.Lines[endLineIndex].Position;
            Debug.Assert(endLinePos >= startLinePos);

            // MultiLine-Comment blocks
            var allBlockRanges = MatchRanges(0, _editor.TextLength, _rexDoxyMultiLineComment);

            // SingleLine-Comments
            var outsideBlocks = GetOutsideBlocks(allBlockRanges);
            foreach (var outsideBlock in outsideBlocks)
            {
                var commentRanges = MatchRanges(outsideBlock, _rexDoxySingleLineComment);
                foreach (var commentRange in commentRanges)
                    commentRange.PushStyle(commands, DoxyStyle.DoxySingleLineComment);
            }

            // Intersection blocks from all MultiLine-Comment blocks
            var intersectingBlockRanges = allBlockRanges.Where(b => b.IntersectsWith(range));
            if (intersectingBlockRanges.Count() > 0)
                StyleBlocks(commands, intersectingBlockRanges);

            // Execute style commands in order
            foreach (var command in commands.Styles)
            {
                _editor.StartStyling(command.Start);
                _editor.SetStyling(command.Length, command.Style);
            }
        }
    }
}
