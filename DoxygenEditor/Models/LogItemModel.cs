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
        public TextPosition Pos { get; }

        public LogItemModel(IconType icon, string name, string source, TextPosition pos)
        {
            Icon = icon;
            Name = name;
            Source = source;
            Pos = pos;
        }
    }
}
