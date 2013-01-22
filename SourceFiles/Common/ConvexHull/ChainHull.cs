using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common.ConvexHull
{
    /// <summary>
    /// Andrew's Monotone Chain Convex Hull algorithm.
    /// Used to get the convex hull of a point cloud.
    /// </summary>
    /// <remarks>
    /// Source: http://www.softsurfer.com/Archive/algorithm_0109/algorithm_0109.htm
    /// </remarks>
    public static class ChainHull
    {
        //Copyright 2001, softSurfer (www.softsurfer.com)

        private static PointComparer _pointComparer = new PointComparer();

        /// <summary>
        /// Returns the convex hull from the given vertices..
        /// </summary>
        public static Vertices GetConvexHull(Vertices vertices)
        {
            //Sort by X-axis
            vertices.Sort(_pointComparer);

            Vector2[] h = new Vector2[vertices.Count];
            Vertices res = new Vertices();

            int top = -1; // indices for bottom and top of the stack
            int i; // array scan index

            // Get the indices of points with min x-coord and min|max y-coord
            const int minmin = 0;
            float xmin = vertices[0].X;
            for (i = 1; i < vertices.Count; i++)
            {
                if (vertices[i].X != xmin)
                    break;
            }

            int minmax = i - 1;
            if (minmax == vertices.Count - 1)
            {
                // degenerate case: all x-coords == xmin
                h[++top] = vertices[minmin];

                if (vertices[minmax].Y != vertices[minmin].Y) // a nontrivial segment
                    h[++top] = vertices[minmax];

                h[++top] = vertices[minmin]; // add polygon endpoint

                for (int j = 0; j < top + 1; j++)
                {
                    res.Add(h[j]);
                }

                return res;
            }

            top = res.Count - 1;

            // Get the indices of points with max x-coord and min|max y-coord
            int maxmax = vertices.Count - 1;
            float xmax = vertices[vertices.Count - 1].X;
            for (i = vertices.Count - 2; i >= 0; i--)
            {
                if (vertices[i].X != xmax)
                    break;
            }
            int maxmin = i + 1;

            // Compute the lower hull on the stack H
            h[++top] = vertices[minmin]; // push minmin point onto stack
            i = minmax;
            while (++i <= maxmin)
            {
                // the lower line joins P[minmin] with P[maxmin]
                if (MathUtils.Area(vertices[minmin], vertices[maxmin], vertices[i]) >= 0 && i < maxmin)
                    continue; // ignore P[i] above or on the lower line

                while (top > 0) // there are at least 2 points on the stack
                {
                    // test if P[i] is left of the line at the stack top
                    if (MathUtils.Area(h[top - 1], h[top], vertices[i]) > 0)
                        break; // P[i] is a new hull vertex
                    else
                        top--; // pop top point off stack
                }
                h[++top] = vertices[i]; // push P[i] onto stack
            }

            // Next, compute the upper hull on the stack H above the bottom hull
            if (maxmax != maxmin) // if distinct xmax points
                h[++top] = vertices[maxmax]; // push maxmax point onto stack
            int bot = top;
            i = maxmin;
            while (--i >= minmax)
            {
                // the upper line joins P[maxmax] with P[minmax]
                if (MathUtils.Area(vertices[maxmax], vertices[minmax], vertices[i]) >= 0 && i > minmax)
                    continue; // ignore P[i] below or on the upper line

                while (top > bot) // at least 2 points on the upper stack
                {
                    // test if P[i] is left of the line at the stack top
                    if (MathUtils.Area(h[top - 1], h[top], vertices[i]) > 0)
                        break; // P[i] is a new hull vertex
                    else
                        top--; // pop top point off stack
                }

                h[++top] = vertices[i]; // push P[i] onto stack
            }

            if (minmax != minmin)
                h[++top] = vertices[minmin]; // push joining endpoint onto stack

            for (int j = 0; j < top + 1; j++)
            {
                res.Add(h[j]);
            }

            return res;
        }

        private class PointComparer : Comparer<Vector2>
        {
            public override int Compare(Vector2 a, Vector2 b)
            {
                int f = a.X.CompareTo(b.X);
                return f != 0 ? f : a.Y.CompareTo(b.Y);
            }
        }
    }
}