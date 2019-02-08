namespace TSP.DoxygenEditor.Lexers.Cpp
{
    class CppToken : BaseToken
    {
        public CppTokenType Type { get; }

        public override bool IsEOF => Type == CppTokenType.EOF;
        public override bool IsValid => Type != CppTokenType.Invalid;

        public CppToken(CppTokenType type, int index, int length, bool isComplete) : base(index, length, isComplete)
        {
            Type = type;
        }

        public override string ToString()
        {
#if DEBUG
            return $"{base.ToString()}, {Type} = {DebugValue}";
#else
            return $"{base.ToString()}, {Type}";
#endif
        }
    }
}
