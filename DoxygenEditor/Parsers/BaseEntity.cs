using System;
using System.Collections.Generic;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Parsers
{
    abstract class BaseEntity : IComparable
    {
        public TextRange Range { get; }
        public abstract string Id { get; }
        public abstract string DisplayName { get; }
        public BaseEntity(TextRange range)
        {
            Range = range;
        }
        public abstract int CompareTo(object obj);
    }
}
