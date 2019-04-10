using System.Collections.Generic;
using TSP.DoxygenEditor.Lexers;

namespace TSP.DoxygenEditor.Styles
{
    interface IStylerData
    {
        int Count { get; }
        void RefreshData(IEnumerable<IBaseToken> tokens);
    }
}
