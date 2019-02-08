using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Models
{
    class LogItemModel
    {
        public enum IconType
        {
            Error = 0,
            Warning,
            Info
        }
        public IconType Icon { get; }
        public string Name { get; }
        public string Source { get; }
        public TextRange Range { get; }

        public LogItemModel(IconType icon, string name, string source, TextRange range)
        {
            Icon = icon;
            Name = name;
            Source = source;
            Range = range;
        }
    }
}
