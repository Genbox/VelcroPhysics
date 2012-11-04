using FarseerPhysics.Physics.Collisions;
using Microsoft.Xna.Framework;
using FarseerPhysics.Common;
using System;
using FarseerPhysics.Collision;

namespace FarseerPhysics.Fluids
{
    public static class FluidUtils
    {
        public static Vector2 ProjectOntoLineClamped(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
        {
            float t;
            return ProjectOntoLineClamped(lineStart, lineEnd, point, out t);
        }

        private static Vector2 ProjectOntoLineClamped(Vector2 lineStart, Vector2 lineEnd, Vector2 point, out float t)
        {
            Vector2 line = lineEnd - lineStart;
            Vector2 startToPoint = point - lineStart;

            t = MathHelper.Clamp(Vector2.Dot(line, startToPoint) / line.LengthSquared(), 0.0f, 1.0f);

            return lineStart + t * line;
        }

        private static bool Intersects(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float p0p2 = MathUtils.Cross(p0, p2);
            float p3p0 = MathUtils.Cross(p3, p0);
            float p2p1 = MathUtils.Cross(p2, p1);

            float den = MathUtils.Cross(p1, p3) + p3p0 + p2p1 + p0p2;

            if (Math.Abs(den) < float.Epsilon)
            {
                return false;
            }

            float t1 = (p3p0 + MathUtils.Cross(p2, p3) + p0p2) / den;
            float t2 = (MathUtils.Cross(p1, p0) + p2p1 + p0p2) / den;

            if (t1 < 0.0f || t1 > 1.0f || t2 < 0.0f || t2 > 1.0f)
            {
                return false;
            }

            return true;
        }

        // Maybe there is better method but this works
        public static bool Intersects(this AABB aabb, ref Vector2 p0, ref Vector2 p1)
        {
            if (aabb.Contains(ref p0) || aabb.Contains(ref p1))
            {
                return true;
            }

            Vector2 min = aabb.LowerBound - new Vector2(float.Epsilon, float.Epsilon);
            Vector2 max = aabb.UpperBound + new Vector2(float.Epsilon, float.Epsilon);

            // Top
            if (Intersects(p0, p1, min, new Vector2(max.X, min.Y)))
            {
                return true;
            }
            // Right
            if (Intersects(p0, p1, new Vector2(max.X, min.Y), max))
            {
                return true;
            }
            // Bottom
            if (Intersects(p0, p1, max, new Vector2(min.X, max.Y)))
            {
                return true;
            }
            // Left
            if (Intersects(p0, p1, new Vector2(min.X, max.Y), min))
            {
                return true;
            }

            return false;
        }

        private static Feature GetNearestFeature(this Vertices v, ref Vector2 point, int index)
        {
            Vector2 p0 = v[index];
            Vector2 p1 = v.NextVertex(index);

            Vector2 proj = ProjectOntoLineClamped(p0, p1, point);
            Vector2 normal = proj - point;
            float d = normal.Length();
            normal.Normalize();

            return new Feature {
                Distance = d,
                Normal = normal,
                Position = proj
            };
        }

        public static Feature GetNearestFeature(this Vertices v, ref Vector2 point)
        {
            Feature nearest = Feature.Empty;

            for (int i = 0; i < v.Count; ++i)
            {
                Feature f = v.GetNearestFeature(ref point, i);
                if (f.Distance < nearest.Distance)
                {
                    nearest = f;
                }
            }

            if (v.PointInPolygon(ref point) >= 0)
            {
                nearest.Distance *= -1.0f;
            }

            return nearest;
        }
    }
}