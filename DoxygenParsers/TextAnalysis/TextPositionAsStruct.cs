namespace TSP.DoxygenEditor.TextAnalysis
{
    public struct TextPositionAsStruct
    {
        public int Index { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public string LineInfo { get { return $"Line: {Line}, Col: {Column}"; } }

        public TextPositionAsStruct(int index, int line, int column)
        {
            Index = index;
            Line = line;
            Column = column;
        }
        public TextPositionAsStruct(int index) : this(index, 0, 0)
        {
        }
        public TextPositionAsStruct(TextPosition other) : this(other.Index, other.Line, other.Column)
        {
        }
        public TextPositionAsStruct(TextPositionAsStruct other) : this(other.Index, other.Line, other.Column)
        {
        }

        public string ToDisplayString()
        {
            return $"@{Index} -> (Line: {Line + 1}, Col: {Column + 1})";
        }

        public override string ToString()
        {
            return $"@{Index} -> ({LineInfo})";
        }
    }
}
