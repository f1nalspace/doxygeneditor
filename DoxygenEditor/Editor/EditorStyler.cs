using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using TSP.DoxygenEditor.Languages.Cpp;
using TSP.DoxygenEditor.Languages.Doxygen;
using TSP.DoxygenEditor.Languages.Html;
using TSP.DoxygenEditor.Lexers;

namespace TSP.DoxygenEditor.Editor
{
    class EditorStyler
    {
        static int styleIndex = 1;

        static readonly int cppMultiLineCommentStyle = styleIndex++;
        static readonly int cppMultiLineCommentDocStyle = styleIndex++;
        static readonly int cppSingleLineCommentStyle = styleIndex++;
        static readonly int cppSingleLineCommentDocStyle = styleIndex++;
        static readonly int cppPreprocessorStyle = styleIndex++;
        static readonly int cppReservedKeywordStyle = styleIndex++;
        static readonly int cppTypeKeywordStyle = styleIndex++;
        static readonly int cppStringStyle = styleIndex++;
        static readonly int cppNumberStyle = styleIndex++;

        static readonly Dictionary<CppTokenKind, int> cppTokenTypeToStyleDict = new Dictionary<CppTokenKind, int>() {
            { CppTokenKind.MultiLineComment, cppMultiLineCommentStyle },
            //{ CppTokenKind.MultiLineCommentDoc, cppMultiLineCommentDocStyle },
            { CppTokenKind.SingleLineComment, cppSingleLineCommentStyle },
            //{ CppTokenKind.SingleLineCommentDoc, cppSingleLineCommentDocStyle },

            { CppTokenKind.PreprocessorStart, cppPreprocessorStyle },
            { CppTokenKind.PreprocessorKeyword, cppPreprocessorStyle },

            { CppTokenKind.ReservedKeyword, cppReservedKeywordStyle },
            { CppTokenKind.TypeKeyword, cppTypeKeywordStyle },

            { CppTokenKind.StringLiteral, cppStringStyle },
            { CppTokenKind.CharLiteral, cppStringStyle },
            { CppTokenKind.IntegerLiteral, cppNumberStyle },
            { CppTokenKind.OctalLiteral, cppNumberStyle },
            { CppTokenKind.HexLiteral, cppNumberStyle },
            { CppTokenKind.IntegerFloatLiteral, cppNumberStyle },
            { CppTokenKind.HexadecimalFloatLiteral, cppNumberStyle },
        };

        static int doxygenBlockStyle = styleIndex++;
        static int doxygenCommandStyle = styleIndex++;
        static int doxygenInvalidCommandStyle = styleIndex++;
        static int doxygenIdentStyle = styleIndex++;
        static int doxygenQuoteStringStyle = styleIndex++;
        static int doxygenArgumentStyle = styleIndex++;

        static Dictionary<DoxygenTokenKind, int> doxygenTokenTypeToStyleDict = new Dictionary<DoxygenTokenKind, int>() {
            { DoxygenTokenKind.DoxyBlockStartSingle, doxygenBlockStyle },
            { DoxygenTokenKind.DoxyBlockStartMulti, doxygenBlockStyle },
            { DoxygenTokenKind.DoxyBlockEnd, doxygenBlockStyle },
            { DoxygenTokenKind.DoxyBlockChars, doxygenBlockStyle },
            { DoxygenTokenKind.Command, doxygenCommandStyle },
            { DoxygenTokenKind.InvalidCommand, doxygenInvalidCommandStyle },
            { DoxygenTokenKind.GroupStart, doxygenCommandStyle },
            { DoxygenTokenKind.GroupEnd, doxygenCommandStyle },
            { DoxygenTokenKind.ArgumentIdent, doxygenIdentStyle },
            { DoxygenTokenKind.ArgumentText, doxygenQuoteStringStyle },
            { DoxygenTokenKind.ArgumentCaption, doxygenArgumentStyle },
            { DoxygenTokenKind.ArgumentFile, doxygenArgumentStyle },
            { DoxygenTokenKind.CommandStart, doxygenCommandStyle },
            { DoxygenTokenKind.CommandEnd, doxygenCommandStyle },
        };

        static int htmlTagCharsStyle = styleIndex++;
        static int htmlTagNameStyle = styleIndex++;
        static int htmlAttrNameStyle = styleIndex++;
        static int htmlAttrValueStyle = styleIndex++;

        static Dictionary<HtmlTokenKind, int> htmlTokenTypeToStyleDict = new Dictionary<HtmlTokenKind, int>() {
            { HtmlTokenKind.TagChars, htmlTagCharsStyle },
            { HtmlTokenKind.TagName, htmlTagNameStyle },
            { HtmlTokenKind.AttrName, htmlAttrNameStyle },
            { HtmlTokenKind.AttrValue, htmlAttrValueStyle },
        };

