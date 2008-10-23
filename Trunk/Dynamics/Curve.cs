#if(!XNA)
using System;
using System.Collections;
using System.Collections.Generic;

namespace FarseerGames.FarseerPhysics.Dynamics
{
    public class Curve
    {
        private CurveKeyCollection _keys = new CurveKeyCollection();
        private CurveLoopType _postLoop;
        private CurveLoopType _preLoop;

        public bool IsConstant
        {
            get { return (_keys.Count <= 1); }
        }

        public CurveKeyCollection Keys
        {
            get { return _keys; }
        }

        public CurveLoopType PostLoop
        {
            get { return _postLoop; }
            set { _postLoop = value; }
        }

        public CurveLoopType PreLoop
        {
            get { return _preLoop; }
            set { _preLoop = value; }
        }

        private float CalcCycle(float t)
        {
            float num = (t - _keys[0].position) * _keys.InvTimeRange;
            if (num < 0f)
            {
                num--;
            }
            int num2 = (int)num;
            return num2;
        }

        public Curve Clone()
        {
            Curve curve = new Curve();
            curve._preLoop = _preLoop;
            curve._postLoop = _postLoop;
            curve._keys = _keys.Clone();
            return curve;
        }

        public void ComputeTangent(int keyIndex, CurveTangent tangentType)
        {
            ComputeTangent(keyIndex, tangentType, tangentType);
        }

        /// <exception cref="ArgumentOutOfRangeException"><c>keyIndex</c> is out of range.</exception>
        public void ComputeTangent(int keyIndex, CurveTangent tangentInType, CurveTangent tangentOutType)
        {
            float num2;
            float num4;
            float num7;
            float num8;
            if ((_keys.Count <= keyIndex) || (keyIndex < 0))
            {
                throw new ArgumentOutOfRangeException("keyIndex");
            }
            CurveKey key = Keys[keyIndex];
            float position = num8 = num4 = key.Position;
            float num = num7 = num2 = key.Value;
            if (keyIndex > 0)
            {
                position = Keys[keyIndex - 1].Position;
                num = Keys[keyIndex - 1].Value;
            }
            if ((keyIndex + 1) < _keys.Count)
            {
                num4 = Keys[keyIndex + 1].Position;
                num2 = Keys[keyIndex + 1].Value;
            }
            if (tangentInType == CurveTangent.Smooth)
            {
                float num10 = num4 - position;
                float num6 = num2 - num;
                if (Math.Abs(num6) < 1.192093E-07f)
                {
                    key.TangentIn = 0f;
                }
                else
                {
                    key.TangentIn = (num6 * Math.Abs((position - num8))) / num10;
                }
            }
            else if (tangentInType == CurveTangent.Linear)
            {
                key.TangentIn = num7 - num;
            }
            else
            {
                key.TangentIn = 0f;
            }
            if (tangentOutType == CurveTangent.Smooth)
            {
                float num9 = num4 - position;
                float num5 = num2 - num;
                if (Math.Abs(num5) < 1.192093E-07f)
                {
                    key.TangentOut = 0f;
                }
                else
                {
                    key.TangentOut = (num5 * Math.Abs((num4 - num8))) / num9;
                }
            }
            else if (tangentOutType == CurveTangent.Linear)
            {
                key.TangentOut = num2 - num7;
            }
            else
            {
                key.TangentOut = 0f;
            }
        }

        public void ComputeTangents(CurveTangent tangentType)
        {
            ComputeTangents(tangentType, tangentType);
        }

        public void ComputeTangents(CurveTangent tangentInType, CurveTangent tangentOutType)
        {
            for (int i = 0; i < Keys.Count; i++)
            {
                ComputeTangent(i, tangentInType, tangentOutType);
            }
        }

