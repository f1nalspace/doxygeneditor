namespace DoxygenEditor.Parsers
{
    public class ParseMessage
    {
        public enum MessageType
        {
            Error = 0,
            Warning,
        }
        public MessageType Type { get; }
        public string Text { get; }
        public SequenceInfo LineInfo { get; }

        public ParseMessage(MessageType type, string text, SequenceInfo lineInfo)
        {
            Type = type;
            Text = text;
            LineInfo = lineInfo;
        }

        public override string ToString()
        {
            return $"{Type}[{LineInfo.Line}:{LineInfo.Start}] {Text}";
        }
    }
}
