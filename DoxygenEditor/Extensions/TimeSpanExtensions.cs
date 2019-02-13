using System;
using System.Globalization;

namespace TSP.DoxygenEditor.Extensions
{
    static class TimeSpanExtensions
    {
        public static string ToMilliseconds(this TimeSpan timeSpan)
        {
            string result = timeSpan.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
            return (result);
        }
    }
}
