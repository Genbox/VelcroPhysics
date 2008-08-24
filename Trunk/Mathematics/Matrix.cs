#if (!XNA)
using System;

namespace FarseerGames.FarseerPhysics.Mathematics
{
    public struct Matrix
    {
        public float M11;
        public float M12;
        public float M13;
        public float M14;
        public float M21;
        public float M22;
        public float M23;
        public float M24;
        public float M31;
        public float M32;
        public float M33;
        public float M34;
        public float M41;
        public float M42;
        public float M43;
        public float M44;
        private static Matrix _identity;       
        static Matrix()
        {
            _identity = new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);
        }

        public Matrix(float m11, float m12, float m13, float m14, float m21, float m22, float m23, float m24, float m31, float m32, float m33, float m34, float m41, float m42, float m43, float m44)
        {
            this.M11 = m11;
            this.M12 = m12;
            this.M13 = m13;
            this.M14 = m14;
            this.M21 = m21;
            this.M22 = m22;
            this.M23 = m23;
            this.M24 = m24;
            this.M31 = m31;
            this.M32 = m32;
            this.M33 = m33;
            this.M34 = m34;
            this.M41 = m41;
            this.M42 = m42;
            this.M43 = m43;
            this.M44 = m44;
        }

        public static Matrix Identity
        {
            get
            {
                return _identity;
            }
        }

        public static void CreateRotationZ(float radians, out Matrix result)
        {
            float num2 = (float)Math.Cos((double)radians);
            float num = (float)Math.Sin((double)radians);
            result.M11 = num2;
            result.M12 = num;
            result.M13 = 0f;
            result.M14 = 0f;
            result.M21 = -num;
            result.M22 = num2;
            result.M23 = 0f;
            result.M24 = 0f;
            result.M31 = 0f;
            result.M32 = 0f;
            result.M33 = 1f;
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
        }

        public static Matrix CreateRotationZ(float radians)
        {
            Matrix matrix;
            float num2 = (float)Math.Cos((double)radians);
            float num = (float)Math.Sin((double)radians);
            matrix.M11 = num2;
            matrix.M12 = num;
            matrix.M13 = 0f;
            matrix.M14 = 0f;
            matrix.M21 = -num;
            matrix.M22 = num2;
            matrix.M23 = 0f;
            matrix.M24 = 0f;
            matrix.M31 = 0f;
            matrix.M32 = 0f;
            matrix.M33 = 1f;
            matrix.M34 = 0f;
            matrix.M41 = 0f;
            matrix.M42 = 0f;
            matrix.M43 = 0f;
            matrix.M44 = 1f;
            return matrix;
        }

