using DoxygenEditor.Parser;

namespace DoxygenEditor.Models
{
    public class LogItemModel
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
        public SequenceInfo LineInfo { get; }

        public LogItemModel(IconType icon, string name, string source, SequenceInfo lineInfo)
        {
            Icon = icon;
            Name = name;
            Source = source;
            LineInfo = LineInfo;
        }
    }
}
