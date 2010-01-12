using System;
using System.Collections.Generic;
using System.Diagnostics;
using FarseerPhysics;
using FarseerPhysics.Common.Decomposition;
using Microsoft.Xna.Framework;

#if !(XBOX360)
[DebuggerDisplay("Count = {Count}")]
#endif
public class Vertices : List<Vector2>
{
    private Vector2 _res;

    public Vertices()
    {
    }

    public Vertices(int capacity)
    {
        Capacity = capacity;
    }

    public Vertices(ref Vector2[] vector2)
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

    public Vertices AddTriangle(EarclipDecomposer.Triangle t)
    {
        // float32 equalTol = .001f;
        // First, find vertices that connect
        int firstP = -1;
        int firstT = -1;
        int secondP = -1;
        int secondT = -1;
        for (int i = 0; i < Count; i++)
        {
            if (t.x[0] == this[i].X && t.y[0] == this[i].Y)
            {
                if (firstP == -1)
                {
                    firstP = i;
                    firstT = 0;
                }
                else
                {
                    secondP = i;
                    secondT = 0;
                }
            }
            else if (t.x[1] == this[i].X && t.y[1] == this[i].Y)
            {
                if (firstP == -1)
                {
                    firstP = i;
                    firstT = 1;
                }
                else
                {
                    secondP = i;
                    secondT = 1;
                }
            }
            else if (t.x[2] == this[i].X && t.y[2] == this[i].Y)
            {
                if (firstP == -1)
                {
                    firstP = i;
                    firstT = 2;
                }
                else
                {
                    secondP = i;
                    secondT = 2;
                }
            }
        }
        // Fix ordering if first should be last vertex of poly
        if (firstP == 0 && secondP == Count - 1)
        {
            firstP = Count - 1;
            secondP = 0;
        }

        // Didn't find it
        if (secondP == -1)
        {
            return null;
        }

        // Find tip index on triangle
        int tipT = 0;
        if (tipT == firstT || tipT == secondT)
            tipT = 1;
        if (tipT == firstT || tipT == secondT)
            tipT = 2;

        Vertices result = new Vertices(Count + 1);
        for (int i = 0; i < Count; i++)
        {
            result.Add(this[i]);

            if (i == firstP)
                result.Add(new Vector2(t.x[tipT], t.y[tipT]));
        }

        return result;
    }

    public int NextIndex(int index)
    {
        if (index == Count - 1)
        {
            return 0;
        }
        return index + 1;
    }

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
        Vector2 pRef = new Vector2(0.0f, 0.0f);
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
    /// Translates the vertices with the specified vector.
    /// </summary>
    /// <param name="vector">The vector.</param>
    public void Translate(ref Vector2 vector)
    {
        for (int i = 0; i < Count; i++)
            this[i] = Vector2.Add(this[i], vector);
    }

    /// <summary>
    /// Scales the vertices with the specified vector.
    /// </summary>
    /// <param name="value">The Value.</param>
    public void Scale(ref Vector2 value)
    {
        for (int i = 0; i < Count; i++)
            this[i] = Vector2.Multiply(this[i], value);
    }

    /// <summary>
    /// Rotate the vertices with the defined value in radians.
    /// </summary>
    /// <param name="value">The amount to rotate by in radians.</param>
    public void Rotate(float value)
    {
        Matrix rotationMatrix;
        Matrix.CreateRotationZ(value, out rotationMatrix);

        for (int i = 0; i < Count; i++)
            this[i] = Vector2.Transform(this[i], rotationMatrix);
    }