        public static void Invert(ref Matrix matrix, out Matrix result)
        {
            float num5 = matrix.M11;
            float num4 = matrix.M12;
            float num3 = matrix.M13;
            float num2 = matrix.M14;
            float num9 = matrix.M21;
            float num8 = matrix.M22;
            float num7 = matrix.M23;
            float num6 = matrix.M24;
            float num17 = matrix.M31;
            float num16 = matrix.M32;
            float num15 = matrix.M33;
            float num14 = matrix.M34;
            float num13 = matrix.M41;
            float num12 = matrix.M42;
            float num11 = matrix.M43;
            float num10 = matrix.M44;
            float num23 = (num15 * num10) - (num14 * num11);
            float num22 = (num16 * num10) - (num14 * num12);
            float num21 = (num16 * num11) - (num15 * num12);
            float num20 = (num17 * num10) - (num14 * num13);
            float num19 = (num17 * num11) - (num15 * num13);
            float num18 = (num17 * num12) - (num16 * num13);
            float num39 = ((num8 * num23) - (num7 * num22)) + (num6 * num21);
            float num38 = -(((num9 * num23) - (num7 * num20)) + (num6 * num19));
            float num37 = ((num9 * num22) - (num8 * num20)) + (num6 * num18);
            float num36 = -(((num9 * num21) - (num8 * num19)) + (num7 * num18));
            float num = 1f / ((((num5 * num39) + (num4 * num38)) + (num3 * num37)) + (num2 * num36));
            result.M11 = num39 * num;
            result.M21 = num38 * num;
            result.M31 = num37 * num;
            result.M41 = num36 * num;
            result.M12 = -(((num4 * num23) - (num3 * num22)) + (num2 * num21)) * num;
            result.M22 = (((num5 * num23) - (num3 * num20)) + (num2 * num19)) * num;
            result.M32 = -(((num5 * num22) - (num4 * num20)) + (num2 * num18)) * num;
            result.M42 = (((num5 * num21) - (num4 * num19)) + (num3 * num18)) * num;
            float num35 = (num7 * num10) - (num6 * num11);
            float num34 = (num8 * num10) - (num6 * num12);
            float num33 = (num8 * num11) - (num7 * num12);
            float num32 = (num9 * num10) - (num6 * num13);
            float num31 = (num9 * num11) - (num7 * num13);
            float num30 = (num9 * num12) - (num8 * num13);
            result.M13 = (((num4 * num35) - (num3 * num34)) + (num2 * num33)) * num;
            result.M23 = -(((num5 * num35) - (num3 * num32)) + (num2 * num31)) * num;
            result.M33 = (((num5 * num34) - (num4 * num32)) + (num2 * num30)) * num;
            result.M43 = -(((num5 * num33) - (num4 * num31)) + (num3 * num30)) * num;
            float num29 = (num7 * num14) - (num6 * num15);
            float num28 = (num8 * num14) - (num6 * num16);
            float num27 = (num8 * num15) - (num7 * num16);
            float num26 = (num9 * num14) - (num6 * num17);
            float num25 = (num9 * num15) - (num7 * num17);
            float num24 = (num9 * num16) - (num8 * num17);
            result.M14 = -(((num4 * num29) - (num3 * num28)) + (num2 * num27)) * num;
            result.M24 = (((num5 * num29) - (num3 * num26)) + (num2 * num25)) * num;
            result.M34 = -(((num5 * num28) - (num4 * num26)) + (num2 * num24)) * num;
            result.M44 = (((num5 * num27) - (num4 * num25)) + (num3 * num24)) * num;
        }

        public static Matrix Invert(Matrix matrix)
        {
            Matrix matrix2;
            float num5 = matrix.M11;
            float num4 = matrix.M12;
            float num3 = matrix.M13;
            float num2 = matrix.M14;
            float num9 = matrix.M21;
            float num8 = matrix.M22;
            float num7 = matrix.M23;
            float num6 = matrix.M24;
            float num17 = matrix.M31;
            float num16 = matrix.M32;
            float num15 = matrix.M33;
            float num14 = matrix.M34;
            float num13 = matrix.M41;
            float num12 = matrix.M42;
            float num11 = matrix.M43;
            float num10 = matrix.M44;
            float num23 = (num15 * num10) - (num14 * num11);
            float num22 = (num16 * num10) - (num14 * num12);
            float num21 = (num16 * num11) - (num15 * num12);
            float num20 = (num17 * num10) - (num14 * num13);
            float num19 = (num17 * num11) - (num15 * num13);
            float num18 = (num17 * num12) - (num16 * num13);
            float num39 = ((num8 * num23) - (num7 * num22)) + (num6 * num21);
            float num38 = -(((num9 * num23) - (num7 * num20)) + (num6 * num19));
            float num37 = ((num9 * num22) - (num8 * num20)) + (num6 * num18);
            float num36 = -(((num9 * num21) - (num8 * num19)) + (num7 * num18));
            float num = 1f / ((((num5 * num39) + (num4 * num38)) + (num3 * num37)) + (num2 * num36));
            matrix2.M11 = num39 * num;
            matrix2.M21 = num38 * num;
            matrix2.M31 = num37 * num;
            matrix2.M41 = num36 * num;
            matrix2.M12 = -(((num4 * num23) - (num3 * num22)) + (num2 * num21)) * num;
            matrix2.M22 = (((num5 * num23) - (num3 * num20)) + (num2 * num19)) * num;
            matrix2.M32 = -(((num5 * num22) - (num4 * num20)) + (num2 * num18)) * num;
            matrix2.M42 = (((num5 * num21) - (num4 * num19)) + (num3 * num18)) * num;
            float num35 = (num7 * num10) - (num6 * num11);
            float num34 = (num8 * num10) - (num6 * num12);
            float num33 = (num8 * num11) - (num7 * num12);
            float num32 = (num9 * num10) - (num6 * num13);
            float num31 = (num9 * num11) - (num7 * num13);
            float num30 = (num9 * num12) - (num8 * num13);
            matrix2.M13 = (((num4 * num35) - (num3 * num34)) + (num2 * num33)) * num;
            matrix2.M23 = -(((num5 * num35) - (num3 * num32)) + (num2 * num31)) * num;
            matrix2.M33 = (((num5 * num34) - (num4 * num32)) + (num2 * num30)) * num;
            matrix2.M43 = -(((num5 * num33) - (num4 * num31)) + (num3 * num30)) * num;
            float num29 = (num7 * num14) - (num6 * num15);
            float num28 = (num8 * num14) - (num6 * num16);
            float num27 = (num8 * num15) - (num7 * num16);
            float num26 = (num9 * num14) - (num6 * num17);
            float num25 = (num9 * num15) - (num7 * num17);
            float num24 = (num9 * num16) - (num8 * num17);
            matrix2.M14 = -(((num4 * num29) - (num3 * num28)) + (num2 * num27)) * num;
            matrix2.M24 = (((num5 * num29) - (num3 * num26)) + (num2 * num25)) * num;
            matrix2.M34 = -(((num5 * num28) - (num4 * num26)) + (num2 * num24)) * num;
            matrix2.M44 = (((num5 * num27) - (num4 * num25)) + (num3 * num24)) * num;
            return matrix2;
        }

