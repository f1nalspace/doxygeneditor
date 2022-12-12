using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using TSP.DoxygenEditor.Extensions;
using TSP.DoxygenEditor.Languages;
using TSP.DoxygenEditor.Languages.Cpp;
using TSP.DoxygenEditor.Languages.Doxygen;
using TSP.DoxygenEditor.Languages.Html;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Models;

namespace TSP.DoxygenEditor.Styles
{
    class EditorStyler : IStylerData, IVisualStyler
    {
        static int styleIndex = 100;

        static readonly int cppMultiLineCommentStyle = styleIndex++;
        static readonly int cppMultiLineCommentDocStyle = styleIndex++;
        static readonly int cppMultiLineCommentDocTextStyle = styleIndex++;
        static readonly int cppSingleLineCommentStyle = styleIndex++;
        static readonly int cppSingleLineCommentDocStyle = styleIndex++;
        static readonly int cppSingleLineCommentDocTextStyle = styleIndex++;

        static readonly int cppPreprocessorBasicStyle = styleIndex++;
        static readonly int cppPreprocessorKeywordStyle = styleIndex++;
        static readonly int cppPreprocessorDefineStyle = styleIndex++;
        static readonly int cppPreprocessorDefineArgumentStyle = styleIndex++;
        static readonly int cppPreprocessorIncludeStyle = styleIndex++;

        static readonly int cppReservedKeywordStyle = styleIndex++;
        static readonly int cppGlobalTypeKeywordStyle = styleIndex++;
        static readonly int cppUserTypeIdentStyle = styleIndex++;
        static readonly int cppMemberIdentStyle = styleIndex++;
        static readonly int cppFunctionIdentStyle = styleIndex++;

        static readonly int cppCharLiteralStyle = styleIndex++;
        static readonly int cppStringLiteralStyle = styleIndex++;
        static readonly int cppNumberLiteralStyle = styleIndex++;

        static readonly Dictionary<CppTokenKind, int> cppTokenTypeToStyleDict = new Dictionary<CppTokenKind, int>() {
            { CppTokenKind.MultiLineComment, cppMultiLineCommentStyle },
            { CppTokenKind.MultiLineCommentDoc, cppMultiLineCommentDocTextStyle },
            { CppTokenKind.SingleLineComment, cppSingleLineCommentStyle },
            { CppTokenKind.SingleLineCommentDoc, cppSingleLineCommentDocTextStyle },

            { CppTokenKind.PreprocessorStart, cppPreprocessorBasicStyle },
            { CppTokenKind.PreprocessorOperator, cppPreprocessorBasicStyle },
            { CppTokenKind.PreprocessorKeyword, cppPreprocessorKeywordStyle },
            { CppTokenKind.PreprocessorDefineSource, cppPreprocessorDefineStyle },
            { CppTokenKind.PreprocessorFunctionSource, cppPreprocessorDefineStyle },
            { CppTokenKind.PreprocessorDefineUsage, cppPreprocessorDefineStyle },
            { CppTokenKind.PreprocessorDefineMatch, cppPreprocessorDefineStyle },
            { CppTokenKind.PreprocessorDefineArgument, cppPreprocessorDefineArgumentStyle },
            { CppTokenKind.PreprocessorInclude, cppPreprocessorIncludeStyle },

            { CppTokenKind.ReservedKeyword, cppReservedKeywordStyle },
            { CppTokenKind.GlobalTypeKeyword, cppGlobalTypeKeywordStyle },
            { CppTokenKind.FunctionIdent, cppFunctionIdentStyle },
            { CppTokenKind.UserTypeIdent, cppUserTypeIdentStyle },
            { CppTokenKind.MemberIdent, cppMemberIdentStyle },

            { CppTokenKind.StringLiteral, cppStringLiteralStyle },
            { CppTokenKind.CharLiteral, cppCharLiteralStyle },

            { CppTokenKind.IntegerLiteral, cppNumberLiteralStyle },
            { CppTokenKind.OctalLiteral, cppNumberLiteralStyle },
            { CppTokenKind.HexLiteral, cppNumberLiteralStyle },
            { CppTokenKind.IntegerFloatLiteral, cppNumberLiteralStyle },
            { CppTokenKind.HexadecimalFloatLiteral, cppNumberLiteralStyle },
        };

        static int doxygenBlockStyle = styleIndex++;
        static int doxygenCommandStyle = styleIndex++;
        static int doxygenInvalidCommandStyle = styleIndex++;
        static int doxygenIdentStyle = styleIndex++;
        static int doxygenQuoteStringStyle = styleIndex++;
        static int doxygenArgumentStyle = styleIndex++;

