using System.Diagnostics;
using Microsoft.Xna.Framework;
using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Collision.Shapes;
using VelcroPhysics.Shared;
using VelcroPhysics.Shared.Optimization;
using VelcroPhysics.Utilities;

namespace VelcroPhysics.Collision.Narrowphase
{
    public static class CollidePolygon
    {
        /// <summary>
        /// Compute the collision manifold between two polygons.
        /// </summary>
        public static void CollidePolygons(ref Manifold manifold, PolygonShape polyA, ref Transform xfA, PolygonShape polyB, ref Transform xfB)
        {
            // Find edge normal of max separation on A - return if separating axis is found
            // Find edge normal of max separation on B - return if separation axis is found
            // Choose reference edge as min(minA, minB)
            // Find incident edge
            // Clip

            manifold.PointCount = 0;
            float totalRadius = polyA.Radius + polyB.Radius;

            int edgeA;
            float separationA = FindMaxSeparation(out edgeA, polyA, ref xfA, polyB, ref xfB);
            if (separationA > totalRadius)
                return;

            int edgeB;
            float separationB = FindMaxSeparation(out edgeB, polyB, ref xfB, polyA, ref xfA);
            if (separationB > totalRadius)
                return;

            PolygonShape poly1; // reference polygon
            PolygonShape poly2; // incident polygon
            Transform xf1, xf2;
            int edge1; // reference edge
            bool flip;
            const float k_tol = 0.1f * Settings.LinearSlop;

            if (separationB > separationA + k_tol)
            {
                poly1 = polyB;
                poly2 = polyA;
                xf1 = xfB;
                xf2 = xfA;
                edge1 = edgeB;
                manifold.Type = ManifoldType.FaceB;
                flip = true;
            }
            else
            {
                poly1 = polyA;
                poly2 = polyB;
                xf1 = xfA;
                xf2 = xfB;
                edge1 = edgeA;
                manifold.Type = ManifoldType.FaceA;
                flip = false;
            }

            FixedArray2<ClipVertex> incidentEdge;
            FindIncidentEdge(out incidentEdge, poly1, ref xf1, edge1, poly2, ref xf2);

            int count1 = poly1.Vertices.Count;
            Vertices vertices1 = poly1.Vertices;

            int iv1 = edge1;
            int iv2 = edge1 + 1 < count1 ? edge1 + 1 : 0;

            Vector2 v11 = vertices1[iv1];
            Vector2 v12 = vertices1[iv2];

            Vector2 localTangent = v12 - v11;
            localTangent.Normalize();

            Vector2 localNormal = MathUtils.Cross(localTangent, 1.0f);
            Vector2 planePoint = 0.5f * (v11 + v12);

            Vector2 tangent = MathUtils.Mul(ref xf1.q, localTangent);
            Vector2 normal = MathUtils.Cross(tangent, 1.0f);

            v11 = MathUtils.Mul(ref xf1, v11);
            v12 = MathUtils.Mul(ref xf1, v12);

            // Face offset.
            float frontOffset = Vector2.Dot(normal, v11);

            // Side offsets, extended by polytope skin thickness.
            float sideOffset1 = -Vector2.Dot(tangent, v11) + totalRadius;
            float sideOffset2 = Vector2.Dot(tangent, v12) + totalRadius;

            // Clip incident edge against extruded edge1 side edges.
            FixedArray2<ClipVertex> clipPoints1;
            FixedArray2<ClipVertex> clipPoints2;

            // Clip to box side 1
            int np = Collision.ClipSegmentToLine(out clipPoints1, ref incidentEdge, -tangent, sideOffset1, iv1);

            if (np < 2)
                return;

            // Clip to negative box side 1
            np = Collision.ClipSegmentToLine(out clipPoints2, ref clipPoints1, tangent, sideOffset2, iv2);

            if (np < 2)
            {
                return;
            }

            // Now clipPoints2 contains the clipped points.
            manifold.LocalNormal = localNormal;
            manifold.LocalPoint = planePoint;

            int pointCount = 0;
            for (int i = 0; i < Settings.MaxManifoldPoints; ++i)
            {
                float separation = Vector2.Dot(normal, clipPoints2[i].V) - frontOffset;

                if (separation <= totalRadius)
                {
                    ManifoldPoint cp = manifold.Points[pointCount];
                    cp.LocalPoint = MathUtils.MulT(ref xf2, clipPoints2[i].V);
                    cp.Id = clipPoints2[i].ID;

                    if (flip)
                    {
                        // Swap features
                        ContactFeature cf = cp.Id.ContactFeature;
                        cp.Id.ContactFeature.IndexA = cf.IndexB;
                        cp.Id.ContactFeature.IndexB = cf.IndexA;
                        cp.Id.ContactFeature.TypeA = cf.TypeB;
                        cp.Id.ContactFeature.TypeB = cf.TypeA;
                    }

                    manifold.Points[pointCount] = cp;

                    ++pointCount;
                }
            }

            manifold.PointCount = pointCount;
        }

