namespace TSP.DoxygenEditor.TextAnalysis
{
    public class TextError
    {
        public TextPosition Pos { get; }
        public string Category { get; }
        public string Message { get; }
        public string Type { get; }
        public string Symbol { get; }
        public object Tag { get; set; }
        public TextError(TextPosition pos, string category, string message, string type, string symbol)
        {
            Pos = pos;
            Category = category;
            Message = message;
            Type = type;
            Symbol = symbol;
        }
        public override string ToString()
        {
            return $"{Pos}, {Category} = {Message}";
        }
    }
}
