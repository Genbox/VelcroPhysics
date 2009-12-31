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
using System.Diagnostics;

namespace FarseerPhysics
{
    /// <summary>
    /// Features contain the contact details of a collision
    /// </summary>
    public struct Features
    {
        /// <summary>
        /// The edge that defines the outward contact normal.
        /// </summary>
        public byte ReferenceEdge;

        /// <summary>
        /// The edge most anti-parallel to the reference edge.
        /// </summary>
        public byte IncidentEdge;

        /// <summary>
        /// The vertex (0 or 1) on the incident edge that was clipped.
        /// </summary>
        public byte IncidentVertex;

        /// <summary>
        /// A value of 1 indicates that the reference edge is on shape2.
        /// </summary>
        public byte Flip;
    }

    /// Contact ids to facilitate warm starting.
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
    public struct ContactID
    {
        /// <summary>
        /// The features that intersect to form the contact point
        /// </summary>
        [System.Runtime.InteropServices.FieldOffset(0)]
        public Features Features;

        /// <summary>
        /// Used to quickly compare contact ids.
        /// </summary>
        [System.Runtime.InteropServices.FieldOffset(0)]
        public uint Key;
    }

    /// <summary>
    /// A manifold point is a contact point belonging to a contact
    /// manifold. It holds details related to the geometry and dynamics
    /// of the contact points.
    /// The local point usage depends on the manifold type:
    /// -ShapeType.Circles: the local center of circleB
    /// -SeparationFunction.FaceA: the local center of cirlceB or the clip point of polygonB
    /// -SeparationFunction.FaceB: the clip point of polygonA
    /// This structure is stored across time steps, so we keep it small.
    /// Note: the impulses are used for internal caching and may not
    /// provide reliable contact forces, especially for high speed collisions.
    /// </summary>
    public struct ManifoldPoint
    {
        /// <summary>
        /// usage depends on manifold type
        /// </summary>
        public Vector2 LocalPoint;

        /// <summary>
        /// the non-penetration impulse
        /// </summary>
        public float NormalImpulse;

        /// <summary>
        /// the friction impulse
        /// </summary>
        public float TangentImpulse;

        /// <summary>
        /// uniquely identifies a contact point between two shapes
        /// </summary>
        public ContactID Id;
    }

    public enum ManifoldType
    {
        Circles,
        FaceA,
        FaceB
    }

    /// <summary>
    /// A manifold for two touching convex shapes.
    /// Box2D supports multiple types of contact:
    /// - clip point versus plane with radius
    /// - point versus point with radius (circles)
    /// The local point usage depends on the manifold type:
    /// -ShapeType.Circles: the local center of circleA
    /// -SeparationFunction.FaceA: the center of faceA
    /// -SeparationFunction.FaceB: the center of faceB
    /// Similarly the local normal usage:
    /// -ShapeType.Circles: not used
    /// -SeparationFunction.FaceA: the normal on polygonA
    /// -SeparationFunction.FaceB: the normal on polygonB
    /// We store contacts in this way so that position correction can
    /// account for movement, which is critical for continuous physics.
    /// All contact scenarios must be expressed in one of these types.
    /// This structure is stored across time steps, so we keep it small.
    /// </summary>
    public struct Manifold
    {
        /// <summary>
        /// the points of contact
        /// </summary>
        public FixedArray2<ManifoldPoint> _points;

        /// <summary>
        /// not use for Type.SeparationFunction.Points
        /// </summary>
        public Vector2 _localPlaneNormal;

        /// <summary>
        /// usage depends on manifold type
        /// </summary>
        public Vector2 _localPoint;

        /// <summary>
        /// The manifold type
        /// </summary>
        public ManifoldType _type;

        /// <summary>
        /// the number of manifold points
        /// </summary>
        public int _pointCount;
    }

