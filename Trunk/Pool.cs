using System.Collections.Generic;

namespace FarseerGames.FarseerPhysics
{
    public class Pool<T> where T : new()
    {
        private Stack<T> _stack;

        public Pool()
        {
            _stack = new Stack<T>();
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