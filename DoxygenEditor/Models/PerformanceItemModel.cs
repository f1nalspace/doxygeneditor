using System;
using System.Collections;

namespace TSP.DoxygenEditor.Models
{
    public class PerformanceItemModel : IComparer
    {
        public int SortIndex { get; }
        public string Size { get; }
        public string What { get; }
        public TimeSpan Duration { get; }
        public PerformanceItemModel(int sortIndex, string size, string what, TimeSpan duration)
        {
            SortIndex = sortIndex;
            Size = size;
            What = what;
            Duration = duration;
        }

        public int Compare(object x, object y)
        {
            if (x == null || y == null)
                return (-1);
            if (!typeof(PerformanceItemModel).Equals(x.GetType()) || !typeof(PerformanceItemModel).Equals(y.GetType()))
                return (-1);
            PerformanceItemModel a = (PerformanceItemModel)x;
            PerformanceItemModel b = (PerformanceItemModel)y;
            int result = b.SortIndex.CompareTo(a.SortIndex);
            return (result);
        }
    }
}
