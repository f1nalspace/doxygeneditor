using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace TSP.DoxygenEditor.Languages.Utils
{
    public static class SyntaxUtils
    {
        public static int GetLineBreakChars(char a, char b)
        {
            Debug.Assert(a == '\r' || a == '\n');
            if ((a == '\r' && b == '\n') || (a == '\n' && b == '\r'))
                return (2);
            else
                return (1);
        }

        private static HashSet<char> InvalidFilenameChars = new HashSet<char>(Path.GetInvalidFileNameChars());
        public static bool IsFilename(char c)
        {
            bool result = (IsAlpha(c) || IsNumeric(c) || (c == '_') || (c == '-') || (c == '.')) && !InvalidFilenameChars.Contains(c);
            return (result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLineBreak(char a)
        {
            return (a == '\r' || a == '\n');
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSpacing(char c)
        {
            bool result = c == ' ' || c == '\f' || c == '\v';
            return (result);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlpha(char c)
        {
            bool result = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
            return (result);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNumeric(char c)
        {
            bool result = (c >= '0' && c <= '9');
            return (result);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHex(char c)
        {
            bool result = IsNumeric(c) || ((c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'));
            return (result);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOctal(char c)
        {
            bool result = c >= '0' && c <= '7';
            return (result);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBinary(char c)
        {
            bool result = c >= '0' && c <= '1';
            return (result);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsIdentStart(char c)
        {
            bool result = IsAlpha(c) || (c == '_');
            return (result);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsIdentPart(char c)
        {
            bool result = IsAlpha(c) || IsNumeric(c) || (c == '_');
            return (result);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsExponentPrefix(char c)
        {
            bool result = c == 'e' || c == 'E' || c == 'p' || c == 'P';
            return (result);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsIntegerSuffix(char c)
        {
            bool result = c == 'u' || c == 'U' || c == 'l' || c == 'L';
            return (result);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFloatSuffix(char c)
        {
            bool result = c == 'f' || c == 'F' || c == 'l' || c == 'L';
            return (result);
        }
    }
}
