namespace TSP.DoxygenEditor.Utils
{
    static class SyntaxUtils
    {
        public static bool IsAlpha(char c)
        {
            bool result = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
            return (result);
        }
        public static bool IsNumeric(char c)
        {
            bool result = (c >= '0' && c <= '9');
            return (result);
        }
        public static bool IsHex(char c)
        {
            bool result = IsNumeric(c) || ((c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'));
            return (result);
        }
        public static bool IsOctal(char c)
        {
            bool result = c >= '0' && c <= '7';
            return (result);
        }
        public static bool IsIdentStart(char c)
        {
            bool result = IsAlpha(c) || (c == '_');
            return (result);
        }
        public static bool IsIdent(char c)
        {
            bool result = IsAlpha(c) || IsNumeric(c) || (c == '_');
            return (result);
        }
    }
}
