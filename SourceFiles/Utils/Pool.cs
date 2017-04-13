using System;
using System.Collections.Generic;
using VelcroPhysics.Primitives.Optimization;

namespace VelcroPhysics.Utils
{
    public class Pool<T> where T : class, IPoolable, new()
    {
        private readonly Queue<T> _queue;

        public Pool(int capacity, bool createInstances = true)
        {
            _queue = new Queue<T>(capacity);

            if (createInstances)
            {
                for (int i = 0; i < capacity; i++)
                {
                    _queue.Enqueue(new T());
                }
            }
        }

        public T GetFromPool()
        {
            if (_queue.Count <= 0)
                return new T();

            T obj = _queue.Dequeue();
            obj.Reset();
            return obj;
        }

        public IEnumerable<T> GetManyFromPool(int count)
        {
            int diff = count - _queue.Count;

            if (diff >= 0)
            {
                //We have all in queue
                for (int i = 0; i < diff; i++)
                {
                    T obj = _queue.Dequeue();
                    obj.Reset();
                    yield return obj;
                }
            }
            else
            {
                //We need to return what is in queue and then make more
                for (int i = 0; i < _queue.Count; i++)
                {
                    T obj = _queue.Dequeue();
                    obj.Reset();
                    yield return obj;
                }

                int remaining = Math.Abs(diff);

                for (int i = 0; i < remaining; i++)
                {
                    yield return new T();
                }
            }
        }

        public void ReturnToPool(T obj)
        {
            _queue.Enqueue(obj);
        }

        public void ReturnToPool(IEnumerable<T> objs)
        {
            foreach (T obj in objs)
            {
                _queue.Enqueue(obj);
            }
        }
    }
}
