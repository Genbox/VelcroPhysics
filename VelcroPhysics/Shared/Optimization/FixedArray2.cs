using System;
using System.Collections;
using System.Collections.Generic;

namespace VelcroPhysics.Shared.Optimization
{
    public struct FixedArray2<T> : IEnumerable<T>
    {
        public T Value0, Value1;

        public T this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return Value0;
                    case 1:
                        return Value1;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        Value0 = value;
                        break;
                    case 1:
                        Value1 = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T value)
        {
            for (int i = 0; i < 2; ++i)
                if (this[i].Equals(value))
                    return i;
            return -1;
        }

        public void Clear()
        {
            Value0 = Value1 = default(T);
        }

        private IEnumerable<T> Enumerate()
        {
            for (int i = 0; i < 2; ++i)
                yield return this[i];
        }
    }
}