        struct StyleEntry
        {
            public int Index { get; }
            public int Length { get; }
            public int Style { get; }

            public int End
            {
                get
                {
                    int result = Index + Math.Max(0, Length - 1);
                    return (result);
                }
            }

            public StyleEntry(int index, int length, int style)
            {
                Index = index;
                Length = length;
                Style = style;
            }

            public StyleEntry(IBaseToken token, int style) : this(token.Index, token.Length, style)
            {
            }

            public bool InterectsWith(StyleEntry other)
            {
                bool result = (Index <= other.End) && (End >= other.Index);
                return (result);
            }
        }

        private readonly List<StyleEntry> _entries = new List<StyleEntry>();

        public EditorStyler()
        {
        }

        public void Refresh(IEnumerable<IBaseToken> tokens)
        {
            _entries.Clear();
            foreach (var token in tokens)
            {
                if (token.Length == 0) continue;
                if (typeof(CppToken).Equals(token.GetType()))
                {
                    CppToken cppToken = (CppToken)token;
                    if (cppTokenTypeToStyleDict.ContainsKey(cppToken.Kind))
                    {
                        int style = cppTokenTypeToStyleDict[cppToken.Kind];
                        _entries.Add(new StyleEntry(token, style));
                    }
                }
                else if (typeof(DoxygenToken).Equals(token.GetType()))
                {
                    DoxygenToken doxygenToken = (DoxygenToken)token;
                    if (doxygenTokenTypeToStyleDict.ContainsKey(doxygenToken.Kind))
                    {
                        int style = doxygenTokenTypeToStyleDict[doxygenToken.Kind];
                        _entries.Add(new StyleEntry(token, style));
                    }
                }
                else if (typeof(HtmlToken).Equals(token.GetType()))
                {
                    HtmlToken htmlToken = (HtmlToken)token;
                    if (htmlTokenTypeToStyleDict.ContainsKey(htmlToken.Kind))
                    {
                        int style = htmlTokenTypeToStyleDict[htmlToken.Kind];
                        _entries.Add(new StyleEntry(token, style));
                    }
                }
            }
        }

        public void InitStyles(Scintilla editor)
        {
            editor.Styles[cppMultiLineCommentStyle].ForeColor = Color.Green;
            editor.Styles[cppMultiLineCommentDocStyle].ForeColor = Color.Purple;
            editor.Styles[cppSingleLineCommentStyle].ForeColor = Color.Green;
            editor.Styles[cppSingleLineCommentDocStyle].ForeColor = Color.Purple;
            editor.Styles[cppPreprocessorStyle].ForeColor = Color.DarkSlateGray;
            editor.Styles[cppReservedKeywordStyle].ForeColor = Color.Blue;
            editor.Styles[cppTypeKeywordStyle].ForeColor = Color.BlueViolet;
            editor.Styles[cppStringStyle].ForeColor = Color.Green;
            editor.Styles[cppNumberStyle].ForeColor = Color.Red;

            editor.Styles[doxygenBlockStyle].ForeColor = Color.DarkViolet;
            editor.Styles[doxygenCommandStyle].ForeColor = Color.Red;
            editor.Styles[doxygenInvalidCommandStyle].ForeColor = Color.Red;
            editor.Styles[doxygenInvalidCommandStyle].Underline = true;
            editor.Styles[doxygenIdentStyle].ForeColor = Color.Blue;
            editor.Styles[doxygenQuoteStringStyle].ForeColor = Color.Green;
            editor.Styles[doxygenArgumentStyle].ForeColor = Color.Orange;

            editor.Styles[htmlTagCharsStyle].ForeColor = Color.DarkRed;
            editor.Styles[htmlTagNameStyle].ForeColor = Color.DarkRed;
            editor.Styles[htmlAttrNameStyle].ForeColor = Color.OrangeRed;
            editor.Styles[htmlAttrValueStyle].ForeColor = Color.CornflowerBlue;
        }

        public int Highlight(Scintilla editor, int startPos, int endPos)
        {
            int length = (endPos - startPos) + 1;

            int result = 0;

            editor.StartStyling(startPos);
            editor.SetStyling(length, 0);
            ++result;

            var rangeEntry = new StyleEntry(startPos, length, 0);
            var intersectingEntries = _entries.Where(r => r.InterectsWith(rangeEntry));

            foreach (StyleEntry entry in intersectingEntries)
            {
                int s = Math.Max(startPos, entry.Index);
                int e = Math.Min(entry.Index + (entry.Length - 1), endPos);
                int l = (e - s) + 1;
                editor.StartStyling(s);
                editor.SetStyling(l, entry.Style);
                ++result;
            }

            return (result);
        }
    }
}
