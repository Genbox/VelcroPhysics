using System.Collections.Generic;

namespace FarseerGames.FarseerPhysics
{
    /// <summary>
    /// Pool used to cache objects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Pool<T> where T : new()
    {
        private Stack<T> _stack;

        public Pool()
        {
            _stack = new Stack<T>();
        }

        public int Count
        {
            get { return _stack.Count; }
        }

        public Pool(int size)
        {
            _stack = new Stack<T>(size);
            for (int i = 0; i < size; i++)
            {
                _stack.Push(new T());
            }
        }

        public T Fetch()
        {
            if (_stack.Count > 0)
            {
                return _stack.Pop();
            }
            return new T();
        }

        public void Insert(T item)
        {
            _stack.Push(item);
        }
    }
}