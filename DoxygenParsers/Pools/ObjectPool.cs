using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TSP.DoxygenEditor.Pools
{
    public class ObjectPool<T> where T : class
    {
        private readonly ConcurrentQueue<T> _objects;
        private readonly Func<T> _generator;
        private int _capacity;
        public ObjectPool(Func<T> generator, int initialCapacity = 256)
        {
            _objects = new ConcurrentQueue<T>();
            _generator = generator;
            _capacity = initialCapacity;
        }

        public T Aquire()
        {
            if (_objects.Count == 0)
            {
                int newCapacity = Math.Max(_capacity * 2, 2);
                for (int i = 0; i < newCapacity; ++i)
                    _objects.Enqueue(_generator());
                _capacity = newCapacity;
            }
            T result;
            if (_objects.TryDequeue(out result))
                return (result);
            return _generator();
        }
        public void Release(IEnumerable<T> list)
        {
            foreach (T item in list)
                _objects.Enqueue(item);
        }
    }
}