    /// <summary>
    /// This is used to compute the current state of a contact manifold.
    /// </summary>
    public struct WorldManifold
    {
        /// <summary>
        /// Evaluate the manifold with supplied transforms. This assumes
        /// modest motion from the original state. This does not change the
        /// point count, impulses, etc. The radii must come from the shapes
        /// that generated the manifold.
        /// </summary>
        /// <param name="manifold">The manifold.</param>
        /// <param name="xfA">The xf A.</param>
        /// <param name="radiusA">The radius A.</param>
        /// <param name="xfB">The xf B.</param>
        /// <param name="radiusB">The radius B.</param>
        public WorldManifold(ref Manifold manifold,
                        ref Transform xfA, float radiusA,
                        ref Transform xfB, float radiusB)
        {
            Normal = Vector2.Zero;
            Points = new FixedArray2<Vector2>();

            if (manifold._pointCount == 0)
            {
                return;
            }

            switch (manifold._type)
            {
                case ManifoldType.Circles:
                    {
                        Vector2 pointA = MathUtils.Multiply(ref xfA, manifold._localPoint);
                        Vector2 pointB = MathUtils.Multiply(ref xfB, manifold._points[0].LocalPoint);
                        Vector2 normal = new Vector2(1.0f, 0.0f);
                        if (Vector2.DistanceSquared(pointA, pointB) > Settings.Epsilon * Settings.Epsilon)
                        {
                            normal = pointB - pointA;
                            normal.Normalize();
                        }

                        Normal = normal;

                        Vector2 cA = pointA + radiusA * normal;
                        Vector2 cB = pointB - radiusB * normal;
                        Points[0] = 0.5f * (cA + cB);
                    }
                    break;

                case ManifoldType.FaceA:
                    {
                        Vector2 normal = MathUtils.Multiply(ref xfA.R, manifold._localPlaneNormal);
                        Vector2 planePoint = MathUtils.Multiply(ref xfA, manifold._localPoint);

                        // Ensure normal points from A to B.
                        Normal = normal;

                        for (int i = 0; i < manifold._pointCount; ++i)
                        {
                            Vector2 clipPoint = MathUtils.Multiply(ref xfB, manifold._points[i].LocalPoint);
                            Vector2 cA = clipPoint + (radiusA - Vector2.Dot(clipPoint - planePoint, normal)) * normal;
                            Vector2 cB = clipPoint - radiusB * normal;
                            Points[i] = 0.5f * (cA + cB);
                        }
                    }
                    break;

                case ManifoldType.FaceB:
                    {
                        Vector2 normal = MathUtils.Multiply(ref xfB.R, manifold._localPlaneNormal);
                        Vector2 planePoint = MathUtils.Multiply(ref xfB, manifold._localPoint);

                        // Ensure normal points from A to B.
                        Normal = -normal;

                        for (int i = 0; i < manifold._pointCount; ++i)
                        {
                            Vector2 clipPoint = MathUtils.Multiply(ref xfA, manifold._points[i].LocalPoint);
                            Vector2 cA = clipPoint - radiusA * normal;
                            Vector2 cB = clipPoint + (radiusB - Vector2.Dot(clipPoint - planePoint, normal)) * normal;
                            Points[i] = 0.5f * (cA + cB);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// world vector pointing from A to B
        /// </summary>
        public Vector2 Normal;

        /// <summary>
        /// world contact point (point of intersection)
        /// </summary>
        public FixedArray2<Vector2> Points;
    }

    /// <summary>
    /// This is used for determining the state of contact points.
    /// </summary>
    public enum PointState
    {
        /// <summary>
        /// point was added in the update
        /// </summary>
        Add,
        /// <summary>
        /// point persisted across the update
        /// </summary>
        Persist,
        /// <summary>
        /// point was removed in the update
        /// </summary>
        Remove
    }

    /// <summary>
    /// Used for computing contact manifolds.
    /// </summary>
    public struct ClipVertex
    {
        public Vector2 Vertex;
        public ContactID Id;
    }

    /// <summary>
    /// Ray-cast input data.
    /// </summary>
    public struct RayCastInput
    {
        public Vector2 Point1, Point2;
        public float MaxFraction;
    }

    /// <summary>
    /// Ray-cast output data.
    /// </summary>
    public struct RayCastOutput
    {
        public Vector2 Normal;
        public float Fraction;
    }

    /// <summary>
    /// A line segment.
    /// </summary>
    public struct Segment
    {
        /// Ray cast against this segment with another segment.
        // Collision Detection in Interactive 3D Environments by Gino van den Bergen
        // From Section 3.4.1
        // x = mu1 * p1 + mu2 * p2
        // mu1 + mu2 = 1 && mu1 >= 0 && mu2 >= 0
        // mu1 = 1 - mu2;
        // x = (1 - mu2) * p1 + mu2 * p2
        //   = p1 + mu2 * (p2 - p1)
        // x = s + a * r (s := start, r := end - start)
        // s + a * r = p1 + mu2 * d (d := p2 - p1)
        // -a * r + mu2 * d = b (b := s - p1)
        // [-r d] * [a; mu2] = b
        // Cramer's rule:
        // denom = det[-r d]
        // a = det[b d] / denom
        // mu2 = det[-r b] / denom
        public bool TestSegment(out float lambda, out Vector2 normal, ref Segment segment, float maxLambda)
        {
            lambda = 0;
            normal = Vector2.Zero;

            Vector2 s = segment.Point1;
            Vector2 r = segment.Point2 - s;
            Vector2 d = Point2 - Point1;
            Vector2 n = MathUtils.Cross(d, 1.0f);

            float k_slop = 100.0f * Settings.Epsilon;
            float denom = -Vector2.Dot(r, n);

            // Cull back facing collision and ignore parallel segments.
            if (denom > k_slop)
            {
                // Does the segment intersect the infinite line associated with this segment?
                Vector2 b = s - Point1;
                float a = Vector2.Dot(b, n);

                if (0.0f <= a && a <= maxLambda * denom)
                {
                    float mu2 = -r.X * b.Y + r.Y * b.X;

                    // Does the segment intersect this segment?
                    if (-k_slop * denom <= mu2 && mu2 <= denom * (1.0f + k_slop))
                    {
                        a /= denom;
                        n.Normalize();
                        lambda = a;
                        normal = n;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// the starting point
        /// </summary>
        public Vector2 Point1;

        /// <summary>
        /// the ending point
        /// </summary>
        public Vector2 Point2;
    }

    /// <summary>
    /// An axis aligned bounding box.
    /// </summary>
    public struct AABB
    {
        /// <summary>
        /// Verify that the bounds are sorted.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValid()
        {
            Vector2 d = UpperBound - LowerBound;
            bool valid = d.X >= 0.0f && d.Y >= 0.0f;
            valid = valid && LowerBound.IsValid() && UpperBound.IsValid();
            return valid;
        }

        /// <summary>
        /// Get the center of the AABB.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetCenter()
        {
            return 0.5f * (LowerBound + UpperBound);
        }

        /// <summary>
        /// Get the extents of the AABB (half-widths).
        /// </summary>
        /// <returns></returns>
        public Vector2 GetExtents()
        {
            return 0.5f * (UpperBound - LowerBound);
        }

        /// <summary>
        /// Combine two AABBs into this one.
        /// </summary>
        /// <param name="aabb1">The aabb1.</param>
        /// <param name="aabb2">The aabb2.</param>
        public void Combine(ref AABB aabb1, ref AABB aabb2)
        {
            LowerBound = Vector2.Min(aabb1.LowerBound, aabb2.LowerBound);
            UpperBound = Vector2.Max(aabb1.UpperBound, aabb2.UpperBound);
        }

        /// <summary>
        /// Does this aabb contain the provided AABB.
        /// </summary>
        /// <param name="aabb">The aabb.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified aabb]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(ref AABB aabb)
        {
            bool result = true;
            result = result && LowerBound.X <= aabb.LowerBound.X;
            result = result && LowerBound.Y <= aabb.LowerBound.Y;
            result = result && aabb.UpperBound.X <= UpperBound.X;
            result = result && aabb.UpperBound.Y <= UpperBound.Y;
            return result;
        }

        public static bool TestOverlap(ref AABB a, ref AABB b)
        {
            Vector2 d1 = b.LowerBound - a.UpperBound;
            Vector2 d2 = a.LowerBound - b.UpperBound;

            if (d1.X > 0.0f || d1.Y > 0.0f)
                return false;

            if (d2.X > 0.0f || d2.Y > 0.0f)
                return false;

            return true;
        }

        public static bool TestOverlap(Shape shapeA, Shape shapeB, ref Transform xfA, ref Transform xfB)
        {
            DistanceInput input = new DistanceInput();
            input.ProxyA.Set(shapeA);
            input.ProxyB.Set(shapeB);
            input.TransformA = xfA;
            input.TransformB = xfB;
            input.UseRadii = true;

            SimplexCache cache;
            DistanceOutput output;
            Distance.ComputeDistance(out output, out cache, ref input);

            return output.Distance < 10.0f * Settings.Epsilon;
        }

        public bool RayCast(out RayCastOutput output, ref RayCastInput input)
        {
            // From Real-time Collision Detection, p179.

            output = new RayCastOutput();

            float tmin = -Settings.MaxFloat;
            float tmax = Settings.MaxFloat;

            Vector2 p = input.Point1;
            Vector2 d = input.Point2 - input.Point1;
            Vector2 absD = MathUtils.Abs(d);

            Vector2 normal = Vector2.Zero;

            for (int i = 0; i < 2; ++i)
            {
                float absD_i = i == 0 ? absD.X : absD.Y;
                float lowerBound_i = i == 0 ? LowerBound.X : LowerBound.Y;
                float upperBound_i = i == 0 ? UpperBound.X : UpperBound.Y;
                float p_i = i == 0 ? p.X : p.Y;

                if (absD_i < Settings.Epsilon)
                {
                    // Parallel.
                    if (p_i < lowerBound_i || upperBound_i < p_i)
                    {
                        return false;
                    }
                }
                else
                {
                    float d_i = i == 0 ? d.X : d.Y;

                    float inv_d = 1.0f / d_i;
                    float t1 = (lowerBound_i - p_i) * inv_d;
                    float t2 = (upperBound_i - p_i) * inv_d;

                    // Sign of the normal vector.
                    float s = -1.0f;

                    if (t1 > t2)
                    {
                        MathUtils.Swap(ref t1, ref t2);
                        s = 1.0f;
                    }

                    // Push the min up
                    if (t1 > tmin)
                    {
                        if (i == 0)
                        {
                            normal.X = s;
                        }
                        else
                        {
                            normal.Y = s;
                        }

                        tmin = t1;
                    }

                    // Pull the max down
                    tmax = Math.Min(tmax, t2);

                    if (tmin > tmax)
                    {
                        return false;
                    }
                }
            }

            // Does the ray start inside the box?
            // Does the ray intersect beyond the max fraction?
            if (tmin < 0.0f || input.MaxFraction < tmin)
            {
                return false;
            }

            // Intersection.
            output.Fraction = tmin;
            output.Normal = normal;
            return true;
        }

        /// <summary>
        /// the lower vertex
        /// </summary>
        public Vector2 LowerBound;

        /// <summary>
        /// the upper vertex
        /// </summary>
        public Vector2 UpperBound;
    }

    public static class Collision
    {
        public static void GetPointStates(out FixedArray2<PointState> state1, out FixedArray2<PointState> state2,
                              ref Manifold manifold1, ref Manifold manifold2)
        {
            state1 = new FixedArray2<PointState>();
            state2 = new FixedArray2<PointState>();

            // Detect persists and removes.
            for (int i = 0; i < manifold1._pointCount; ++i)
            {
                ContactID id = manifold1._points[i].Id;

                state1[i] = PointState.Remove;

                for (int j = 0; j < manifold2._pointCount; ++j)
                {
                    if (manifold2._points[j].Id.Key == id.Key)
                    {
                        state1[i] = PointState.Persist;
                        break;
                    }
                }
            }

            // Detect persists and adds.
            for (int i = 0; i < manifold2._pointCount; ++i)
            {
                ContactID id = manifold2._points[i].Id;

                state2[i] = PointState.Add;

                for (int j = 0; j < manifold1._pointCount; ++j)
                {
                    if (manifold1._points[j].Id.Key == id.Key)
                    {
                        state2[i] = PointState.Persist;
                        break;
                    }
                }
            }
        }


        /// <summary>
        /// Compute the collision manifold between two circles.
        /// </summary>
        /// <param name="manifold">The manifold.</param>
        /// <param name="circle1">The circle1.</param>
        /// <param name="xf1">The XF1.</param>
        /// <param name="circle2">The circle2.</param>
        /// <param name="xf2">The XF2.</param>
        public static void CollideCircles(ref Manifold manifold,
                              CircleShape circle1, ref Transform xf1,
                              CircleShape circle2, ref Transform xf2)
        {
            manifold._pointCount = 0;

            Vector2 p1 = MathUtils.Multiply(ref xf1, circle1.Position);
            Vector2 p2 = MathUtils.Multiply(ref xf2, circle2.Position);

            Vector2 d = p2 - p1;
            float distSqr = Vector2.Dot(d, d);
            float radius = circle1.Radius + circle2.Radius;
            if (distSqr > radius * radius)
            {
                return;
            }

            manifold._type = ManifoldType.Circles;
            manifold._localPoint = circle1.Position;
            manifold._localPlaneNormal = Vector2.Zero;
            manifold._pointCount = 1;

            var p0 = manifold._points[0];

            p0.LocalPoint = circle2.Position;
            p0.Id.Key = 0;

            manifold._points[0] = p0;
        }

        /// <summary>
        /// Compute the collision manifold between a polygon and a circle.
        /// </summary>
        /// <param name="manifold">The manifold.</param>
        /// <param name="polygon">The polygon.</param>
        /// <param name="xf1">The XF1.</param>
        /// <param name="circle">The circle.</param>
        /// <param name="xf2">The XF2.</param>
        public static void CollidePolygonAndCircle(ref Manifold manifold,
                                       PolygonShape polygon, ref Transform xf1,
                                       CircleShape circle, ref Transform xf2)
        {
            manifold._pointCount = 0;

            // Compute circle position in the frame of the polygon.
            Vector2 c = MathUtils.Multiply(ref xf2, circle.Position);
            Vector2 cLocal = MathUtils.MultiplyT(ref xf1, c);

            // Find the min separating edge.
            int normalIndex = 0;
            float separation = -Settings.MaxFloat;
            float radius = polygon.Radius + circle.Radius;
            int vertexCount = polygon.VertexCount;

            for (int i = 0; i < vertexCount; ++i)
            {
                float s = Vector2.Dot(polygon.Normals[i], cLocal - polygon.Vertices[i]);

                if (s > radius)
                {
                    // Early out.
                    return;
                }

                if (s > separation)
                {
                    separation = s;
                    normalIndex = i;
                }
            }

            // Vertices that subtend the incident face.
            int vertIndex1 = normalIndex;
            int vertIndex2 = vertIndex1 + 1 < vertexCount ? vertIndex1 + 1 : 0;
            Vector2 v1 = polygon.Vertices[vertIndex1];
            Vector2 v2 = polygon.Vertices[vertIndex2];

            // If the center is inside the polygon ...
            if (separation < Settings.Epsilon)
            {
                manifold._pointCount = 1;
                manifold._type = ManifoldType.FaceA;
                manifold._localPlaneNormal = polygon.Normals[normalIndex];
                manifold._localPoint = 0.5f * (v1 + v2);

                var p0 = manifold._points[0];

                p0.LocalPoint = circle.Position;
                p0.Id.Key = 0;

                manifold._points[0] = p0;

                return;
            }

            // Compute barycentric coordinates
            float u1 = Vector2.Dot(cLocal - v1, v2 - v1);
            float u2 = Vector2.Dot(cLocal - v2, v1 - v2);
            if (u1 <= 0.0f)
            {
                if (Vector2.DistanceSquared(cLocal, v1) > radius * radius)
                {
                    return;
                }

                manifold._pointCount = 1;
                manifold._type = ManifoldType.FaceA;
                manifold._localPlaneNormal = cLocal - v1;
                manifold._localPlaneNormal.Normalize();
                manifold._localPoint = v1;

                var p0b = manifold._points[0];

                p0b.LocalPoint = circle.Position;
                p0b.Id.Key = 0;

                manifold._points[0] = p0b;

            }
            else if (u2 <= 0.0f)
            {
                if (Vector2.DistanceSquared(cLocal, v2) > radius * radius)
                {
                    return;
                }

                manifold._pointCount = 1;
                manifold._type = ManifoldType.FaceA;
                manifold._localPlaneNormal = cLocal - v2;
                manifold._localPlaneNormal.Normalize();
                manifold._localPoint = v2;

                var p0c = manifold._points[0];

                p0c.LocalPoint = circle.Position;
                p0c.Id.Key = 0;

                manifold._points[0] = p0c;
            }
            else
            {
                Vector2 faceCenter = 0.5f * (v1 + v2);
                float separation2 = Vector2.Dot(cLocal - faceCenter, polygon.Normals[vertIndex1]);
                if (separation2 > radius)
                {
                    return;
                }

                manifold._pointCount = 1;
                manifold._type = ManifoldType.FaceA;
                manifold._localPlaneNormal = polygon.Normals[vertIndex1];
                manifold._localPoint = faceCenter;

                var p0d = manifold._points[0];

                p0d.LocalPoint = circle.Position;
                p0d.Id.Key = 0;

                manifold._points[0] = p0d;
            }
        }

        /// <summary>
        /// Compute the collision manifold between two polygons.
        /// </summary>
        /// <param name="manifold">The manifold.</param>
        /// <param name="polyA">The poly A.</param>
        /// <param name="xfA">The xf A.</param>
        /// <param name="polyB">The poly B.</param>
        /// <param name="xfB">The xf B.</param>
        public static void CollidePolygons(ref Manifold manifold,
                               PolygonShape polyA, ref Transform xfA,
                               PolygonShape polyB, ref Transform xfB)
        {
            manifold._pointCount = 0;
            float totalRadius = polyA.Radius + polyB.Radius;

            int edgeA;
            float separationA = FindMaxSeparation(out edgeA, polyA, ref xfA, polyB, ref xfB);
            if (separationA > totalRadius)
                return;

            int edgeB;
            float separationB = FindMaxSeparation(out edgeB, polyB, ref xfB, polyA, ref xfA);
            if (separationB > totalRadius)
                return;

            PolygonShape poly1;	// reference polygon
            PolygonShape poly2;	// incident polygon
            Transform xf1, xf2;
            int edge1;		// reference edge
            byte flip;
            const float k_relativeTol = 0.98f;
            const float k_absoluteTol = 0.001f;

            if (separationB > k_relativeTol * separationA + k_absoluteTol)
            {
                poly1 = polyB;
                poly2 = polyA;
                xf1 = xfB;
                xf2 = xfA;
                edge1 = edgeB;
                manifold._type = ManifoldType.FaceB;
                flip = 1;
            }
            else
            {
                poly1 = polyA;
                poly2 = polyB;
                xf1 = xfA;
                xf2 = xfB;
                edge1 = edgeA;
                manifold._type = ManifoldType.FaceA;
                flip = 0;
            }

            FixedArray2<ClipVertex> incidentEdge;
            FindIncidentEdge(out incidentEdge, poly1, ref xf1, edge1, poly2, ref xf2);

            int count1 = poly1.VertexCount;

            Vector2 v11 = poly1.Vertices[edge1];
            Vector2 v12 = edge1 + 1 < count1 ? poly1.Vertices[edge1 + 1] : poly1.Vertices[0];

            Vector2 localTangent = v12 - v11;
            localTangent.Normalize();

            Vector2 localNormal = MathUtils.Cross(localTangent, 1.0f);
            Vector2 planePoint = 0.5f * (v11 + v12);

            Vector2 tangent = MathUtils.Multiply(ref xf1.R, localTangent);
            Vector2 normal = MathUtils.Cross(tangent, 1.0f);

            v11 = MathUtils.Multiply(ref xf1, v11);
            v12 = MathUtils.Multiply(ref xf1, v12);

            // Face offset.
            float frontOffset = Vector2.Dot(normal, v11);

            // Side offsets, extended by polytope skin thickness.
            float sideOffset1 = -Vector2.Dot(tangent, v11);
            float sideOffset2 = Vector2.Dot(tangent, v12);

            // Clip incident edge against extruded edge1 side edges.
            FixedArray2<ClipVertex> clipPoints1;
            FixedArray2<ClipVertex> clipPoints2;

            // Clip to box side 1
            int np = ClipSegmentToLine(out clipPoints1, ref incidentEdge, -tangent, sideOffset1);

            if (np < 2)
                return;

            // Clip to negative box side 1
            np = ClipSegmentToLine(out clipPoints2, ref clipPoints1, tangent, sideOffset2);

            if (np < 2)
            {
                return;
            }

            // Now clipPoints2 contains the clipped points.
            manifold._localPlaneNormal = localNormal;
            manifold._localPoint = planePoint;

            int pointCount = 0;
            for (int i = 0; i < Settings.MaxManifoldPoints; ++i)
            {
                float separation = Vector2.Dot(normal, clipPoints2[i].Vertex) - frontOffset;

                if (separation <= totalRadius)
                {
                    ManifoldPoint cp = manifold._points[pointCount];
                    cp.LocalPoint = MathUtils.MultiplyT(ref xf2, clipPoints2[i].Vertex);
                    cp.Id = clipPoints2[i].Id;
                    cp.Id.Features.Flip = flip;
                    manifold._points[pointCount] = cp;

                    ++pointCount;
                }
            }

            manifold._pointCount = pointCount;
        }

        /// <summary>
        /// Clipping for contact manifolds.
        /// </summary>
        /// <param name="vOut">The v out.</param>
        /// <param name="vIn">The v in.</param>
        /// <param name="normal">The normal.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        private static int ClipSegmentToLine(out FixedArray2<ClipVertex> vOut, ref FixedArray2<ClipVertex> vIn,
                                    Vector2 normal, float offset)
        {
            vOut = new FixedArray2<ClipVertex>();

            // Start with no output points
            int numOut = 0;

            // Calculate the distance of end points to the line
            float distance0 = Vector2.Dot(normal, vIn[0].Vertex) - offset;
            float distance1 = Vector2.Dot(normal, vIn[1].Vertex) - offset;

            // If the points are behind the plane
            if (distance0 <= 0.0f) vOut[numOut++] = vIn[0];
            if (distance1 <= 0.0f) vOut[numOut++] = vIn[1];

            // If the points are on different sides of the plane
            if (distance0 * distance1 < 0.0f)
            {
                // Find intersection point of edge and plane
                float interp = distance0 / (distance0 - distance1);

                var cv = vOut[numOut];

                cv.Vertex = vIn[0].Vertex + interp * (vIn[1].Vertex - vIn[0].Vertex);
                if (distance0 > 0.0f)
                {
                    cv.Id = vIn[0].Id;
                }
                else
                {
                    cv.Id = vIn[1].Id;
                }

                vOut[numOut] = cv;

                ++numOut;
            }

            return numOut;
        }

        /// <summary>
        /// Find the separation between poly1 and poly2 for a give edge normal on poly1.
        /// </summary>
        /// <param name="poly1">The poly1.</param>
        /// <param name="xf1">The XF1.</param>
        /// <param name="edge1">The edge1.</param>
        /// <param name="poly2">The poly2.</param>
        /// <param name="xf2">The XF2.</param>
        /// <returns></returns>
        static float EdgeSeparation(PolygonShape poly1, ref Transform xf1, int edge1,
                                    PolygonShape poly2, ref Transform xf2)
        {
            int count1 = poly1.VertexCount;
            int count2 = poly2.VertexCount;

            Debug.Assert(0 <= edge1 && edge1 < count1);

            // Convert normal from poly1's frame into poly2's frame.
#if MATH_OVERLOADS
            Vector2 normal1World = MathUtils.Multiply(ref xf1.R, poly1._normals[edge1]);
            Vector2 normal1 = MathUtils.MultiplyT(ref xf2.R, normal1World);
#else
            Vector2 p1n = poly1.Normals[edge1];
            Vector2 normal1World = new Vector2(xf1.R.col1.X * p1n.X + xf1.R.col2.X * p1n.Y, xf1.R.col1.Y * p1n.X + xf1.R.col2.Y * p1n.Y);
            Vector2 normal1 = new Vector2(normal1World.X * xf2.R.col1.X + normal1World.Y * xf2.R.col1.Y, normal1World.X * xf2.R.col2.X + normal1World.Y * xf2.R.col2.Y);
#endif
            // Find support vertex on poly2 for -normal.
            int index = 0;
            float minDot = Settings.MaxFloat;

            for (int i = 0; i < count2; ++i)
            {
#if !MATH_OVERLOADS // inlining this made it 1ms slower
                float dot = Vector2.Dot(poly2.Vertices[i], normal1);
#else
                Vector2 p2vi = poly2._vertices[i];
                float dot = p2vi.X * normal1.X + p2vi.Y * normal1.Y;
#endif
                if (dot < minDot)
                {
                    minDot = dot;
                    index = i;
                }
            }

#if MATH_OVERLOADS
	        Vector2 v1 = MathUtils.Multiply(ref xf1, poly1._vertices[edge1]);
	        Vector2 v2 = MathUtils.Multiply(ref xf2, poly2._vertices[index]);
#else
            Vector2 p1ve = poly1.Vertices[edge1];
            Vector2 p2vi = poly2.Vertices[index];
            Vector2 v1 = new Vector2(xf1.Position.X + xf1.R.col1.X * p1ve.X + xf1.R.col2.X * p1ve.Y,
                                     xf1.Position.Y + xf1.R.col1.Y * p1ve.X + xf1.R.col2.Y * p1ve.Y);
            Vector2 v2 = new Vector2(xf2.Position.X + xf2.R.col1.X * p2vi.X + xf2.R.col2.X * p2vi.Y,
                                     xf2.Position.Y + xf2.R.col1.Y * p2vi.X + xf2.R.col2.Y * p2vi.Y);
#endif

#if !MATH_OVERLOADS // inlining is 1ms slower
            float separation = Vector2.Dot(v2 - v1, normal1World);
#else
            Vector2 v2subv1 = new Vector2(v2.X - v1.X, v2.Y - v1.Y);
            float separation = v2subv1.X * normal1World.X + v2subv1.Y * normal1World.Y;
#endif
            return separation;
        }

        /// <summary>
        /// Find the max separation between poly1 and poly2 using edge normals from poly1.
        /// </summary>
        /// <param name="edgeIndex">Index of the edge.</param>
        /// <param name="poly1">The poly1.</param>
        /// <param name="xf1">The XF1.</param>
        /// <param name="poly2">The poly2.</param>
        /// <param name="xf2">The XF2.</param>
        /// <returns></returns>
        static float FindMaxSeparation(out int edgeIndex,
                                        PolygonShape poly1, ref Transform xf1,
                                        PolygonShape poly2, ref Transform xf2)
        {
            int count1 = poly1.VertexCount;

            // Vector pointing from the centroid of poly1 to the centroid of poly2.
            Vector2 d = MathUtils.Multiply(ref xf2, poly2.Centroid) - MathUtils.Multiply(ref xf1, poly1.Centroid);
            Vector2 dLocal1 = MathUtils.MultiplyT(ref xf1.R, d);

            // Find edge normal on poly1 that has the largest projection onto d.
            int edge = 0;
            float maxDot = -Settings.MaxFloat;
            for (int i = 0; i < count1; ++i)
            {
                float dot = Vector2.Dot(poly1.Normals[i], dLocal1);
                if (dot > maxDot)
                {
                    maxDot = dot;
                    edge = i;
                }
            }

            // Get the separation for the edge normal.
            float s = EdgeSeparation(poly1, ref xf1, edge, poly2, ref xf2);

            // Check the separation for the previous edge normal.
            int prevEdge = edge - 1 >= 0 ? edge - 1 : count1 - 1;
            float sPrev = EdgeSeparation(poly1, ref xf1, prevEdge, poly2, ref xf2);

            // Check the separation for the next edge normal.
            int nextEdge = edge + 1 < count1 ? edge + 1 : 0;
            float sNext = EdgeSeparation(poly1, ref xf1, nextEdge, poly2, ref xf2);

            // Find the best edge and the search direction.
            int bestEdge;
            float bestSeparation;
            int increment;
            if (sPrev > s && sPrev > sNext)
            {
                increment = -1;
                bestEdge = prevEdge;
                bestSeparation = sPrev;
            }
            else if (sNext > s)
            {
                increment = 1;
                bestEdge = nextEdge;
                bestSeparation = sNext;
            }
            else
            {
                edgeIndex = edge;
                return s;
            }

            // Perform a local search for the best edge normal.
            for (; ; )
            {
                if (increment == -1)
                    edge = bestEdge - 1 >= 0 ? bestEdge - 1 : count1 - 1;
                else
                    edge = bestEdge + 1 < count1 ? bestEdge + 1 : 0;

                s = EdgeSeparation(poly1, ref xf1, edge, poly2, ref xf2);

                if (s > bestSeparation)
                {
                    bestEdge = edge;
                    bestSeparation = s;
                }
                else
                {
                    break;
                }
            }

            edgeIndex = bestEdge;
            return bestSeparation;
        }

        static void FindIncidentEdge(out FixedArray2<ClipVertex> c,
                                     PolygonShape poly1, ref Transform xf1, int edge1,
                                     PolygonShape poly2, ref Transform xf2)
        {
            c = new FixedArray2<ClipVertex>();

            int count1 = poly1.VertexCount;
            int count2 = poly2.VertexCount;

            Debug.Assert(0 <= edge1 && edge1 < count1);

            // Get the normal of the reference edge in poly2's frame.
            Vector2 normal1 = MathUtils.MultiplyT(ref xf2.R, MathUtils.Multiply(ref xf1.R, poly1.Normals[edge1]));

            // Find the incident edge on poly2.
            int index = 0;
            float minDot = Settings.MaxFloat;
            for (int i = 0; i < count2; ++i)
            {
                float dot = Vector2.Dot(normal1, poly2.Normals[i]);
                if (dot < minDot)
                {
                    minDot = dot;
                    index = i;
                }
            }

            // Build the clip vertices for the incident edge.
            int i1 = index;
            int i2 = i1 + 1 < count2 ? i1 + 1 : 0;

            var cv0 = c[0];

            cv0.Vertex = MathUtils.Multiply(ref xf2, poly2.Vertices[i1]);
            cv0.Id.Features.ReferenceEdge = (byte)edge1;
            cv0.Id.Features.IncidentEdge = (byte)i1;
            cv0.Id.Features.IncidentVertex = 0;

            c[0] = cv0;

            var cv1 = c[1];
            cv1.Vertex = MathUtils.Multiply(ref xf2, poly2.Vertices[i2]);
            cv1.Id.Features.ReferenceEdge = (byte)edge1;
            cv1.Id.Features.IncidentEdge = (byte)i2;
            cv1.Id.Features.IncidentVertex = 1;

            c[1] = cv1;
        }
    }
}