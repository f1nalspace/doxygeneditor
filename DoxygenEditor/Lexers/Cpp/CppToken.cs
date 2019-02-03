namespace DoxygenEditor.Lexers.Cpp
{
    public class CppToken : BaseToken
    {
        public CppTokenType Type { get; }

        public override bool IsEOF => Type == CppTokenType.EOF;
        public override bool IsValid => Type != CppTokenType.Invalid;

        public CppToken(CppTokenType type, int index, int length, bool isComplete) : base(index, length, isComplete)
        {
            Type = type;
        }
    }
}
