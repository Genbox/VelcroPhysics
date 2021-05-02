using System.Diagnostics;
using Genbox.VelcroPhysics.Collision.ContactSystem;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Collision.Narrowphase
{
    public static class CollideEdge
    {
        /// <summary>Compute contact points for edge versus circle. This accounts for edge connectivity.</summary>
        /// <param name="manifold">The manifold.</param>
        /// <param name="edgeA">The edge A.</param>
        /// <param name="transformA">The transform A.</param>
        /// <param name="circleB">The circle B.</param>
        /// <param name="transformB">The transform B.</param>
        public static void CollideEdgeAndCircle(ref Manifold manifold, EdgeShape edgeA, ref Transform transformA, CircleShape circleB, ref Transform transformB)
        {
            manifold.PointCount = 0;

            // Compute circle in frame of edge
            Vector2 Q = MathUtils.MulT(ref transformA, MathUtils.Mul(ref transformB, ref circleB._position));

            Vector2 A = edgeA.Vertex1, B = edgeA.Vertex2;
            Vector2 e = B - A;

            // Normal points to the right for a CCW winding
            Vector2 n = new Vector2(e.Y, -e.X);
            float offset = MathUtils.Dot(n, Q - A);

            bool oneSided = edgeA.OneSided;
            if (oneSided && offset < 0.0f)
                return;

            // Barycentric coordinates
            float u = Vector2.Dot(e, B - Q);
            float v = Vector2.Dot(e, Q - A);

            float radius = edgeA.Radius + circleB.Radius;

            ContactFeature cf;
            cf.IndexB = 0;
            cf.TypeB = ContactFeatureType.Vertex;

            // Region A
            if (v <= 0.0f)
            {
                Vector2 P1 = A;
                Vector2 d1 = Q - P1;
                float dd1 = Vector2.Dot(d1, d1);
                if (dd1 > radius * radius)
                    return;

                // Is there an edge connected to A?
                if (edgeA.OneSided)
                {
                    Vector2 A1 = edgeA.Vertex0;
                    Vector2 B1 = A;
                    Vector2 e1 = B1 - A1;
                    float u1 = Vector2.Dot(e1, B1 - Q);

                    // Is the circle in Region AB of the previous edge?
                    if (u1 > 0.0f)
                        return;
                }

                cf.IndexA = 0;
                cf.TypeA = ContactFeatureType.Vertex;
                manifold.PointCount = 1;
                manifold.Type = ManifoldType.Circles;
                manifold.LocalNormal = Vector2.Zero;
                manifold.LocalPoint = P1;
                manifold.Points.Value0.Id.Key = 0;
                manifold.Points.Value0.Id.ContactFeature = cf;
                manifold.Points.Value0.LocalPoint = circleB.Position;
                return;
            }

            // Region B
            if (u <= 0.0f)
            {
                Vector2 P2 = B;
                Vector2 d2 = Q - P2;
                float dd2 = Vector2.Dot(d2, d2);
                if (dd2 > radius * radius)
                    return;

                // Is there an edge connected to B?
                if (edgeA.OneSided)
                {
                    Vector2 B2 = edgeA.Vertex3;
                    Vector2 A2 = B;
                    Vector2 e2 = B2 - A2;
                    float v2 = Vector2.Dot(e2, Q - A2);

                    // Is the circle in Region AB of the next edge?
                    if (v2 > 0.0f)
                        return;
                }

                cf.IndexA = 1;
                cf.TypeA = (byte)ContactFeatureType.Vertex;
                manifold.PointCount = 1;
                manifold.Type = ManifoldType.Circles;
                manifold.LocalNormal = Vector2.Zero;
                manifold.LocalPoint = P2;
                manifold.Points.Value0.Id.Key = 0;
                manifold.Points.Value0.Id.ContactFeature = cf;
                manifold.Points.Value0.LocalPoint = circleB.Position;
                return;
            }

            // Region AB
            float den = Vector2.Dot(e, e);
            Debug.Assert(den > 0.0f);
            Vector2 P = (1.0f / den) * (u * A + v * B);
            Vector2 d = Q - P;
            float dd = Vector2.Dot(d, d);
            if (dd > radius * radius)
                return;

            if (offset < 0.0f)
                n = new Vector2(-n.X, -n.Y);

            n.Normalize();

            cf.IndexA = 0;
            cf.TypeA = ContactFeatureType.Face;
            manifold.PointCount = 1;
            manifold.Type = ManifoldType.FaceA;
            manifold.LocalNormal = n;
            manifold.LocalPoint = A;
            manifold.Points.Value0.Id.Key = 0;
            manifold.Points.Value0.Id.ContactFeature = cf;
            manifold.Points.Value0.LocalPoint = circleB.Position;
        }

        /// <summary>Collides and edge and a polygon, taking into account edge adjacency.</summary>
        public static void CollideEdgeAndPolygon(ref Manifold manifold, EdgeShape edgeA, ref Transform xfA, PolygonShape polygonB, ref Transform xfB)
        {
            EPCollider.Collide(ref manifold, edgeA, ref xfA, polygonB, ref xfB);
        }
    }
}