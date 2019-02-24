namespace TSP.FastTokenizer
{
    class TextPosition
    {
        public int Index { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }
        public TextPosition(int index, int line, int column)
        {
            Index = index;
            Line = line;
            Column = column;
        }
        public TextPosition(TextPosition other)
        {
            Index = other.Index;
            Line = other.Line;
            Column = other.Column;
        }
        public void AdvanceLine()
        {
            ++Line;
            Column = 1;
        }
        public void AdvanceColumn(int charCount = 1)
        {
            Index += charCount;
            Column += charCount;
        }

        public override string ToString()
        {
            return $"{Line}:{Column} ({Index})";
        }
    }
}
