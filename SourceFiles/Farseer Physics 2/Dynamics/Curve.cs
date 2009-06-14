#if(!XNA)

#region License

/*
MIT License
Copyright © 2006 The Mono.Xna Team

All rights reserved.

Authors:
Olivier Dufour (Duff)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

#endregion License

using System;
using System.Collections;
using System.Collections.Generic;

namespace FarseerGames.FarseerPhysics.Dynamics
{
    public class Curve
    {
        private CurveKeyCollection _keys;
        private CurveLoopType _postLoop;
        private CurveLoopType _preLoop;

        public Curve()
        {
            _keys = new CurveKeyCollection();
        }

        public bool IsConstant
        {
            get { return _keys.Count <= 1; }
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

        public Curve Clone()
        {
            Curve curve = new Curve();

            curve._keys = _keys.Clone();
            curve._preLoop = _preLoop;
            curve._postLoop = _postLoop;

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
                    key.TangentIn = (num6*Math.Abs((position - num8)))/num10;
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
                    key.TangentOut = (num5*Math.Abs((num4 - num8)))/num9;
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
            CurveKey first = _keys[0];
            CurveKey last = _keys[_keys.Count - 1];

            if (position < first.Position)
            {
                switch (PreLoop)
                {
                    case CurveLoopType.Constant:
                        //constant
                        return first.Value;

                    case CurveLoopType.Linear:
                        // linear y = a*x +b with a tangeant of last point
                        return first.Value - first.TangentIn*(first.Position - position);

                    case CurveLoopType.Cycle:
                        //start -> end / start -> end
                        int cycle = GetNumberOfCycle(position);
                        float virtualPos = position - (cycle*(last.Position - first.Position));
                        return GetCurvePosition(virtualPos);

                    case CurveLoopType.CycleOffset:
                        //make the curve continue (with no step) so must up the curve each cycle of delta(value)
                        cycle = GetNumberOfCycle(position);
                        virtualPos = position - (cycle*(last.Position - first.Position));
                        return (GetCurvePosition(virtualPos) + cycle*(last.Value - first.Value));

                    case CurveLoopType.Oscillate:
                        //go back on curve from end and target start
                        // start-> end / end -> start
                        cycle = GetNumberOfCycle(position);
                        if (0 == cycle%2f) //if pair
                            virtualPos = position - (cycle*(last.Position - first.Position));
                        else
                            virtualPos = last.Position - position + first.Position +
                                         (cycle*(last.Position - first.Position));
                        return GetCurvePosition(virtualPos);
                }
            }
            else if (position > last.Position)
            {
                int cycle;
                switch (PostLoop)
                {
                    case CurveLoopType.Constant:
                        //constant
                        return last.Value;

                    case CurveLoopType.Linear:
                        // linear y = a*x +b with a tangeant of last point
                        return last.Value + first.TangentOut*(position - last.Position);

                    case CurveLoopType.Cycle:
                        //start -> end / start -> end
                        cycle = GetNumberOfCycle(position);
                        float virtualPos = position - (cycle*(last.Position - first.Position));
                        return GetCurvePosition(virtualPos);

                    case CurveLoopType.CycleOffset:
                        //make the curve continue (with no step) so must up the curve each cycle of delta(value)
                        cycle = GetNumberOfCycle(position);
                        virtualPos = position - (cycle*(last.Position - first.Position));
                        return (GetCurvePosition(virtualPos) + cycle*(last.Value - first.Value));

                    case CurveLoopType.Oscillate:
                        //go back on curve from end and target start
                        // start-> end / end -> start
                        cycle = GetNumberOfCycle(position);
                        //virtualPos = position - (cycle*(last.Position - first.Position));
                        if (0 == cycle%2f) //if pair
                            virtualPos = position - (cycle*(last.Position - first.Position));
                        else
                            virtualPos = last.Position - position + first.Position +
                                         (cycle*(last.Position - first.Position));
                        return GetCurvePosition(virtualPos);
                }
            }

            //in curve
            return GetCurvePosition(position);
        }

        private int GetNumberOfCycle(float position)
        {
            float cycle = (position - _keys[0].Position)/(_keys[_keys.Count - 1].Position - _keys[0].Position);
            if (cycle < 0f)
                cycle--;
            return (int) cycle;
        }

        private float GetCurvePosition(float position)
        {
            //only for position in curve
            CurveKey prev = _keys[0];
            CurveKey next;
            for (int i = 1; i < _keys.Count; i++)
            {
                next = Keys[i];
                if (next.Position >= position)
                {
                    if (prev.Continuity == CurveContinuity.Step)
                    {
                        if (position >= 1f)
                        {
                            return next.Value;
                        }
                        return prev.Value;
                    }
                    float t = (position - prev.Position)/(next.Position - prev.Position); //to have t in [0,1]
                    float ts = t*t;
                    float tss = ts*t;
                    //After a lot of search on internet I have found all about spline function
                    // and bezier (phi'sss ancien) but finaly use hermite curve
                    //http://en.wikipedia.org/wiki/Cubic_Hermite_spline
                    //P(t) = (2*t^3 - 3t^2 + 1)*P0 + (t^3 - 2t^2 + t)m0 + (-2t^3 + 3t^2)P1 + (t^3-t^2)m1
                    //with P0.value = prev.value , m0 = prev.tangentOut, P1= next.value, m1 = next.TangentIn
                    return (2*tss - 3*ts + 1f)*prev.Value + (tss - 2*ts + t)*prev.TangentOut + (3*ts - 2*tss)*next.Value +
                           (tss - ts)*next.TangentIn;
                }
                prev = next;
            }
            return 0f;
        }
    }

    public class CurveKey : IEquatable<CurveKey>, IComparable<CurveKey>
    {
        private CurveContinuity _continuity;
        private float _position;
        private float _tangentIn;
        private float _tangentOut;
        private float _value;

        public CurveKey(float position, float value)
            : this(position, value, 0, 0, CurveContinuity.Smooth)
        {
        }

        public CurveKey(float position, float value, float tangentIn, float tangentOut)
            : this(position, value, tangentIn, tangentOut, CurveContinuity.Smooth)
        {
        }

        public CurveKey(float position, float value, float tangentIn, float tangentOut, CurveContinuity continuity)
        {
            _position = position;
            _value = value;
            _tangentIn = tangentIn;
            _tangentOut = tangentOut;
            _continuity = continuity;
        }

        public CurveContinuity Continuity
        {
            get { return _continuity; }
            set { _continuity = value; }
        }

        public float Position
        {
            get { return _position; }
        }

        public float TangentIn
        {
            get { return _tangentIn; }
            set { _tangentIn = value; }
        }

        public float TangentOut
        {
            get { return _tangentOut; }
            set { _tangentOut = value; }
        }

        public float Value
        {
            get { return _value; }
            set { _value = value; }
        }

        #region IComparable<CurveKey> Members

        public int CompareTo(CurveKey other)
        {
            return _position.CompareTo(other._position);
        }

        #endregion

        #region IEquatable<CurveKey> Members

        public bool Equals(CurveKey other)
        {
            return (this == other);
        }

        #endregion

        public static bool operator !=(CurveKey a, CurveKey b)
        {
            return !(a == b);
        }

        public static bool operator ==(CurveKey a, CurveKey b)
        {
            if (Equals(a, null))
                return Equals(b, null);

            if (Equals(b, null))
                return Equals(a, null);

            return (a._position == b._position)
                   && (a._value == b._value)
                   && (a._tangentIn == b._tangentIn)
                   && (a._tangentOut == b._tangentOut)
                   && (a._continuity == b._continuity);
        }

        public CurveKey Clone()
        {
            return new CurveKey(_position, _value, _tangentIn, _tangentOut, _continuity);
        }

        public override bool Equals(object obj)
        {
            return (obj is CurveKey) ? ((CurveKey) obj) == this : false;
        }

        public override int GetHashCode()
        {
            return _position.GetHashCode() ^ _value.GetHashCode() ^ _tangentIn.GetHashCode() ^
                   _tangentOut.GetHashCode() ^ _continuity.GetHashCode();
        }
    }

    public class CurveKeyCollection : ICollection<CurveKey>
    {
        private const bool _isReadOnly = false;
        private List<CurveKey> _innerlist;

        public CurveKeyCollection()
        {
            _innerlist = new List<CurveKey>();
        }

        /// <exception cref="ArgumentNullException"><c>value</c> is null.</exception>
        /// <exception cref="IndexOutOfRangeException"><c>IndexOutOfRangeException</c>.</exception>
        public CurveKey this[int index]
        {
            get { return _innerlist[index]; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                if (index >= _innerlist.Count)
                    throw new IndexOutOfRangeException();

                if (_innerlist[index].Position == value.Position)
                    _innerlist[index] = value;
                else
                {
                    _innerlist.RemoveAt(index);
                    _innerlist.Add(value);
                }
            }
        }

        #region ICollection<CurveKey> Members

        public int Count
        {
            get { return _innerlist.Count; }
        }

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
        }

        /// <exception cref="ArgumentNullException"><c>item</c> is null.</exception>
        public void Add(CurveKey item)
        {
            if (item == null)
                throw new ArgumentNullException();

            if (_innerlist.Count == 0)
            {
                _innerlist.Add(item);
                return;
            }

            for (int i = 0; i < _innerlist.Count; i++)
            {
                if (item.Position < _innerlist[i].Position)
                {
                    _innerlist.Insert(i, item);
                    return;
                }
            }

            _innerlist.Add(item);
        }

        public void Clear()
        {
            _innerlist.Clear();
        }

        public bool Contains(CurveKey item)
        {
            return _innerlist.Contains(item);
        }

        public void CopyTo(CurveKey[] array, int arrayIndex)
        {
            _innerlist.CopyTo(array, arrayIndex);
        }

        public IEnumerator<CurveKey> GetEnumerator()
        {
            return _innerlist.GetEnumerator();
        }

        public bool Remove(CurveKey item)
        {
            return _innerlist.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _innerlist.GetEnumerator();
        }

        #endregion

        public CurveKeyCollection Clone()
        {
            CurveKeyCollection ckc = new CurveKeyCollection();
            foreach (CurveKey key in _innerlist)
                ckc.Add(key);
            return ckc;
        }

        public int IndexOf(CurveKey item)
        {
            return _innerlist.IndexOf(item);
        }

        public void RemoveAt(int index)
        {
            _innerlist.RemoveAt(index);
        }
    }
}

#endif