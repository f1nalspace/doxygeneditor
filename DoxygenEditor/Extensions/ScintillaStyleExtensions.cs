using ScintillaNET;
using TSP.DoxygenEditor.Styles;

namespace TSP.DoxygenEditor.Extensions
{
    static class ScintillaStyleExtensions
    {
        public static void Set(this Style outStyle, ColorThemeStyle inStyle)
        {
            outStyle.Bold = inStyle.IsBold;
            outStyle.ForeColor = inStyle.Color;
        }
    }
}
