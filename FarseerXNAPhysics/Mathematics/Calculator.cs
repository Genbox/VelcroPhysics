using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerXNAPhysics.Mathematics
{
    public static class Calculator
    {
        public const float PiX2 = 6.28318531f;
        public const float DegreesToRadiansRatio = 57.29577957855f;

        public static float Sin(float angle)
        {
            return (float)Math.Sin((double)angle);
        }

        public static float Cos(float angle)
        {
            return (float)Math.Cos((double)angle);
        }

        public static float ACos(float value) {
            return (float)Math.Acos((double)value);
        }

        public static float ATan2(float y, float x) {
            return (float)Math.Atan2((double)y,(double)x);
        }

        public static float Clamp(float value, float low, float high){
	        return Math.Max(low, Math.Min(value, high));
        }

        public static float DistanceBetweenPointAndPoint(Vector2 point1, Vector2 point2)
        {
            Vector2 v = Vector2.Subtract(point1, point2);
            return v.Length();
        }

        public static float DistanceBetweenPointAndLineSegment(Vector2 point, Vector2 lineEndPoint1, Vector2 lineEndPoint2)
        {
            Vector2 v = Vector2.Subtract(lineEndPoint2, lineEndPoint1);
            Vector2 w = Vector2.Subtract(point, lineEndPoint1);

            float c1 = Vector2.Dot(w, v);
            if (c1 <= 0) return DistanceBetweenPointAndPoint(point,lineEndPoint1);

            float c2 = Vector2.Dot(v, v);
            if (c2 <= c1) return DistanceBetweenPointAndPoint(point, lineEndPoint2);

            float b = c1 / c2;
            Vector2 pointOnLine = Vector2.Add(lineEndPoint1, Vector2.Multiply(v, b));
            return DistanceBetweenPointAndPoint(point, pointOnLine);
        }

        public static float Cross(Vector2 value1, Vector2 value2){
	        return value1.X * value2.Y - value1.Y * value2.X;
        }

        public static Vector2 Cross(Vector2 value1, float value2){
            return new Vector2(value2*value1.Y, -value2*value1.X);
        }

        public static Vector2 Cross(float value2,Vector2 value1){
            return new Vector2(-value2*value1.Y, value2*value1.X);
        }

        public static void Vector2Add(ref Vector2 vecOut, ref Vector2 vec1, ref Vector2 vec2) {
            vecOut.X = vec1.X + vec2.X;
            vecOut.Y = vec1.Y + vec2.Y;
        }

        public static Vector2 Project(Vector2 projectVector, Vector2 onToVector) {
            float multiplier = 0;
            float numerator = (onToVector.X * projectVector.X + onToVector.Y * projectVector.Y);
            float denominator = (onToVector.X * onToVector.X + onToVector.Y * onToVector.Y);
            
            if(denominator !=0){
                multiplier = numerator/denominator;
            }
 
            return Vector2.Multiply(onToVector,multiplier);
        }
   
    }
}
