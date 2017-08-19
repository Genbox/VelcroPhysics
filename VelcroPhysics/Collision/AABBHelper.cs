using Microsoft.Xna.Framework;
using VelcroPhysics.Shared;
using VelcroPhysics.Utilities;

namespace VelcroPhysics.Collision
{
    public static class AABBHelper
    {
        public static void ComputeEdgeAABB(ref Vector2 start, ref Vector2 end, ref Transform transform, out AABB aabb)
        {
            Vector2 v1 = MathUtils.Mul(ref transform, ref start);
            Vector2 v2 = MathUtils.Mul(ref transform, ref end);

            aabb.LowerBound = Vector2.Min(v1, v2);
            aabb.UpperBound = Vector2.Max(v1, v2);

            Vector2 r = new Vector2(Settings.PolygonRadius, Settings.PolygonRadius);
            aabb.LowerBound = aabb.LowerBound - r;
            aabb.UpperBound = aabb.UpperBound + r;
        }

        public static void ComputeCircleAABB(ref Vector2 pos, float radius, ref Transform transform, out AABB aabb)
        {
            Vector2 p = transform.p + MathUtils.Mul(transform.q, pos);
            aabb.LowerBound = new Vector2(p.X - radius, p.Y - radius);
            aabb.UpperBound = new Vector2(p.X + radius, p.Y + radius);
        }

        public static void ComputePolygonAABB(Vertices vertices, ref Transform transform, out AABB aabb)
        {
            Vector2 lower = MathUtils.Mul(ref transform, vertices[0]);
            Vector2 upper = lower;

            for (int i = 1; i < vertices.Count; ++i)
            {
                Vector2 v = MathUtils.Mul(ref transform, vertices[i]);
                lower = Vector2.Min(lower, v);
                upper = Vector2.Max(upper, v);
            }

            Vector2 r = new Vector2(Settings.PolygonRadius, Settings.PolygonRadius);
            aabb.LowerBound = lower - r;
            aabb.UpperBound = upper + r;
        }
    }
}