    /// <summary>
    /// Assuming the polygon is simple; determines whether the polygon is convex.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if it is convex; otherwise, <c>false</c>.
    /// </returns>
    public bool IsConvex()
    {
        bool isPositive = false;

        for (int i = 0; i < Count; ++i)
        {
            int lower = (i == 0) ? (Count - 1) : (i - 1);
            int middle = i;
            int upper = (i == Count - 1) ? (0) : (i + 1);

            float dx0 = this[middle].X - this[lower].X;
            float dy0 = this[middle].Y - this[lower].Y;
            float dx1 = this[upper].X - this[middle].X;
            float dy1 = this[upper].Y - this[middle].Y;

            float cross = dx0 * dy1 - dx1 * dy0;
            // Cross product should have same sign
            // for each vertex if poly is convex.
            bool newIsP = (cross >= 0) ? true : false;
            if (i == 0)
            {
                isPositive = newIsP;
            }
            else if (isPositive != newIsP)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsCounterClockWise()
    {
        return (GetSignedArea() > 0.0f);
    }

    /// <summary>
    /// Forces counter clock wise order.
    /// </summary>
    public void ForceCounterClockWise()
    {
        // the sign of the 'area' of the polygon is all
        // we are interested in.
        float area = GetSignedArea();
        if (area > 0)
        {
            Reverse();
        }
    }

    /*
* Check if the lines a0->a1 and b0->b1 cross.
* If they do, intersectionPoint will be filled
* with the point of crossing.
*
* Grazing lines should not return true.
*/
    public static bool intersect(Vector2 a0, Vector2 a1, Vector2 b0, Vector2 b1, out Vector2 intersectionPoint)
    {
        intersectionPoint = Vector2.Zero;
        if (a0 == b0 || a0 == b1 || a1 == b0 || a1 == b1) return false;
        float x1 = a0.X; float y1 = a0.Y;
        float x2 = a1.X; float y2 = a1.Y;
        float x3 = b0.X; float y3 = b0.Y;
        float x4 = b1.X; float y4 = b1.Y;

        //AABB early exit
        if (Math.Max(x1, x2) < Math.Min(x3, x4) || Math.Max(x3, x4) < Math.Min(x1, x2)) return false;
        if (Math.Max(y1, y2) < Math.Min(y3, y4) || Math.Max(y3, y4) < Math.Min(y1, y2)) return false;

        float ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3));
        float ub = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3));
        float denom = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
        if (Math.Abs(denom) < Settings.Epsilon)
        {
            //Lines are too close to parallel to call
            return false;
        }
        ua /= denom;
        ub /= denom;

        if ((0 < ua) && (ua < 1) && (0 < ub) && (ub < 1))
        {
            intersectionPoint.X = (x1 + ua * (x2 - x1));
            intersectionPoint.Y = (y1 + ua * (y2 - y1));
            //printf("%f, %f -> %f, %f crosses %f, %f -> %f, %f\n",x1,y1,x2,y2,x3,y3,x4,y4);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check for edge crossings
    /// </summary>
    /// <returns></returns>
    public bool IsSimple()
    {
        for (int i = 0; i < Count; ++i)
        {
            int iplus = (i + 1 > Count - 1) ? 0 : i + 1;
            Vector2 a1 = new Vector2(this[i].X, this[i].Y);
            Vector2 a2 = new Vector2(this[iplus].X, this[iplus].Y);
            for (int j = i + 1; j < Count; ++j)
            {
                int jplus = (j + 1 > Count - 1) ? 0 : j + 1;
                Vector2 b1 = new Vector2(this[j].X, this[j].Y);
                Vector2 b2 = new Vector2(this[jplus].X, this[jplus].Y);

                Vector2 temp;

                if (intersect(a1, a2, b1, b2, out temp))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void MergeParallelEdges(float tolerance)
    {
        if (Count <= 3)
            //Can't do anything useful here to a triangle
            return;

        bool[] mergeMe = new bool[Count];
        int newNVertices = Count;
        for (int i = 0; i < Count; ++i)
        {
            int lower = (i == 0) ? (Count - 1) : (i - 1);
            int middle = i;
            int upper = (i == Count - 1) ? (0) : (i + 1);
            float dx0 = this[middle].X - this[lower].X;
            float dy0 = this[middle].Y - this[lower].Y;
            float dx1 = this[upper].X - this[middle].X;
            float dy1 = this[upper].Y - this[middle].Y;
            float norm0 = (float)Math.Sqrt(dx0 * dx0 + dy0 * dy0);
            float norm1 = (float)Math.Sqrt(dx1 * dx1 + dy1 * dy1);
            if (!(norm0 > 0.0f && norm1 > 0.0f) && newNVertices > 3)
            {
                //Merge identical points
                mergeMe[i] = true;
                --newNVertices;
            }
            dx0 /= norm0; dy0 /= norm0;
            dx1 /= norm1; dy1 /= norm1;
            float cross = dx0 * dy1 - dx1 * dy0;
            float dot = dx0 * dx1 + dy0 * dy1;
            if (Math.Abs(cross) < tolerance && dot > 0 && newNVertices > 3)
            {
                mergeMe[i] = true;
                --newNVertices;
            }
            else
            {
                mergeMe[i] = false;
            }
        }

        if (newNVertices == Count || newNVertices == 0)
        {
            //Nothing needed to be merged
            return;
        }

        //Copy the old vertices to a new list
        Vertices oldVertices = new Vertices(this);

        //Clear this list
        Clear();

        int currIndex = 0;
        for (int i = 0; i < Count; ++i)
        {
            if (mergeMe[i] || newNVertices == 0 || currIndex == newNVertices)
                continue;

            Debug.Assert(currIndex < newNVertices);

            //Add the old vertices that needs to be kept
            Add(oldVertices[i]);
            ++currIndex;
        }
    }

    /*
 * Checks if polygon is valid for use in Box2d engine.
 * Last ditch effort to ensure no invalid polygons are
 * added to world geometry.
 *
 * Performs a full check, for simplicity, convexity,
 * orientation, minimum angle, and volume.  This won't
 * be very efficient, and a lot of it is redundant when
 * other tools in this section are used.
 */
    bool IsUsable(bool printErrors)
    {
        int error = -1;
        bool noError = true;
        if (Count < 3 || Count > Settings.MaxPolygonVertices) { noError = false; error = 0; }
        if (!IsConvex()) { noError = false; error = 1; }
        if (!IsSimple()) { noError = false; error = 2; }
        if (GetArea() < Settings.Epsilon) { noError = false; error = 3; }

        //Compute normals
        Vector2[] normals = new Vector2[Count];
        Vertices vertices = new Vertices(Count);
        for (int i = 0; i < Count; ++i)
        {
            vertices[i] = new Vector2(this[i].X, this[i].Y);
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
            //int iplus = (i==nVertices-1)?0:i+1;

            //Parallel sides check
            float cross = MathUtils.Cross(normals[iminus], normals[i]);
            cross = MathUtils.Clamp(cross, -1.0f, 1.0f);
            float angle = (float)Math.Asin(cross);
            if (angle <= Settings.AngularSlop)
            {
                noError = false;
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
                    noError = false;
                    error = 5;
                }
            }


            Vector2 centroid = vertices.GetCentroid();
            Vector2 n1 = normals[iminus];
            Vector2 n2 = normals[i];
            Vector2 v = vertices[i] - centroid; ;

            Vector2 d = new Vector2();
            d.X = Vector2.Dot(n1, v); // - toiSlop;
            d.Y = Vector2.Dot(n2, v); // - toiSlop;

            // Shifting the edge inward by toiSlop should
            // not cause the plane to pass the centroid.
            if ((d.X < 0.0f) || (d.Y < 0.0f))
            {
                noError = false;
                error = 6;
            }

        }

        if (!noError && printErrors)
        {
            Debug.WriteLine("Found invalid polygon, ");
            switch (error)
            {
                case 0:
                    Debug.WriteLine(string.Format("must have between 3 and {0} vertices.\n", Settings.MaxPolygonVertices));
                    break;
                case 1:
                    Debug.WriteLine("must be convex.\n");
                    break;
                case 2:
                    Debug.WriteLine("must be simple (cannot intersect itself).\n");
                    break;
                case 3:
                    Debug.WriteLine("area is too small.\n");
                    break;
                case 4:
                    Debug.WriteLine("sides are too close to parallel.\n");
                    break;
                case 5:
                    Debug.WriteLine("polygon is too thin.\n");
                    break;
                case 6:
                    Debug.WriteLine("core shape generation would move edge past centroid (too thin).\n");
                    break;
                default:
                    Debug.WriteLine("don't know why.\n");
                    break;
            }
        }
        return noError;
    }

    #region Cowdozer's Extension

    /// <summary>
    /// Creates a convex hull of the Vertices. Note: Vertices must 
    /// be of a simple polygon, i.e. edges do not overlap.
    /// </summary>
    /// <remarks>
    /// Implemented using Melkman's Convex Hull Algorithm - O(n) time complexity.
    /// Reference: http://www.ams.sunysb.edu/~jsbm/courses/345/melkman.pdf
    /// Requires that vertices are of a simple polygon. Handles collinear points.
    /// </remarks>
    /// <returns>A convex hull in counterclockwise winding order.</returns>
    public Vertices GetConvexHull()
    {
        //With less than 3 vertices, this is about the best we can do for a convex hull
        if (Count < 3)
            return this;

        //We'll never need a queue larger than the current number of Vertices +1
        //Create double-ended queue
        Vector2[] deque = new Vector2[Count + 1];
        int qf = 3, qb = 0; //Queue front index, queue back index
        int qfm1, qbm1;     //qfm1 = second element, qbm1 = second last element

        //Start by placing first 3 vertices in convex CCW order
        int startIndex = 3;
        float k = IsLeft(this[0], this[1], this[2]);
        if (k == 0)
        {
            //Vertices are collinear.
            deque[0] = this[0];
            deque[1] = this[2]; //We can skip vertex 1 because it should be between 0 and 2
            deque[2] = this[0];
            qf = 2;

            //Go until the end of the collinear sequence of vertices
            for (startIndex = 3; startIndex < this.Count; startIndex++)
                if (IsLeft(deque[0], deque[1], this[startIndex]) == 0) //This point is also collinear
                    deque[1] = this[startIndex];
                else break;
        }
        else
        {
            deque[0] = deque[3] = this[2];
            if (k > 0)
            {
                //Is Left.  Set deque = {2, 0, 1, 2}
                deque[1] = this[0];
                deque[2] = this[1];
            }
            else
            {
                //Is Right. Set deque = {2, 1, 0, 2}
                deque[1] = this[1];
                deque[2] = this[0];
            }
        }

        qfm1 = qf == 0 ? deque.Length - 1 : qf - 1;       //qfm1 = qf - 1;
        qbm1 = qb == deque.Length - 1 ? 0 : qb + 1;       //qbm1 = qb + 1;

        //Add vertices one at a time and adjust convex hull as needed
        for (int i = startIndex; i < Count; i++)
        {
            Vector2 nextPt = this[i];

            //Ignore if it is already within the convex hull we have constructed
            if (IsLeft(deque[qfm1], deque[qf], nextPt) > 0 &&
                IsLeft(deque[qb], deque[qbm1], nextPt) > 0)
                continue;

            //Pop front until convex
            while (!(IsLeft(deque[qfm1], deque[qf], nextPt) > 0))
            {
                //Pop the front element from the queue
                qf = qfm1;                                    //qf--;
                qfm1 = qf == 0 ? deque.Length - 1 : qf - 1;   //qfm1 = qf - 1;
            }
            //Add vertex to the front of the queue
            qf = qf == deque.Length - 1 ? 0 : qf + 1;         //qf++;
            qfm1 = qf == 0 ? deque.Length - 1 : qf - 1;       //qfm1 = qf - 1;
            deque[qf] = nextPt;

            //Pop back until convex
            while (!(IsLeft(deque[qb], deque[qbm1], nextPt) > 0))
            {
                //Pop the back element from the queue
                qb = qbm1;                                    //qb++;
                qbm1 = qb == deque.Length - 1 ? 0 : qb + 1;   //qbm1 = qb + 1;
            }
            //Add vertex to the back of the queue
            qb = qb == 0 ? deque.Length - 1 : qb - 1;         //qb--;
            qbm1 = qb == deque.Length - 1 ? 0 : qb + 1;       //qbm1 = qb + 1;
            deque[qb] = nextPt;
        }

        //Create the convex hull from what is left in the deque
        Vertices convexHull = new Vertices(Count + 1);
        if (qb < qf)
            for (int i = qb; i < qf; i++)
                convexHull.Add(deque[i]);
        else
        {
            for (int i = 0; i < qf; i++)
                convexHull.Add(deque[i]);
            for (int i = qb; i < deque.Length; i++)
                convexHull.Add(deque[i]);
        }
        return convexHull;
    }

    /// <summary>
    /// Returns a positive number if c is to the left of the line going from a to b.
    /// </summary>
    /// <remarks>Used by method <c>GetConvexHull()</c>.</remarks>
    /// <returns>Positive number if points arc left, negative if points arc right, 
    /// and 0 if points are collinear.</returns>
    private float IsLeft(Vector2 a, Vector2 b, Vector2 c)
    {
        //cross product
        return (b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y);
    }

    #endregion
}