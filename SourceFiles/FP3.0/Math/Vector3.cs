using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace FarseerPhysics.Math
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct Vector3 : IEquatable<Vector3>
    {
        public float X;
        public float Y;
        public float Z;
        private static Vector3 _zero;
        private static Vector3 _one;
        private static Vector3 _unitX;
        private static Vector3 _unitY;
        private static Vector3 _unitZ;
        private static Vector3 _up;
        private static Vector3 _down;
        private static Vector3 _right;
        private static Vector3 _left;
        private static Vector3 _forward;
        private static Vector3 _backward;

        public static Vector3 Zero
        {
            get { return _zero; }
        }

        public static Vector3 One
        {
            get { return _one; }
        }

        public static Vector3 UnitX
        {
            get { return _unitX; }
        }

        public static Vector3 UnitY
        {
            get { return _unitY; }
        }

        public static Vector3 UnitZ
        {
            get { return _unitZ; }
        }

        public static Vector3 Up
        {
            get { return _up; }
        }

        public static Vector3 Down
        {
            get { return _down; }
        }

        public static Vector3 Right
        {
            get { return _right; }
        }

        public static Vector3 Left
        {
            get { return _left; }
        }

        public static Vector3 Forward
        {
            get { return _forward; }
        }

        public static Vector3 Backward
        {
            get { return _backward; }
        }

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3(float value)
        {
            X = Y = Z = value;
        }

        public Vector3(Vector2 value, float z)
        {
            X = value.X;
            Y = value.Y;
            Z = z;
        }

        public override string ToString()
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            return string.Format(currentCulture, "{{X:{0} Y:{1} Z:{2}}}",
                                 new object[] { X.ToString(currentCulture), Y.ToString(currentCulture), Z.ToString(currentCulture) });
        }

        public bool Equals(Vector3 other)
        {
            return (((X == other.X) && (Y == other.Y)) && (Z == other.Z));
        }

        public override bool Equals(object obj)
        {
            bool flag = false;
            if (obj is Vector3)
            {
                flag = Equals((Vector3)obj);
            }
            return flag;
        }

        public override int GetHashCode()
        {
            return ((X.GetHashCode() + Y.GetHashCode()) + Z.GetHashCode());
        }

        public float Length()
        {
            float num = ((X * X) + (Y * Y)) + (Z * Z);
            return (float)System.Math.Sqrt((double)num);
        }

        public float LengthSquared()
        {
            return (((X * X) + (Y * Y)) + (Z * Z));
        }

        public static float Distance(Vector3 value1, Vector3 value2)
        {
            float num3 = value1.X - value2.X;
            float num2 = value1.Y - value2.Y;
            float num = value1.Z - value2.Z;
            float num4 = ((num3 * num3) + (num2 * num2)) + (num * num);
            return (float)System.Math.Sqrt((double)num4);
        }

        public static void Distance(ref Vector3 value1, ref Vector3 value2, out float result)
        {
            float num3 = value1.X - value2.X;
            float num2 = value1.Y - value2.Y;
            float num = value1.Z - value2.Z;
            float num4 = ((num3 * num3) + (num2 * num2)) + (num * num);
            result = (float)System.Math.Sqrt((double)num4);
        }

        public static float DistanceSquared(Vector3 value1, Vector3 value2)
        {
            float num3 = value1.X - value2.X;
            float num2 = value1.Y - value2.Y;
            float num = value1.Z - value2.Z;
            return (((num3 * num3) + (num2 * num2)) + (num * num));
        }

        public static void DistanceSquared(ref Vector3 value1, ref Vector3 value2, out float result)
        {
            float num3 = value1.X - value2.X;
            float num2 = value1.Y - value2.Y;
            float num = value1.Z - value2.Z;
            result = ((num3 * num3) + (num2 * num2)) + (num * num);
        }

        public static float Dot(Vector3 vector1, Vector3 vector2)
        {
            return (((vector1.X * vector2.X) + (vector1.Y * vector2.Y)) + (vector1.Z * vector2.Z));
        }

        public static void Dot(ref Vector3 vector1, ref Vector3 vector2, out float result)
        {
            result = ((vector1.X * vector2.X) + (vector1.Y * vector2.Y)) + (vector1.Z * vector2.Z);
        }

        public void Normalize()
        {
            float num2 = ((X * X) + (Y * Y)) + (Z * Z);
            float num = 1f / ((float)System.Math.Sqrt((double)num2));
            X *= num;
            Y *= num;
            Z *= num;
        }

        public static Vector3 Normalize(Vector3 value)
        {
            Vector3 vector;
            float num2 = ((value.X * value.X) + (value.Y * value.Y)) + (value.Z * value.Z);
            float num = 1f / ((float)System.Math.Sqrt((double)num2));
            vector.X = value.X * num;
            vector.Y = value.Y * num;
            vector.Z = value.Z * num;
            return vector;
        }

        public static void Normalize(ref Vector3 value, out Vector3 result)
        {
            float num2 = ((value.X * value.X) + (value.Y * value.Y)) + (value.Z * value.Z);
            float num = 1f / ((float)System.Math.Sqrt((double)num2));
            result.X = value.X * num;
            result.Y = value.Y * num;
            result.Z = value.Z * num;
        }

        public static Vector3 Cross(Vector3 vector1, Vector3 vector2)
        {
            Vector3 vector;
            vector.X = (vector1.Y * vector2.Z) - (vector1.Z * vector2.Y);
            vector.Y = (vector1.Z * vector2.X) - (vector1.X * vector2.Z);
            vector.Z = (vector1.X * vector2.Y) - (vector1.Y * vector2.X);
            return vector;
        }

        public static void Cross(ref Vector3 vector1, ref Vector3 vector2, out Vector3 result)
        {
            float num3 = (vector1.Y * vector2.Z) - (vector1.Z * vector2.Y);
            float num2 = (vector1.Z * vector2.X) - (vector1.X * vector2.Z);
            float num = (vector1.X * vector2.Y) - (vector1.Y * vector2.X);
            result.X = num3;
            result.Y = num2;
            result.Z = num;
        }

        public static Vector3 Reflect(Vector3 vector, Vector3 normal)
        {
            Vector3 vector2;
            float num = ((vector.X * normal.X) + (vector.Y * normal.Y)) + (vector.Z * normal.Z);
            vector2.X = vector.X - ((2f * num) * normal.X);
            vector2.Y = vector.Y - ((2f * num) * normal.Y);
            vector2.Z = vector.Z - ((2f * num) * normal.Z);
            return vector2;
        }

        public static void Reflect(ref Vector3 vector, ref Vector3 normal, out Vector3 result)
        {
            float num = ((vector.X * normal.X) + (vector.Y * normal.Y)) + (vector.Z * normal.Z);
            result.X = vector.X - ((2f * num) * normal.X);
            result.Y = vector.Y - ((2f * num) * normal.Y);
            result.Z = vector.Z - ((2f * num) * normal.Z);
        }

        public static Vector3 Min(Vector3 value1, Vector3 value2)
        {
            Vector3 vector;
            vector.X = (value1.X < value2.X) ? value1.X : value2.X;
            vector.Y = (value1.Y < value2.Y) ? value1.Y : value2.Y;
            vector.Z = (value1.Z < value2.Z) ? value1.Z : value2.Z;
            return vector;
        }

        public static void Min(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.X = (value1.X < value2.X) ? value1.X : value2.X;
            result.Y = (value1.Y < value2.Y) ? value1.Y : value2.Y;
            result.Z = (value1.Z < value2.Z) ? value1.Z : value2.Z;
        }

        public static Vector3 Max(Vector3 value1, Vector3 value2)
        {
            Vector3 vector;
            vector.X = (value1.X > value2.X) ? value1.X : value2.X;
            vector.Y = (value1.Y > value2.Y) ? value1.Y : value2.Y;
            vector.Z = (value1.Z > value2.Z) ? value1.Z : value2.Z;
            return vector;
        }

        public static void Max(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.X = (value1.X > value2.X) ? value1.X : value2.X;
            result.Y = (value1.Y > value2.Y) ? value1.Y : value2.Y;
            result.Z = (value1.Z > value2.Z) ? value1.Z : value2.Z;
        }

        public static Vector3 Clamp(Vector3 value1, Vector3 min, Vector3 max)
        {
            Vector3 vector;
            float x = value1.X;
            x = (x > max.X) ? max.X : x;
            x = (x < min.X) ? min.X : x;
            float y = value1.Y;
            y = (y > max.Y) ? max.Y : y;
            y = (y < min.Y) ? min.Y : y;
            float z = value1.Z;
            z = (z > max.Z) ? max.Z : z;
            z = (z < min.Z) ? min.Z : z;
            vector.X = x;
            vector.Y = y;
            vector.Z = z;
            return vector;
        }

        public static void Clamp(ref Vector3 value1, ref Vector3 min, ref Vector3 max, out Vector3 result)
        {
            float x = value1.X;
            x = (x > max.X) ? max.X : x;
            x = (x < min.X) ? min.X : x;
            float y = value1.Y;
            y = (y > max.Y) ? max.Y : y;
            y = (y < min.Y) ? min.Y : y;
            float z = value1.Z;
            z = (z > max.Z) ? max.Z : z;
            z = (z < min.Z) ? min.Z : z;
            result.X = x;
            result.Y = y;
            result.Z = z;
        }

        public static Vector3 Lerp(Vector3 value1, Vector3 value2, float amount)
        {
            Vector3 vector;
            vector.X = value1.X + ((value2.X - value1.X) * amount);
            vector.Y = value1.Y + ((value2.Y - value1.Y) * amount);
            vector.Z = value1.Z + ((value2.Z - value1.Z) * amount);
            return vector;
        }

        public static void Lerp(ref Vector3 value1, ref Vector3 value2, float amount, out Vector3 result)
        {
            result.X = value1.X + ((value2.X - value1.X) * amount);
            result.Y = value1.Y + ((value2.Y - value1.Y) * amount);
            result.Z = value1.Z + ((value2.Z - value1.Z) * amount);
        }

        public static Vector3 Barycentric(Vector3 value1, Vector3 value2, Vector3 value3, float amount1, float amount2)
        {
            Vector3 vector;
            vector.X = (value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X));
            vector.Y = (value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y));
            vector.Z = (value1.Z + (amount1 * (value2.Z - value1.Z))) + (amount2 * (value3.Z - value1.Z));
            return vector;
        }

        public static void Barycentric(ref Vector3 value1, ref Vector3 value2, ref Vector3 value3, float amount1,
                                       float amount2, out Vector3 result)
        {
            result.X = (value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X));
            result.Y = (value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y));
            result.Z = (value1.Z + (amount1 * (value2.Z - value1.Z))) + (amount2 * (value3.Z - value1.Z));
        }

        public static Vector3 SmoothStep(Vector3 value1, Vector3 value2, float amount)
        {
            Vector3 vector;
            amount = (amount > 1f) ? 1f : ((amount < 0f) ? 0f : amount);
            amount = (amount * amount) * (3f - (2f * amount));
            vector.X = value1.X + ((value2.X - value1.X) * amount);
            vector.Y = value1.Y + ((value2.Y - value1.Y) * amount);
            vector.Z = value1.Z + ((value2.Z - value1.Z) * amount);
            return vector;
        }

        public static void SmoothStep(ref Vector3 value1, ref Vector3 value2, float amount, out Vector3 result)
        {
            amount = (amount > 1f) ? 1f : ((amount < 0f) ? 0f : amount);
            amount = (amount * amount) * (3f - (2f * amount));
            result.X = value1.X + ((value2.X - value1.X) * amount);
            result.Y = value1.Y + ((value2.Y - value1.Y) * amount);
            result.Z = value1.Z + ((value2.Z - value1.Z) * amount);
        }

        public static Vector3 CatmullRom(Vector3 value1, Vector3 value2, Vector3 value3, Vector3 value4, float amount)
        {
            Vector3 vector;
            float num = amount * amount;
            float num2 = amount * num;
            vector.X = 0.5f *
                       ((((2f * value2.X) + ((-value1.X + value3.X) * amount)) +
                         (((((2f * value1.X) - (5f * value2.X)) + (4f * value3.X)) - value4.X) * num)) +
                        ((((-value1.X + (3f * value2.X)) - (3f * value3.X)) + value4.X) * num2));
            vector.Y = 0.5f *
                       ((((2f * value2.Y) + ((-value1.Y + value3.Y) * amount)) +
                         (((((2f * value1.Y) - (5f * value2.Y)) + (4f * value3.Y)) - value4.Y) * num)) +
                        ((((-value1.Y + (3f * value2.Y)) - (3f * value3.Y)) + value4.Y) * num2));
            vector.Z = 0.5f *
                       ((((2f * value2.Z) + ((-value1.Z + value3.Z) * amount)) +
                         (((((2f * value1.Z) - (5f * value2.Z)) + (4f * value3.Z)) - value4.Z) * num)) +
                        ((((-value1.Z + (3f * value2.Z)) - (3f * value3.Z)) + value4.Z) * num2));
            return vector;
        }

        public static void CatmullRom(ref Vector3 value1, ref Vector3 value2, ref Vector3 value3, ref Vector3 value4,
                                      float amount, out Vector3 result)
        {
            float num = amount * amount;
            float num2 = amount * num;
            result.X = 0.5f *
                       ((((2f * value2.X) + ((-value1.X + value3.X) * amount)) +
                         (((((2f * value1.X) - (5f * value2.X)) + (4f * value3.X)) - value4.X) * num)) +
                        ((((-value1.X + (3f * value2.X)) - (3f * value3.X)) + value4.X) * num2));
            result.Y = 0.5f *
                       ((((2f * value2.Y) + ((-value1.Y + value3.Y) * amount)) +
                         (((((2f * value1.Y) - (5f * value2.Y)) + (4f * value3.Y)) - value4.Y) * num)) +
                        ((((-value1.Y + (3f * value2.Y)) - (3f * value3.Y)) + value4.Y) * num2));
            result.Z = 0.5f *
                       ((((2f * value2.Z) + ((-value1.Z + value3.Z) * amount)) +
                         (((((2f * value1.Z) - (5f * value2.Z)) + (4f * value3.Z)) - value4.Z) * num)) +
                        ((((-value1.Z + (3f * value2.Z)) - (3f * value3.Z)) + value4.Z) * num2));
        }

        public static Vector3 Hermite(Vector3 value1, Vector3 tangent1, Vector3 value2, Vector3 tangent2, float amount)
        {
            Vector3 vector;
            float num = amount * amount;
            float num2 = amount * num;
            float num6 = ((2f * num2) - (3f * num)) + 1f;
            float num5 = (-2f * num2) + (3f * num);
            float num4 = (num2 - (2f * num)) + amount;
            float num3 = num2 - num;
            vector.X = (((value1.X * num6) + (value2.X * num5)) + (tangent1.X * num4)) + (tangent2.X * num3);
            vector.Y = (((value1.Y * num6) + (value2.Y * num5)) + (tangent1.Y * num4)) + (tangent2.Y * num3);
            vector.Z = (((value1.Z * num6) + (value2.Z * num5)) + (tangent1.Z * num4)) + (tangent2.Z * num3);
            return vector;
        }

        public static void Hermite(ref Vector3 value1, ref Vector3 tangent1, ref Vector3 value2, ref Vector3 tangent2,
                                   float amount, out Vector3 result)
        {
            float num = amount * amount;
            float num2 = amount * num;
            float num6 = ((2f * num2) - (3f * num)) + 1f;
            float num5 = (-2f * num2) + (3f * num);
            float num4 = (num2 - (2f * num)) + amount;
            float num3 = num2 - num;
            result.X = (((value1.X * num6) + (value2.X * num5)) + (tangent1.X * num4)) + (tangent2.X * num3);
            result.Y = (((value1.Y * num6) + (value2.Y * num5)) + (tangent1.Y * num4)) + (tangent2.Y * num3);
            result.Z = (((value1.Z * num6) + (value2.Z * num5)) + (tangent1.Z * num4)) + (tangent2.Z * num3);
        }

        public static Vector3 Negate(Vector3 value)
        {
            Vector3 vector;
            vector.X = -value.X;
            vector.Y = -value.Y;
            vector.Z = -value.Z;
            return vector;
        }

        public static void Negate(ref Vector3 value, out Vector3 result)
        {
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
        }

        public static Vector3 Add(Vector3 value1, Vector3 value2)
        {
            Vector3 vector;
            vector.X = value1.X + value2.X;
            vector.Y = value1.Y + value2.Y;
            vector.Z = value1.Z + value2.Z;
            return vector;
        }

        public static void Add(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
            result.Z = value1.Z + value2.Z;
        }

        public static Vector3 Subtract(Vector3 value1, Vector3 value2)
        {
            Vector3 vector;
            vector.X = value1.X - value2.X;
            vector.Y = value1.Y - value2.Y;
            vector.Z = value1.Z - value2.Z;
            return vector;
        }

        public static void Subtract(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
            result.Z = value1.Z - value2.Z;
        }

        public static Vector3 Multiply(Vector3 value1, Vector3 value2)
        {
            Vector3 vector;
            vector.X = value1.X * value2.X;
            vector.Y = value1.Y * value2.Y;
            vector.Z = value1.Z * value2.Z;
            return vector;
        }

        public static void Multiply(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;
            result.Z = value1.Z * value2.Z;
        }

        public static Vector3 Multiply(Vector3 value1, float scaleFactor)
        {
            Vector3 vector;
            vector.X = value1.X * scaleFactor;
            vector.Y = value1.Y * scaleFactor;
            vector.Z = value1.Z * scaleFactor;
            return vector;
        }

        public static void Multiply(ref Vector3 value1, float scaleFactor, out Vector3 result)
        {
            result.X = value1.X * scaleFactor;
            result.Y = value1.Y * scaleFactor;
            result.Z = value1.Z * scaleFactor;
        }

        public static Vector3 Divide(Vector3 value1, Vector3 value2)
        {
            Vector3 vector;
            vector.X = value1.X / value2.X;
            vector.Y = value1.Y / value2.Y;
            vector.Z = value1.Z / value2.Z;
            return vector;
        }

        public static void Divide(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
            result.Z = value1.Z / value2.Z;
        }

        public static Vector3 Divide(Vector3 value1, float value2)
        {
            Vector3 vector;
            float num = 1f / value2;
            vector.X = value1.X * num;
            vector.Y = value1.Y * num;
            vector.Z = value1.Z * num;
            return vector;
        }

        public static void Divide(ref Vector3 value1, float value2, out Vector3 result)
        {
            float num = 1f / value2;
            result.X = value1.X * num;
            result.Y = value1.Y * num;
            result.Z = value1.Z * num;
        }

        public static Vector3 operator -(Vector3 value)
        {
            Vector3 vector;
            vector.X = -value.X;
            vector.Y = -value.Y;
            vector.Z = -value.Z;
            return vector;
        }

        public static bool operator ==(Vector3 value1, Vector3 value2)
        {
            return (((value1.X == value2.X) && (value1.Y == value2.Y)) && (value1.Z == value2.Z));
        }

        public static bool operator !=(Vector3 value1, Vector3 value2)
        {
            if ((value1.X == value2.X) && (value1.Y == value2.Y))
            {
                return (value1.Z != value2.Z);
            }
            return true;
        }

        public static Vector3 operator +(Vector3 value1, Vector3 value2)
        {
            Vector3 vector;
            vector.X = value1.X + value2.X;
            vector.Y = value1.Y + value2.Y;
            vector.Z = value1.Z + value2.Z;
            return vector;
        }

        public static Vector3 operator -(Vector3 value1, Vector3 value2)
        {
            Vector3 vector;
            vector.X = value1.X - value2.X;
            vector.Y = value1.Y - value2.Y;
            vector.Z = value1.Z - value2.Z;
            return vector;
        }

        public static Vector3 operator *(Vector3 value1, Vector3 value2)
        {
            Vector3 vector;
            vector.X = value1.X * value2.X;
            vector.Y = value1.Y * value2.Y;
            vector.Z = value1.Z * value2.Z;
            return vector;
        }

        public static Vector3 operator *(Vector3 value, float scaleFactor)
        {
            Vector3 vector;
            vector.X = value.X * scaleFactor;
            vector.Y = value.Y * scaleFactor;
            vector.Z = value.Z * scaleFactor;
            return vector;
        }

        public static Vector3 operator *(float scaleFactor, Vector3 value)
        {
            Vector3 vector;
            vector.X = value.X * scaleFactor;
            vector.Y = value.Y * scaleFactor;
            vector.Z = value.Z * scaleFactor;
            return vector;
        }

        public static Vector3 operator /(Vector3 value1, Vector3 value2)
        {
            Vector3 vector;
            vector.X = value1.X / value2.X;
            vector.Y = value1.Y / value2.Y;
            vector.Z = value1.Z / value2.Z;
            return vector;
        }

        public static Vector3 operator /(Vector3 value, float divider)
        {
            Vector3 vector;
            float num = 1f / divider;
            vector.X = value.X * num;
            vector.Y = value.Y * num;
            vector.Z = value.Z * num;
            return vector;
        }

        static Vector3()
        {
            _zero = new Vector3();
            _one = new Vector3(1f, 1f, 1f);
            _unitX = new Vector3(1f, 0f, 0f);
            _unitY = new Vector3(0f, 1f, 0f);
            _unitZ = new Vector3(0f, 0f, 1f);
            _up = new Vector3(0f, 1f, 0f);
            _down = new Vector3(0f, -1f, 0f);
            _right = new Vector3(1f, 0f, 0f);
            _left = new Vector3(-1f, 0f, 0f);
            _forward = new Vector3(0f, 0f, -1f);
            _backward = new Vector3(0f, 0f, 1f);
        }
    }
}