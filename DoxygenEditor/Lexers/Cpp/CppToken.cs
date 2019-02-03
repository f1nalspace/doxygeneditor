namespace DoxygenEditor.Lexers.Cpp
{
    public class CppToken : BaseToken
    {
        public CppTokenType Type { get; }

        public CppToken(CppTokenType type, int index, int length, bool isComplete) : base(index, length, isComplete)
        {
            Type = type;
        }
    }
}
