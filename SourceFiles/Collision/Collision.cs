﻿/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2010 Ian Qvist
* 
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

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision
{
    internal enum ContactFeatureType : byte
    {
        Vertex = 0,
        Face = 1,
    }

    /// <summary>
    /// The features that intersect to form the contact point
    /// This must be 4 bytes or less.
    /// </summary>
    public struct ContactFeature
    {
        /// <summary>
        /// Feature index on ShapeA
        /// </summary>
        public byte IndexA;

        /// <summary>
        /// Feature index on ShapeB
        /// </summary>
        public byte IndexB;

        /// <summary>
        /// The feature type on ShapeA
        /// </summary>
        public byte TypeA;

        /// <summary>
        /// The feature type on ShapeB
        /// </summary>
        public byte TypeB;
    }

    /// <summary>
    /// Contact ids to facilitate warm starting.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct ContactID
    {
        /// <summary>
        /// The features that intersect to form the contact point
        /// </summary>
        [FieldOffset(0)]
        public ContactFeature Features;

        /// <summary>
        /// Used to quickly compare contact ids.
        /// </summary>
        [FieldOffset(0)]
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
        /// Uniquely identifies a contact point between two Shapes
        /// </summary>
        public ContactID Id;

        public Vector2 LocalPoint;

        public float NormalImpulse;

        public float TangentImpulse;
    }

    public enum ManifoldType
    {
        Circles,
        FaceA,
        FaceB
    }

    public enum EdgeType
    {
        Isolated,
        Concave,
        Flat,
        Convex
    }

    /// <summary>
    /// A manifold for two touching convex Shapes.
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
        /// Not use for Type.SeparationFunction.Points
        /// </summary>
        public Vector2 LocalNormal;

        /// <summary>
        /// Usage depends on manifold type
        /// </summary>
        public Vector2 LocalPoint;

        /// <summary>
        /// The number of manifold points
        /// </summary>
        public int PointCount;

        /// <summary>
        /// The points of contact
        /// </summary>
        public FixedArray2<ManifoldPoint> Points;

        public ManifoldType Type;
    }

    /// <summary>
    /// This is used to compute the current state of a contact manifold.
    /// </summary>
    public struct WorldManifold
    {
        /// <summary>
        /// world vector pointing from A to B
        /// </summary>
        public Vector2 Normal;

        /// <summary>
        /// world contact point (point of intersection)
        /// </summary>
        public FixedArray2<Vector2> Points;

        /// <summary>
        /// Evaluate the manifold with supplied transforms. This assumes
        /// modest motion from the original state. This does not change the
        /// point count, impulses, etc. The radii must come from the Shapes
        /// that generated the manifold.
        /// </summary>
        /// <param name="manifold">The manifold.</param>
        /// <param name="transformA">The transform for A.</param>
        /// <param name="radiusA">The radius for A.</param>
        /// <param name="transformB">The transform for B.</param>
        /// <param name="radiusB">The radius for B.</param>
        public WorldManifold(ref Manifold manifold,
                             ref Transform transformA, float radiusA,
                             ref Transform transformB, float radiusB)
        {
            Points = new FixedArray2<Vector2>();

            if (manifold.PointCount == 0)
            {
                Normal = Vector2.UnitY;
                return;
            }

            switch (manifold.Type)
            {
                case ManifoldType.Circles:
                    {
                        Vector2 pointA = MathUtils.Multiply(ref transformA, manifold.LocalPoint);
                        Vector2 pointB = MathUtils.Multiply(ref transformB, manifold.Points[0].LocalPoint);
                        Normal = new Vector2(1.0f, 0.0f);
                        if (Vector2.DistanceSquared(pointA, pointB) > Settings.Epsilon * Settings.Epsilon)
                        {
                            Normal = pointB - pointA;
                            Normal.Normalize();
                        }

                        Vector2 cA = pointA + radiusA * Normal;
                        Vector2 cB = pointB - radiusB * Normal;
                        Points[0] = 0.5f * (cA + cB);
                    }
                    break;

                case ManifoldType.FaceA:
                    {
                        Normal = MathUtils.Multiply(ref transformA.R, manifold.LocalNormal);
                        Vector2 planePoint = MathUtils.Multiply(ref transformA, manifold.LocalPoint);

                        for (int i = 0; i < manifold.PointCount; ++i)
                        {
                            Vector2 clipPoint = MathUtils.Multiply(ref transformB, manifold.Points[i].LocalPoint);
                            Vector2 cA = clipPoint + (radiusA - Vector2.Dot(clipPoint - planePoint, Normal)) * Normal;
                            Vector2 cB = clipPoint - radiusB * Normal;
                            Points[i] = 0.5f * (cA + cB);
                        }
                    }
                    break;

                case ManifoldType.FaceB:
                    {
                        Normal = MathUtils.Multiply(ref transformB.R, manifold.LocalNormal);
                        Vector2 planePoint = MathUtils.Multiply(ref transformB, manifold.LocalPoint);

                        for (int i = 0; i < manifold.PointCount; ++i)
                        {
                            Vector2 clipPoint = MathUtils.Multiply(ref transformA, manifold.Points[i].LocalPoint);
                            Vector2 cA = clipPoint - radiusA * Normal;
                            Vector2 cB = clipPoint + (radiusB - Vector2.Dot(clipPoint - planePoint, Normal)) * Normal;
                            Points[i] = 0.5f * (cA + cB);
                        }
                        // Ensure normal points from A to B.
                        Normal *= -1;
                    }
                    break;
                default:
                    Normal = Vector2.UnitY;
                    break;
            }
        }
    }

    /// <summary>
    /// This is used for determining the state of contact points.
    /// </summary>
    public enum PointState
    {
        /// <summary>
        /// Point does not exist
        /// </summary>
        Null,

        /// <summary>
        /// Point was added in the update
        /// </summary>
        Add,

        /// <summary>
        /// Point persisted across the update
        /// </summary>
        Persist,

        /// <summary>
        /// Point was removed in the update
        /// </summary>
        Remove,
    }

    /// <summary>
    /// Used for computing contact manifolds.
    /// </summary>
    public struct ClipVertex
    {
        public ContactID ID;
        public Vector2 V;
    }

    /// <summary>
    /// Ray-cast input data. The ray extends from p1 to p1 + maxFraction * (p2 - p1).
    /// </summary>
    public struct RayCastInput
    {
        public float MaxFraction;
        public Vector2 Point1, Point2;
    }

    /// <summary>
    /// Ray-cast output data.  The ray hits at p1 + fraction * (p2 - p1), where p1 and p2
    /// come from RayCastInput. 
    /// </summary>
    public struct RayCastOutput
    {
        public float Fraction;
        public Vector2 Normal;
    }

    /// <summary>
    /// An axis aligned bounding box.
    /// </summary>
    public struct AABB
    {
        /// <summary>
        /// The lower vertex
        /// </summary>
        public Vector2 LowerBound;

        /// <summary>
        /// The upper vertex
        /// </summary>
        public Vector2 UpperBound;

        public AABB(ref Vector2 min, ref Vector2 max)
        {
            LowerBound = min;
            UpperBound = max;
        }

        public AABB(float width, float height, Vector2 position)
        {
            LowerBound = position;
            UpperBound = new Vector2(position.X + width, position.Y + height);
        }

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
        /// <value></value>
        public Vector2 Center
        {
            get { return 0.5f * (LowerBound + UpperBound); }
        }

        /// <summary>
        /// Get the extents of the AABB (half-widths).
        /// </summary>
        /// <value></value>
        public Vector2 Extents
        {
            get { return 0.5f * (UpperBound - LowerBound); }
        }

        /// <summary>
        /// Get the perimeter length
        /// </summary>
        /// <value></value>
        public float Perimeter
        {
            get
            {
                float wx = UpperBound.X - LowerBound.X;
                float wy = UpperBound.Y - LowerBound.Y;
                return 2.0f * (wx + wy);
            }
        }

        /// <summary>
        /// Combine an AABB into this one.
        /// </summary>
        /// <param name="aabb">The aabb.</param>
        public void Combine(ref AABB aabb)
        {
            LowerBound = Vector2.Min(LowerBound, aabb.LowerBound);
            UpperBound = Vector2.Max(UpperBound, aabb.UpperBound);
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
        /// 	<c>true</c> if it contains the specified aabb; otherwise, <c>false</c>.
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

        /// <summary>
        /// Determines whether the AAABB contains the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>
        /// 	<c>true</c> if it contains the specified point; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(ref Vector2 point)
        {
            //using epsilon to try and gaurd against float rounding errors.
            if ((point.X > (LowerBound.X + Settings.Epsilon) && point.X < (UpperBound.X - Settings.Epsilon) &&
                 (point.Y > (LowerBound.Y + Settings.Epsilon) && point.Y < (UpperBound.Y - Settings.Epsilon))))
            {
                return true;
            }
            return false;
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

        public static bool TestOverlap(Shape shapeA, int indexA,
                                       Shape shapeB, int indexB,
                                       ref Transform xfA, ref Transform xfB)
        {
            DistanceInput input = new DistanceInput();
            input.ProxyA.Set(shapeA, indexA);
            input.ProxyB.Set(shapeB, indexB);
            input.TransformA = xfA;
            input.TransformB = xfB;
            input.UseRadii = true;

            SimplexCache cache;
            DistanceOutput output;
            Distance.ComputeDistance(out output, out cache, ref input);

            return output.Distance < 10.0f * Settings.Epsilon;
        }


        // From Real-time Collision Detection, p179.
        public bool RayCast(out RayCastOutput output, ref RayCastInput input)
        {
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
        /// Gets the vertices of the AABB.
        /// </summary>
        /// <value>The corners of the AABB</value>
        public Vertices Vertices
        {
            get
            {
                Vertices vertices = new Vertices();
                vertices.Add(LowerBound);
                vertices.Add(new Vector2(LowerBound.X, UpperBound.Y));
                vertices.Add(UpperBound);
                vertices.Add(new Vector2(UpperBound.X, LowerBound.Y));
                return vertices;
            }
        }
    }

    public static class Collision
    {
        #region EPAxisType enum

        private enum EPAxisType
        {
            Unknown,
            EdgeA,
            EdgeB,
        }

        #endregion

        private static PolygonShape s_polygonA = new PolygonShape();
        private static PolygonShape s_polygonB = new PolygonShape();

        public static void GetPointStates(out FixedArray2<PointState> state1, out FixedArray2<PointState> state2,
                                          ref Manifold manifold1, ref Manifold manifold2)
        {
            state1 = new FixedArray2<PointState>();
            state2 = new FixedArray2<PointState>();

            // Detect persists and removes.
            for (int i = 0; i < manifold1.PointCount; ++i)
            {
                ContactID id = manifold1.Points[i].Id;

                state1[i] = PointState.Remove;

                for (int j = 0; j < manifold2.PointCount; ++j)
                {
                    if (manifold2.Points[j].Id.Key == id.Key)
                    {
                        state1[i] = PointState.Persist;
                        break;
                    }
                }
            }

            // Detect persists and adds.
            for (int i = 0; i < manifold2.PointCount; ++i)
            {
                ContactID id = manifold2.Points[i].Id;

                state2[i] = PointState.Add;

                for (int j = 0; j < manifold1.PointCount; ++j)
                {
                    if (manifold1.Points[j].Id.Key == id.Key)
                    {
                        state2[i] = PointState.Persist;
                        break;
                    }
                }
            }
        }


        /// Compute the collision manifold between two circles.
        public static void CollideCircles(ref Manifold manifold,
                                          CircleShape circleA, ref Transform xfA,
                                          CircleShape circleB, ref Transform xfB)
        {
            manifold.PointCount = 0;

            Vector2 pA = MathUtils.Multiply(ref xfA, circleA.Position);
            Vector2 pB = MathUtils.Multiply(ref xfB, circleB.Position);

            Vector2 d = pB - pA;
            float distSqr = Vector2.Dot(d, d);
            float rA = circleA.Radius;
            float rB = circleB.Radius;
            float radius = rA + rB;
            if (distSqr > radius * radius)
            {
                return;
            }

            manifold.Type = ManifoldType.Circles;
            manifold.LocalPoint = circleA.Position;
            manifold.LocalNormal = Vector2.Zero;
            manifold.PointCount = 1;

            var p0 = manifold.Points[0];

            p0.LocalPoint = circleB.Position;
            p0.Id.Key = 0;

            manifold.Points[0] = p0;
        }

        /// <summary>
        /// Compute the collision manifold between a polygon and a circle.
        /// </summary>
        /// <param name="manifold">The manifold.</param>
        /// <param name="polygonA">The polygon A.</param>
        /// <param name="transformA">The transform of A.</param>
        /// <param name="circleB">The circle B.</param>
        /// <param name="transformB">The transform of B.</param>
        public static void CollidePolygonAndCircle(ref Manifold manifold,
                                                   PolygonShape polygonA, ref Transform transformA,
                                                   CircleShape circleB, ref Transform transformB)
        {
            manifold.PointCount = 0;

            // Compute circle position in the frame of the polygon.
            Vector2 c = MathUtils.Multiply(ref transformB, circleB.Position);
            Vector2 cLocal = MathUtils.MultiplyT(ref transformA, c);

            // Find the min separating edge.
            int normalIndex = 0;
            float separation = -Settings.MaxFloat;
            float radius = polygonA.Radius + circleB.Radius;
            int vertexCount = polygonA.Vertices.Count;

            for (int i = 0; i < vertexCount; ++i)
            {
                float s = Vector2.Dot(polygonA.Normals[i], cLocal - polygonA.Vertices[i]);

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
            Vector2 v1 = polygonA.Vertices[vertIndex1];
            Vector2 v2 = polygonA.Vertices[vertIndex2];

            // If the center is inside the polygon ...
            if (separation < Settings.Epsilon)
            {
                manifold.PointCount = 1;
                manifold.Type = ManifoldType.FaceA;
                manifold.LocalNormal = polygonA.Normals[normalIndex];
                manifold.LocalPoint = 0.5f * (v1 + v2);

                var p0 = manifold.Points[0];

                p0.LocalPoint = circleB.Position;
                p0.Id.Key = 0;

                manifold.Points[0] = p0;

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

                manifold.PointCount = 1;
                manifold.Type = ManifoldType.FaceA;
                manifold.LocalNormal = cLocal - v1;
                manifold.LocalNormal.Normalize();
                manifold.LocalPoint = v1;

                var p0b = manifold.Points[0];

                p0b.LocalPoint = circleB.Position;
                p0b.Id.Key = 0;

                manifold.Points[0] = p0b;
            }
            else if (u2 <= 0.0f)
            {
                if (Vector2.DistanceSquared(cLocal, v2) > radius * radius)
                {
                    return;
                }

                manifold.PointCount = 1;
                manifold.Type = ManifoldType.FaceA;
                manifold.LocalNormal = cLocal - v2;
                manifold.LocalNormal.Normalize();
                manifold.LocalPoint = v2;

                var p0c = manifold.Points[0];

                p0c.LocalPoint = circleB.Position;
                p0c.Id.Key = 0;

                manifold.Points[0] = p0c;
            }
            else
            {
                Vector2 faceCenter = 0.5f * (v1 + v2);
                float separation2 = Vector2.Dot(cLocal - faceCenter, polygonA.Normals[vertIndex1]);
                if (separation2 > radius)
                {
                    return;
                }

                manifold.PointCount = 1;
                manifold.Type = ManifoldType.FaceA;
                manifold.LocalNormal = polygonA.Normals[vertIndex1];
                manifold.LocalPoint = faceCenter;

                var p0d = manifold.Points[0];

                p0d.LocalPoint = circleB.Position;
                p0d.Id.Key = 0;

                manifold.Points[0] = p0d;
            }
        }

        /// <summary>
        /// Compute the collision manifold between two polygons.
        /// </summary>
        /// <param name="manifold">The manifold.</param>
        /// <param name="polyA">The poly A.</param>
        /// <param name="transformA">The transform A.</param>
        /// <param name="polyB">The poly B.</param>
        /// <param name="transformB">The transform B.</param>
        public static void CollidePolygons(ref Manifold manifold,
                                           PolygonShape polyA, ref Transform transformA,
                                           PolygonShape polyB, ref Transform transformB)
        {
            manifold.PointCount = 0;
            float totalRadius = polyA.Radius + polyB.Radius;

            int edgeA = 0;
            float separationA = FindMaxSeparation(out edgeA, polyA, ref transformA, polyB, ref transformB);
            if (separationA > totalRadius)
                return;

            int edgeB = 0;
            float separationB = FindMaxSeparation(out edgeB, polyB, ref transformB, polyA, ref transformA);
            if (separationB > totalRadius)
                return;

            PolygonShape poly1; // reference polygon
            PolygonShape poly2; // incident polygon
            Transform xf1, xf2;
            int edge1; // reference edge
            bool flip;
            const float k_relativeTol = 0.98f;
            const float k_absoluteTol = 0.001f;

            if (separationB > k_relativeTol * separationA + k_absoluteTol)
            {
                poly1 = polyB;
                poly2 = polyA;
                xf1 = transformB;
                xf2 = transformA;
                edge1 = edgeB;
                manifold.Type = ManifoldType.FaceB;
                flip = true;
            }
            else
            {
                poly1 = polyA;
                poly2 = polyB;
                xf1 = transformA;
                xf2 = transformB;
                edge1 = edgeA;
                manifold.Type = ManifoldType.FaceA;
                flip = false;
            }

            FixedArray2<ClipVertex> incidentEdge;
            FindIncidentEdge(out incidentEdge, poly1, ref xf1, edge1, poly2, ref xf2);

            int count1 = poly1.Vertices.Count;

            int iv1 = edge1;
            int iv2 = edge1 + 1 < count1 ? edge1 + 1 : 0;

            Vector2 v11 = poly1.Vertices[iv1];
            Vector2 v12 = poly1.Vertices[iv2];

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
            float sideOffset1 = -Vector2.Dot(tangent, v11) + totalRadius;
            float sideOffset2 = Vector2.Dot(tangent, v12) + totalRadius;

            // Clip incident edge against extruded edge1 side edges.
            FixedArray2<ClipVertex> clipPoints1;
            FixedArray2<ClipVertex> clipPoints2;
            int np;

            // Clip to box side 1
            np = ClipSegmentToLine(out clipPoints1, ref incidentEdge, -tangent, sideOffset1, iv1);

            if (np < 2)
                return;

            // Clip to negative box side 1
            np = ClipSegmentToLine(out clipPoints2, ref clipPoints1, tangent, sideOffset2, iv2);

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
                    cp.LocalPoint = MathUtils.MultiplyT(ref xf2, clipPoints2[i].V);
                    cp.Id = clipPoints2[i].ID;

                    if (flip)
                    {
                        // Swap features
                        ContactFeature cf = cp.Id.Features;
                        cp.Id.Features.IndexA = cf.IndexB;
                        cp.Id.Features.IndexB = cf.IndexA;
                        cp.Id.Features.TypeA = cf.TypeB;
                        cp.Id.Features.TypeB = cf.TypeA;
                    }

                    manifold.Points[pointCount] = cp;

                    ++pointCount;
                }
            }

            manifold.PointCount = pointCount;
        }

        /// <summary>
        // Compute contact points for edge versus circle.
        // This accounts for edge connectivity.
        /// </summary>
        /// <param name="manifold">The manifold.</param>
        /// <param name="edgeA">The edge A.</param>
        /// <param name="transformA">The transform A.</param>
        /// <param name="circleB">The circle B.</param>
        /// <param name="transformB">The transform B.</param>
        public static void CollideEdgeAndCircle(ref Manifold manifold,
                                                EdgeShape edgeA, ref Transform transformA,
                                                CircleShape circleB, ref Transform transformB)
        {
            manifold.PointCount = 0;

            // Compute circle in frame of edge
            Vector2 Q = MathUtils.MultiplyT(ref transformA, MathUtils.Multiply(ref transformB, circleB.Position));

            Vector2 A = edgeA.Vertex1, B = edgeA.Vertex2;
            Vector2 e = B - A;

            // Barycentric coordinates
            float u = Vector2.Dot(e, B - Q);
            float v = Vector2.Dot(e, Q - A);

            float radius = edgeA.Radius + circleB.Radius;

            ContactFeature cf;
            cf.IndexB = 0;
            cf.TypeB = (byte)ContactFeatureType.Vertex;

            Vector2 P, d;

            // Region A
            if (v <= 0.0f)
            {
                P = A;
                d = Q - P;
                float dd = Vector2.Dot(d, d);
                if (dd > radius * radius)
                {
                    return;
                }

                // Is there an edge connected to A?
                if (edgeA.HasVertex0)
                {
                    Vector2 A1 = edgeA.Vertex0;
                    Vector2 B1 = A;
                    Vector2 e1 = B1 - A1;
                    float u1 = Vector2.Dot(e1, B1 - Q);

                    // Is the circle in Region AB of the previous edge?
                    if (u1 > 0.0f)
                    {
                        return;
                    }
                }

                cf.IndexA = 0;
                cf.TypeA = (byte)ContactFeatureType.Vertex;
                manifold.PointCount = 1;
                manifold.Type = ManifoldType.Circles;
                manifold.LocalNormal = Vector2.Zero;
                manifold.LocalPoint = P;
                var mp = new ManifoldPoint();
                mp.Id.Key = 0;
                mp.Id.Features = cf;
                mp.LocalPoint = circleB.Position;
                manifold.Points[0] = mp;
                return;
            }

            // Region B
            if (u <= 0.0f)
            {
                P = B;
                d = Q - P;
                float dd = Vector2.Dot(d, d);
                if (dd > radius * radius)
                {
                    return;
                }

                // Is there an edge connected to B?
                if (edgeA.HasVertex3)
                {
                    Vector2 B2 = edgeA.Vertex3;
                    Vector2 A2 = B;
                    Vector2 e2 = B2 - A2;
                    float v2 = Vector2.Dot(e2, Q - A2);

                    // Is the circle in Region AB of the next edge?
                    if (v2 > 0.0f)
                    {
                        return;
                    }
                }

                cf.IndexA = 1;
                cf.TypeA = (byte)ContactFeatureType.Vertex;
                manifold.PointCount = 1;
                manifold.Type = ManifoldType.Circles;
                manifold.LocalNormal = Vector2.Zero;
                manifold.LocalPoint = P;
                var mp = new ManifoldPoint();
                mp.Id.Key = 0;
                mp.Id.Features = cf;
                mp.LocalPoint = circleB.Position;
                manifold.Points[0] = mp;
                return;
            }

            // Region AB
            float den = Vector2.Dot(e, e);
            Debug.Assert(den > 0.0f);
            P = (1.0f / den) * (u * A + v * B);
            d = Q - P;
            float dd2 = Vector2.Dot(d, d);
            if (dd2 > radius * radius)
            {
                return;
            }

            Vector2 n = new Vector2(-e.Y, e.X);
            if (Vector2.Dot(n, Q - A) < 0.0f)
            {
                n = new Vector2(-n.X, -n.Y);
            }
            n.Normalize();

            cf.IndexA = 0;
            cf.TypeA = (byte)ContactFeatureType.Face;
            manifold.PointCount = 1;
            manifold.Type = ManifoldType.FaceA;
            manifold.LocalNormal = n;
            manifold.LocalPoint = A;
            var mp2 = new ManifoldPoint();
            mp2.Id.Key = 0;
            mp2.Id.Features = cf;
            mp2.LocalPoint = circleB.Position;
            manifold.Points[0] = mp2;
        }

        private static void ComputeEdgeSeperation(ref Vector2 v1, ref Vector2 n, PolygonShape polygonB, out EPAxis axis)
        {
            // EdgeA separation
            axis.Type = EPAxisType.EdgeA;
            axis.Index = 0;
            axis.Separation = Vector2.Dot(n, polygonB.Vertices[0] - v1);
            for (int i = 1; i < polygonB.Vertices.Count; ++i)
            {
                float s = Vector2.Dot(n, polygonB.Vertices[i] - v1);
                if (s < axis.Separation)
                {
                    axis.Separation = s;
                }
            }
        }

        private static void ComputePolygonSeperation(ref Vector2 v1, ref Vector2 v2, PolygonShape polygonB,
                                                       float radius, out EPAxis axis)
        {
            // PolygonB separation
            axis.Type = EPAxisType.EdgeB;
            axis.Index = 0;
            axis.Separation = float.MinValue;
            for (int i = 0; i < polygonB.Vertices.Count; ++i)
            {
                float s1 = Vector2.Dot(polygonB.Normals[i], v1 - polygonB.Vertices[i]);
                float s2 = Vector2.Dot(polygonB.Normals[i], v2 - polygonB.Vertices[i]);
                float s = Math.Min(s1, s2);
                if (s > axis.Separation)
                {
                    axis.Index = i;
                    axis.Separation = s;
                    if (s > radius)
                    {
                        break;
                    }
                }
            }
        }

        private static void FindIncidentEdge(ref FixedArray2<ClipVertex> c, PolygonShape poly1, int edge1,
                                             PolygonShape poly2)
        {
            int count1 = poly1.Vertices.Count;
            int count2 = poly2.Vertices.Count;

            Debug.Assert(0 <= edge1 && edge1 < count1);

            // Get the normal of the reference edge in poly2's frame.
            Vector2 normal1 = poly1.Normals[edge1];

            // Find the incident edge on poly2.
            int index = 0;
            float minDot = float.MaxValue;
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

            ClipVertex ctemp = new ClipVertex();
            ctemp.V = poly2.Vertices[i1];
            ctemp.ID.Features.IndexA = (byte)edge1;
            ctemp.ID.Features.IndexB = (byte)i1;
            ctemp.ID.Features.TypeA = (byte)ContactFeatureType.Face;
            ctemp.ID.Features.TypeB = (byte)ContactFeatureType.Vertex;
            c[0] = ctemp;

            ctemp.V = poly2.Vertices[i2];
            ctemp.ID.Features.IndexA = (byte)edge1;
            ctemp.ID.Features.IndexB = (byte)i2;
            ctemp.ID.Features.TypeA = (byte)ContactFeatureType.Face;
            ctemp.ID.Features.TypeB = (byte)ContactFeatureType.Vertex;
            c[1] = ctemp;
        }

        /// <summary>
        /// Collide and edge and polygon. This uses the SAT and clipping to produce up to 2 contact points.
        /// Edge adjacency is handle to produce locally valid contact points and normals. This is intended
        /// to allow the polygon to slide smoothly over an edge chain.
        ///
        /// Algorithm
        /// 1. Classify front-side or back-side collision with edge.
        /// 2. Compute separation
        /// 3. Process adjacent edges
        /// 4. Classify adjacent edge as convex, flat, null, or concave
        /// 5. Skip null or concave edges. Concave edges get a separate manifold.
        /// 6. If the edge is flat, compute contact points as normal. Discard boundary points.
        /// 7. If the edge is convex, compute it's separation.
        /// 8. Use the minimum separation of up to three edges. If the minimum separation
        ///    is not the primary edge, return.
        /// 9. If the minimum separation is the primary edge, compute the contact points and return.
        /// </summary>
        /// <param name="manifold">The manifold.</param>
        /// <param name="edgeA">The edge A.</param>
        /// <param name="xfA">The xf A.</param>
        /// <param name="polygonB_in">The polygon b_in.</param>
        /// <param name="xfB">The xf B.</param>
        public static void CollideEdgeAndPolygon(ref Manifold manifold,
                                                 EdgeShape edgeA, ref Transform xfA,
                                                 PolygonShape polygonB_in, ref Transform xfB)
        {
            manifold.PointCount = 0;

            Transform xf;
            MathUtils.MultiplyT(ref xfA, ref xfB, out xf);

            // Create a polygon for edge shape A
            s_polygonA.SetAsEdge(edgeA.Vertex1, edgeA.Vertex2);

            // Build polygonB in frame A
            s_polygonB.Radius = polygonB_in.Radius;
            s_polygonB.Centroid = MathUtils.Multiply(ref xf, polygonB_in.Centroid);
            s_polygonB.Vertices = new Vertices(polygonB_in.Vertices.Count);
            s_polygonB.Normals = new Vertices(polygonB_in.Vertices.Count);
            for (int i = 0; i < polygonB_in.Vertices.Count; ++i)
            {
                s_polygonB.Vertices.Add(MathUtils.Multiply(ref xf, polygonB_in.Vertices[i]));
                s_polygonB.Normals.Add(MathUtils.Multiply(ref xf.R, polygonB_in.Normals[i]));
            }

            float totalRadius = s_polygonA.Radius + s_polygonB.Radius;

            // Edge geometry
            Vector2 v1 = edgeA.Vertex1;
            Vector2 v2 = edgeA.Vertex2;
            Vector2 e = v2 - v1;
            Vector2 edgeNormal = new Vector2(e.Y, -e.X);
            edgeNormal.Normalize();

            // Determine side
            bool isFrontSide = Vector2.Dot(edgeNormal, s_polygonB.Centroid - v1) >= 0.0f;
            if (isFrontSide == false)
            {
                edgeNormal = -edgeNormal;
            }

            // Compute primary separating axis
            EPAxis edgeAxis;

            ComputeEdgeSeperation(ref v1, ref edgeNormal, s_polygonB, out edgeAxis);

            if (edgeAxis.Separation > totalRadius)
            {
                // Shapes are separated
                return;
            }

            // Classify adjacent edges
            FixedArray2<EdgeType> types = new FixedArray2<EdgeType>();
            //types[0] = EdgeType.Isolated;
            //types[1] = EdgeType.Isolated;
            if (edgeA.HasVertex0)
            {
                Vector2 v0 = edgeA.Vertex0;
                float s = Vector2.Dot(edgeNormal, v0 - v1);

                if (s > 0.1f * Settings.LinearSlop)
                {
                    types[0] = EdgeType.Concave;
                }
                else if (s >= -0.1f * Settings.LinearSlop)
                {
                    types[0] = EdgeType.Flat;
                }
                else
                {
                    types[0] = EdgeType.Convex;
                }
            }

            if (edgeA.HasVertex3)
            {
                Vector2 v3 = edgeA.Vertex3;
                float s = Vector2.Dot(edgeNormal, v3 - v2);
                if (s > 0.1f * Settings.LinearSlop)
                {
                    types[1] = EdgeType.Concave;
                }
                else if (s >= -0.1f * Settings.LinearSlop)
                {
                    types[1] = EdgeType.Flat;
                }
                else
                {
                    types[1] = EdgeType.Convex;
                }
            }

            if (types[0] == EdgeType.Convex)
            {
                // Check separation on previous edge.
                Vector2 v0 = edgeA.Vertex0;
                Vector2 e0 = v1 - v0;

                Vector2 n0 = new Vector2(e0.Y, -e0.X);
                n0.Normalize();
                if (isFrontSide == false)
                {
                    n0 = -n0;
                }

                EPAxis axis1;

                ComputeEdgeSeperation(ref v0, ref n0, s_polygonB, out axis1);
                if (axis1.Separation > edgeAxis.Separation)
                {
                    // The polygon should collide with previous edge
                    return;
                }
            }

            if (types[1] == EdgeType.Convex)
            {
                // Check separation on next edge.
                Vector2 v3 = edgeA.Vertex3;
                Vector2 e2 = v3 - v2;

                Vector2 n2 = new Vector2(e2.Y, -e2.X);
                n2.Normalize();
                if (isFrontSide == false)
                {
                    n2 = -n2;
                }

                EPAxis axis2;

                ComputeEdgeSeperation(ref v2, ref n2, s_polygonB, out axis2);

                if (axis2.Separation > edgeAxis.Separation)
                {
                    // The polygon should collide with the next edge
                    return;
                }
            }

            EPAxis polygonAxis;
            ComputePolygonSeperation(ref v1, ref v2, s_polygonB, totalRadius, out polygonAxis);
            if (polygonAxis.Separation > totalRadius)
            {
                return;
            }

            // Use hysteresis for jitter reduction.
            const float k_relativeTol = 0.98f;
            const float k_absoluteTol = 0.001f;

            EPAxis primaryAxis;
            if (polygonAxis.Separation > k_relativeTol * edgeAxis.Separation + k_absoluteTol)
            {
                primaryAxis = polygonAxis;
            }
            else
            {
                primaryAxis = edgeAxis;
            }

            PolygonShape poly1;
            PolygonShape poly2;
            if (primaryAxis.Type == EPAxisType.EdgeA)
            {
                poly1 = s_polygonA;
                poly2 = s_polygonB;
                if (isFrontSide == false)
                {
                    primaryAxis.Index = 1;
                }
                manifold.Type = ManifoldType.FaceA;
            }
            else
            {
                poly1 = s_polygonB;
                poly2 = s_polygonA;
                manifold.Type = ManifoldType.FaceB;
            }

            int edge1 = primaryAxis.Index;

            FixedArray2<ClipVertex> incidentEdge = new FixedArray2<ClipVertex>();
            FindIncidentEdge(ref incidentEdge, poly1, primaryAxis.Index, poly2);
            int count1 = poly1.Vertices.Count;
            int iv1 = edge1;
            int iv2 = edge1 + 1 < count1 ? edge1 + 1 : 0;

            Vector2 v11 = poly1.Vertices[iv1];
            Vector2 v12 = poly1.Vertices[iv2];

            Vector2 tangent = v12 - v11;
            tangent.Normalize();

            Vector2 normal = MathUtils.Cross(tangent, 1.0f);
            Vector2 planePoint = 0.5f * (v11 + v12);

            // Face offset.
            float frontOffset = Vector2.Dot(normal, v11);

            // Side offsets, extended by polytope skin thickness.
            float sideOffset1 = -Vector2.Dot(tangent, v11) + totalRadius;
            float sideOffset2 = Vector2.Dot(tangent, v12) + totalRadius;

            // Clip incident edge against extruded edge1 side edges.
            FixedArray2<ClipVertex> clipPoints1;
            FixedArray2<ClipVertex> clipPoints2;

            // Clip to box side 1
            int np = ClipSegmentToLine(out clipPoints1, ref incidentEdge, -tangent, sideOffset1, iv1);

            if (np < Settings.MaxManifoldPoints)
            {
                return;
            }

            // Clip to negative box side 1
            np = ClipSegmentToLine(out clipPoints2, ref clipPoints1, tangent, sideOffset2, iv2);

            if (np < Settings.MaxManifoldPoints)
            {
                return;
            }

            // Now clipPoints2 contains the clipped points.
            if (primaryAxis.Type == EPAxisType.EdgeA)
            {
                manifold.LocalNormal = normal;
                manifold.LocalPoint = planePoint;
            }
            else
            {
                manifold.LocalNormal = MathUtils.MultiplyT(ref xf.R, normal);
                manifold.LocalPoint = MathUtils.MultiplyT(ref xf, planePoint);
            }

            int pointCount = 0;
            for (int i = 0; i < Settings.MaxManifoldPoints; ++i)
            {
                float separation = Vector2.Dot(normal, clipPoints2[i].V) - frontOffset;

                if (separation <= totalRadius)
                {
                    ManifoldPoint cp = manifold.Points[pointCount];

                    if (primaryAxis.Type == EPAxisType.EdgeA)
                    {
                        cp.LocalPoint = MathUtils.MultiplyT(ref xf, clipPoints2[i].V);
                        cp.Id = clipPoints2[i].ID;
                    }
                    else
                    {
                        cp.LocalPoint = clipPoints2[i].V;
                        cp.Id.Features.TypeA = clipPoints2[i].ID.Features.TypeB;
                        cp.Id.Features.TypeB = clipPoints2[i].ID.Features.TypeA;
                        cp.Id.Features.IndexA = clipPoints2[i].ID.Features.IndexB;
                        cp.Id.Features.IndexB = clipPoints2[i].ID.Features.IndexA;
                    }

                    manifold.Points[pointCount] = cp;
                    if (cp.Id.Features.TypeA == (byte)ContactFeatureType.Vertex &&
                        types[cp.Id.Features.IndexA] == EdgeType.Flat)
                    {
                        continue;
                    }

                    ++pointCount;
                }
            }

            manifold.PointCount = pointCount;
        }

        /// <summary>
        /// Clipping for contact manifolds.
        /// </summary>
        /// <param name="vOut">The v out.</param>
        /// <param name="vIn">The v in.</param>
        /// <param name="normal">The normal.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="vertexIndexA">The vertex index A.</param>
        /// <returns></returns>
        public static int ClipSegmentToLine(out FixedArray2<ClipVertex> vOut, ref FixedArray2<ClipVertex> vIn,
                                            Vector2 normal, float offset, int vertexIndexA)
        {
            vOut = new FixedArray2<ClipVertex>();

            // Start with no output points
            int numOut = 0;

            // Calculate the distance of end points to the line
            float distance0 = Vector2.Dot(normal, vIn[0].V) - offset;
            float distance1 = Vector2.Dot(normal, vIn[1].V) - offset;

            // If the points are behind the plane
            if (distance0 <= 0.0f) vOut[numOut++] = vIn[0];
            if (distance1 <= 0.0f) vOut[numOut++] = vIn[1];

            // If the points are on different sides of the plane
            if (distance0 * distance1 < 0.0f)
            {
                // Find intersection point of edge and plane
                float interp = distance0 / (distance0 - distance1);

                var cv = vOut[numOut];

                cv.V = vIn[0].V + interp * (vIn[1].V - vIn[0].V);

                // VertexA is hitting edgeB.
                cv.ID.Features.IndexA = (byte)vertexIndexA;
                cv.ID.Features.IndexB = vIn[0].ID.Features.IndexB;
                cv.ID.Features.TypeA = (byte)ContactFeatureType.Vertex;
                cv.ID.Features.TypeB = (byte)ContactFeatureType.Face;

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
        private static float EdgeSeparation(PolygonShape poly1, ref Transform xf1, int edge1,
                                            PolygonShape poly2, ref Transform xf2)
        {
            int count1 = poly1.Vertices.Count;
            int count2 = poly2.Vertices.Count;

            Debug.Assert(0 <= edge1 && edge1 < count1);

            // Convert normal from poly1's frame into poly2's frame.
#if MATH_OVERLOADS
            Vector2 normal1World = MathUtils.Multiply(ref xf1.R, poly1._normals[edge1]);
            Vector2 normal1 = MathUtils.MultiplyT(ref xf2.R, normal1World);
#else
            Vector2 p1n = poly1.Normals[edge1];
            Vector2 normal1World = new Vector2(xf1.R.col1.X * p1n.X + xf1.R.col2.X * p1n.Y,
                                               xf1.R.col1.Y * p1n.X + xf1.R.col2.Y * p1n.Y);
            Vector2 normal1 = new Vector2(normal1World.X * xf2.R.col1.X + normal1World.Y * xf2.R.col1.Y,
                                          normal1World.X * xf2.R.col2.X + normal1World.Y * xf2.R.col2.Y);
#endif
            // Find support vertex on poly2 for -normal.
            int index = 0;
            float minDot = Settings.MaxFloat;

            for (int i = 0; i < count2; ++i)
            {
#if !MATH_OVERLOADS
                // inlining this made it 1ms slower
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

#if !MATH_OVERLOADS
            // inlining is 1ms slower
            float separation = Vector2.Dot(v2 - v1, normal1World);
#else
            Vector2 v2subv1 = new Vector2(v2.X - v1.X, v2.Y - v1.Y);
            float separation = v2subv1.X * normal1World.X + v2subv1.Y * normal1World.Y;
#endif
            return separation;
        }

        /// <summary>
        // Find the max separation between poly1 and poly2 using edge normals from poly1.
        /// </summary>
        /// <param name="edgeIndex">Index of the edge.</param>
        /// <param name="poly1">The poly1.</param>
        /// <param name="xf1">The XF1.</param>
        /// <param name="poly2">The poly2.</param>
        /// <param name="xf2">The XF2.</param>
        /// <returns></returns>
        private static float FindMaxSeparation(out int edgeIndex,
                                               PolygonShape poly1, ref Transform xf1,
                                               PolygonShape poly2, ref Transform xf2)
        {
            edgeIndex = -1;
            int count1 = poly1.Vertices.Count;

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

        private static void FindIncidentEdge(out FixedArray2<ClipVertex> c,
                                             PolygonShape poly1, ref Transform xf1, int edge1,
                                             PolygonShape poly2, ref Transform xf2)
        {
            c = new FixedArray2<ClipVertex>();

            int count1 = poly1.Vertices.Count;
            int count2 = poly2.Vertices.Count;

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

            cv0.V = MathUtils.Multiply(ref xf2, poly2.Vertices[i1]);
            cv0.ID.Features.IndexA = (byte)edge1;
            cv0.ID.Features.IndexB = (byte)i1;
            cv0.ID.Features.TypeA = (byte)ContactFeatureType.Face;
            cv0.ID.Features.TypeB = (byte)ContactFeatureType.Vertex;

            c[0] = cv0;

            var cv1 = c[1];
            cv1.V = MathUtils.Multiply(ref xf2, poly2.Vertices[i2]);
            cv1.ID.Features.IndexA = (byte)edge1;
            cv1.ID.Features.IndexB = (byte)i2;
            cv1.ID.Features.TypeA = (byte)ContactFeatureType.Face;
            cv1.ID.Features.TypeB = (byte)ContactFeatureType.Vertex;

            c[1] = cv1;
        }

        #region Nested type: EPAxis

        private struct EPAxis
        {
            public int Index;
            public float Separation;
            public EPAxisType Type;
        }

        #endregion
    }
}