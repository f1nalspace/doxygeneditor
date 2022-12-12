using System;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Parsers
{
    public abstract class BaseEntity : IComparable, IBaseEntity
    {
        public TextRange StartRange { get; }
        private TextRange _endRange;
        public TextRange EndRange {
            get { return _endRange; }
            set { _endRange = new TextRange(value); }
        }
        public int Length => EndRange.Index - StartRange.Index;
        public abstract string DisplayName { get; }
        public abstract string Id { get; set; }
        public abstract string Value { get; set; }

        public BaseEntity(TextRange range)
        {
            StartRange = new TextRange(range.Position, 0);
            _endRange = new TextRange(range.Position, range.Length);
        }
        public abstract int CompareTo(object obj);
    }
}
