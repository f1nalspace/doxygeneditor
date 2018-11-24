using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DoxygenEditor
{
    class DoxygenLexer
    {
        private readonly ScintillaNET.Scintilla _editor;

        [Flags]
        enum DoxyStyle : int
        {
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
                if ((other.Start >= Start) && (other.Start <= End))
                    return true;
                if ((other.End >= Start) && (other.End <= End))
                    return true;
                return false;
            }

            public void SetStyle(int style)
            {
                _editor.StartStyling(Start);
                _editor.SetStyling(Length, style);
            }
            public void SetStyle(DoxyStyle style)
            {
                SetStyle((int)style);
            }

            public override string ToString()
            {
                return $"{Start} to {End}: {Text}";
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
        private readonly static string IDENTIFIER = "\\w";

        private readonly Regex _rexDoxyMultiLineComment = new Regex("\\/\\*\\!(.*?)\\*\\/", RegexOptions.Singleline | RegexOptions.Compiled);
        private readonly Regex _rexDoxySingleLineComment = new Regex("\\/\\/([^\n]+)?", RegexOptions.Singleline | RegexOptions.Compiled);
        private readonly Regex _rexDoxyCommand = new Regex($"(?:(@ref){BLANKS}?({IDENTIFIER}+(?:\\(\\))?)?)|(?:(@subpage){BLANKS}?({IDENTIFIER}+)?{BLANKS}?(\\\"[^\"]+\\\")?)|(?:(@code)({"\\{([^}]+)\\}"})?)|(?:(@{IDENTIFIER}+){BLANKS}?({IDENTIFIER}+)?{BLANKS}?((?:\"([^\"]+)\")|(?:[^\\n^@]+\\n))?)", RegexOptions.Compiled);
        private readonly Regex _rexHtmlTag = new Regex($"(</?)({IDENTIFIER}+)((?:\\s+{IDENTIFIER}+(?:\\s*=\\s*(?:\".*?\"|'.*?'|[\\^'\" >\\s]+))?)+\\s*|\\s*)(/?>)", RegexOptions.Compiled);
        private readonly Regex _rexHtmlAttribs = new Regex($"({IDENTIFIER}+)(\\=)?((?:(\")([^\"]+)(\"))|(?:(')([^']+)(')))?", RegexOptions.Compiled);

        public void InitStyles()
        {
            var defaultStyle = GetStyle(DoxyStyle.Default);
            defaultStyle.ForeColor = Color.Black;

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
        }

        private int FindStartOfLine(string search, int startIndex, int increment)
        {
            if (increment < 0)
            {
                // Backwards search
                for (int i = startIndex; i > 0; i -= increment)
                {
                    string lineText = _editor.Lines[i].Text.TrimStart();
                    if (lineText.StartsWith(search))
                        return i;
                }
            }
            else if (increment > 0)
            {
                // Forward search
                for (int i = startIndex; i < _editor.Lines.Count; i += increment)
                {
                    string lineText = _editor.Lines[i].Text.TrimStart();
                    if (lineText.StartsWith(search))
                        return i;
                }
            }
            return -1;
        }

        private IEnumerable<Range> MatchRanges(int startPos, int length, Regex rex)
        {
            List<Range> result = new List<Range>();
            string text = _editor.GetTextRange(startPos, length);
            var matches = rex.Matches(text);
            foreach (Match match in matches)
                result.Add(new Range(_editor, startPos + match.Index, match.Length));
            return (result);
        }
        private IEnumerable<RangeGroup> MatchGroupRanges(int startPos, int length, Regex rex)
        {
            List<RangeGroup> result = new List<RangeGroup>();
            string text = _editor.GetTextRange(startPos, length);
            var matches = rex.Matches(text);
            foreach (Match match in matches)
            {
                RangeGroup rangeGrp = new RangeGroup();
                foreach (Group group in match.Groups)
                    rangeGrp.Add(new Range(_editor, startPos + group.Index, group.Length));
                result.Add(rangeGrp);
            }
            return (result);
        }

        private void StyleHtmlBlock(Range blockRange)
        {
            var tagGroups = MatchGroupRanges(blockRange.Start, blockRange.Length, _rexHtmlTag);
            foreach (var tagGroup in tagGroups)
            {
                tagGroup.Ranges[1].SetStyle(DoxyStyle.HtmlTagOp);
                tagGroup.Ranges[2].SetStyle(DoxyStyle.HtmlTagIdent);
                var attrRange = tagGroup.Ranges[3];
                var attrGroups = MatchGroupRanges(attrRange.Start, attrRange.Length, _rexHtmlAttribs);
                foreach (var attrGroup in attrGroups)
                {
                    attrGroup.Ranges[1].SetStyle(DoxyStyle.HtmlAttrName);
                    attrGroup.Ranges[2].SetStyle(DoxyStyle.HtmlAttrOp);
                    attrGroup.Ranges[4].SetStyle(DoxyStyle.HtmlAttrQuote);
                    attrGroup.Ranges[5].SetStyle(DoxyStyle.HtmlAttrValue);
                    attrGroup.Ranges[6].SetStyle(DoxyStyle.HtmlAttrQuote);
                }
                tagGroup.Ranges[4].SetStyle(DoxyStyle.HtmlTagOp);
            }
        }

        private void StyleDoxygenBlock(Range blockRange)
        {
            StyleHtmlBlock(blockRange);
            var commandGroups = MatchGroupRanges(blockRange.Start, blockRange.Length, _rexDoxyCommand);
            foreach (var commentGroup in commandGroups)
            {
                if (commentGroup.Ranges[0].Text.StartsWith("@ref"))
                {
                    commentGroup.Ranges[1].SetStyle(DoxyStyle.DoxyCommandIdentifier);
                    commentGroup.Ranges[2].SetStyle(DoxyStyle.DoxyCommandName);
                }
                else if (commentGroup.Ranges[0].Text.StartsWith("@subpage"))
                {
                    commentGroup.Ranges[3].SetStyle(DoxyStyle.DoxyCommandIdentifier);
                    commentGroup.Ranges[4].SetStyle(DoxyStyle.DoxyCommandName);
                    commentGroup.Ranges[5].SetStyle(DoxyStyle.DoxyCommandCaption);
                }
                else if (commentGroup.Ranges[0].Text.StartsWith("@code"))
                {
                    commentGroup.Ranges[6].SetStyle(DoxyStyle.DoxyCommandIdentifier);
                    commentGroup.Ranges[7].SetStyle(DoxyStyle.DoxyCodeType);
                }
                else
                {
                    commentGroup.Ranges[9].SetStyle(DoxyStyle.DoxyCommandIdentifier);
                    commentGroup.Ranges[10].SetStyle(DoxyStyle.DoxyCommandName);
                    commentGroup.Ranges[11].SetStyle(DoxyStyle.DoxyCommandCaption);
                }
            }
        }

        private void StyleBlocks(IEnumerable<Range> blockRanges)
        {
            foreach (var blockRange in blockRanges)
            {
                blockRange.SetStyle(DoxyStyle.Default);

                // Style begin of comment block
                Range beginRange = new Range(_editor, blockRange.Start, "/*!".Length);
                beginRange.SetStyle(DoxyStyle.DoxyMultiLineComment);

                // Clear style of inner range
                Range innerRange = new Range(_editor, blockRange.Start + "/*!".Length, blockRange.Length - "/*!*/".Length);
                innerRange.SetStyle(DoxyStyle.Default);
                StyleDoxygenBlock(innerRange);

                // Style end of comment block
                Range endRange = new Range(_editor, blockRange.End - 1, "*/".Length);
                endRange.SetStyle(DoxyStyle.DoxyMultiLineComment);
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

            Range range = new Range(_editor, rangeStartPos, rangeEndPos - rangeStartPos);

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
                var commentRanges = MatchRanges(outsideBlock.Start, outsideBlock.Length, _rexDoxySingleLineComment);
                foreach (var commentRange in commentRanges)
                    commentRange.SetStyle(DoxyStyle.DoxySingleLineComment);
            }

            // Intersection blocks from all MultiLine-Comment blocks
            var intersectingBlockRanges = allBlockRanges.Where(b => b.IntersectsWith(range));
            if (intersectingBlockRanges.Count() > 0)
                StyleBlocks(intersectingBlockRanges);
            else
                StyleBlocks(allBlockRanges);
        }
    }
}
