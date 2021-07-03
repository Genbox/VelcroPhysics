using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Genbox.VelcroPhysics.Shared
{
    public class Pool<T>
    {
        private readonly Func<T> _objectCreator;
        private readonly Action<T> _objectReset;
        private readonly Queue<T> _queue;

        public Pool(Func<T> objectCreator, Action<T> objectReset = null, int capacity = 16, bool preCreateInstances = true)
        {
            _objectCreator = objectCreator;
            _objectReset = objectReset;
            _queue = new Queue<T>(capacity);

            if (!preCreateInstances)
                return;

            for (int i = 0; i < capacity; i++)
            {
                T obj = objectCreator();
                _queue.Enqueue(obj);
            }
        }

        public int LeftInPool => _queue.Count;

        public T GetFromPool(bool reset = false)
        {
            if (_queue.Count == 0)
                return _objectCreator();

            T obj = _queue.Dequeue();

            if (reset)
                _objectReset?.Invoke(obj);

            return obj;
        }

        public IEnumerable<T> GetManyFromPool(int count)
        {
            Debug.Assert(count != 0);

            for (int i = 0; i < count; i++)
            {
                yield return GetFromPool();
            }
        }

        public void ReturnToPool(T obj, bool reset = true)
        {
            if (reset)
                _objectReset?.Invoke(obj);

            _queue.Enqueue(obj);
        }

        public void ReturnToPool(IEnumerable<T> objs, bool reset = true)
        {
            foreach (T obj in objs)
            {
                ReturnToPool(obj, reset);
            }
        }
    }
}