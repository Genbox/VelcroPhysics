/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using FarseerPhysics.Collision;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common
{
#if !(XBOX360)
    [DebuggerDisplay("Count = {Count} Vertices = {ToString()}")]
#endif
    public class Vertices : List<Vector2>
    {
        public Vertices()
        {
        }

        public Vertices(int capacity)
        {
            Capacity = capacity;
        }

        public Vertices(Vector2[] vector2)
        {
            for (int i = 0; i < vector2.Length; i++)
            {
                Add(vector2[i]);
            }
        }

        public Vertices(IList<Vector2> vertices)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                Add(vertices[i]);
            }
        }

        internal bool AttachedToBody { get; set; }
        public List<Vertices> Holes { get; set; }

        //TODO: Remove
        /// <summary>
        /// Nexts the index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public int NextIndex(int index)
        {
            if (index == Count - 1)
                return 0;

            return index + 1;
        }

        //TODO: Remove
        public Vector2 NextVertex(int index)
        {
            return this[NextIndex(index)];
        }

        //TODO: Remove
        /// <summary>
        /// Gets the previous index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public int PreviousIndex(int index)
        {
            if (index == 0)
            {
                return Count - 1;
            }
            return index - 1;
        }

        //TODO: Remove
        public Vector2 PreviousVertex(int index)
        {
            return this[PreviousIndex(index)];
        }

        /// <summary>
        /// Gets the signed area.
        /// </summary>
        /// <returns></returns>
        public float GetSignedArea()
        {
            int i;
            float area = 0;

            for (i = 0; i < Count; i++)
            {
                int j = (i + 1) % Count;
                area += this[i].X * this[j].Y;
                area -= this[i].Y * this[j].X;
            }
            area /= 2.0f;
            return area;
        }

        /// <summary>
        /// Gets the area.
        /// </summary>
        /// <returns></returns>
        public float GetArea()
        {
            int i;
            float area = 0;

            for (i = 0; i < Count; i++)
            {
                int j = (i + 1) % Count;
                area += this[i].X * this[j].Y;
                area -= this[i].Y * this[j].X;
            }
            area /= 2.0f;
            return (area < 0 ? -area : area);
        }

        /// <summary>
        /// Gets the centroid.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetCentroid()
        {
            // Same algorithm is used by Box2D

            Vector2 c = Vector2.Zero;
            float area = 0.0f;

            const float inv3 = 1.0f / 3.0f;
            Vector2 pRef = Vector2.Zero;
            for (int i = 0; i < Count; ++i)
            {
                // Triangle vertices.
                Vector2 p1 = pRef;
                Vector2 p2 = this[i];
                Vector2 p3 = i + 1 < Count ? this[i + 1] : this[0];

                Vector2 e1 = p2 - p1;
                Vector2 e2 = p3 - p1;

                float D = MathUtils.Cross(e1, e2);

                float triangleArea = 0.5f * D;
                area += triangleArea;

                // Area weighted centroid
                c += triangleArea * inv3 * (p1 + p2 + p3);
            }

            // Centroid
            c *= 1.0f / area;
            return c;
        }

        /// <summary>
        /// Gets the radius based on area.
        /// </summary>
        /// <returns></returns>
        public float GetRadius()
        {
            float area = GetSignedArea();

            double radiusSqrd = (double)area / MathHelper.Pi;
            if (radiusSqrd < 0)
                radiusSqrd *= -1;

            return (float)Math.Sqrt(radiusSqrd);
        }

        /// <summary>
        /// Returns an AABB for vertex.
        /// </summary>
        /// <returns></returns>
        public AABB GetCollisionBox()
        {
            AABB aabb;
            Vector2 lowerBound = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 upperBound = new Vector2(float.MinValue, float.MinValue);

            for (int i = 0; i < Count; ++i)
            {
                if (this[i].X < lowerBound.X)
                {
                    lowerBound.X = this[i].X;
                }
                if (this[i].X > upperBound.X)
                {
                    upperBound.X = this[i].X;
                }

                if (this[i].Y < lowerBound.Y)
                {
                    lowerBound.Y = this[i].Y;
                }
                if (this[i].Y > upperBound.Y)
                {
                    upperBound.Y = this[i].Y;
                }
            }

            aabb.LowerBound = lowerBound;
            aabb.UpperBound = upperBound;

            return aabb;
        }

        public void Translate(Vector2 value)
        {
            Translate(ref value);
        }

        /// <summary>
        /// Translates the vertices with the specified vector.
        /// </summary>
        /// <param name="value">The vector.</param>
        public void Translate(ref Vector2 value)
        {
            Debug.Assert(!AttachedToBody, "Translating vertices that are used by a Body can result in unstable behavior. Use Body.Position instead.");

            for (int i = 0; i < Count; i++)
                this[i] = Vector2.Add(this[i], value);

            if (Holes != null && Holes.Count > 0)
            {
                foreach (Vertices hole in Holes)
                {
                    hole.Translate(ref value);
                }
            }
        }

        public void Scale(Vector2 value)
        {
            Scale(ref value);
        }

        /// <summary>
        /// Scales the vertices with the specified vector.
        /// </summary>
        /// <param name="value">The Value.</param>
        public void Scale(ref Vector2 value)
        {
            Debug.Assert(!AttachedToBody, "Scaling vertices that are used by a Body can result in unstable behavior.");

            for (int i = 0; i < Count; i++)
                this[i] = Vector2.Multiply(this[i], value);

            if (Holes != null && Holes.Count > 0)
            {
                foreach (Vertices hole in Holes)
                {
                    hole.Scale(ref value);
                }
            }
        }

        /// <summary>
        /// Rotate the vertices with the defined value in radians.
        /// Warning: Using this method on an active set of vertices of a Body,
        /// will cause problems with collisions. Use Body.Rotation instead.
        /// </summary>
        /// <param name="value">The amount to rotate by in radians.</param>
        public void Rotate(float value)
        {
            Debug.Assert(!AttachedToBody, "Rotating vertices that are used by a Body can result in unstable behavior.");

            Matrix rotationMatrix;
            Matrix.CreateRotationZ(value, out rotationMatrix);

            for (int i = 0; i < Count; i++)
                this[i] = Vector2.Transform(this[i], rotationMatrix);

            if (Holes != null && Holes.Count > 0)
            {
                foreach (Vertices hole in Holes)
                {
                    hole.Rotate(value);
                }
            }
        }

        /// <summary>
        /// Determines whether the polygon is convex.
        /// O(n^2) running time.
        /// 
        /// Assumptions:
        /// - The polygon is in counter clockwise order
        /// - The polygon has no overlapping edges
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if it is convex; otherwise, <c>false</c>.
        /// </returns>
        public bool IsConvex()
        {
            // Checks the polygon is convex and the interior is to the left of each edge.
            for (int i = 0; i < Count; ++i)
            {
                int next = i + 1 < Count ? i + 1 : 0;
                Vector2 edge = this[next] - this[i];

                for (int j = 0; j < Count; ++j)
                {
                    // Don't check vertices on the current edge.
                    if (j == i || j == next)
                        continue;

                    Vector2 r = this[j] - this[i];

                    float s = edge.X * r.Y - edge.Y * r.X;

                    if (s <= 0.0f)
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Indicates if the vertices are in counter clockwise order.
        /// </summary>
        public bool IsCounterClockWise()
        {
            //We just return true for lines
            if (Count < 3)
                return true;

            return (GetSignedArea() > 0.0f);
        }

        /// <summary>
        /// Forces counter clock wise order.
        /// </summary>
        public void ForceCounterClockWise()
        {
            if (!IsCounterClockWise())
                Reverse();
        }

        //TODO: Check for something like Bentley–Ottmann for simple polygon test, around O(n + log n), but with fewer assumptions.

        /// <summary>
        /// Checks if the vertices forms an simple polygon by checking for edge crossings.
        /// </summary>
        public bool IsSimple()
        {
            for (int i = 0; i < Count; ++i)
            {
                int iplus = (i + 1 > Count - 1) ? 0 : i + 1;
                Vector2 a1 = this[i];
                Vector2 a2 = this[iplus];
                for (int j = i + 1; j < Count; ++j)
                {
                    int jplus = (j + 1 > Count - 1) ? 0 : j + 1;
                    Vector2 b1 = this[j];
                    Vector2 b2 = this[jplus];

                    Vector2 temp;

                    if (LineTools.LineIntersect2(a1, a2, b1, b2, out temp))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if polygon is valid for use in Box2d engine.
        /// Last ditch effort to ensure no invalid polygons are
        /// added to world geometry.
        ///
        /// Performs a full check, for simplicity, convexity,
        /// orientation, minimum angle, and volume.  This won't
        /// be very efficient, and a lot of it is redundant when
        /// other tools in this section are used.
        /// 
        /// From Eric Jordan's convex decomposition library
        /// </summary>
        /// <returns></returns>
        public bool CheckPolygon(out int error, out string errorMessage)
        {
            error = -1;
            errorMessage = null;

            if (Count < 3 || Count > Settings.MaxPolygonVertices)
            {
                error = 0;
            }
            if (!IsConvex())
            {
                error = 1;
            }
            if (!IsSimple())
            {
                error = 2;
            }
            if (GetArea() < Settings.Epsilon)
            {
                error = 3;
            }

            //Compute normals
            Vector2[] normals = new Vector2[Count];
            Vertices vertices = new Vertices(Count);
            for (int i = 0; i < Count; ++i)
            {
                vertices.Add(new Vector2(this[i].X, this[i].Y));
                int i1 = i;
                int i2 = i + 1 < Count ? i + 1 : 0;
                Vector2 edge = new Vector2(this[i2].X - this[i1].X, this[i2].Y - this[i1].Y);
                normals[i] = MathUtils.Cross(edge, 1.0f);
                normals[i].Normalize();
            }

            //Required side checks
            for (int i = 0; i < Count; ++i)
            {
                int iminus = (i == 0) ? Count - 1 : i - 1;

                //Parallel sides check
                float cross = MathUtils.Cross(normals[iminus], normals[i]);
                cross = MathUtils.Clamp(cross, -1.0f, 1.0f);
                float angle = (float)Math.Asin(cross);
                if (angle <= Settings.AngularSlop)
                {
                    error = 4;
                    break;
                }

                //Too skinny check
                for (int j = 0; j < Count; ++j)
                {
                    if (j == i || j == (i + 1) % Count)
                    {
                        continue;
                    }
                    float s = Vector2.Dot(normals[i], vertices[j] - vertices[i]);
                    if (s >= -Settings.LinearSlop)
                    {
                        error = 5;
                    }
                }


                Vector2 centroid = vertices.GetCentroid();
                Vector2 n1 = normals[iminus];
                Vector2 n2 = normals[i];
                Vector2 v = vertices[i] - centroid;

                Vector2 d = new Vector2();
                d.X = Vector2.Dot(n1, v); // - toiSlop;
                d.Y = Vector2.Dot(n2, v); // - toiSlop;

                // Shifting the edge inward by toiSlop should
                // not cause the plane to pass the centroid.
                if ((d.X < 0.0f) || (d.Y < 0.0f))
                {
                    error = 6;
                }
            }

            if (error != -1)
            {
                switch (error)
                {
                    case 0:
                        errorMessage = string.Format("Polygon error: must have between 3 and {0} vertices.",
                                                      Settings.MaxPolygonVertices);
                        break;
                    case 1:
                        errorMessage = "Polygon error: must be convex.";
                        break;
                    case 2:
                        errorMessage = "Polygon error: must be simple (cannot intersect itself).";
                        break;
                    case 3:
                        errorMessage = "Polygon error: area is too small.";
                        break;
                    case 4:
                        errorMessage = "Polygon error: sides are too close to parallel.";
                        break;
                    case 5:
                        errorMessage = "Polygon error: polygon is too thin.";
                        break;
                    case 6:
                        errorMessage = "Polygon error: core shape generation would move edge past centroid (too thin).";
                        break;
                    default:
                        errorMessage = "Polygon error: error " + error.ToString();
                        break;
                }
            }
            return error != -1;
        }

        public bool CheckPolygon()
        {
            string errorMessage;
            int errorCode;
            var result = CheckPolygon(out errorCode, out errorMessage);
            if (!result && errorMessage != null)
            {
                Debug.WriteLine(errorMessage);
            }
            return result;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < Count; i++)
            {
                builder.Append(this[i].ToString());
                if (i < Count - 1)
                {
                    builder.Append(" ");
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Projects to axis.
        /// </summary>
        /// <param name="axis">The axis.</param>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        public void ProjectToAxis(ref Vector2 axis, out float min, out float max)
        {
            // To project a point on an axis use the dot product
            float dotProduct = Vector2.Dot(axis, this[0]);
            min = dotProduct;
            max = dotProduct;

            for (int i = 0; i < Count; i++)
            {
                dotProduct = Vector2.Dot(this[i], axis);
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

        /// <summary>
        /// Winding number test for a point in a polygon.
        /// </summary>
        /// See more info about the algorithm here: http://softsurfer.com/Archive/algorithm_0103/algorithm_0103.htm
        /// <param name="point">The point to be tested.</param>
        /// <returns>-1 if the winding number is zero and the point is outside
        /// the polygon, 1 if the point is inside the polygon, and 0 if the point
        /// is on the polygons edge.</returns>
        public int PointInPolygon(ref Vector2 point)
        {
            // Winding number
            int wn = 0;

            // Iterate through polygon's edges
            for (int i = 0; i < Count; i++)
            {
                // Get points
                Vector2 p1 = this[i];
                Vector2 p2 = this[NextIndex(i)];

                // Test if a point is directly on the edge
                Vector2 edge = p2 - p1;
                float area = MathUtils.Area(ref p1, ref p2, ref point);
                if (area == 0f && Vector2.Dot(point - p1, edge) >= 0f && Vector2.Dot(point - p2, edge) <= 0f)
                {
                    return 0;
                }
                // Test edge for intersection with ray from point
                if (p1.Y <= point.Y)
                {
                    if (p2.Y > point.Y && area > 0f)
                    {
                        ++wn;
                    }
                }
                else
                {
                    if (p2.Y <= point.Y && area < 0f)
                    {
                        --wn;
                    }
                }
            }
            return (wn == 0 ? -1 : 1);
        }

        /// <summary>
        /// Compute the sum of the angles made between the test point and each pair of points making up the polygon. 
        /// If this sum is 2pi then the point is an interior point, if 0 then the point is an exterior point. 
        /// ref: http://ozviz.wasp.uwa.edu.au/~pbourke/geometry/insidepoly/  - Solution 2 
        /// </summary>
        public bool PointInPolygonAngle(ref Vector2 point)
        {
            double angle = 0;

            // Iterate through polygon's edges
            for (int i = 0; i < Count; i++)
            {
                // Get points
                Vector2 p1 = this[i] - point;
                Vector2 p2 = this[NextIndex(i)] - point;

                angle += MathUtils.VectorAngle(ref p1, ref p2);
            }

            if (Math.Abs(angle) < Math.PI)
            {
                return false;
            }

            return true;
        }

        public void Transform(ref Matrix transform)
        {
            // Transform main polygon
            for (int i = 0; i < Count; i++)
                this[i] = Vector2.Transform(this[i], transform);

            // Transform holes
            if (Holes != null && Holes.Count > 0)
            {
                for (int i = 0; i < Holes.Count; i++)
                {
                    Vector2[] temp = Holes[i].ToArray();
                    Vector2.Transform(temp, ref transform, temp);

                    Holes[i] = new Vertices(temp);
                }
            }
        }
    }
}