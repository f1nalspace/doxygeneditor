namespace TSP.FastTokenizer
{
    struct Token
    {
        public TokenKind Kind { get; }
        public TextPosition Start { get; set; }
        public TextPosition End { get; set; }
        public int Length => End.Index - Start.Index;
        public string Value { get; }

        public Token(TokenKind kind, TextPosition start, TextPosition end, string value = null)
        {
            Kind = kind;
            Start = start;
            End = end;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Kind}@{Start.Index} (line {Start.Line}, column {Start.Column}): {Value}";
        }
    }
}
