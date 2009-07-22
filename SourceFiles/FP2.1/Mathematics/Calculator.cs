using System;

#if (XNA)
using Microsoft.Xna.Framework;
#endif

namespace FarseerGames.FarseerPhysics.Mathematics
{
    /// <summary>
    /// A calculator with common math functions and constants.
    /// </summary>
    public static class Calculator
    {
        public const float DegreesToRadiansRatio = 57.29577957855f;
        public const float RadiansToDegreesRatio = 1f / 57.29577957855f;
        //NOTE: Commented line, use MathHelper.TwoPi instead
        //public const float TwoPi = 6.28318531f;
        private static Vector2 _curveEnd;
        private static Random _random = new Random();
        private static Vector2 _startCurve;
        private static Vector2 _temp;

        /// Temp variables to speed up the following code.
        private static float _tPow2;

        private static float _wayToGo;
        private static float _wayToGoPow2;

        public static float Sin(float angle)
        {
            return (float)Math.Sin(angle);
        }

        public static float Cos(float angle)
        {
            return (float)Math.Cos(angle);
        }

        public static float ACos(float value)
        {
            return (float)Math.Acos(value);
        }

        public static float ATan2(float y, float x)
        {
            return (float)Math.Atan2(y, x);
        }

        //performs bilinear interpolation of a point
        public static float BiLerp(Vector2 point, Vector2 min, Vector2 max, float value1, float value2, float value3,
                                   float value4, float minValue, float maxValue)
        {
            float x = point.X;
            float y = point.Y;

            x = MathHelper.Clamp(x, min.X, max.X);
            y = MathHelper.Clamp(y, min.Y, max.Y);

            float xRatio = (x - min.X) / (max.X - min.X);
            float yRatio = (y - min.Y) / (max.Y - min.Y);

            float top = MathHelper.Lerp(value1, value4, xRatio);
            float bottom = MathHelper.Lerp(value2, value3, xRatio);

            float value = MathHelper.Lerp(top, bottom, yRatio);
            value = MathHelper.Clamp(value, minValue, maxValue);
            return value;
        }

        public static float Clamp(float value, float low, float high)
        {
            return Math.Max(low, Math.Min(value, high));
        }

        public static float DistanceBetweenPointAndPoint(ref Vector2 point1, ref Vector2 point2)
        {
            Vector2 v;
            Vector2.Subtract(ref point1, ref point2, out v);
            return v.Length();
        }

        public static float DistanceBetweenPointAndLineSegment(ref Vector2 point, ref Vector2 lineEndPoint1, ref Vector2 lineEndPoint2)
        {
            Vector2 v = Vector2.Subtract(lineEndPoint2, lineEndPoint1);
            Vector2 w = Vector2.Subtract(point, lineEndPoint1);

            float c1 = Vector2.Dot(w, v);
            if (c1 <= 0) return DistanceBetweenPointAndPoint(ref point, ref lineEndPoint1);

            float c2 = Vector2.Dot(v, v);
            if (c2 <= c1) return DistanceBetweenPointAndPoint(ref point, ref lineEndPoint2);

            float b = c1 / c2;
            Vector2 pointOnLine = Vector2.Add(lineEndPoint1, Vector2.Multiply(v, b));
            return DistanceBetweenPointAndPoint(ref point, ref  pointOnLine);
        }

        public static float Cross(ref Vector2 value1, ref Vector2 value2)
        {
            return value1.X * value2.Y - value1.Y * value2.X;
        }

        public static Vector2 Cross(ref Vector2 value1, float value2)
        {
            return new Vector2(value2 * value1.Y, -value2 * value1.X);
        }

        public static Vector2 Cross(float value2, ref Vector2 value1)
        {
            return new Vector2(-value2 * value1.Y, value2 * value1.X);
        }

        public static void Cross(ref Vector2 value1, ref Vector2 value2, out float ret)
        {
            ret = value1.X * value2.Y - value1.Y * value2.X;
        }

        public static void Cross(ref Vector2 value1, ref float value2, out Vector2 ret)
        {
            ret = value1; //necassary to get past a compile error on 360
            ret.X = value2 * value1.Y;
            ret.Y = -value2 * value1.X;
        }

        public static void Cross(ref float value2, ref Vector2 value1, out Vector2 ret)
        {
            ret = value1; //necassary to get past a compile error on 360
            ret.X = -value2 * value1.Y;
            ret.Y = value2 * value1.X;
        }

        public static Vector2 Project(ref Vector2 projectVector, ref Vector2 onToVector)
        {
            float multiplier = 0;
            float numerator = (onToVector.X * projectVector.X + onToVector.Y * projectVector.Y);
            float denominator = (onToVector.X * onToVector.X + onToVector.Y * onToVector.Y);

            if (denominator != 0)
            {
                multiplier = numerator / denominator;
            }

            return Vector2.Multiply(onToVector, multiplier);
        }

        public static void Truncate(ref Vector2 vector, float maxLength, out Vector2 truncatedVector)
        {
            float length = vector.Length();
            length = Math.Min(length, maxLength);
            if (length > 0)
            {
                vector.Normalize();
            }
            Vector2.Multiply(ref vector, length, out truncatedVector);
        }

        public static float DegreesToRadians(float degrees)
        {
            return degrees * RadiansToDegreesRatio;
        }

        public static float RandomNumber(float min, float max)
        {
            return (float)((max - min) * _random.NextDouble() + min);
        }

        public static bool IsBetweenNonInclusive(float number, float min, float max)
        {
            if (number > min && number < max)
            {
                return true;
            }
            return false;
        }

        public static float VectorToRadians(ref Vector2 vector)
        {
            return (float)Math.Atan2(vector.X, -(double)vector.Y);
        }

