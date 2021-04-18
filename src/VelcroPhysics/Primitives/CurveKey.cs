#if STANDARD_IMPLEMENTATION

// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'MONOGAME LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.Serialization;

// ReSharper disable once CheckNamespace
namespace Microsoft.Xna.Framework
{
    /// <summary>Key point on the <see cref="Curve" />.</summary>
    [DataContract]
    public class CurveKey : IEquatable<CurveKey>, IComparable<CurveKey>
    {
        /// <summary>Compares whether two <see cref="CurveKey" /> instances are not equal.</summary>
        /// <param name="value1"><see cref="CurveKey" /> instance on the left of the not equal sign.</param>
        /// <param name="value2"><see cref="CurveKey" /> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
        public static bool operator !=(CurveKey value1, CurveKey value2)
        {
            return !(value1 == value2);
        }

        /// <summary>Compares whether two <see cref="CurveKey" /> instances are equal.</summary>
        /// <param name="value1"><see cref="CurveKey" /> instance on the left of the equal sign.</param>
        /// <param name="value2"><see cref="CurveKey" /> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(CurveKey value1, CurveKey value2)
        {
            if (Equals(value1, null))
                return Equals(value2, null);

            if (Equals(value2, null))
                return Equals(value1, null);

            return value1.Position == value2.Position
                   && value1.Value == value2.Value
                   && value1.TangentIn == value2.TangentIn
                   && value1.TangentOut == value2.TangentOut
                   && value1.Continuity == value2.Continuity;
        }

        /// <summary>Creates a copy of this key.</summary>
        /// <returns>A copy of this key.</returns>
        public CurveKey Clone()
        {
            return new CurveKey(Position, Value, TangentIn, TangentOut, Continuity);
        }

        /// <summary>
        /// Gets or sets the indicator whether the segment between this point and the next point on the curve is discrete
        /// or continuous.
        /// </summary>
        [DataMember]
        public CurveContinuity Continuity { get; set; }

        /// <summary>Gets a position of the key on the curve.</summary>
        [DataMember]
        public float Position { get; }

        /// <summary>Gets or sets a tangent when approaching this point from the previous point on the curve.</summary>
        [DataMember]
        public float TangentIn { get; set; }

        /// <summary>Gets or sets a tangent when leaving this point to the next point on the curve.</summary>
        [DataMember]
        public float TangentOut { get; set; }

        /// <summary>Gets a value of this point.</summary>
        [DataMember]
        public float Value { get; set; }

        /// <summary>Creates a new instance of <see cref="CurveKey" /> class with position: 0 and value: 0.</summary>
        public CurveKey() : this(0, 0)
        {
            // This parameterless constructor is needed for correct serialization of CurveKeyCollection and CurveKey.
        }

        /// <summary>Creates a new instance of <see cref="CurveKey" /> class.</summary>
        /// <param name="position">Position on the curve.</param>
        /// <param name="value">Value of the control point.</param>
        public CurveKey(float position, float value)
            : this(position, value, 0, 0, CurveContinuity.Smooth) { }

        /// <summary>Creates a new instance of <see cref="CurveKey" /> class.</summary>
        /// <param name="position">Position on the curve.</param>
        /// <param name="value">Value of the control point.</param>
        /// <param name="tangentIn">Tangent approaching point from the previous point on the curve.</param>
        /// <param name="tangentOut">Tangent leaving point toward next point on the curve.</param>
        public CurveKey(float position, float value, float tangentIn, float tangentOut)
            : this(position, value, tangentIn, tangentOut, CurveContinuity.Smooth) { }

        /// <summary>Creates a new instance of <see cref="CurveKey" /> class.</summary>
        /// <param name="position">Position on the curve.</param>
        /// <param name="value">Value of the control point.</param>
        /// <param name="tangentIn">Tangent approaching point from the previous point on the curve.</param>
        /// <param name="tangentOut">Tangent leaving point toward next point on the curve.</param>
        /// <param name="continuity">Indicates whether the curve is discrete or continuous.</param>
        public CurveKey(float position, float value, float tangentIn, float tangentOut, CurveContinuity continuity)
        {
            Position = position;
            Value = value;
            TangentIn = tangentIn;
            TangentOut = tangentOut;
            Continuity = continuity;
        }

        public int CompareTo(CurveKey other)
        {
            return Position.CompareTo(other.Position);
        }

        public bool Equals(CurveKey other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return obj is CurveKey key && Equals(key);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode() ^ Value.GetHashCode() ^ TangentIn.GetHashCode() ^
                   TangentOut.GetHashCode() ^ Continuity.GetHashCode();
        }
    }
}
#endif