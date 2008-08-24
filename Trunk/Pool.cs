using System.Collections.Generic;

namespace FarseerGames.FarseerPhysics
{
    public class Pool<T> where T : new()
    {
        private readonly Stack<T> stack;

        public Pool()
        {
            stack = new Stack<T>();
        }

        public Pool(int size)
        {
            stack = new Stack<T>(size);
            for (int i = 0; i < size; i++)
            {
                stack.Push(new T());
            }
        }

        public T Fetch()
        {
            if (stack.Count > 0)
            {
                return stack.Pop();
            }
            return new T();
        }

        public void Release(T item)
        {
            stack.Push(item);
        }
    }
}