        static int doxygenConfigCommentStyle = styleIndex++;
        static int doxygenConfigKeyStyle = styleIndex++;
        static int doxygenConfigOpStyle = styleIndex++;
        static int doxygenConfigValueStyle = styleIndex++;

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
            { DoxygenTokenKind.Code, Style.Default },

            { DoxygenTokenKind.ConfigComment, doxygenConfigCommentStyle },
            { DoxygenTokenKind.ConfigKey, doxygenConfigKeyStyle },
            { DoxygenTokenKind.ConfigOpAddAssign, doxygenConfigOpStyle },
            { DoxygenTokenKind.ConfigOpAssign, doxygenConfigOpStyle },
            { DoxygenTokenKind.ConfigOpAddLine, doxygenConfigOpStyle },
            { DoxygenTokenKind.ConfigValue, doxygenConfigValueStyle },
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

        private readonly static HashSet<int> allowedMatchStyles = new HashSet<int> {
            cppPreprocessorDefineStyle,
            cppUserTypeIdentStyle,
            cppMemberIdentStyle,
            cppFunctionIdentStyle,
            doxygenIdentStyle,
            doxygenArgumentStyle,
        };

        private readonly List<StyleEntry> _entries = new List<StyleEntry>();
        public int Count => _entries.Count;

        private readonly WorkspaceModel _workspace;
        public EditorStyler(WorkspaceModel workspace)
        {
            _workspace = workspace;
        }

        public StyleEntry FindStyleFromPosition(int position)
        {
            StyleEntry result = _entries.FirstOrDefault((e) => allowedMatchStyles.Contains(e.Style) && position >= e.Index && position <= e.End);
            return (result);
        }

        public void RefreshData(IEnumerable<IBaseToken> tokens)
        {
            _entries.Clear();
            foreach (IBaseToken token in tokens)
            {
                if (token.Length == 0) continue;
                if (typeof(CppToken).Equals(token.GetType()))
                {
                    CppToken cppToken = (CppToken)token;
                    int style;
                    if (cppTokenTypeToStyleDict.TryGetValue(cppToken.Kind, out style))
                    {
#if DEBUG
                        _entries.Add(new StyleEntry(LanguageKind.Cpp, token, style, token.Value));
#else
                        _entries.Add(new StyleEntry(LanguageKind.Cpp, token, style));
#endif
                    }
                }
                else if (typeof(DoxygenToken).Equals(token.GetType()))
                {
                    DoxygenToken doxygenToken = (DoxygenToken)token;
                    int style;
                    if (doxygenTokenTypeToStyleDict.TryGetValue(doxygenToken.Kind, out style))
                    {
                        LanguageKind styleKind = LanguageKind.Doxygen;
                        if (doxygenToken.Kind == DoxygenTokenKind.Code)
                            styleKind = LanguageKind.DoxygenCode;
#if DEBUG
                        _entries.Add(new StyleEntry(styleKind, token, style, doxygenToken.Value));
#else
                        _entries.Add(new StyleEntry(styleKind, token, style));
#endif
                    }
                }
                else if (typeof(HtmlToken).Equals(token.GetType()))
                {
                    HtmlToken htmlToken = (HtmlToken)token;
                    int style;
                    if (htmlTokenTypeToStyleDict.TryGetValue(htmlToken.Kind, out style))
                    {
#if DEBUG
                        _entries.Add(new StyleEntry(LanguageKind.Html, token, style, htmlToken.Value));
#else
                        _entries.Add(new StyleEntry(LanguageKind.Html, token, style));
#endif
                    }
                }
            }
        }

