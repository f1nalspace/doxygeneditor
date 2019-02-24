using System;

namespace TSP.DoxygenEditor.TextAnalysis
{
    class StreamCursor : TextCursor
    {
        private readonly TextStream _textStream;
        public StreamCursor(TextStream stream, int startLine = 1, int startColumn = 1) : base(stream, startLine, startColumn)
        {
            _textStream = stream;
        }
        public override void AdvanceColumn(int charCount = 1)
        {
            base.AdvanceColumn(charCount);
            _textStream.AdvanceChar(charCount);
        }
        public override void Set(TextPosition position)
        {
            base.Set(position);
            _textStream.SetPosition(position.Index);
        }
    }
}