        public static void CreateTranslation(float xPosition, float yPosition, float zPosition, out Matrix result)
        {
            result.M11 = 1f;
            result.M12 = 0f;
            result.M13 = 0f;
            result.M14 = 0f;
            result.M21 = 0f;
            result.M22 = 1f;
            result.M23 = 0f;
            result.M24 = 0f;
            result.M31 = 0f;
            result.M32 = 0f;
            result.M33 = 1f;
            result.M34 = 0f;
            result.M41 = xPosition;
            result.M42 = yPosition;
            result.M43 = zPosition;
            result.M44 = 1f;
        }

        public static void Transpose(ref Matrix matrix, out Matrix result)
        {
            float num16 = matrix.M11;
            float num15 = matrix.M12;
            float num14 = matrix.M13;
            float num13 = matrix.M14;
            float num12 = matrix.M21;
            float num11 = matrix.M22;
            float num10 = matrix.M23;
            float num9 = matrix.M24;
            float num8 = matrix.M31;
            float num7 = matrix.M32;
            float num6 = matrix.M33;
            float num5 = matrix.M34;
            float num4 = matrix.M41;
            float num3 = matrix.M42;
            float num2 = matrix.M43;
            float num = matrix.M44;
            result.M11 = num16;
            result.M12 = num12;
            result.M13 = num8;
            result.M14 = num4;
            result.M21 = num15;
            result.M22 = num11;
            result.M23 = num7;
            result.M24 = num3;
            result.M31 = num14;
            result.M32 = num10;
            result.M33 = num6;
            result.M34 = num2;
            result.M41 = num13;
            result.M42 = num9;
            result.M43 = num5;
            result.M44 = num;
        }

