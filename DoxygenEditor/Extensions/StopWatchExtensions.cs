using System;
using System.Diagnostics;

namespace TSP.DoxygenEditor.Extensions
{
    static class StopWatchExtensions
    {
        public static TimeSpan StopAndReturn(this Stopwatch watch)
        {
            watch.Stop();
            TimeSpan result = watch.Elapsed;
            return (result);
        }
    }
}
