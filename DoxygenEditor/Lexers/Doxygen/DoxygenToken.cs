namespace TSP.DoxygenEditor.Lexers.Doxygen
{
    class DoxygenToken : BaseToken
    {
        public DoxygenTokenType Type { get; }
        public override bool IsEOF => Type == DoxygenTokenType.EOF;
        public override bool IsValid => Type != DoxygenTokenType.Invalid;
        public DoxygenToken(DoxygenTokenType type, int index, int length, bool isComplete) : base(index, length, isComplete)
        {
            Type = type;
        }
        public override string ToString()
        {
            return $"{Type} {base.ToString()}";
        }
    }
}
