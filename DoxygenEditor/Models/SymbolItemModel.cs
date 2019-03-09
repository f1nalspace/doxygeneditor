using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Models
{
    public class SymbolItemModel
    {
        public string Id { get; set; }
        public string Caption { get; set; }
        public string Type { get; set; }
        public TextPosition Position { get; set; }
    }
}
