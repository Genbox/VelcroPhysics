using System;
using System.Collections.Generic;
using System.Text;
using FarseerGames.FarseerPhysics.Mathematics;

#if (XNA)
using Microsoft.Xna.Framework;
#endif

namespace FarseerGames.FarseerPhysics.Collisions
{
    /// <summary>
    /// Provides an implementation of a strongly typed List with Vector2
    /// </summary>
    public class Vertices : List<Vector2>
    {
        private Vector2 _res;
        private Vector2 _vectorTemp1 = Vector2.Zero;
        private Vector2 _vectorTemp2 = Vector2.Zero;
        private Vector2 _vectorTemp3 = Vector2.Zero;
        private Vector2 _vectorTemp4 = Vector2.Zero;
        private Vector2 _vectorTemp5 = Vector2.Zero;

        public Vertices()
        {
        }

        public Vertices(Vector2[] vector2)
        {
            for (int i = 0; i < vector2.Length; i++)
            {
                Add(vector2[i]);
            }
        }

        public Vertices(Vertices vertices)
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

        public int PreviousIndex(int index)
        {
            if (index == 0)
            {
                return Count - 1;
            }
            return index - 1;
        }

        public Vector2 GetEdge(int index)
        {
            int nextIndex = NextIndex(index);
            _vectorTemp2 = this[nextIndex];
            _vectorTemp3 = this[index];
            Vector2.Subtract(ref _vectorTemp2, ref _vectorTemp3, out _vectorTemp1);
            return _vectorTemp1;
        }

        public void GetEdge(int index, out Vector2 edge)
        {
            int nextIndex = NextIndex(index);
            _vectorTemp2 = this[nextIndex];
            _vectorTemp3 = this[index];
            Vector2.Subtract(ref _vectorTemp2, ref _vectorTemp3, out edge);
        }

        public Vector2 GetEdgeMidPoint(int index)
        {
            GetEdge(index, out _vectorTemp1);
            Vector2.Multiply(ref _vectorTemp1, .5f, out _vectorTemp2);

            _vectorTemp3 = this[index];
            Vector2.Add(ref _vectorTemp3, ref _vectorTemp2, out _vectorTemp1);

            return _vectorTemp1;
        }

        public void GetEdgeMidPoint(int index, out Vector2 midPoint)
        {
            GetEdge(index, out _vectorTemp1);
            Vector2.Multiply(ref _vectorTemp1, .5f, out _vectorTemp2);
            _vectorTemp3 = this[index];
            Vector2.Add(ref _vectorTemp3, ref _vectorTemp2, out midPoint);
        }

        public Vector2 GetEdgeNormal(int index)
        {
            GetEdge(index, out _vectorTemp1);

            _vectorTemp2.X = -_vectorTemp1.Y;
            _vectorTemp2.Y = _vectorTemp1.X;

            Vector2.Normalize(ref _vectorTemp2, out _vectorTemp3);

            return _vectorTemp3;
        }

        public void GetEdgeNormal(int index, out Vector2 edgeNormal)
        {
            GetEdge(index, out _vectorTemp4);
            _vectorTemp5.X = -_vectorTemp4.Y;
            _vectorTemp5.Y = _vectorTemp4.X;
            Vector2.Normalize(ref _vectorTemp5, out edgeNormal);
        }

        public Vector2 GetVertexNormal(int index)
        {
            GetEdgeNormal(index, out _vectorTemp1);

            int prevIndex = PreviousIndex(index);
            GetEdgeNormal(prevIndex, out _vectorTemp2);

            Vector2.Add(ref _vectorTemp1, ref _vectorTemp2, out _vectorTemp3);

            Vector2.Normalize(ref _vectorTemp3, out _vectorTemp1);

            return _vectorTemp1;
        }

        public void GetVertexNormal(int index, out Vector2 vertexNormal)
        {
            GetEdgeNormal(index, out _vectorTemp1);
            int prevIndex = PreviousIndex(index);
            GetEdgeNormal(prevIndex, out _vectorTemp2);
            Vector2.Add(ref _vectorTemp1, ref _vectorTemp2, out _vectorTemp3);
            Vector2.Normalize(ref _vectorTemp3, out vertexNormal);
        }

        /// <summary>
        /// Finds the shortest edge.
        /// </summary>
        /// <returns></returns>
        public float GetShortestEdge()
        {
            float shortestEdge = float.MaxValue;
            for (int i = 0; i < Count; i++)
            {
                GetEdge(i, out _vectorTemp1);
                float length = _vectorTemp1.Length();
                if (length < shortestEdge)
                {
                    shortestEdge = length;
                }
            }
            return shortestEdge;
        }

        /// <summary>
        /// Divides the edges up into the specified length.
        /// </summary>
        /// <param name="maxEdgeLength">Length of the max edge.</param>
        public void SubDivideEdges(float maxEdgeLength)
        {
            Vertices verticesTemp = new Vertices();
            for (int i = 0; i < Count; i++)
            {
                Vector2 vertA = this[i];
                Vector2 vertB = this[NextIndex(i)];
                Vector2 edge;
                Vector2.Subtract(ref vertA, ref vertB, out edge);
                float edgeLength = edge.Length();

                verticesTemp.Add(vertA);
                if (edgeLength > maxEdgeLength) //need to subdivide
                {
                    double edgeCount = Math.Ceiling(edgeLength/(double) maxEdgeLength);

                    for (int j = 0; j < edgeCount - 1; j++)
                    {
                        Vector2 vert = Vector2.Lerp(vertA, vertB, (j + 1)/(float) edgeCount);
                        verticesTemp.Add(vert);
                    }
                }
            }

            Clear();
            for (int k = 0; k < verticesTemp.Count; k++)
            {
                Add(verticesTemp[k]);
            }
        }

