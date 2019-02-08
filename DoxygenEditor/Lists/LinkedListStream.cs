﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using TSP.DoxygenEditor.Extensions;

namespace TSP.DoxygenEditor.Lists
{
    class LinkedListStream<T> where T : class
    {
        private readonly LinkedList<T> _items = new LinkedList<T>();
        private LinkedListNode<T> _currentNode;
        public T CurrentValue
        {
            get
            {
                T result = _currentNode?.Value;
                return (result);
            }
        }
        public LinkedListNode<T> CurrentNode => _currentNode;

        public bool IsEOF
        {
            get
            {
                bool result = (_items.Count == 0) || (_currentNode == null);
                return (result);
            }
        }

        public LinkedListStream(IEnumerable<T> items)
        {
            _items.AddRange(items);
            _currentNode = _items.First;
        }

        public T Peek()
        {
            if (_currentNode != null)
            {
                T v = _currentNode.Value;
                return (v);
            }
            return (default(T));
        }

        public X Peek<X>() where X : T
        {
            if (_currentNode != null)
            {
                T v = _currentNode.Value;
                if (v != null && typeof(X).Equals(v.GetType()))
                    return ((X)v);
            }
            return (default(X));
        }

        public T Peek(Func<T, bool> func)
        {
            LinkedListNode<T> n = _currentNode;
            while (n != null)
            {
                T v = n.Value;
                if (v != null && func(v))
                    return (v);
                n = n.Next;
            }
            return (default(T));
        }

        public T Next()
        {
            if (_currentNode != null)
                _currentNode = _currentNode.Next;
            else
                _currentNode = null;
            if (_currentNode != null)
            {
                T v = _currentNode.Value;
                return (v);
            }
            return (default(T));
        }

        public X Next<X>() where X : T
        {
            if (_currentNode != null)
                _currentNode = _currentNode.Next;
            else
                _currentNode = null;
            if (_currentNode != null)
            {
                T v = _currentNode.Value;
                if (v != null && typeof(X).Equals(v.GetType()))
                    return ((X)v);
            }
            return (default(X));
        }
    }
}
