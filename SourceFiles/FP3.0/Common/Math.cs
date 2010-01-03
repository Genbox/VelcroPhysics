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

using Microsoft.Xna.Framework;
using System;

namespace FarseerPhysics
{
    public static class MathUtils
    {
        private static float tempFloat1;
        private static float tempFloat2;
        private static float tempFloat3;
        private static Vector2 tempVector1;
        private static Vector2 tempVector2;
        private static Vector2 tempVector3;

        public static float Cross(Vector2 a, Vector2 b)
        {
            Cross(ref a, ref b, out tempFloat1);
            return tempFloat1;
        }

        public static Vector2 Cross(Vector2 a, float s)
        {
            Cross(ref a, s, out tempVector1);
            return tempVector1;
        }

        public static Vector2 Cross(float s, Vector2 a)
        {
            Cross(s, ref a, out tempVector1);
            return tempVector1;
        }

        //Ref versions
        public static void Cross(ref Vector2 a, ref Vector2 b, out float c)
        {
            c = a.X * b.Y - a.Y * b.X;
        }

        public static void Cross(ref Vector2 a, float s, out Vector2 b)
        {
            b = new Vector2(s * a.Y, -s * a.X);
        }

        public static void Cross(float s, ref Vector2 a, out Vector2 b)
        {
            b = new Vector2(-s * a.Y, s * a.X);
        }

        public static Vector2 Abs(Vector2 v)
        {
            Abs(ref v, out tempVector1);
            return tempVector1;
        }

        //Ref versions
        public static void Abs(ref Vector2 v, out Vector2 r)
        {
            r = new Vector2(Math.Abs(v.X), Math.Abs(v.Y));
        }

        public static Vector2 Multiply(ref Mat22 A, Vector2 v)
        {
            return new Vector2(A.Col1.X * v.X + A.Col2.X * v.Y, A.Col1.Y * v.X + A.Col2.Y * v.Y);
        }

        public static Vector2 Multiply(ref Transform T, Vector2 v)
        {
            return new Vector2(T.Position.X + T.R.Col1.X * v.X + T.R.Col2.X * v.Y, T.Position.Y + T.R.Col1.Y * v.X + T.R.Col2.Y * v.Y);
        }

        public static Vector2 MultiplyT(ref Mat22 A, Vector2 v)
        {
            MultiplyT(ref A, ref v, out tempVector1);
            return tempVector1;
        }

        public static Vector2 MultiplyT(ref Transform T, Vector2 v)
        {
            MultiplyT(ref T, ref v, out tempVector1);
            return tempVector1;
        }

        //Ref versions
        public static void Multiply(ref Mat22 A, ref Vector2 v, out Vector2 r)
        {
            r = new Vector2(A.Col1.X * v.X + A.Col2.X * v.Y, A.Col1.Y * v.X + A.Col2.Y * v.Y);
        }

        public static void Multiply(ref Transform T, ref Vector2 v, out Vector2 r)
        {
            float x = T.Position.X + T.R.Col1.X * v.X + T.R.Col2.X * v.Y;
            float y = T.Position.Y + T.R.Col1.Y * v.X + T.R.Col2.Y * v.Y;

            r = new Vector2(x, y);
        }

        public static void MultiplyT(ref Mat22 A, ref Vector2 v, out Vector2 r)
        {
            Vector2.Dot(ref v, ref A.Col1, out tempFloat1);
            Vector2.Dot(ref v, ref A.Col2, out tempFloat2);

            r = new Vector2(tempFloat1, tempFloat2);
        }

        public static void MultiplyT(ref Transform T, ref Vector2 v, out Vector2 r)
        {
            Vector2.Subtract(ref v, ref T.Position, out tempVector1);
            MultiplyT(ref T.R, ref tempVector1, out r);
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
    }

    /// A 2-by-2 matrix. Stored in column-major order.
    public struct Mat22
    {
        /// Construct this matrix using columns.
        public Mat22(Vector2 c1, Vector2 c2)
        {
            Col1 = c1;
            Col2 = c2;
        }

        /// Construct this matrix using scalars.
        public Mat22(float a11, float a12, float a21, float a22)
        {
            Col1 = new Vector2(a11, a21);
            Col2 = new Vector2(a12, a22);
        }

        /// Construct this matrix using an angle. This matrix becomes
        /// an orthonormal rotation matrix.
        public Mat22(float angle)
        {
            // TODO_ERIN compute sin+cos together.
            float c = (float)Math.Cos(angle), s = (float)Math.Sin(angle);
            Col1 = new Vector2(c, s);
            Col2 = new Vector2(-s, c);
        }

        /// Initialize this matrix using columns.
        public void Set(Vector2 c1, Vector2 c2)
        {
            Col1 = c1;
            Col2 = c2;
        }

        /// Initialize this matrix using an angle. This matrix becomes
        /// an orthonormal rotation matrix.
        public void Set(float angle)
        {
            float c = (float)Math.Cos(angle), s = (float)Math.Sin(angle);
            Col1.X = c; Col2.X = -s;
            Col1.Y = s; Col2.Y = c;
        }

        /// Set this to the identity matrix.
        public void SetIdentity()
        {
            Col1.X = 1.0f; Col2.X = 0.0f;
            Col1.Y = 0.0f; Col2.Y = 1.0f;
        }