        public void ForceCounterClockWiseOrder()
        {
            // the sign of the 'area' of the polygon is all
            // we are interested in.
            float area = GetSignedArea();
            if (area > 0)
            {
                Reverse();
            }
        }

        private float GetSignedArea()
        {
            int i;
            float area = 0;

            for (i = 0; i < Count; i++)
            {
                int j = (i + 1)%Count;
                area += this[i].X*this[j].Y;
                area -= this[i].Y*this[j].X;
            }
            area /= 2.0f;
            return area;
        }

        public float GetArea()
        {
            int i;
            float area = 0;

            for (i = 0; i < Count; i++)
            {
                int j = (i + 1)%Count;
                area += this[i].X*this[j].Y;
                area -= this[i].Y*this[j].X;
            }
            area /= 2.0f;
            return (area < 0 ? -area : area);
        }

        public Vector2 GetCentroid()
        {
            float area = GetArea();
            return GetCentroid(area);
        }

        public Vector2 GetCentroid(float area)
        {
            //calc centroid on counter clockwise verts.
            Vertices verts = new Vertices(this);
            verts.ForceCounterClockWiseOrder();

            float cx = 0, cy = 0;
            int i;

            float factor;
            //Debug.WriteLine(verts.ToString());
            for (i = 0; i < Count; i++)
            {
                int j = (i + 1)%Count;

                factor = -(verts[i].X*verts[j].Y - verts[j].X*verts[i].Y);
                cx += (verts[i].X + verts[j].X)*factor;
                cy += (verts[i].Y + verts[j].Y)*factor;
                //Debug.WriteLine(i.ToString() + factor.ToString() + " -- " + verts[i].ToString());
            }
            area *= 6.0f;
            factor = 1/area;
            cx *= factor;
            cy *= factor;
            _res.X = cx;
            _res.Y = cy;
            return _res;
        }

        /// <summary>
        /// Gets the moment of inertia from the vertices
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Can't calculate MOI on zero vertices</exception>
        public float GetMomentOfInertia()
        {
            Vertices verts = new Vertices(this);
            verts.ForceCounterClockWiseOrder();
            Vector2 centroid = verts.GetCentroid();
            verts.Translate(-centroid);

            if (verts.Count == 0)
            {
                throw new ArgumentException("Can't calculate MOI on zero vertices");
            }
            if (verts.Count == 1)
            {
                return 0;
            }

            float denom = 0;
            float numer = 0;
            Vector2 v2;
            Vector2 v1 = verts[verts.Count - 1];
            for (int index = 0; index < verts.Count; index++, v1 = v2)
            {
                v2 = verts[index];
                float a;
                Vector2.Dot(ref v2, ref v2, out a);
                float b;
                Vector2.Dot(ref v2, ref v1, out b);
                float c;
                Vector2.Dot(ref v1, ref v1, out c);
                //Vector2.Cross(ref v1, ref v2, out d);
                float d;
                Calculator.Cross(ref v1, ref v2, out d);
                d = Math.Abs(d);
                numer += d;
                denom += (a + b + c)*d;
            }
            return denom/(numer*6);
        }

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

        public void Translate(Vector2 vector)
        {
            for (int i = 0; i < Count; i++)
            {
                this[i] = Vector2.Add(this[i], vector);
            }
        }

        /// <summary>
        /// Creates a rectangle with the specified width and height.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>The vertices that define a rectangle</returns>
        public static Vertices CreateRectangle(float width, float height)
        {
            Vertices vertices = new Vertices();
            vertices.Add(new Vector2(-width*.5f, -height*.5f));
            vertices.Add(new Vector2(-width*.5f, -height*.25f));
            vertices.Add(new Vector2(-width*.5f, 0));
            vertices.Add(new Vector2(-width*.5f, height*.25f));
            vertices.Add(new Vector2(-width*.5f, height*.5f));
            vertices.Add(new Vector2(-width*.25f, height*.5f));
            vertices.Add(new Vector2(0, height*.5f));
            vertices.Add(new Vector2(width*.25f, height*.5f));
            vertices.Add(new Vector2(width*.5f, height*.5f));
            vertices.Add(new Vector2(width*.5f, height*.25f));
            vertices.Add(new Vector2(width*.5f, 0));
            vertices.Add(new Vector2(width*.5f, -height*.25f));
            vertices.Add(new Vector2(width*.5f, -height*.5f));
            vertices.Add(new Vector2(width*.25f, -height*.5f));
            vertices.Add(new Vector2(0, -height*.5f));
            vertices.Add(new Vector2(-width*.25f, -height*.5f));
            return vertices;
        }

        /// <summary>
        /// Creates a circle with the specified radius and number of edges.
        /// </summary>
        /// <param name="radius">The radius.</param>
        /// <param name="numberOfEdges">The number of edges. The more edges, the more it resembles a circle</param>
        /// <returns></returns>
        public static Vertices CreateCircle(float radius, int numberOfEdges)
        {
            Vertices vertices = new Vertices();

            float stepSize = Calculator.TwoPi/numberOfEdges;
            vertices.Add(new Vector2(radius, 0));
            for (int i = 1; i < numberOfEdges; i++)
            {
                vertices.Add(new Vector2(radius*Calculator.Cos(stepSize*i), -radius*Calculator.Sin(stepSize*i)));
            }
            return vertices;
        }

        public void Scale(Vector2 value)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                this[i] *= value;
            }
        }

        public override string ToString()
        {
            //Note: Converted to stringbuilder for Farseer 2.0
            StringBuilder toString = new StringBuilder();
            for (int i = 0; i < Count; i++)
            {
                toString.Append(this[i].ToString());
                if (i < Count - 1)
                {
                    toString.Append(" ");
                }
            }
            return toString.ToString();
        }
    }
}