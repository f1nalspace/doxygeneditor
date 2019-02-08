namespace TSP.DoxygenEditor.Lexers
{
    class InvalidToken : BaseToken
    {
        public override bool IsEOF => true;
        public override bool IsValid => false;
        public InvalidToken(int offset, int length) : base(offset, length, false)
        {
        }
    }
}
