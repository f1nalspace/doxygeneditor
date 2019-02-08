﻿using System.Collections.Generic;

namespace TSP.DoxygenEditor.Extensions
{
    static class LinkedListExtensions
    {
        public static void AddRange<T>(this LinkedList<T> list, IEnumerable<T> items)
        {
            foreach (T item in items)
                list.AddLast(item);
        }
    }
}