        public float Evaluate(float position)
        {
            if (_keys.Count == 0)
            {
                return 0f;
            }
            if (_keys.Count == 1)
            {
                return _keys[0].internalValue;
            }
            CurveKey key = _keys[0];
            CurveKey key2 = _keys[_keys.Count - 1];
            float t = position;
            float num6 = 0f;
            if (t < key.position)
            {
                if (_preLoop == CurveLoopType.Constant)
                {
                    return key.internalValue;
                }
                if (_preLoop == CurveLoopType.Linear)
                {
                    return (key.internalValue - (key.tangentIn * (key.position - t)));
                }
                if (!_keys.IsCacheAvailable)
                {
                    _keys.ComputeCacheValues();
                }
                float num5 = CalcCycle(t);
                float num3 = t - (key.position + (num5 * _keys.TimeRange));
                if (_preLoop == CurveLoopType.Cycle)
                {
                    t = key.position + num3;
                }
                else if (_preLoop == CurveLoopType.CycleOffset)
                {
                    t = key.position + num3;
                    num6 = (key2.internalValue - key.internalValue) * num5;
                }
                else
                {
                    t = ((((int)num5) & 1) != 0) ? (key2.position - num3) : (key.position + num3);
                }
            }
            else if (key2.position < t)
            {
                if (_postLoop == CurveLoopType.Constant)
                {
                    return key2.internalValue;
                }
                if (_postLoop == CurveLoopType.Linear)
                {
                    return (key2.internalValue - (key2.tangentOut * (key2.position - t)));
                }
                if (!_keys.IsCacheAvailable)
                {
                    _keys.ComputeCacheValues();
                }
                float num4 = CalcCycle(t);
                float num2 = t - (key.position + (num4 * _keys.TimeRange));
                if (_postLoop == CurveLoopType.Cycle)
                {
                    t = key.position + num2;
                }
                else if (_postLoop == CurveLoopType.CycleOffset)
                {
                    t = key.position + num2;
                    num6 = (key2.internalValue - key.internalValue) * num4;
                }
                else
                {
                    t = ((((int)num4) & 1) != 0) ? (key2.position - num2) : (key.position + num2);
                }
            }
            CurveKey key4 = null;
            CurveKey key3 = null;
            t = FindSegment(t, ref key4, ref key3);
            return (num6 + Hermite(key4, key3, t));
        }

        private float FindSegment(float t, ref CurveKey k0, ref CurveKey k1)
        {
            float num2 = t;
            k0 = _keys[0];
            for (int i = 1; i < _keys.Count; i++)
            {
                k1 = _keys[i];
                if (k1.position >= t)
                {
                    double position = k0.position;
                    double num6 = k1.position;
                    double num5 = t;
                    double num3 = num6 - position;
                    num2 = 0f;
                    if (num3 > 1E-10)
                    {
                        num2 = (float)((num5 - position) / num3);
                    }
                    return num2;
                }
                k0 = k1;
            }
            return num2;
        }

        private static float Hermite(CurveKey k0, CurveKey k1, float t)
        {
            if (k0.Continuity == CurveContinuity.Step)
            {
                if (t >= 1f)
                {
                    return k1.internalValue;
                }
                return k0.internalValue;
            }
            float num = t * t;
            float num2 = num * t;
            float internalValue = k0.internalValue;
            float num5 = k1.internalValue;
            float tangentOut = k0.tangentOut;
            float tangentIn = k1.tangentIn;
            return ((((internalValue * (((2f * num2) - (3f * num)) + 1f)) + (num5 * ((-2f * num2) + (3f * num)))) +
                     (tangentOut * ((num2 - (2f * num)) + t))) + (tangentIn * (num2 - num)));
        }
    }

    public class CurveKey : IEquatable<CurveKey>, IComparable<CurveKey>
    {
        internal CurveContinuity continuity;
        internal float internalValue;
        internal float position;
        internal float tangentIn;
        internal float tangentOut;

        public CurveKey(float position, float value)
        {
            this.position = position;
            internalValue = value;
        }

        public CurveKey(float position, float value, float tangentIn, float tangentOut)
        {
            this.position = position;
            internalValue = value;
            this.tangentIn = tangentIn;
            this.tangentOut = tangentOut;
        }

        public CurveKey(float position, float value, float tangentIn, float tangentOut, CurveContinuity continuity)
        {
            this.position = position;
            internalValue = value;
            this.tangentIn = tangentIn;
            this.tangentOut = tangentOut;
            this.continuity = continuity;
        }

        public CurveContinuity Continuity
        {
            get { return continuity; }
            set { continuity = value; }
        }

        public float Position
        {
            get { return position; }
        }

        public float TangentIn
        {
            get { return tangentIn; }
            set { tangentIn = value; }
        }

        public float TangentOut
        {
            get { return tangentOut; }
            set { tangentOut = value; }
        }

        public float Value
        {
            get { return internalValue; }
            set { internalValue = value; }
        }

        #region IComparable<CurveKey> Members

        public int CompareTo(CurveKey other)
        {
            if (position == other.position)
            {
                return 0;
            }
            if (position >= other.position)
            {
                return 1;
            }
            return -1;
        }

        #endregion

        #region IEquatable<CurveKey> Members