        public static Vector2 RadiansToVector(float radians)
        {
            return new Vector2((float)Math.Sin(radians), -(float)Math.Cos(radians));
        }

        public static void RadiansToVector(float radians, ref Vector2 vector)
        {
            vector.X = (float)Math.Sin(radians);
            vector.Y = -(float)Math.Cos(radians);
        }

        public static void RotateVector(ref Vector2 vector, float radians)
        {
            float length = vector.Length();
            float newRadians = (float)Math.Atan2(vector.X, -(double)vector.Y) + radians;

            vector.X = (float)Math.Sin(newRadians) * length;
            vector.Y = -(float)Math.Cos(newRadians) * length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="t">Value between 0.0f and 1.0f.</param>
        /// <returns></returns>
        public static Vector2 LinearBezierCurve(ref Vector2 start, ref Vector2 end, float t)
        {
            return start + (end - start) * t;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="curve"></param>
        /// <param name="end"></param>
        /// <param name="t">Value between 0.0f and 1.0f.</param>
        /// <returns></returns>
        public static Vector2 QuadraticBezierCurve(ref Vector2 start, ref Vector2 curve, ref Vector2 end, float t)
        {
            _wayToGo = 1.0f - t;

            return _wayToGo * _wayToGo * start
                   + 2.0f * t * _wayToGo * curve
                   + t * t * end;
        }

        public static Vector2 QuadraticBezierCurve(ref Vector2 start, ref Vector2 curve, ref Vector2 end, float t, ref float radians)
        {
            _startCurve = start + (curve - start) * t;
            _curveEnd = curve + (end - curve) * t;
            _temp = _curveEnd - _startCurve;

            radians = (float)Math.Atan2(_temp.X, -(double)_temp.Y);
            return _startCurve + _temp * t;
        }

        public static Vector2 CubicBezierCurve2(ref Vector2 start, ref Vector2 startPointsTo, ref Vector2 end, ref Vector2 endPointsTo,
                                                float t)
        {
            Vector2 temp1 = start + startPointsTo;
            Vector2 temp2 = end + endPointsTo;

            return CubicBezierCurve(ref start, ref temp1, ref temp2, ref end, t);
        }

        public static Vector2 CubicBezierCurve2(ref Vector2 start, ref Vector2 startPointsTo, ref Vector2 end, ref Vector2 endPointsTo,
                                                float t, ref float radians)
        {
            Vector2 temp1 = start + startPointsTo;
            Vector2 temp2 = end + endPointsTo;

            return CubicBezierCurve(ref start, ref temp1, ref temp2, ref end, t, ref radians);
        }

        public static Vector2 CubicBezierCurve2(ref Vector2 start, float startPointDirection, float startPointLength,
                                                ref Vector2 end, float endPointDirection, float endPointLength,
                                                float t, ref float radians)
        {
            Vector2 temp1 = RadiansToVector(startPointDirection) * startPointLength;
            Vector2 temp2 = RadiansToVector(endPointDirection) * endPointLength;
            return CubicBezierCurve(ref start,
                                    ref temp1,
                                    ref temp2,
                                    ref end,
                                    t,
                                    ref radians);
        }

        public static Vector2 CubicBezierCurve(ref Vector2 start, ref Vector2 curve1, ref Vector2 curve2, ref Vector2 end, float t)
        {
            _tPow2 = t * t;
            _wayToGo = 1.0f - t;
            _wayToGoPow2 = _wayToGo * _wayToGo;

            return _wayToGo * _wayToGoPow2 * start
                   + 3.0f * t * _wayToGoPow2 * curve1
                   + 3.0f * _tPow2 * _wayToGo * curve2
                   + t * _tPow2 * end;
        }

        public static Vector2 CubicBezierCurve(ref Vector2 start, ref Vector2 curve1, ref Vector2 curve2, ref Vector2 end, float t,
                                               ref float radians)
        {
            Vector2 temp1 = start + (curve1 - start) * t;
            Vector2 temp2 = curve1 + (curve2 - curve1) * t;
            Vector2 temp3 = curve2 + (end - curve2) * t;

            return QuadraticBezierCurve(ref temp1, ref temp2, ref temp3, t, ref radians);
        }

        //Interpolate normal vectors ...
        public static void InterpolateNormal(ref Vector2 vector1, ref Vector2 vector2, float t, out Vector2 vector)
        {
            vector = vector1 + (vector2 - vector1) * t;
            vector.Normalize();
        }

        public static void InterpolateNormal(ref Vector2 vector1, ref Vector2 vector2, float t)
        {
            vector1 += (vector2 - vector1) * t;
            vector1.Normalize();
        }

        public static float InterpolateRotation(float radians1, float radians2, float t)
        {
            Vector2 vector1 = new Vector2((float)Math.Sin(radians1), -(float)Math.Cos(radians1));
            Vector2 vector2 = new Vector2((float)Math.Sin(radians2), -(float)Math.Cos(radians2));

            vector1 += (vector2 - vector1) * t;
            vector1.Normalize();

            return (float)Math.Atan2(vector1.X, -(double)vector1.Y);
        }

        public static void ProjectToAxis(ref Vector2[] points, ref Vector2 axis, out float min, out float max)
        {
            // To project a point on an axis use the dot product
            axis.Normalize();
            float dotProduct = Vector2.Dot(axis, points[0]);
            min = dotProduct;
            max = dotProduct;

            for (int i = 0; i < points.Length; i++)
            {
                dotProduct = Vector2.Dot(points[i], axis);
                if (dotProduct < min)
                {
                    min = dotProduct;
                }
                else
                {
                    if (dotProduct > max)
                    {
                        max = dotProduct;
                    }
                }
            }
        }
    }
}