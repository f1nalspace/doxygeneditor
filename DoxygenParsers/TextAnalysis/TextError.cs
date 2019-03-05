namespace TSP.DoxygenEditor.TextAnalysis
{
    public class TextError
    {
        public TextPosition Pos { get; }
        public string Category { get; }
        public string Message { get; }
        public TextError(TextPosition pos, string category, string message)
        {
            Pos = new TextPosition(pos);
            Category = category;
            Message = message;
        }
        public override string ToString()
        {
            return $"{Pos}, {Category} = {Message}";
        }
    }
}
