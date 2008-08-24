using System;
using System.Collections.Generic;
using System.Text;

namespace FarseerGames.FarseerPhysics {
    public class Pool<T> where T : new() {
        Stack<T> stack;

        public Pool() {
            stack = new Stack<T>();
        }

        public Pool(int size) {
            stack = new Stack<T>(size);
            for (int i = 0; i < size; i++) {
                stack.Push(new T());
            }
        }

        public T Fetch() {
            if (stack.Count > 0) {
                return stack.Pop();
            }
            else {
                return new T();
            }
        }

        public void Release(T item){
            stack.Push(item);
        }
    }
}
