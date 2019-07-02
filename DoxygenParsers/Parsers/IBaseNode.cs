using System;
using System.Collections.Generic;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Parsers
{
    public interface IBaseNode: IComparable
    {
        IBaseNode Parent { get; }
        int Level { get; }
        IEnumerable<IBaseNode> Children { get; }
        bool ShowChildren { get; }
        string FullId { get; }
        string Id { get; }
        TextRange StartRange { get; }
        TextRange EndRange { get; }
        void AddChild(IBaseNode child);
        IBaseNode FindNodeByRange(TextRange range);
        IEnumerable<TChild> GetChildrenAs<TChild>() where TChild : IBaseNode;
    }
}
