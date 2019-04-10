using System.Collections.Generic;
using System.Drawing;

namespace TSP.DoxygenEditor.Styles
{
    public struct ColorThemeStyle
    {
        public Color Color { get; internal set; }
        public bool IsBold { get; internal set; }
    }

    public abstract class ColorThemeStyles<TKey>
    {
        private readonly Dictionary<TKey, ColorThemeStyle> _styles = new Dictionary<TKey, ColorThemeStyle>();
        public IEnumerable<TKey> Keys => _styles.Keys;
        public ColorThemeStyle this[TKey key]
        {
            get
            {
                if (_styles.ContainsKey(key))
                    return _styles[key];
                else
                    return new ColorThemeStyle();
            }
        }
        public ColorThemeStyles()
        {
            ResetToDefault();
        }
        public void Set(TKey key, ColorThemeStyle style)
        {
            _styles[key] = style;
        }
        public void Assign(ColorThemeStyles<TKey> other)
        {
            ResetToDefault();
            foreach (var keyPair in other._styles)
                _styles[keyPair.Key] = keyPair.Value;
        }

        public abstract void ResetToDefault();
    }

    public enum CppStyleKind
    {
        None = 0,
        MultiLineComment,
        MultiLineCommentDoc,
        MultiLineCommentDocText,
        SingleLineComment,
        SingleLineCommentDoc,
        SingleLineCommentDocText,

        PreprocessorBasic,
        PreprocessorKeyword,
        PreprocessorDefine,
        PreprocessorDefineArgument,
        PreprocessorInclude,

        ReservedKeyword,
        GlobalTypeKeyword,
        UserTypeKeyword,
        MemberKeyword,
        FunctionKeyword,

        CharLiteral,
        StringLiteral,
        NumberLiteral,
    }
    public abstract class CppColorTheme : ColorThemeStyles<CppStyleKind>
    {
    }
    public class DefaultCppColorTheme : CppColorTheme
    {
        public static readonly ColorThemeStyle DefaultMultiLineComment = new ColorThemeStyle() { Color = Color.Green };
        public static readonly ColorThemeStyle DefaultMultiLineCommentDoc = new ColorThemeStyle() { Color = Color.Purple };
        public static readonly ColorThemeStyle DefaultMultiLineCommentDocText = new ColorThemeStyle() { Color = Color.Navy };
        public static readonly ColorThemeStyle DefaultSingleLineComment = new ColorThemeStyle() { Color = Color.Green };
        public static readonly ColorThemeStyle DefaultSingleLineCommentDoc = new ColorThemeStyle() { Color = Color.Purple };
        public static readonly ColorThemeStyle DefaultSingleLineCommentDocText = new ColorThemeStyle() { Color = Color.Navy };

        public static readonly ColorThemeStyle DefaultPreprocessorBasic = new ColorThemeStyle() { Color = Color.DarkSlateGray };
        public static readonly ColorThemeStyle DefaultPreprocessorKeyword = new ColorThemeStyle() { Color = Color.DarkSlateGray };
        public static readonly ColorThemeStyle DefaultPreprocessorDefine = new ColorThemeStyle() { Color = Color.BlueViolet };
        public static readonly ColorThemeStyle DefaultPreprocessorDefineArgument = new ColorThemeStyle() { Color = Color.Magenta };
        public static readonly ColorThemeStyle DefaultPreprocessorInclude = new ColorThemeStyle() { Color = Color.Brown };

        public static readonly ColorThemeStyle DefaultReservedKeyword = new ColorThemeStyle() { Color = Color.Blue };
        public static readonly ColorThemeStyle DefaultGlobalTypeKeyword = new ColorThemeStyle() { Color = Color.DarkBlue };
        public static readonly ColorThemeStyle DefaultUserTypeKeyword = new ColorThemeStyle() { Color = Color.MediumVioletRed };
        public static readonly ColorThemeStyle DefaultMemberKeyword = new ColorThemeStyle() { Color = Color.PaleVioletRed };
        public static readonly ColorThemeStyle DefaultFunctionKeyword = new ColorThemeStyle() { Color = Color.FromArgb(63, 122, 233) };

        public static readonly ColorThemeStyle DefaultCharLiteral = new ColorThemeStyle() { Color = Color.OrangeRed };
        public static readonly ColorThemeStyle DefaultStringLiteral = new ColorThemeStyle() { Color = Color.OrangeRed };
        public static readonly ColorThemeStyle DefaultNumberLiteral = new ColorThemeStyle() { Color = Color.OrangeRed };

        public DefaultCppColorTheme() : base()
        {
        }
        public override void ResetToDefault()
        {
            Set(CppStyleKind.MultiLineComment, DefaultMultiLineComment);
            Set(CppStyleKind.MultiLineCommentDoc, DefaultMultiLineCommentDoc);
            Set(CppStyleKind.MultiLineCommentDocText, DefaultMultiLineCommentDocText);
            Set(CppStyleKind.SingleLineComment, DefaultSingleLineComment);
            Set(CppStyleKind.SingleLineCommentDoc, DefaultSingleLineCommentDoc);
            Set(CppStyleKind.SingleLineCommentDocText, DefaultSingleLineCommentDocText);

            Set(CppStyleKind.PreprocessorBasic, DefaultPreprocessorBasic);
            Set(CppStyleKind.PreprocessorKeyword, DefaultPreprocessorKeyword);
            Set(CppStyleKind.PreprocessorDefine, DefaultPreprocessorDefine);
            Set(CppStyleKind.PreprocessorDefineArgument, DefaultPreprocessorDefineArgument);
            Set(CppStyleKind.PreprocessorInclude, DefaultPreprocessorInclude);

            Set(CppStyleKind.ReservedKeyword, DefaultReservedKeyword);
            Set(CppStyleKind.GlobalTypeKeyword, DefaultGlobalTypeKeyword);
            Set(CppStyleKind.UserTypeKeyword, DefaultUserTypeKeyword);
            Set(CppStyleKind.MemberKeyword, DefaultMemberKeyword);
            Set(CppStyleKind.FunctionKeyword, DefaultFunctionKeyword);

            Set(CppStyleKind.CharLiteral, DefaultCharLiteral);
            Set(CppStyleKind.StringLiteral, DefaultStringLiteral);
            Set(CppStyleKind.NumberLiteral, DefaultNumberLiteral);
        }
    }

    public abstract class ColorTheme
    {
        public abstract CppColorTheme Cpp { get; }
    }

    public class DefaultColorTheme : ColorTheme
    {
        public override CppColorTheme Cpp { get; }

        public DefaultColorTheme()
        {
            Cpp = new DefaultCppColorTheme();
        }
    }

    public static class ColorThemeManager
    {
        private static ColorTheme _current = null;
        public static ColorTheme Current
        {
            get
            {
                if (_current == null)
                    _current = new DefaultColorTheme();
                return (_current);
            }
        }
    }
}
