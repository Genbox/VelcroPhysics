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

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision
{
    /// <summary>
    /// Features contain the contact details of a collision
    /// </summary>
    public struct Features
    {
        /// <summary>
        /// A value of 1 indicates that the reference edge is on shape2.
        /// </summary>
        public byte Flip;

        /// <summary>
        /// The edge most anti-parallel to the reference edge.
        /// </summary>
        public byte IncidentEdge;

        /// <summary>
        /// The vertex (0 or 1) on the incident edge that was clipped.
        /// </summary>
        public byte IncidentVertex;

        /// <summary>
        /// The edge that defines the outward contact normal.
        /// </summary>
        public byte ReferenceEdge;
    }

    /// Contact ids to facilitate warm starting.
    [StructLayout(LayoutKind.Explicit)]
    public struct ContactID
    {
        /// <summary>
        /// The features that intersect to form the contact point
        /// </summary>
        [FieldOffset(0)] public Features Features;

        /// <summary>
        /// Used to quickly compare contact ids.
        /// </summary>
        [FieldOffset(0)] public uint Key;
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
        /// uniquely identifies a contact point between two shapes
        /// </summary>
        public ContactID Id;

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
        /// not use for Type.SeparationFunction.Points
        /// </summary>
        public Vector2 LocalNormal;

        /// <summary>
        /// usage depends on manifold type
        /// </summary>
        public Vector2 LocalPoint;

        /// <summary>
        /// the number of manifold points
        /// </summary>
        public int PointCount;

        /// <summary>
        /// the points of contact
        /// </summary>
        public FixedArray2<ManifoldPoint> Points;

        /// <summary>
        /// The manifold type
        /// </summary>
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
                        Vector2 pointA = MathUtils.Multiply(ref xfA, manifold.LocalPoint);
                        Vector2 pointB = MathUtils.Multiply(ref xfB, manifold.Points[0].LocalPoint);
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
                        Normal = MathUtils.Multiply(ref xfA.R, manifold.LocalNormal);
                        Vector2 planePoint = MathUtils.Multiply(ref xfA, manifold.LocalPoint);

                        for (int i = 0; i < manifold.PointCount; ++i)
                        {
                            Vector2 clipPoint = MathUtils.Multiply(ref xfB, manifold.Points[i].LocalPoint);
                            Vector2 cA = clipPoint + (radiusA - Vector2.Dot(clipPoint - planePoint, Normal)) * Normal;
                            Vector2 cB = clipPoint - radiusB * Normal;
                            Points[i] = 0.5f * (cA + cB);
                        }
                    }
                    break;

                case ManifoldType.FaceB:
                    {
                        Normal = MathUtils.Multiply(ref xfB.R, manifold.LocalNormal);
                        Vector2 planePoint = MathUtils.Multiply(ref xfB, manifold.LocalPoint);

                        for (int i = 0; i < manifold.PointCount; ++i)
                        {
                            Vector2 clipPoint = MathUtils.Multiply(ref xfA, manifold.Points[i].LocalPoint);
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
        public ContactID Id;
        public Vector2 Vertex;
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
    public struct AABB : IEquatable<AABB>
    {
        //TODO: Can cause trouble with VB.net
        /// <summary>
        /// the lower vertex
        /// </summary>
        public Vector2 LowerBound;

        /// <summary>
        /// the upper vertex
        /// </summary>
        public Vector2 UpperBound;

        public AABB(Vector2 min, Vector2 max)
        {
            LowerBound = min;
            UpperBound = max;
        }

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
        /// Gets the width.
        /// </summary>
        /// <Value>The width.</Value>
        public float Width
        {
            get { return UpperBound.X - LowerBound.X; }
        }

        /// <summary>
        /// Gets the height.
        /// </summary>
        /// <Value>The height.</Value>
        public float Height
        {
            get { return UpperBound.Y - LowerBound.Y; }
        }

        /// <summary>
        /// Gets or sets the position of the AABB
        /// The position is the same as LowerBound
        /// </summary>
        /// <value>The position.</value>
        public Vector2 Position
        {
            get { return LowerBound; }
            set { LowerBound = value; }
        }

        /// <summary>
        /// Verify that the bounds are sorted.
        /// </summary>
        /// <value>
        ///   &lt;c&gt;true&lt;/c&gt; if this instance is valid; otherwise, &lt;c&gt;false&lt;/c&gt;.
        /// </value>
        public bool Valid
        {
            get
            {
                Vector2 d = UpperBound - LowerBound;
                bool valid = d.X >= 0.0f && d.Y >= 0.0f;
                valid = valid && LowerBound.IsValid() && UpperBound.IsValid();
                return valid;
            }
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

        #region IEquatable<AABB> Members

        public bool Equals(AABB other)
        {
            return ((LowerBound == other.LowerBound) && (UpperBound == other.UpperBound));
        }

        #endregion

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
        /// Gets the vertices of the AABB.
        /// </summary>
        /// <returns>The corners of the AABB</returns>
        public Vertices GetVertices()
        {
            Vertices vertices = new Vertices();
            vertices.Add(LowerBound);
            vertices.Add(new Vector2(LowerBound.X, UpperBound.Y));
            vertices.Add(UpperBound);
            vertices.Add(new Vector2(UpperBound.X, LowerBound.Y));
            return vertices;
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

        public override bool Equals(object obj)
        {
            if (obj is AABB)
                return Equals((AABB) obj);

            return false;
        }

        public bool Equals(ref AABB other)
        {
            return ((LowerBound == other.LowerBound) && (UpperBound == other.UpperBound));
        }

        public override int GetHashCode()
        {
            return (LowerBound.GetHashCode() + UpperBound.GetHashCode());
        }

        public static bool operator ==(AABB a, AABB b)
        {
            return a.Equals(ref b);
        }

        public static bool operator !=(AABB a, AABB b)
        {
            return !a.Equals(ref b);
        }

        /// <summary>
        /// Gets the distance to the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="distance">The distance.</param>
        public void GetDistance(ref Vector2 point, out float distance)
        {
            float xDistance = Math.Abs(point.X - ((UpperBound.X + LowerBound.X) * .5f)) -
                              (UpperBound.X - LowerBound.X) * .5f;
            float yDistance = Math.Abs(point.Y - ((UpperBound.Y + LowerBound.Y) * .5f)) -
                              (UpperBound.Y - LowerBound.Y) * .5f;

            if (xDistance > 0 && yDistance > 0)
            {
                distance = (float) Math.Sqrt(xDistance * xDistance + yDistance * yDistance);
            }
            else
            {
                distance = Math.Max(xDistance, yDistance);
            }
        }
    }

    public static class CollisionManager
    {
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

        /// <summary>
        /// Compute the collision manifold between two circles.
        /// </summary>
        /// <param name="manifold">The manifold.</param>
        /// <param name="circleA">The circleA.</param>
        /// <param name="xfA">The transform for the first circle.</param>
        /// <param name="circleB">The circleB.</param>
        /// <param name="xfB">The transform for the second circle.</param>
        public static void CollideCircles(out Manifold manifold,
                                          CircleShape circleA, ref Transform xfA,
                                          CircleShape circleB, ref Transform xfB)
        {
            manifold = new Manifold();

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
        /// <param name="polygonA">The polygon.</param>
        /// <param name="xfA">The XF1.</param>
        /// <param name="circleB">The circle.</param>
        /// <param name="xfB">The XF2.</param>
        public static void CollidePolygonAndCircle(out Manifold manifold,
                                                   PolygonShape polygonA, ref Transform xfA,
                                                   CircleShape circleB, ref Transform xfB)
        {
            manifold = new Manifold();

            // Compute circle position in the frame of the polygon.
            Vector2 c = MathUtils.Multiply(ref xfB, circleB.Position);
            Vector2 cLocal = MathUtils.MultiplyT(ref xfA, c);

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
        /// <param name="xfA">The xf A.</param>
        /// <param name="polyB">The poly B.</param>
        /// <param name="xfB">The xf B.</param>
        public static void CollidePolygons(out Manifold manifold,
                                           PolygonShape polyA, ref Transform xfA,
                                           PolygonShape polyB, ref Transform xfB)
        {
            manifold = new Manifold();

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
                manifold.Type = ManifoldType.FaceB;
                flip = 1;
            }
            else
            {
                poly1 = polyA;
                poly2 = polyB;
                xf1 = xfA;
                xf2 = xfB;
                edge1 = edgeA;
                manifold.Type = ManifoldType.FaceA;
                flip = 0;
            }

            FixedArray2<ClipVertex> incidentEdge;
            FindIncidentEdge(out incidentEdge, poly1, ref xf1, edge1, poly2, ref xf2);

            int count1 = poly1.Vertices.Count;

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
            manifold.LocalNormal = localNormal;
            manifold.LocalPoint = planePoint;

            int pointCount = 0;
            for (int i = 0; i < 2; ++i)
            {
                float separation = Vector2.Dot(normal, clipPoints2[i].Vertex) - frontOffset;

                if (separation <= totalRadius)
                {
                    ManifoldPoint cp = manifold.Points[pointCount];
                    cp.LocalPoint = MathUtils.MultiplyT(ref xf2, clipPoints2[i].Vertex);
                    cp.Id = clipPoints2[i].Id;
                    cp.Id.Features.Flip = flip;
                    manifold.Points[pointCount] = cp;

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
        private static float EdgeSeparation(PolygonShape poly1, ref Transform xf1, int edge1,
                                            PolygonShape poly2, ref Transform xf2)
        {
            int count1 = poly1.Vertices.Count;
            int count2 = poly2.Vertices.Count;

            Debug.Assert(0 <= edge1 && edge1 < count1);

            // Convert normal from poly1's frame into poly2's frame.

            Vector2 p1n = poly1.Normals[edge1];
            Vector2 normal1World = new Vector2(xf1.R.Col1.X * p1n.X + xf1.R.Col2.X * p1n.Y,
                                               xf1.R.Col1.Y * p1n.X + xf1.R.Col2.Y * p1n.Y);
            Vector2 normal1 = new Vector2(normal1World.X * xf2.R.Col1.X + normal1World.Y * xf2.R.Col1.Y,
                                          normal1World.X * xf2.R.Col2.X + normal1World.Y * xf2.R.Col2.Y);

            // Find support vertex on poly2 for -normal.
            int index = 0;
            float minDot = Settings.MaxFloat;

            for (int i = 0; i < count2; ++i)
            {
                // inlining this made it 1ms slower
                float dot = Vector2.Dot(poly2.Vertices[i], normal1);

                if (dot < minDot)
                {
                    minDot = dot;
                    index = i;
                }
            }

            Vector2 p1ve = poly1.Vertices[edge1];
            Vector2 p2vi = poly2.Vertices[index];
            Vector2 v1 = new Vector2(xf1.Position.X + xf1.R.Col1.X * p1ve.X + xf1.R.Col2.X * p1ve.Y,
                                     xf1.Position.Y + xf1.R.Col1.Y * p1ve.X + xf1.R.Col2.Y * p1ve.Y);
            Vector2 v2 = new Vector2(xf2.Position.X + xf2.R.Col1.X * p2vi.X + xf2.R.Col2.X * p2vi.Y,
                                     xf2.Position.Y + xf2.R.Col1.Y * p2vi.X + xf2.R.Col2.Y * p2vi.Y);

            // inlining is 1ms slower
            float separation = Vector2.Dot(v2 - v1, normal1World);

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
        private static float FindMaxSeparation(out int edgeIndex,
                                               PolygonShape poly1, ref Transform xf1,
                                               PolygonShape poly2, ref Transform xf2)
        {
            int count1 = poly1.Vertices.Count;

            // Vector pointing from the centroid of poly1 to the centroid of poly2.
            Vector2 d = MathUtils.Multiply(ref xf2, poly2.MassData.Center) -
                        MathUtils.Multiply(ref xf1, poly1.MassData.Center);
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
            for (;;)
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

            cv0.Vertex = MathUtils.Multiply(ref xf2, poly2.Vertices[i1]);
            cv0.Id.Features.ReferenceEdge = (byte) edge1;
            cv0.Id.Features.IncidentEdge = (byte) i1;
            cv0.Id.Features.IncidentVertex = 0;

            c[0] = cv0;

            var cv1 = c[1];
            cv1.Vertex = MathUtils.Multiply(ref xf2, poly2.Vertices[i2]);
            cv1.Id.Features.ReferenceEdge = (byte) edge1;
            cv1.Id.Features.IncidentEdge = (byte) i2;
            cv1.Id.Features.IncidentVertex = 1;

            c[1] = cv1;
        }
    }
}