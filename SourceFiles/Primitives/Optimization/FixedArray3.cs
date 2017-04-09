using System;
using System.Collections;
using System.Collections.Generic;

namespace VelcroPhysics.Primitives.Optimization
{
    public struct FixedArray3<T> : IEnumerable<T>
    {
        public T Value0, Value1, Value2;

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
                    case 2:
                        return Value2;
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
                    case 2:
                        Value2 = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public bool Contains(T value)
        {
            for (int i = 0; i < 3; ++i)
                if (Equals(this[i], value))
                    return true;
            return false;
        }

        public int IndexOf(T value)
        {
            for (int i = 0; i < 3; ++i)
                if (Equals(this[i], value))
                    return i;
            return -1;
        }

        public void Clear()
        {
            Value0 = Value1 = Value2 = default(T);
        }

        public void Clear(T value)
        {
            for (int i = 0; i < 3; ++i)
                if (Equals(this[i], value))
                    this[i] = default(T);
        }

        private IEnumerable<T> Enumerate()
        {
            for (int i = 0; i < 3; ++i)
                yield return this[i];
        }
    }
}