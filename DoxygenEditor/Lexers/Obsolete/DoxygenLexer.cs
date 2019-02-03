using DoxygenEditor.Parsers;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DoxygenEditor.Lexers.Obsolete
{
    static class DoxygenLexer
    {
        // @TODO(final): Make doxygen-keywords configurable
        public static readonly HashSet<string> DoxygenKeywords = new HashSet<string>{
            "attention",
            "author",
            "brief",
            "code",
            "code{.c}",
            "code{.cpp}",
            "cond",
            "defgroup",
            "endcode",
            "endcond",
            "file",
            "mainpage",
            "note",
            "page",
            "param",
            "ref",
            "return",
            "section",
            "see",
            "subpage",
            "subsection",
            "tableofcontents",
            "todo",
            "version",
            "warning",
            "{",
            "}"
        };

        private const int DoxygenDefaultStyle = 30;
        private const int DoxygenCommandStyle = 31;
        private const int DoxygenIdentStyle = 32;

        private static void Init(Scintilla editor)
        {
            editor.Styles[DoxygenDefaultStyle].ForeColor = Color.Black;
            editor.Styles[DoxygenCommandStyle].ForeColor = Color.Blue;
            editor.Styles[DoxygenIdentStyle].ForeColor = Color.Green;

            editor.Styles[Style.Html.Default].ForeColor = Color.Black;
            editor.Styles[Style.Html.Tag].ForeColor = Color.Blue;
        }

        private static Regex DoxygenCommandRex
        {
            get
            {
                StringBuilder pattern = new StringBuilder();
                pattern.Append("([@\\\\](?:");
                pattern.Append(string.Join("|", DoxygenKeywords.OrderBy(f => f.Length).Select(f => f.Replace("{", "\\{").Replace("}", "\\}"))));
                pattern.Append("))\\s*([a-zA-Z0-9_]+)?");
                Regex result = new Regex(pattern.ToString(), RegexOptions.Multiline);
                return (result);
            }
        }

        private static void LexBlock(Scintilla editor, int startPos, int endPos)
        {
            int textLength = (endPos - startPos) + 1;
            string text = editor.GetTextRange(startPos, textLength);

            // Default doxygen style
            editor.Lexer = Lexer.Null;
            editor.StartStyling(startPos);
            editor.SetStyling(textLength, DoxygenDefaultStyle);

            // Highlight html
            editor.Lexer = Lexer.Html;
            editor.Colorize(startPos, endPos);

            // Highlight doxygen
            editor.Lexer = Lexer.Null;


            // Commands
            IEnumerable<Match> matches = RexUtils.GetMatches(text, DoxygenCommandRex);
            foreach (Match match in matches)
            {
                editor.StartStyling(startPos + match.Groups[1].Index);
                editor.SetStyling(match.Groups[1].Length, DoxygenCommandStyle);

                string command = match.Groups[1].Value.Substring(1);
                if ("page".Equals(command, StringComparison.InvariantCultureIgnoreCase) ||
                    "section".Equals(command, StringComparison.InvariantCultureIgnoreCase) ||
                    "subsection".Equals(command, StringComparison.InvariantCultureIgnoreCase) ||
                    "subpage".Equals(command, StringComparison.InvariantCultureIgnoreCase)
                    )
                {
                    if (match.Groups.Count >= 3)
                    {
                        editor.StartStyling(startPos + match.Groups[2].Index);
                        editor.SetStyling(match.Groups[2].Length, DoxygenIdentStyle);
                    }
                }
            }
        }

        private static Regex DoxygenCommentBlockRex = new Regex("(\\/\\*\\!)(.*?)(\\*\\/)", RegexOptions.Singleline | RegexOptions.Compiled);
        private static Regex DoxygenCodeBlockRex = new Regex("(\\/\\*\\!)(.*?)(\\*\\/)", RegexOptions.Singleline | RegexOptions.Compiled);

        private static void DoxyColorize(Scintilla editor, int startPos, int endPos)
        {

        }

        public static void Lex(Scintilla editor, int startPos, int endPos)
        {
            // Highlight C++
            editor.Lexer = Lexer.Cpp;
            CppScintillaLexer.InitStyles(editor);
            editor.Colorize(startPos, endPos);

            // Find all intersecting comment matches + Convert matches into SequenceInfo's
            int rangeLen = (endPos - startPos) + 1;
            SequenceInfo textRange = new SequenceInfo() { Start = startPos, Length = rangeLen };
            string fullText = editor.Text;
            IEnumerable<Match> allCommentMatches = RexUtils.GetMatches(fullText, DoxygenCommentBlockRex);
            List<SequenceInfo> allCommentSequences = new List<SequenceInfo>();
            foreach (Match match in allCommentMatches)
                allCommentSequences.Add(new SequenceInfo() { Start = match.Index, Length = match.Length });
            IEnumerable<SequenceInfo> commentIntersections = allCommentSequences.Where(f => f.IntersectsWith(textRange));

            foreach (SequenceInfo commentBlock in commentIntersections)
            {
                // Highlight HTML for comment block
                editor.Lexer = Lexer.Html;
                HtmlScintillaLexer.InitStyles(editor);
                editor.Colorize(commentBlock.Start, commentBlock.End);

                // Highlight Doxygen (Custom + Styles)
                editor.Lexer = Lexer.Null;
                DoxyColorize(editor, commentBlock.Start, commentBlock.End);

                // Find doxygen code blocks for C or C++
                string commentText = fullText.Substring(commentBlock.Start, commentBlock.Length);
                IEnumerable<Match> codeBlockMatches = RexUtils.GetMatches(commentText, DoxygenCodeBlockRex);
                foreach (Match codeBlockMatch in codeBlockMatches)
                {
                    // Highlight C++ for code block
                    SequenceInfo codeBlock = new SequenceInfo() { Start = commentBlock.Start + codeBlockMatch.Index, Length = codeBlockMatch.Length };
                    editor.Lexer = Lexer.Cpp;
                    CppScintillaLexer.InitStyles(editor);
                    editor.Colorize(codeBlock.Start, codeBlock.End);
                }
            }
        }
    }
}
