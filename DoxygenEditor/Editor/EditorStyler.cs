using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Lexers.Cpp;
using TSP.DoxygenEditor.Lexers.Doxygen;
using TSP.DoxygenEditor.Lexers.Html;

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

        static readonly Dictionary<CppTokenType, int> cppTokenTypeToStyleDict = new Dictionary<CppTokenType, int>() {
            { CppTokenType.MultiLineComment, cppMultiLineCommentStyle },
            { CppTokenType.MultiLineCommentDoc, cppMultiLineCommentDocStyle },
            { CppTokenType.SingleLineComment, cppSingleLineCommentStyle },
            { CppTokenType.SingleLineCommentDoc, cppSingleLineCommentDocStyle },

            { CppTokenType.Preprocessor, cppPreprocessorStyle },

            { CppTokenType.ReservedKeyword, cppReservedKeywordStyle },
            { CppTokenType.TypeKeyword, cppTypeKeywordStyle },

            { CppTokenType.String, cppStringStyle },
            { CppTokenType.Integer, cppNumberStyle },
            { CppTokenType.Float, cppNumberStyle },
            { CppTokenType.Double, cppNumberStyle },
            { CppTokenType.Hex, cppNumberStyle },
            { CppTokenType.Octal, cppNumberStyle },
            { CppTokenType.Binary, cppNumberStyle },

            { CppTokenType.Typedef, cppReservedKeywordStyle },
            { CppTokenType.Struct, cppReservedKeywordStyle },
            { CppTokenType.Union, cppReservedKeywordStyle },
        };

        static int doxygenTextStyle = styleIndex++;
        static int doxygenBlockStyle = styleIndex++;
        static int doxygenCommandStyle = styleIndex++;
        static int doxygenIdentStyle = styleIndex++;
        static int doxygenCaptionStyle = styleIndex++;
        static int doxygenCodeBlockStyle = styleIndex++;
        static int doxygenCodeTypeStyle = styleIndex++;

        static Dictionary<DoxygenTokenType, int> doxygenTokenTypeToStyleDict = new Dictionary<DoxygenTokenType, int>() {
            { DoxygenTokenType.Text, doxygenTextStyle },
            { DoxygenTokenType.BlockStart, doxygenBlockStyle },
            { DoxygenTokenType.BlockEnd, doxygenBlockStyle },
            { DoxygenTokenType.BlockChars, doxygenBlockStyle },
            { DoxygenTokenType.Command, doxygenCommandStyle },
            { DoxygenTokenType.GroupStart, doxygenCommandStyle },
            { DoxygenTokenType.GroupEnd, doxygenCommandStyle },
            { DoxygenTokenType.Ident, doxygenIdentStyle },
            { DoxygenTokenType.Caption, doxygenCaptionStyle },
            { DoxygenTokenType.CodeBlock, doxygenCodeBlockStyle },
            { DoxygenTokenType.CodeType, doxygenCodeTypeStyle },
        };

        static int htmlTagCharsStyle = styleIndex++;
        static int htmlTagNameStyle = styleIndex++;
        static int htmlAttrNameStyle = styleIndex++;
        static int htmlAttrValueStyle = styleIndex++;

        static Dictionary<HtmlTokenType, int> htmlTokenTypeToStyleDict = new Dictionary<HtmlTokenType, int>() {
            { HtmlTokenType.TagChars, htmlTagCharsStyle },
            { HtmlTokenType.TagName, htmlTagNameStyle },
            { HtmlTokenType.AttrName, htmlAttrNameStyle },
            { HtmlTokenType.AttrValue, htmlAttrValueStyle },
        };

        class StyleEntry
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

            public StyleEntry(BaseToken token, int style) : this(token.Index, token.Length, style)
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

        public void Refresh(IEnumerable<BaseToken> tokens)
        {
            _entries.Clear();
            foreach (var token in tokens)
            {
                if (token.Length == 0) continue;
                if (typeof(CppToken).Equals(token.GetType()))
                {
                    CppToken cppToken = (CppToken)token;
                    if (cppTokenTypeToStyleDict.ContainsKey(cppToken.Type))
                    {
                        int style = cppTokenTypeToStyleDict[cppToken.Type];
                        _entries.Add(new StyleEntry(token, style));
                    }
                }
                else if (typeof(DoxygenToken).Equals(token.GetType()))
                {
                    DoxygenToken doxygenToken = (DoxygenToken)token;
                    if (doxygenTokenTypeToStyleDict.ContainsKey(doxygenToken.Type))
                    {
                        int style = doxygenTokenTypeToStyleDict[doxygenToken.Type];
                        _entries.Add(new StyleEntry(token, style));
                    }
                }
                else if (typeof(HtmlToken).Equals(token.GetType()))
                {
                    HtmlToken htmlToken = (HtmlToken)token;
                    if (htmlTokenTypeToStyleDict.ContainsKey(htmlToken.Type))
                    {
                        int style = htmlTokenTypeToStyleDict[htmlToken.Type];
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
            editor.Styles[cppTypeKeywordStyle].ForeColor = Color.Blue;
            editor.Styles[cppStringStyle].ForeColor = Color.Green;
            editor.Styles[cppNumberStyle].ForeColor = Color.Red;

            editor.Styles[doxygenTextStyle].ForeColor = Color.Black;
            editor.Styles[doxygenBlockStyle].ForeColor = Color.DarkViolet;
            editor.Styles[doxygenCommandStyle].ForeColor = Color.Red;
            editor.Styles[doxygenIdentStyle].ForeColor = Color.Blue;
            editor.Styles[doxygenCaptionStyle].ForeColor = Color.Green;
            editor.Styles[doxygenCodeBlockStyle].ForeColor = Color.Black;
            editor.Styles[doxygenCodeTypeStyle].ForeColor = Color.Red;

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
