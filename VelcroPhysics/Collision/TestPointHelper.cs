using Microsoft.Xna.Framework;
using VelcroPhysics.Shared;
using VelcroPhysics.Utilities;

namespace VelcroPhysics.Collision
{
    public static class TestPointHelper
    {
        public static bool TestPointCircle(ref Vector2 pos, float radius, ref Vector2 point, ref Transform transform)
        {
            Vector2 center = transform.p + MathUtils.Mul(transform.q, pos);
            Vector2 d = point - center;
            return Vector2.Dot(d, d) <= radius * radius;
        }

        public static bool TestPointPolygon(Vertices vertices, Vertices normals, ref Vector2 point, ref Transform transform)
        {
            Vector2 pLocal = MathUtils.MulT(transform.q, point - transform.p);

            for (int i = 0; i < vertices.Count; ++i)
            {
                float dot = Vector2.Dot(normals[i], pLocal - vertices[i]);
                if (dot > 0.0f)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
