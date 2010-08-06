/*
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com 
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
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common
{
    public static class MathUtils
    {
        public static float Cross(Vector2 a, Vector2 b)
        {
            return a.X*b.Y - a.Y*b.X;
        }

        public static Vector2 Cross(Vector2 a, float s)
        {
            return new Vector2(s*a.Y, -s*a.X);
        }

        public static Vector2 Cross(float s, Vector2 a)
        {
            return new Vector2(-s*a.Y, s*a.X);
        }

        public static Vector2 Abs(Vector2 v)
        {
            return new Vector2(Math.Abs(v.X), Math.Abs(v.Y));
        }

        public static Vector2 Multiply(ref Mat22 A, Vector2 v)
        {
            return new Vector2(A.col1.X*v.X + A.col2.X*v.Y, A.col1.Y*v.X + A.col2.Y*v.Y);
        }

        public static Vector2 MultiplyT(ref Mat22 A, Vector2 v)
        {
            return new Vector2(Vector2.Dot(v, A.col1), Vector2.Dot(v, A.col2));
        }

        public static Vector2 Multiply(ref Transform T, Vector2 v)
        {
            float x = T.Position.X + T.R.col1.X*v.X + T.R.col2.X*v.Y;
            float y = T.Position.Y + T.R.col1.Y*v.X + T.R.col2.Y*v.Y;

            return new Vector2(x, y);
        }

        public static Vector2 MultiplyT(ref Transform T, Vector2 v)
        {
            return MultiplyT(ref T.R, v - T.Position);
        }

        // A^T * B
        public static void MultiplyT(ref Mat22 A, ref Mat22 B, out Mat22 C)
        {
            Vector2 c1 = new Vector2(Vector2.Dot(A.col1, B.col1), Vector2.Dot(A.col2, B.col1));
            Vector2 c2 = new Vector2(Vector2.Dot(A.col1, B.col2), Vector2.Dot(A.col2, B.col2));
            C = new Mat22(c1, c2);
        }

        public static void MultiplyT(ref Transform A, ref Transform B, out Transform C)
        {
            Mat22 R;
            MultiplyT(ref A.R, ref B.R, out R);
            C = new Transform(B.Position - A.Position, ref R);
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }

        /// This function is used to ensure that a floating point number is
        /// not a NaN or infinity.
        public static bool IsValid(float x)
        {
            if (float.IsNaN(x))
            {
                // NaN.
                return false;
            }

            return !float.IsInfinity(x);
        }

        public static bool IsValid(this Vector2 x)
        {
            return IsValid(x.X) && IsValid(x.Y);
        }


        /// This is a approximate yet fast inverse square-root.
        public static float InvSqrt(float x)
        {
            FloatConverter convert = new FloatConverter();
            convert.x = x;
            float xhalf = 0.5f*x;
            convert.i = 0x5f3759df - (convert.i >> 1);
            x = convert.x;
            x = x*(1.5f - xhalf*x*x);
            return x;
        }

        public static int Clamp(int a, int low, int high)
        {
            return Math.Max(low, Math.Min(a, high));
        }

        public static float Clamp(float a, float low, float high)
        {
            return Math.Max(low, Math.Min(a, high));
        }

        public static Vector2 Clamp(Vector2 a, Vector2 low, Vector2 high)
        {
            return Vector2.Max(low, Vector2.Min(a, high));
        }

        public static void Cross(ref Vector2 a, ref Vector2 b, out float c)
        {
            c = a.X*b.Y - a.Y*b.X;
        }

        public static float Area(Vector2 a, Vector2 b, Vector2 c)
        {
            return (((b.X - a.X)*(c.Y - a.Y)) - ((c.X - a.X)*(b.Y - a.Y)));
        }

        /// <summary>
        /// Determines if three vertices are collinear (ie. on a straight line)
        /// </summary>
        /// <param name="a">First vertex</param>
        /// <param name="b">Second vertex</param>
        /// <param name="c">Third vertex</param>
        /// <returns></returns>
        public static bool Collinear(Vector2 a, Vector2 b, Vector2 c)
        {
            return Collinear(a, b, c, 0);
        }

        public static bool Collinear(Vector2 a, Vector2 b, Vector2 c, float tolerance)
        {
            return Area(a, b, c) <= tolerance;
        }


        public static void Cross(float s, ref Vector2 a, out Vector2 b)
        {
            b = new Vector2(-s*a.Y, s*a.X);
        }

        #region Nested type: FloatConverter

        [StructLayout(LayoutKind.Explicit)]
        internal struct FloatConverter
        {
            [FieldOffset(0)] public float x;
            [FieldOffset(0)] public int i;
        }

        #endregion
    }

    /// A 2-by-2 matrix. Stored in column-major order.
    public struct Mat22
    {
        public Vector2 col1, col2;

        /// construct this matrix using columns.
        public Mat22(Vector2 c1, Vector2 c2)
        {
            col1 = c1;
            col2 = c2;
        }

        /// construct this matrix using scalars.
        public Mat22(float a11, float a12, float a21, float a22)
        {
            col1 = new Vector2(a11, a21);
            col2 = new Vector2(a12, a22);
        }

        /// construct this matrix using an angle. This matrix becomes
        /// an orthonormal rotation matrix.
        public Mat22(float angle)
        {
            // TODO_ERIN compute sin+cos together.
            float c = (float) Math.Cos(angle), s = (float) Math.Sin(angle);
            col1 = new Vector2(c, s);
            col2 = new Vector2(-s, c);
        }

        /// Initialize this matrix using columns.
        public void Set(Vector2 c1, Vector2 c2)
        {
            col1 = c1;
            col2 = c2;
        }

        /// Initialize this matrix using an angle. This matrix becomes
        /// an orthonormal rotation matrix.
        public void Set(float angle)
        {
            float c = (float) Math.Cos(angle), s = (float) Math.Sin(angle);
            col1.X = c;
            col2.X = -s;
            col1.Y = s;
            col2.Y = c;
        }

        /// Set this to the identity matrix.
        public void SetIdentity()
        {
            col1.X = 1.0f;
            col2.X = 0.0f;
            col1.Y = 0.0f;
            col2.Y = 1.0f;
        }

        /// Set this matrix to all zeros.
        public void SetZero()
        {
            col1.X = 0.0f;
            col2.X = 0.0f;
            col1.Y = 0.0f;
            col2.Y = 0.0f;
        }

        /// Extract the angle from this matrix (assumed to be
        /// a rotation matrix).
        public float GetAngle()
        {
            return (float) Math.Atan2(col1.Y, col1.X);
        }

        public Mat22 GetInverse()
        {
            float a = col1.X, b = col2.X, c = col1.Y, d = col2.Y;
            float det = a*d - b*c;
            if (det != 0.0f)
            {
                det = 1.0f/det;
            }

            return new Mat22(new Vector2(det*d, -det*c), new Vector2(-det*b, det*a));
        }

        /// Solve A * x = b, where b is a column vector. This is more efficient
        /// than computing the inverse in one-shot cases.
        public Vector2 Solve(Vector2 b)
        {
            float a11 = col1.X, a12 = col2.X, a21 = col1.Y, a22 = col2.Y;
            float det = a11*a22 - a12*a21;
            if (det != 0.0f)
            {
                det = 1.0f/det;
            }

            return new Vector2(det*(a22*b.X - a12*b.Y), det*(a11*b.Y - a21*b.X));
        }

        public static void Add(ref Mat22 A, ref Mat22 B, out Mat22 R)
        {
            R = new Mat22(A.col1 + B.col1, A.col2 + B.col2);
        }
    }

    /// A 3-by-3 matrix. Stored in column-major order.
    public struct Mat33
    {
        public Vector3 col1, col2, col3;

        /// construct this matrix using columns.
        public Mat33(Vector3 c1, Vector3 c2, Vector3 c3)
        {
            col1 = c1;
            col2 = c2;
            col3 = c3;
        }

        /// Set this matrix to all zeros.
        public void SetZero()
        {
            col1 = Vector3.Zero;
            col2 = Vector3.Zero;
            col3 = Vector3.Zero;
        }

        /// Solve A * x = b, where b is a column vector. This is more efficient
        /// than computing the inverse in one-shot cases.
        public Vector3 Solve33(Vector3 b)
        {
            float det = Vector3.Dot(col1, Vector3.Cross(col2, col3));
            if (det != 0.0f)
            {
                det = 1.0f/det;
            }

            return new Vector3(det*Vector3.Dot(b, Vector3.Cross(col2, col3)),
                               det*Vector3.Dot(col1, Vector3.Cross(b, col3)),
                               det*Vector3.Dot(col1, Vector3.Cross(col2, b)));
        }

        /// Solve A * x = b, where b is a column vector. This is more efficient
        /// than computing the inverse in one-shot cases. Solve only the upper
        /// 2-by-2 matrix equation.
        public Vector2 Solve22(Vector2 b)
        {
            float a11 = col1.X, a12 = col2.X, a21 = col1.Y, a22 = col2.Y;
            float det = a11*a22 - a12*a21;

            if (det != 0.0f)
            {
                det = 1.0f/det;
            }

            return new Vector2(det*(a22*b.X - a12*b.Y), det*(a11*b.Y - a21*b.X));
        }
    }

    /// A transform contains translation and rotation. It is used to represent
    /// the position and orientation of rigid frames.
    public struct Transform
    {
        public Vector2 Position;
        public Mat22 R;

        /// Initialize using a position vector and a rotation matrix.
        public Transform(Vector2 position, ref Mat22 r)
        {
            Position = position;
            R = r;
        }

        /// Set this to the identity transform.
        public void SetIdentity()
        {
            Position = Vector2.Zero;
            R.SetIdentity();
        }

        /// Set this based on the position and angle.
        public void Set(Vector2 p, float angle)
        {
            Position = p;
            R.Set(angle);
        }

        /// Calculate the angle that the rotation matrix represents.
        public float GetAngle()
        {
            return (float) Math.Atan2(R.col1.Y, R.col1.X);
        }
    }

    /// This describes the motion of a body/shape for TOI computation.
    /// Shapes are defined with respect to the body origin, which may
    /// no coincide with the center of mass. However, to support dynamics
    /// we must interpolate the center of mass position.
    public struct Sweep
    {
        ///< center world positions
        public float a;

        ///< world angles
        ///< center world positions
        public float a0;

        ///< world angles
        ///< local center of mass position
        public Vector2 c;

        ///< local center of mass position
        public Vector2 c0;

        public Vector2 localCenter;

        /// Get the interpolated transform at a specific time.
        /// @param alpha is a factor in [0,1], where 0 indicates t0.
        public void GetTransform(out Transform xf, float alpha)
        {
            xf = new Transform();
            xf.Position = (1.0f - alpha)*c0 + alpha*c;
            float angle = (1.0f - alpha)*a0 + alpha*a;
            xf.R.Set(angle);

            // Shift to origin
            xf.Position -= MathUtils.Multiply(ref xf.R, localCenter);
        }

        /// Advance the sweep forward, yielding a new initial state.
        /// @param t the new initial time.
        public void Advance(float t)
        {
            c0 = (1.0f - t)*c0 + t*c;
            a0 = (1.0f - t)*a0 + t*a;
        }

        /// Normalize the angles.
        public void Normalize()
        {
            float twoPi = 2.0f*(float) Math.PI;
            float d = twoPi*(float) Math.Floor(a0/twoPi);
            a0 -= d;
            a -= d;
        }
    }
}