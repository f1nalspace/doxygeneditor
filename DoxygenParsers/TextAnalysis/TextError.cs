namespace TSP.DoxygenEditor.TextAnalysis
{
    public readonly struct TextError
    {
        public TextPosition Pos { get; }
        public string Category { get; }
        public string Message { get; }
        public string What { get; }
        public string Symbol { get; }
        public object Tag { get; }

        public TextError(TextPosition pos, string category, string message, string what, string symbol, object tag = null)
        {
            Pos = pos;
            Category = category;
            Message = message;
            What = what;
            Symbol = symbol;
            Tag = tag;
        }

        public override string ToString()
        {
            return $"{Pos}, {Category} = {Message}";
        }
    }
}