        /// Set this matrix to all zeros.
        public void SetZero()
        {
            Col1.X = 0.0f; Col2.X = 0.0f;
            Col1.Y = 0.0f; Col2.Y = 0.0f;
        }

        /// Extract the angle from this matrix (assumed to be
        /// a rotation matrix).
        public float GetAngle()
        {
            return (float)Math.Atan2(Col1.Y, Col1.X);
        }

        public Mat22 GetInverse()
        {
            float a = Col1.X, b = Col2.X, c = Col1.Y, d = Col2.Y;
            float det = a * d - b * c;
            if (det != 0.0f)
            {
                det = 1.0f / det;
            }

            return new Mat22(new Vector2(det * d, -det * c), new Vector2(-det * b, det * a));
        }

        /// Solve A * x = b, where b is a column vector. This is more efficient
        /// than computing the inverse in one-shot cases.
        public Vector2 Solve(Vector2 b)
        {
            float a11 = Col1.X, a12 = Col2.X, a21 = Col1.Y, a22 = Col2.Y;
            float det = a11 * a22 - a12 * a21;
            if (det != 0.0f)
            {
                det = 1.0f / det;
            }

            return new Vector2(det * (a22 * b.X - a12 * b.Y), det * (a11 * b.Y - a21 * b.X));
        }

        public static void Add(ref Mat22 A, ref Mat22 B, out Mat22 R)
        {
            R = new Mat22(A.Col1 + B.Col1, A.Col2 + B.Col2);
        }

        public Vector2 Col1, Col2;
    }

    /// A 3-by-3 matrix. Stored in column-major order.
    public struct Mat33
    {

        /// Construct this matrix using columns.
        public Mat33(Vector3 c1, Vector3 c2, Vector3 c3)
        {
            Col1 = c1;
            Col2 = c2;
            Col3 = c3;
        }

        /// Set this matrix to all zeros.
        public void SetZero()
        {
            Col1 = Vector3.Zero;
            Col2 = Vector3.Zero;
            Col3 = Vector3.Zero;
        }

        /// Solve A * x = b, where b is a column vector. This is more efficient
        /// than computing the inverse in one-shot cases.
        public Vector3 Solve33(Vector3 b)
        {
            float det = Vector3.Dot(Col1, Vector3.Cross(Col2, Col3));
            if (det != 0.0f)
            {
                det = 1.0f / det;
            }

            return new Vector3(det * Vector3.Dot(b, Vector3.Cross(Col2, Col3)),
                                det * Vector3.Dot(Col1, Vector3.Cross(b, Col3)),
                                det * Vector3.Dot(Col1, Vector3.Cross(Col2, b)));
        }

        /// Solve A * x = b, where b is a column vector. This is more efficient
        /// than computing the inverse in one-shot cases. Solve only the upper
        /// 2-by-2 matrix equation.
        public Vector2 Solve22(Vector2 b)
        {
            float a11 = Col1.X, a12 = Col2.X, a21 = Col1.Y, a22 = Col2.Y;
            float det = a11 * a22 - a12 * a21;

            if (det != 0.0f)
            {
                det = 1.0f / det;
            }

            return new Vector2(det * (a22 * b.X - a12 * b.Y), det * (a11 * b.Y - a21 * b.X));
        }

        public Vector3 Col1, Col2, Col3;
    }

    /// A transform contains translation and rotation. It is used to represent
    /// the position and orientation of rigid frames.
    public struct Transform
    {
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
            return (float)Math.Atan2(R.Col1.Y, R.Col1.X);
        }

        public Vector2 Position;
        public Mat22 R;
    }

    /// This describes the motion of a body/shape for TOI computation.
    /// Shapes are defined with respect to the body origin, which may
    /// no coincide with the center of mass. However, to support dynamics
    /// we must interpolate the center of mass position.
    public struct Sweep
    {
        /// Get the interpolated transform at a specific time.
        /// @param alpha is a factor in [0,1], where 0 indicates t0.
        public void GetTransform(out Transform xf, float alpha)
        {
            xf = new Transform();
            xf.Position = (1.0f - alpha) * Center0 + alpha * Center;
            float angle = (1.0f - alpha) * Angle0 + alpha * Angle;
            xf.R.Set(angle);

            // Shift to origin
            xf.Position -= MathUtils.Multiply(ref xf.R, LocalCenter);
        }

        /// Advance the sweep forward, yielding a new initial state.
        /// @param t the new initial time.
        public void Advance(float t)
        {
            if (TimeInt0 < t && 1.0f - TimeInt0 > Settings.Epsilon)
            {
                float alpha = (t - TimeInt0) / (1.0f - TimeInt0);
                Center0 = (1.0f - alpha) * Center0 + alpha * Center;
                Angle0 = (1.0f - alpha) * Angle0 + alpha * Angle;
                TimeInt0 = t;
            }
        }

        /// <summary>
        /// local center of mass position
        /// </summary>
        public Vector2 LocalCenter;
        
        /// <summary>
        /// Center world positions
        /// </summary>
        public Vector2 Center0, Center;

        /// <summary>
        /// world angles
        /// </summary>
        public float Angle0, Angle;
        
        /// <summary>
        /// time interval = [t0,1], where t0 is in [0,1]
        /// </summary>
        public float TimeInt0;
    }
}
