﻿using System;
using System.Collections;

namespace TSP.DoxygenEditor.Models
{
    public class PerformanceItemModel : IComparer
    {
        public object Tag { get; }
        public string Name { get; }
        public int SortIndex { get; }
        public string Input { get; }
        public string Output { get; }
        public string What { get; }
        public TimeSpan Duration { get; }
        public PerformanceItemModel(object tag, string name, int sortIndex, string input, string output, string what, TimeSpan duration)
        {
            Tag = tag;
            Name = name;
            SortIndex = sortIndex;
            Input = input;
            Output = output;
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