        public bool Equals(CurveKey other)
        {
            return (((((other != null) && (other.position == position)) &&
                      ((other.internalValue == internalValue) && (other.tangentIn == tangentIn))) &&
                     (other.tangentOut == tangentOut)) && (other.continuity == continuity));
        }

        #endregion

        public CurveKey Clone()
        {
            return new CurveKey(position, internalValue, tangentIn, tangentOut, continuity);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CurveKey);
        }

        public override int GetHashCode()
        {
            return ((((position.GetHashCode() + internalValue.GetHashCode()) + tangentIn.GetHashCode()) +
                     tangentOut.GetHashCode()) + continuity.GetHashCode());
        }

        public static bool operator ==(CurveKey a, CurveKey b)
        {
            bool flag3 = null == a;
            bool flag2 = null == b;
            if (flag3 || flag2)
            {
                return (flag3 == flag2);
            }
            return a.Equals(b);
        }

        public static bool operator !=(CurveKey a, CurveKey b)
        {
            bool flag3 = a == null;
            bool flag2 = b == null;
            if (flag3 || flag2)
            {
                return (flag3 != flag2);
            }
            return ((((a.position != b.position) || (a.internalValue != b.internalValue)) ||
                     ((a.tangentIn != b.tangentIn) || (a.tangentOut != b.tangentOut))) || (a.continuity != b.continuity));
        }
    }


    public class CurveKeyCollection : ICollection<CurveKey>
    {
        internal float InvTimeRange;
        internal bool IsCacheAvailable = true;
        private List<CurveKey> _keys = new List<CurveKey>();
        internal float TimeRange;

        /// <exception cref="ArgumentNullException"><c>value</c> is null.</exception>
        public CurveKey this[int index]
        {
            get { return _keys[index]; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                if (_keys[index].Position == value.Position)
                {
                    _keys[index] = value;
                }
                else
                {
                    _keys.RemoveAt(index);
                    Add(value);
                }
            }
        }

        #region ICollection<CurveKey> Members

        /// <exception cref="ArgumentNullException"><c>item</c> is null.</exception>
        public void Add(CurveKey item)
        {
            if (item == null)
            {
                throw new ArgumentNullException();
            }
            int index = _keys.BinarySearch(item);
            if (index >= 0)
            {
                while ((index < _keys.Count) && (item.Position == _keys[index].Position))
                {
                    index++;
                }
            }
            else
            {
                index = ~index;
            }
            _keys.Insert(index, item);
            IsCacheAvailable = false;
        }

        public void Clear()
        {
            _keys.Clear();
            TimeRange = InvTimeRange = 0f;
            IsCacheAvailable = false;
        }

        public bool Contains(CurveKey item)
        {
            return _keys.Contains(item);
        }

        public void CopyTo(CurveKey[] array, int arrayIndex)
        {
            _keys.CopyTo(array, arrayIndex);
            IsCacheAvailable = false;
        }

        public IEnumerator<CurveKey> GetEnumerator()
        {
            return _keys.GetEnumerator();
        }

        public bool Remove(CurveKey item)
        {
            IsCacheAvailable = false;
            return _keys.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _keys.GetEnumerator();
        }

        // Properties
        public int Count
        {
            get { return _keys.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion

        public CurveKeyCollection Clone()
        {
            CurveKeyCollection keys = new CurveKeyCollection();
            keys._keys = new List<CurveKey>(_keys);
            keys.InvTimeRange = InvTimeRange;
            keys.TimeRange = TimeRange;
            keys.IsCacheAvailable = true;
            return keys;
        }

        internal void ComputeCacheValues()
        {
            TimeRange = InvTimeRange = 0f;
            if (_keys.Count > 1)
            {
                TimeRange = _keys[_keys.Count - 1].Position - _keys[0].Position;
                if (TimeRange > float.Epsilon)
                {
                    InvTimeRange = 1f / TimeRange;
                }
            }
            IsCacheAvailable = true;
        }

        public int IndexOf(CurveKey item)
        {
            return _keys.IndexOf(item);
        }

        public void RemoveAt(int index)
        {
            _keys.RemoveAt(index);
            IsCacheAvailable = false;
        }
    }

    public enum CurveLoopType
    {
        Constant,
        Cycle,
        CycleOffset,
        Oscillate,
        Linear
    }

    public enum CurveTangent
    {
        Flat,
        Linear,
        Smooth
    }

    public enum CurveContinuity
    {
        Smooth,
        Step
    }
}
#endif