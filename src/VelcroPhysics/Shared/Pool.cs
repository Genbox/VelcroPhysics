using System;
using System.Collections.Generic;
using System.Diagnostics;
using VelcroPhysics.Shared.Optimization;

namespace VelcroPhysics.Shared
{
    public class Pool<T> where T : IPoolable<T>
    {
        private readonly Queue<T> _queue;
        private readonly Func<T> _objectCreator;

        public Pool(Func<T> objectCreator, int capacity = 16, bool preCreateInstances = true)
        {
            _objectCreator = objectCreator;
            _queue = new Queue<T>(capacity);

            if (!preCreateInstances)
                return;

            for (int i = 0; i < capacity; i++)
            {
                T obj = objectCreator();
                obj.Pool = this;
                _queue.Enqueue(obj);
            }
        }

        public int LeftInPool => _queue.Count;

        public T GetFromPool()
        {
            if (_queue.Count == 0)
                return _objectCreator();

            return _queue.Dequeue();
        }

        public IEnumerable<T> GetManyFromPool(int count)
        {
            Debug.Assert(count != 0);

            for (int i = 0; i < count; i++)
            {
                yield return GetFromPool();
            }
        }

        public void ReturnToPool(T obj)
        {
            obj.Reset();
            _queue.Enqueue(obj);
        }

        public void ReturnToPool(IEnumerable<T> objs)
        {
            foreach (T obj in objs)
            {
                obj.Reset();
                _queue.Enqueue(obj);
            }
        }
    }
}