        public static void Multiply(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
        {
            float num16 = (((matrix1.M11 * matrix2.M11) + (matrix1.M12 * matrix2.M21)) + (matrix1.M13 * matrix2.M31)) + (matrix1.M14 * matrix2.M41);
            float num15 = (((matrix1.M11 * matrix2.M12) + (matrix1.M12 * matrix2.M22)) + (matrix1.M13 * matrix2.M32)) + (matrix1.M14 * matrix2.M42);
            float num14 = (((matrix1.M11 * matrix2.M13) + (matrix1.M12 * matrix2.M23)) + (matrix1.M13 * matrix2.M33)) + (matrix1.M14 * matrix2.M43);
            float num13 = (((matrix1.M11 * matrix2.M14) + (matrix1.M12 * matrix2.M24)) + (matrix1.M13 * matrix2.M34)) + (matrix1.M14 * matrix2.M44);
            float num12 = (((matrix1.M21 * matrix2.M11) + (matrix1.M22 * matrix2.M21)) + (matrix1.M23 * matrix2.M31)) + (matrix1.M24 * matrix2.M41);
            float num11 = (((matrix1.M21 * matrix2.M12) + (matrix1.M22 * matrix2.M22)) + (matrix1.M23 * matrix2.M32)) + (matrix1.M24 * matrix2.M42);
            float num10 = (((matrix1.M21 * matrix2.M13) + (matrix1.M22 * matrix2.M23)) + (matrix1.M23 * matrix2.M33)) + (matrix1.M24 * matrix2.M43);
            float num9 = (((matrix1.M21 * matrix2.M14) + (matrix1.M22 * matrix2.M24)) + (matrix1.M23 * matrix2.M34)) + (matrix1.M24 * matrix2.M44);
            float num8 = (((matrix1.M31 * matrix2.M11) + (matrix1.M32 * matrix2.M21)) + (matrix1.M33 * matrix2.M31)) + (matrix1.M34 * matrix2.M41);
            float num7 = (((matrix1.M31 * matrix2.M12) + (matrix1.M32 * matrix2.M22)) + (matrix1.M33 * matrix2.M32)) + (matrix1.M34 * matrix2.M42);
            float num6 = (((matrix1.M31 * matrix2.M13) + (matrix1.M32 * matrix2.M23)) + (matrix1.M33 * matrix2.M33)) + (matrix1.M34 * matrix2.M43);
            float num5 = (((matrix1.M31 * matrix2.M14) + (matrix1.M32 * matrix2.M24)) + (matrix1.M33 * matrix2.M34)) + (matrix1.M34 * matrix2.M44);
            float num4 = (((matrix1.M41 * matrix2.M11) + (matrix1.M42 * matrix2.M21)) + (matrix1.M43 * matrix2.M31)) + (matrix1.M44 * matrix2.M41);
            float num3 = (((matrix1.M41 * matrix2.M12) + (matrix1.M42 * matrix2.M22)) + (matrix1.M43 * matrix2.M32)) + (matrix1.M44 * matrix2.M42);
            float num2 = (((matrix1.M41 * matrix2.M13) + (matrix1.M42 * matrix2.M23)) + (matrix1.M43 * matrix2.M33)) + (matrix1.M44 * matrix2.M43);
            float num = (((matrix1.M41 * matrix2.M14) + (matrix1.M42 * matrix2.M24)) + (matrix1.M43 * matrix2.M34)) + (matrix1.M44 * matrix2.M44);
            result.M11 = num16;
            result.M12 = num15;
            result.M13 = num14;
            result.M14 = num13;
            result.M21 = num12;
            result.M22 = num11;
            result.M23 = num10;
            result.M24 = num9;
            result.M31 = num8;
            result.M32 = num7;
            result.M33 = num6;
            result.M34 = num5;
            result.M41 = num4;
            result.M42 = num3;
            result.M43 = num2;
            result.M44 = num;
        }

        public Vector3 Right
        {
            get
            {
                Vector3 vector;
                vector.X = this.M11;
                vector.Y = this.M12;
                vector.Z = this.M13;
                return vector;
            }
            set
            {
                this.M11 = value.X;
                this.M12 = value.Y;
                this.M13 = value.Z;
            }
        }

        public Vector3 Up
        {
            get
            {
                Vector3 vector;
                vector.X = this.M21;
                vector.Y = this.M22;
                vector.Z = this.M23;
                return vector;
            }
            set
            {
                this.M21 = value.X;
                this.M22 = value.Y;
                this.M23 = value.Z;
            }
        }

        public static void Add(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
        {
            result.M11 = matrix1.M11 + matrix2.M11;
            result.M12 = matrix1.M12 + matrix2.M12;
            result.M13 = matrix1.M13 + matrix2.M13;
            result.M14 = matrix1.M14 + matrix2.M14;
            result.M21 = matrix1.M21 + matrix2.M21;
            result.M22 = matrix1.M22 + matrix2.M22;
            result.M23 = matrix1.M23 + matrix2.M23;
            result.M24 = matrix1.M24 + matrix2.M24;
            result.M31 = matrix1.M31 + matrix2.M31;
            result.M32 = matrix1.M32 + matrix2.M32;
            result.M33 = matrix1.M33 + matrix2.M33;
            result.M34 = matrix1.M34 + matrix2.M34;
            result.M41 = matrix1.M41 + matrix2.M41;
            result.M42 = matrix1.M42 + matrix2.M42;
            result.M43 = matrix1.M43 + matrix2.M43;
            result.M44 = matrix1.M44 + matrix2.M44;
        }
    }
}
#endif