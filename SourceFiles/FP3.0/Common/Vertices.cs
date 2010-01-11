using System.Collections.Generic;
using System.Diagnostics;
using FarseerPhysics;
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

    public Vector2[] GetVerticesArray()
    {
        return ToArray();
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
    private float GetSignedArea()
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
        float area = GetArea();
        return GetCentroid(area);
    }

    /// <summary>
    /// Gets the centroid.
    /// </summary>
    /// <param name="area">The area.</param>
    /// <returns></returns>
    public Vector2 GetCentroid(float area)
    {
        Vertices verts = this;

        float cx = 0, cy = 0;
        int i;

        float signedarea = GetSignedArea();
        float factor;

        if (signedarea > 0)	//if it's meant to be reversed, go through the vertices backwards
        {
            for (i = Count - 1; i >= 0; i--)
            {
                int j = (i - 1) % Count;

                if (j < 0) { j += Count; }

                factor = -(verts[i].X * verts[j].Y - verts[j].X * verts[i].Y);
                cx += (verts[i].X + verts[j].X) * factor;
                cy += (verts[i].Y + verts[j].Y) * factor;
            }

        }
        else
        {
            for (i = 0; i < Count; i++)
            {
                int j = (i + 1) % Count;

                factor = -(verts[i].X * verts[j].Y - verts[j].X * verts[i].Y);
                cx += (verts[i].X + verts[j].X) * factor;
                cy += (verts[i].Y + verts[j].Y) * factor;
            }
        }

        area *= 6.0f;
        factor = 1 / area;
        cx *= factor;
        cy *= factor;
        _res.X = cx;
        _res.Y = cy;

        return _res;
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
    /// Determines whether the polygon is convex.
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

    public void MakeCCW()
    {
        int br = 0;

        // find bottom right point
        for (int i = 1; i < Count; ++i)
        {
            if (this[i].Y < this[br].Y || (this[i].Y == this[br].Y && this[i].X > this[br].X))
            {
                br = i;
            }
        }

        // reverse poly if clockwise
        if (!MathUtils.Left(At(br - 1), At(br), At(br + 1)))
        {
            Reverse();
        }
    }

    public bool IsReflex(int i)
    {
        return MathUtils.Right(At(i - 1), At(i), At(i + 1));
    }

    public Vector2 At(int i)
    {
        return this[Wrap(i, Count)];
    }

    private int Wrap(int a, int b)
    {
        return a < 0 ? a % b + b : a % b;
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