        /// <summary>
        /// Find the max separation between poly1 and poly2 using edge normals from poly1.
        /// </summary>
        private static float FindMaxSeparation(out int edgeIndex, PolygonShape poly1, ref Transform xf1, PolygonShape poly2, ref Transform xf2)
        {
            int count1 = poly1.Vertices.Count;
            int count2 = poly2.Vertices.Count;
            Vertices n1s = poly1.Normals;
            Vertices v1s = poly1.Vertices;
            Vertices v2s = poly2.Vertices;
            Transform xf = MathUtils.MulT(xf2, xf1);

            int bestIndex = 0;
            float maxSeparation = -Settings.MaxFloat;
            for (int i = 0; i < count1; ++i)
            {
                // Get poly1 normal in frame2.
                Vector2 n = MathUtils.Mul(ref xf.q, n1s[i]);
                Vector2 v1 = MathUtils.Mul(ref xf, v1s[i]);

                // Find deepest point for normal i.
                float si = Settings.MaxFloat;
                for (int j = 0; j < count2; ++j)
                {
                    float sij = Vector2.Dot(n, v2s[j] - v1);
                    if (sij < si)
                    {
                        si = sij;
                    }
                }

                if (si > maxSeparation)
                {
                    maxSeparation = si;
                    bestIndex = i;
                }
            }

            edgeIndex = bestIndex;
            return maxSeparation;
        }

        private static void FindIncidentEdge(out FixedArray2<ClipVertex> c, PolygonShape poly1, ref Transform xf1, int edge1, PolygonShape poly2, ref Transform xf2)
        {
            Vertices normals1 = poly1.Normals;

            int count2 = poly2.Vertices.Count;
            Vertices vertices2 = poly2.Vertices;
            Vertices normals2 = poly2.Normals;

            Debug.Assert(0 <= edge1 && edge1 < poly1.Vertices.Count);

            // Get the normal of the reference edge in poly2's frame.
            Vector2 normal1 = MathUtils.MulT(ref xf2.q, MathUtils.Mul(ref xf1.q, normals1[edge1]));

            // Find the incident edge on poly2.
            int index = 0;
            float minDot = Settings.MaxFloat;
            for (int i = 0; i < count2; ++i)
            {
                float dot = Vector2.Dot(normal1, normals2[i]);
                if (dot < minDot)
                {
                    minDot = dot;
                    index = i;
                }
            }

            // Build the clip vertices for the incident edge.
            int i1 = index;
            int i2 = i1 + 1 < count2 ? i1 + 1 : 0;

            c = new FixedArray2<ClipVertex>();
            c.Value0.V = MathUtils.Mul(ref xf2, vertices2[i1]);
            c.Value0.ID.ContactFeature.IndexA = (byte)edge1;
            c.Value0.ID.ContactFeature.IndexB = (byte)i1;
            c.Value0.ID.ContactFeature.TypeA = ContactFeatureType.Face;
            c.Value0.ID.ContactFeature.TypeB = ContactFeatureType.Vertex;

            c.Value1.V = MathUtils.Mul(ref xf2, vertices2[i2]);
            c.Value1.ID.ContactFeature.IndexA = (byte)edge1;
            c.Value1.ID.ContactFeature.IndexB = (byte)i2;
            c.Value1.ID.ContactFeature.TypeA = ContactFeatureType.Face;
            c.Value1.ID.ContactFeature.TypeB = ContactFeatureType.Vertex;
        }
    }
}
