using System.Collections.Generic;
using TSP.DoxygenEditor.Lexers;

namespace TSP.DoxygenEditor.Editor
{
    interface IStylerData
    {
        int Count { get; }
        void RefreshData(IEnumerable<IBaseToken> tokens);
    }
}
