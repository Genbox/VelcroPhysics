using System;

namespace VelcroPhysics.Primitives.Optimization
{
    public struct FixedArray8<T>
    {
        private T _value0;
        private T _value1;
        private T _value2;
        private T _value3;
        private T _value4;
        private T _value5;
        private T _value6;
        private T _value7;

        public T this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    case 7:
                        return _value7;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = value;
                        break;
                    case 1:
                        _value1 = value;
                        break;
                    case 2:
                        _value2 = value;
                        break;
                    case 3:
                        _value3 = value;
                        break;
                    case 4:
                        _value4 = value;
                        break;
                    case 5:
                        _value5 = value;
                        break;
                    case 6:
                        _value6 = value;
                        break;
                    case 7:
                        _value7 = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }
    }
}