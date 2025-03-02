using TSP.DoxygenEditor.Languages;

namespace TSP.DoxygenEditor.TextAnalysis
{
    public readonly struct TextError
    {
        public LanguageKind Lang { get; }
        public TextPosition Pos { get; }
        public string Category { get; }
        public string Message { get; }
        public string What { get; }
        public string Symbol { get; }

        public TextError(LanguageKind lang, TextPosition pos, string category, string message, string what, string symbol)
        {
            Lang = lang;
            Pos = pos;
            Category = category;
            Message = message;
            What = what;
            Symbol = symbol;
        }

        public override string ToString()
        {
            return $"[{Lang}] {Pos}, {Category} = {Message}";
        }
    }
}
