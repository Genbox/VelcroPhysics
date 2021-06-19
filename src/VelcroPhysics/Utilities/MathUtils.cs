/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using System;
using Genbox.VelcroPhysics.Shared;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Utilities
{
    public static class MathUtils
    {
        /// <summary>Perform the cross product on two vectors.</summary>
        public static float Cross(ref Vector2 a, ref Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        /// <summary>Perform the cross product on two vectors.</summary>
        public static float Cross(Vector2 a, Vector2 b)
        {
            return Cross(ref a, ref b);
        }

        /// <summary>Perform the cross product on two vectors.</summary>
        public static Vector3 Cross(Vector3 a, Vector3 b)
        {
            return new Vector3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
        }

        /// <summary>Perform the cross product on two vectors.</summary>
        public static Vector2 Cross(Vector2 a, float s)
        {
            return new Vector2(s * a.Y, -s * a.X);
        }

        /// <summary>Perform the cross product on two vectors.</summary>
        public static Vector2 Cross(float s, Vector2 a)
        {
            return new Vector2(-s * a.Y, s * a.X);
        }

        public static Vector2 Abs(Vector2 v)
        {
            return new Vector2(Math.Abs(v.X), Math.Abs(v.Y));
        }

        public static float Abs(float value)
        {
            return Math.Abs(value);
        }

        public static Vector2 Mul(ref Mat22 a, Vector2 v)
        {
            return Mul(ref a, ref v);
        }

        public static Vector2 Mul(ref Mat22 a, ref Vector2 v)
        {
            return new Vector2(a.ex.X * v.X + a.ey.X * v.Y, a.ex.Y * v.X + a.ey.Y * v.Y);
        }

        public static Vector2 Mul(ref Transform T, Vector2 v)
        {
            return Mul(ref T, ref v);
        }

        public static Vector2 Mul(ref Transform T, ref Vector2 v)
        {
            float x = T.q.c * v.X - T.q.s * v.Y + T.p.X;
            float y = T.q.s * v.X + T.q.c * v.Y + T.p.Y;

            return new Vector2(x, y);
        }

        public static Vector2 MulT(ref Mat22 a, Vector2 v)
        {
            return MulT(ref a, ref v);
        }

        public static Vector2 MulT(ref Mat22 a, ref Vector2 v)
        {
            return new Vector2(v.X * a.ex.X + v.Y * a.ex.Y, v.X * a.ey.X + v.Y * a.ey.Y);
        }

        public static Vector2 MulT(ref Transform T, Vector2 v)
        {
            return MulT(ref T, ref v);
        }

        public static Vector2 MulT(ref Transform T, ref Vector2 v)
        {
            float px = v.X - T.p.X;
            float py = v.Y - T.p.Y;
            float x = T.q.c * px + T.q.s * py;
            float y = -T.q.s * px + T.q.c * py;

            return new Vector2(x, y);
        }

        // A^T * B
        public static void MulT(ref Mat22 a, ref Mat22 b, out Mat22 c)
        {
            c = new Mat22();
            c.ex.X = a.ex.X * b.ex.X + a.ex.Y * b.ex.Y;
            c.ex.Y = a.ey.X * b.ex.X + a.ey.Y * b.ex.Y;
            c.ey.X = a.ex.X * b.ey.X + a.ex.Y * b.ey.Y;
            c.ey.Y = a.ey.X * b.ey.X + a.ey.Y * b.ey.Y;
        }

        /// <summary>Multiply a matrix times a vector.</summary>
        public static Vector3 Mul(Mat33 a, Vector3 v)
        {
            return v.X * a.ex + v.Y * a.ey + v.Z * a.ez;
        }

        public static Transform Mul(Transform a, Transform b)
        {
            // v2 = A.q.Rot(B.q.Rot(v1) + B.p) + A.p
            //    = (A.q * B.q).Rot(v1) + A.q.Rot(B.p) + A.p

            Transform c = new Transform();
            c.q = Mul(a.q, b.q);
            c.p = Mul(a.q, b.p) + a.p;
            return c;
        }

        public static void MulT(ref Transform a, ref Transform b, out Transform c)
        {
            // v2 = A.q' * (B.q * v1 + B.p - A.p)
            //    = A.q' * B.q * v1 + A.q' * (B.p - A.p)

            c = new Transform();
            c.q = MulT(a.q, b.q);
            c.p = MulT(a.q, b.p - a.p);
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }

        /// <summary>Multiply a matrix times a vector.</summary>
        public static Vector2 Mul22(Mat33 a, Vector2 v)
        {
            return new Vector2(a.ex.X * v.X + a.ey.X * v.Y, a.ex.Y * v.X + a.ey.Y * v.Y);
        }

        /// <summary>Multiply two rotations: q * r</summary>
        public static Rot Mul(Rot q, Rot r)
        {
            // [qc -qs] * [rc -rs] = [qc*rc-qs*rs -qc*rs-qs*rc]
            // [qs  qc]   [rs  rc]   [qs*rc+qc*rs -qs*rs+qc*rc]
            // s = qs * rc + qc * rs
            // c = qc * rc - qs * rs
            Rot qr;
            qr.s = q.s * r.c + q.c * r.s;
            qr.c = q.c * r.c - q.s * r.s;
            return qr;
        }

        public static Vector2 MulT(Transform T, Vector2 v)
        {
            float px = v.X - T.p.X;
            float py = v.Y - T.p.Y;
            float x = T.q.c * px + T.q.s * py;
            float y = -T.q.s * px + T.q.c * py;

            return new Vector2(x, y);
        }

        /// <summary>Transpose multiply two rotations: qT * r</summary>
        public static Rot MulT(Rot q, Rot r)
        {
            // [ qc qs] * [rc -rs] = [qc*rc+qs*rs -qc*rs+qs*rc]
            // [-qs qc]   [rs  rc]   [-qs*rc+qc*rs qs*rs+qc*rc]
            // s = qc * rs - qs * rc
            // c = qc * rc + qs * rs
            Rot qr;
            qr.s = q.c * r.s - q.s * r.c;
            qr.c = q.c * r.c + q.s * r.s;
            return qr;
        }

        public static Transform MulT(Transform a, Transform b)
        {
            // v2 = A.q' * (B.q * v1 + B.p - A.p)
            //    = A.q' * B.q * v1 + A.q' * (B.p - A.p)

            Transform c = new Transform();
            c.q = MulT(a.q, b.q);
            c.p = MulT(a.q, b.p - a.p);
            return c;
        }

        /// <summary>Rotate a vector</summary>
        /// <param name="q">The rotation matrix</param>
        /// <param name="v">The value</param>
        public static Vector2 Mul(Rot q, Vector2 v)
        {
            return new Vector2(q.c * v.X - q.s * v.Y, q.s * v.X + q.c * v.Y);
        }

        /// <summary>Inverse rotate a vector</summary>
        /// <param name="q">The rotation matrix</param>
        /// <param name="v">The value</param>
        public static Vector2 MulT(Rot q, Vector2 v)
        {
            return new Vector2(q.c * v.X + q.s * v.Y, -q.s * v.X + q.c * v.Y);
        }

        /// <summary>Get the skew vector such that dot(skew_vec, other) == cross(vec, other)</summary>
        public static Vector2 Skew(Vector2 input)
        {
            return new Vector2(-input.Y, input.X);
        }

        /// <summary>This function is used to ensure that a floating point number is not a NaN or infinity.</summary>
        /// <param name="x">The x.</param>
        /// <returns><c>true</c> if the specified x is valid; otherwise, <c>false</c>.</returns>
        public static bool IsValid(float x)
        {
            if (float.IsNaN(x))
                return false;

            return !float.IsInfinity(x);
        }

        public static bool IsValid(this Vector2 x)
        {
            return IsValid(x.X) && IsValid(x.Y);
        }

        public static int Clamp(int a, int low, int high)
        {
            return Math.Max(low, Math.Min(a, high));
        }

        public static float Clamp(float a, float low, float high)
        {
            return Max(low, Min(a, high));
        }

        public static Vector2 Clamp(Vector2 a, Vector2 low, Vector2 high)
        {
            return Vector2.Max(low, Vector2.Min(a, high));
        }

        public static void Cross(ref Vector2 a, ref Vector2 b, out float c)
        {
            c = a.X * b.Y - a.Y * b.X;
        }

        /// <summary>
        /// Return the angle between two vectors on a plane The angle is from vector 1 to vector 2, positive anticlockwise
        /// The result is between -pi -> pi
        /// </summary>
        public static double VectorAngle(ref Vector2 p1, ref Vector2 p2)
        {
            double theta1 = Math.Atan2(p1.Y, p1.X);
            double theta2 = Math.Atan2(p2.Y, p2.X);
            double dtheta = theta2 - theta1;

            while (dtheta > MathConstants.Pi)
            {
                dtheta -= MathConstants.TwoPi;
            }

            while (dtheta < -MathConstants.Pi)
            {
                dtheta += MathConstants.TwoPi;
            }

            return dtheta;
        }

        /// <summary>Perform the dot product on two vectors.</summary>
        public static float Dot(Vector3 a, Vector3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        /// <summary>Perform the dot product on two vectors.</summary>
        public static float Dot(ref Vector2 a, ref Vector2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        /// <summary>Perform the dot product on two vectors.</summary>
        public static float Dot(Vector2 a, Vector2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static double VectorAngle(Vector2 p1, Vector2 p2)
        {
            return VectorAngle(ref p1, ref p2);
        }

        /// <summary>Returns a positive number if c is to the left of the line going from a to b.</summary>
        /// <returns>Positive number if point is left, negative if point is right, and 0 if points are collinear.</returns>
        public static float Area(Vector2 a, Vector2 b, Vector2 c)
        {
            return Area(ref a, ref b, ref c);
        }

        /// <summary>Returns a positive number if c is to the left of the line going from a to b.</summary>
        /// <returns>Positive number if point is left, negative if point is right, and 0 if points are collinear.</returns>
        public static float Area(ref Vector2 a, ref Vector2 b, ref Vector2 c)
        {
            return a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y);
        }

        /// <summary>Determines if three vertices are collinear (ie. on a straight line)</summary>
        public static bool IsCollinear(ref Vector2 a, ref Vector2 b, ref Vector2 c, float tolerance = 0)
        {
            return FloatInRange(Area(ref a, ref b, ref c), -tolerance, tolerance);
        }

        public static void Cross(float s, ref Vector2 a, out Vector2 b)
        {
            b = new Vector2(-s * a.Y, s * a.X);
        }

        public static bool FloatEquals(float value1, float value2)
        {
            return Math.Abs(value1 - value2) <= MathConstants.Epsilon;
        }

        /// <summary>Checks if a floating point Value is equal to another, within a certain tolerance.</summary>
        /// <returns>True if the values are "equal", false otherwise.</returns>
        public static bool FloatEquals(float value1, float value2, float delta)
        {
            return FloatInRange(value1, value2 - delta, value2 + delta);
        }

        /// <summary>Checks if a floating point Value is within a specified range of values (inclusive).</summary>
        /// <param name="value">The Value to check.</param>
        /// <param name="min">The minimum Value.</param>
        /// <param name="max">The maximum Value.</param>
        /// <returns>True if the Value is within the range specified, false otherwise.</returns>
        public static bool FloatInRange(float value, float min, float max)
        {
            return value >= min && value <= max;
        }

        public static Vector2 Mul(ref Rot rot, Vector2 axis)
        {
            return Mul(rot, axis);
        }

        public static Vector2 MulT(ref Rot rot, Vector2 axis)
        {
            return MulT(rot, axis);
        }

        public static float Distance(Vector2 a, Vector2 b)
        {
            Vector2 c = a - b;
            return c.Length();
        }

        public static float Distance(ref Vector2 a, ref Vector2 b)
        {
            Vector2 c = a - b;
            return c.Length();
        }

        public static float DistanceSquared(ref Vector2 a, ref Vector2 b)
        {
            Vector2 c = a - b;
            return Dot(ref c, ref c);
        }

        public static float Max(float valueA, float valueB)
        {
            return Math.Max(valueA, valueB);
        }

        public static int Max(int valueA, int valueB)
        {
            return Math.Max(valueA, valueB);
        }

        public static float Min(float valueA, float valueB)
        {
            return Math.Min(valueA, valueB);
        }

        public static int Min(int valueA, int valueB)
        {
            return Math.Min(valueA, valueB);
        }

        public static int Sign(float value)
        {
            return Math.Sign(value);
        }

        /// <summary>
        /// Convert this vector into a unit vector. Returns the length.
        /// </summary>
        public static float Normalize(ref Vector2 v)
        {
            float length = v.Length();
            if (length < MathConstants.Epsilon)
            {
                return 0.0f;
            }
            float invLength = 1.0f / length;
            v.X *= invLength;
            v.Y *= invLength;

            return length;
        }

        public static float Sqrt(float value)
        {
            return (float)Math.Sqrt(value);
        }

        public static float Cosf(float value)
        {
            return (float)Math.Cos(value);
        }

        public static float Sinf(float value)
        {
            return (float)Math.Sin(value);
        }

        public static float Ceil(float log)
        {
            return (float)Math.Ceiling(log);
        }

        public static float Log(float log)
        {
            return (float)Math.Log(log);
        }
    }
}