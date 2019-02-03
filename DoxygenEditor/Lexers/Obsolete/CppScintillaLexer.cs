using ScintillaNET;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DoxygenEditor.Lexers.Obsolete
{
    static class CppScintillaLexer
    {
        // @TODO(final): Make primary-keywords configurable
        private static readonly HashSet<string> CPrimaryKeywords = new HashSet<string>{
            // C99
            "auto",
            "break",
            "case",
            "const",
            "continue",
            "default",
            "do",
            "else",
            "enum",
            "extern",
            "for",
            "goto",
            "if",
            "inline",
            "register",
            "restrict",
            "return",
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
            "_Alignas",
            "_Alignof",
            "__asm__",
            "__volatile__",

            // C++
            "abstract",
            "alignas",
            "alignof",
            "asm",
            "catch",
            "class",
            "constexpr",
            "const_cast",
            "decltype",
            "delete",
            "dynamic_cast",
            "explicit",
            "export",
            "false",
            "friend",
            "mutable",
            "namespace",
            "new",
            "noexcept",
            "nullptr",
            "operator",
            "override",
            "private",
            "protected",
            "public",
            "reinterpret_cast",
            "static_assert",
            "static_cast",
            "template",
            "this",
            "thread_local",
            "throw",
            "try",
            "typeid",
            "typename",
            "virtual",
        };

        // @TODO(final): Make secondary-keywords configurable
        private static readonly HashSet<string> CSecondaryKeywords = new HashSet<string>{
            // C99
            "char",
            "double",
            "float",
            "int",
            "long",
            "short",
            "_Bool",
            "_Complex",
            "_Imaginary",

            // C++
            "bool",
            "complex",
            "imaginary",
        };

        // @TODO(final): Make global-class-keywords configurable
        private static readonly HashSet<string> CGlobalClassKeywords = new HashSet<string>
        {
            "NULL",
            "int8_t",
            "int16_t",
            "int32_t",
            "int64_t",
            "intptr_t",
            "offset_t",
            "size_t",
            "ssize_t",
            "time_t",
            "uint8_t",
            "uint16_t",
            "uint32_t",
            "uint64_t",
            "uintptr_t",
            "wchar_t",
        };

        

        // @TODO(final): Make task-keywords configurable
        private static readonly HashSet<string> CTaskKeywords = new HashSet<string>{
            "@TODO",
            "@FIXME",
            "@BUG",
            "@NOTE",
            "@SPEED",
            "@HACK",
            "@STUPID",
            "@UNDONE",
            "@INCOMPLETE",
            "TODO",
            "HACK",
            "UNDONE"
        };

        public static void InitStyles(Scintilla editor)
        {
            editor.SetProperty("lexer.cpp.track.preprocessor", "0");

            string primaryKeywords = string.Join(" ", CPrimaryKeywords.OrderBy(f => f.Length));
            string secondaryKeywords = string.Join(" ", CSecondaryKeywords.OrderBy(f => f.Length));
            string globalClassKeywords = string.Join(" ", CGlobalClassKeywords.OrderBy(f => f.Length));
            string commentKeywords = string.Join(" ", DoxygenLexer.DoxygenKeywords.OrderBy(f => f.Length));
            string taskKeywords = string.Join(" ", CTaskKeywords.OrderBy(f => f.Length));

            // Primary keywords and identifiers
            editor.SetKeywords(0, primaryKeywords);
            // Secondary keywords and identifiers
            editor.SetKeywords(1, secondaryKeywords);
            // Documentation comment keywords
            editor.SetKeywords(2, commentKeywords);
            // Global classes and typedefs
            editor.SetKeywords(3, globalClassKeywords);
            // Preprocessor definitions
            //editor.SetKeywords(4, types);
            // Task marker and error marker keywords
            editor.SetKeywords(5, taskKeywords);

            editor.Styles[Style.Cpp.Default].ForeColor = Color.Silver;

            editor.Styles[Style.Cpp.Comment].ForeColor = Color.Green;
            editor.Styles[Style.Cpp.CommentLine].ForeColor = Color.Green;

            // Primary/Secondary keywords
            editor.Styles[Style.Cpp.Word].ForeColor = Color.Blue;
            editor.Styles[Style.Cpp.Word2].ForeColor = Color.Purple;

            // Constants
            editor.Styles[Style.Cpp.Number].ForeColor = Color.Olive;
            editor.Styles[Style.Cpp.String].ForeColor = Color.FromArgb(163, 21, 21);
            editor.Styles[Style.Cpp.StringEol].BackColor = Color.Pink; // Unterminated string
            editor.Styles[Style.Cpp.HashQuotedString].ForeColor = Color.Red;
            editor.Styles[Style.Cpp.Character].ForeColor = Color.FromArgb(163, 21, 21);
            editor.Styles[Style.Cpp.Verbatim].ForeColor = Color.FromArgb(163, 21, 21);

            //editor.Styles[Style.Cpp.Identifier].ForeColor = Color.DarkBlue;

            editor.Styles[Style.Cpp.Operator].ForeColor = Color.Purple;

            editor.Styles[Style.Cpp.GlobalClass].ForeColor = Color.Purple;

            editor.Styles[Style.Cpp.Preprocessor].ForeColor = Color.Maroon;

            editor.Styles[Style.Cpp.TaskMarker].ForeColor = Color.Blue;

            // Documentations
            editor.Styles[Style.Cpp.CommentDoc].ForeColor = Color.DarkMagenta;
            editor.Styles[Style.Cpp.CommentLineDoc].ForeColor = Color.DarkMagenta;
            editor.Styles[Style.Cpp.CommentDocKeyword].ForeColor = Color.Blue;
            editor.Styles[Style.Cpp.CommentDocKeywordError].ForeColor = Color.Orange;
        }

        public static void Lex(Scintilla editor, int startPos, int endPos)
        {
            editor.Lexer = Lexer.Cpp;
            InitStyles(editor);
            editor.Colorize(startPos, endPos);
        }
    }
}
