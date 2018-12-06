namespace DoxygenEditor.Lexer
{
    interface IDoxygenLexer
    {
        void Init();
        void Style(int rangeStartPos, int rangeEndPos);
    }
}