        private void ApplyCppStyle(Scintilla editor, ColorTheme theme)
        {
            CppColorTheme cppTheme = theme.Cpp;

            editor.Styles[cppMultiLineCommentStyle].Set(cppTheme[CppStyleKind.MultiLineComment]);
            editor.Styles[cppMultiLineCommentDocStyle].Set(cppTheme[CppStyleKind.MultiLineCommentDoc]);
            editor.Styles[cppMultiLineCommentDocTextStyle].Set(cppTheme[CppStyleKind.MultiLineCommentDocText]);
            editor.Styles[cppSingleLineCommentStyle].Set(cppTheme[CppStyleKind.SingleLineComment]);
            editor.Styles[cppSingleLineCommentDocStyle].Set(cppTheme[CppStyleKind.SingleLineCommentDoc]);
            editor.Styles[cppSingleLineCommentDocTextStyle].Set(cppTheme[CppStyleKind.SingleLineCommentDocText]);

            editor.Styles[cppPreprocessorBasicStyle].Set(cppTheme[CppStyleKind.PreprocessorBasic]);
            editor.Styles[cppPreprocessorKeywordStyle].Set(cppTheme[CppStyleKind.PreprocessorKeyword]);
            editor.Styles[cppPreprocessorDefineStyle].Set(cppTheme[CppStyleKind.PreprocessorDefine]);
            editor.Styles[cppPreprocessorDefineArgumentStyle].Set(cppTheme[CppStyleKind.PreprocessorDefineArgument]);
            editor.Styles[cppPreprocessorIncludeStyle].Set(cppTheme[CppStyleKind.PreprocessorInclude]);

            editor.Styles[cppReservedKeywordStyle].Set(cppTheme[CppStyleKind.ReservedKeyword]);
            editor.Styles[cppGlobalTypeKeywordStyle].Set(cppTheme[CppStyleKind.GlobalTypeKeyword]);
            editor.Styles[cppUserTypeIdentStyle].Set(cppTheme[CppStyleKind.UserTypeKeyword]);
            editor.Styles[cppMemberIdentStyle].Set(cppTheme[CppStyleKind.MemberKeyword]);
            editor.Styles[cppFunctionIdentStyle].Set(cppTheme[CppStyleKind.FunctionKeyword]);

            editor.Styles[cppStringLiteralStyle].Set(cppTheme[CppStyleKind.StringLiteral]);
            editor.Styles[cppCharLiteralStyle].Set(cppTheme[CppStyleKind.CharLiteral]);
            editor.Styles[cppNumberLiteralStyle].Set(cppTheme[CppStyleKind.NumberLiteral]);

        }

        private void ApplyDoxygenStyle(Scintilla editor)
        {
            // Block styles
            editor.Styles[doxygenBlockStyle].ForeColor = Color.DarkViolet;
            editor.Styles[doxygenCommandStyle].ForeColor = Color.Red;
            editor.Styles[doxygenInvalidCommandStyle].ForeColor = Color.Red;
            editor.Styles[doxygenInvalidCommandStyle].Underline = true;
            editor.Styles[doxygenIdentStyle].ForeColor = Color.Blue;
            editor.Styles[doxygenQuoteStringStyle].ForeColor = Color.Green;
            editor.Styles[doxygenArgumentStyle].ForeColor = Color.Red;

            // Config styles
            editor.Styles[doxygenConfigCommentStyle].ForeColor = Color.Gray;
            editor.Styles[doxygenConfigKeyStyle].ForeColor = Color.Blue;
            editor.Styles[doxygenConfigKeyStyle].Bold = false;
            editor.Styles[doxygenConfigValueStyle].ForeColor = Color.Green;
            editor.Styles[doxygenConfigOpStyle].ForeColor = Color.Black;
            editor.Styles[doxygenConfigOpStyle].Bold = true;
        }

        private void ApplyHtmlStyle(Scintilla editor)
        {
            editor.Styles[htmlTagCharsStyle].ForeColor = Color.DarkRed;
            editor.Styles[htmlTagNameStyle].ForeColor = Color.DarkRed;
            editor.Styles[htmlAttrNameStyle].ForeColor = Color.OrangeRed;
            editor.Styles[htmlAttrValueStyle].ForeColor = Color.CornflowerBlue;
        }

        public void ApplyStyles(Scintilla editor)
        {
            ColorTheme theme = ColorThemeManager.Current;

            ApplyCppStyle(editor, theme);
            ApplyDoxygenStyle(editor);
            ApplyHtmlStyle(editor);

            editor.Indicators[0].Style = IndicatorStyle.FullBox;
            editor.Indicators[0].ForeColor = Color.Red;
        }

        public void Highlight(Scintilla editor, int startPos, int endPos)
        {
            Debug.Assert(startPos < endPos);

            int length = (endPos - startPos) + 1;

            editor.StartStyling(startPos);
            editor.SetStyling(length, 0);

            StyleEntry rangeEntry = new StyleEntry(LanguageKind.None, startPos, length, 0);
            IEnumerable<StyleEntry> intersectingEntries = _entries.Where(r => r.InterectsWith(rangeEntry));

            foreach (StyleEntry entry in intersectingEntries)
            {
                int s = Math.Max(startPos, entry.Index);
                int e = Math.Min(entry.Index + (entry.Length - 1), endPos);
                int l = (e - s) + 1;
                editor.StartStyling(s);
                editor.SetStyling(l, entry.Style);
            }
        }
    }
}
