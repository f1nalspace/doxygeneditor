using System;
using System.Collections.Generic;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Parsers
{
    abstract class BaseEntity : IComparable
    {
        public TextRange StartRange { get; }
        public TextRange EndRange { get; set; }
        public abstract string Id { get; }
        public abstract string DisplayName { get; }
        public BaseEntity(TextRange range)
        {
            StartRange = range;
            EndRange = range;
        }
        public abstract int CompareTo(object obj);
    }
}
