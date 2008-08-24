using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

#if (XNA)
using Microsoft.Xna.Framework;
#endif
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Collisions {
    public class Vertices : List<Vector2> {
        private Vector2 vectorTemp1 = Vector2.Zero;
        private Vector2 vectorTemp2 = Vector2.Zero;
        private Vector2 vectorTemp3 = Vector2.Zero;

        public Vertices() { }

        public Vertices(Vector2[] vector2) {
            for (int i = 0; i < vector2.Length; i++) {
                Add(vector2[i]);
            }
        }

        public Vertices(Vertices vertices) {
            for (int i = 0; i < vertices.Count; i++) {
                Add(vertices[i]);
            }
        }

        public Int32 NextIndex(Int32 index) {
            if (index == this.Count - 1) {
                return 0;
            }
            else {
                return index + 1;
            }
        }

        public Int32 PreviousIndex(Int32 index) {
            if (index == 0) {
                return this.Count - 1;
            }
            else {
                return index - 1;
            }
        }

        public Vector2[] VerticesArray
        {
            get
            {
                return this.ToArray();
            }
        }

        public Vector2 GetEdge(Int32 index) {
            Int32 nextIndex = NextIndex(index);
            vectorTemp2 = this[nextIndex];
            vectorTemp3 = this[index];
            Vector2.Subtract(ref vectorTemp2, ref vectorTemp3, out this.vectorTemp1);
            return this.vectorTemp1;
        }

        public void GetEdge(Int32 index, out Vector2 edge) {
            Int32 nextIndex = NextIndex(index);
            vectorTemp2 = this[nextIndex];
            vectorTemp3 = this[index];
            Vector2.Subtract(ref vectorTemp2, ref vectorTemp3, out edge);
        }

        public Vector2 GetEdgeMidPoint(Int32 index) {
            //Vector2 edge = GetEdge(index);
            GetEdge(index, out vectorTemp1);
            //edge = Vector2.Multiply(edge, .5f);
            Vector2.Multiply(ref vectorTemp1, .5f, out vectorTemp2);

            //Vector2 midPoint = Vector2.Add(localVertices[index], edge);
            vectorTemp3 = this[index];
            Vector2.Add(ref vectorTemp3, ref vectorTemp2, out vectorTemp1);

            return vectorTemp1;
        }

        public void GetEdgeMidPoint(Int32 index, out Vector2 midPoint) {
            GetEdge(index, out vectorTemp1);
            Vector2.Multiply(ref vectorTemp1, .5f, out vectorTemp2);
            vectorTemp3 = this[index];
            Vector2.Add(ref vectorTemp3, ref vectorTemp2, out midPoint);
        }

        public Vector2 GetEdgeNormal(Int32 index) {
            //Vector2 edge = GetEdge(index);
            GetEdge(index, out vectorTemp1);

            //Vector2 edgeNormal = new Vector2(-edge.Y, edge.X);
            vectorTemp2.X = -vectorTemp1.Y;
            vectorTemp2.Y = vectorTemp1.X;

            //edgeNormal.Normalize();
            Vector2.Normalize(ref vectorTemp2, out vectorTemp3);

            return vectorTemp3;
        }

        private Vector2 vectorTemp4 = Vector2.Zero;
        private Vector2 vectorTemp5 = Vector2.Zero;
        public void GetEdgeNormal(Int32 index, out Vector2 edgeNormal) {
            GetEdge(index, out vectorTemp4);
            vectorTemp5.X = -vectorTemp4.Y;
            vectorTemp5.Y = vectorTemp4.X;
            Vector2.Normalize(ref vectorTemp5, out edgeNormal);
        }

        public Vector2 GetVertexNormal(Int32 index) {
            //Vector2 nextEdge = GetEdgeNormal(index);
            GetEdgeNormal(index, out vectorTemp1);

            //Vector2 prevEdge = GetEdgeNormal(PreviousIndex(index));
            int prevIndex = PreviousIndex(index);
            GetEdgeNormal(prevIndex, out vectorTemp2);

            //Vector2 vertexNormal = Vector2.Add(nextEdge, prevEdge);
            Vector2.Add(ref vectorTemp1, ref vectorTemp2, out vectorTemp3);

            //vertexNormal.Normalize();
            Vector2.Normalize(ref vectorTemp3, out vectorTemp1);

            return vectorTemp1;
        }

        public void GetVertexNormal(Int32 index, out Vector2 vertexNormal) {
            GetEdgeNormal(index, out vectorTemp1);
            int prevIndex = PreviousIndex(index);
            GetEdgeNormal(prevIndex, out vectorTemp2);
            Vector2.Add(ref vectorTemp1, ref vectorTemp2, out vectorTemp3);
            Vector2.Normalize(ref vectorTemp3, out vertexNormal);
        }

        public float GetShortestEdge() {
            float shortestEdge = float.MaxValue;
            for (int i = 0; i < this.Count; i++) {
                GetEdge(i, out vectorTemp1);
                float length = vectorTemp1.Length();
                if (length < shortestEdge) { shortestEdge = length; }
            }
            return shortestEdge;
        }

        public void SubDivideEdges(float maxEdgeLength) {
            //throw new NotImplementedException("SubDivideEdges is not yet implemented");
            //TODO: Implement SubDivideEdges()
            Vertices verticesTemp = new Vertices(); 
            double edgeCount;
            float edgeLength;
            Vector2 vertA = Vector2.Zero;
            Vector2 vertB = Vector2.Zero;
            Vector2 edge = Vector2.Zero;
            for (int i = 0; i < this.Count; i++)
            {
                
                vertA = this[i];
                vertB = this[this.NextIndex(i)];
                edge = Vector2.Zero;
                Vector2.Subtract(ref vertA, ref vertB, out edge);
                edgeLength = edge.Length();

                verticesTemp.Add(vertA);
                if (edgeLength > maxEdgeLength) //need to subdivide
                {
                    edgeCount = Math.Ceiling((double)edgeLength / (double)maxEdgeLength);                    
                    
                    for (int j = 0; j < edgeCount - 1;j++ )
                    {
                        Vector2 vert = Vector2.Lerp(vertA, vertB, (float)(j+1) / (float)edgeCount);
                        verticesTemp.Add(vert);
                    } 
                }
            }
            //Debug.WriteLine(verticesTemp.ToString());
            Clear();
            for (int k = 0; k < verticesTemp.Count; k++)
            {
                this.Add(verticesTemp[k]);
            }
            //Debug.WriteLine(this.ToString());
        }

        public void ForceCounterClockWiseOrder()
        {
            // the sign of the 'area' of the polygon is all
            // we are interested in.
            float area = GetSignedArea();
            if (area > 0)
            {
                this.Reverse();
            }  
        }

        private float GetSignedArea()
        {
            int i, j;
            float area = 0;

            for (i = 0; i < Count; i++)
            {
                j = (i + 1) % Count;
                area += this[i].X * this[j].Y;
                area -= this[i].Y * this[j].X;
            }
            area /= 2.0f;
            return area;
        }

        public float GetArea()
        {
            int i, j;
            float area = 0;
                        
            for (i = 0; i < Count; i++)
            {
                j = (i + 1) % Count;
                area += this[i].X * this[j].Y;
                area -= this[i].Y * this[j].X;
            }
            area /= 2.0f;
            return (area < 0 ? -area : area);
        }

        Vector2 res = new Vector2();
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
            int i, j;

            float factor = 0;
            //Debug.WriteLine(verts.ToString());
            for (i = 0; i < Count; i++)
            {
                j = (i + 1) % Count;

                factor = -(verts[i].X * verts[j].Y - verts[j].X * verts[i].Y);
                cx += (verts[i].X + verts[j].X) * factor;
                cy += (verts[i].Y + verts[j].Y) * factor;
                //Debug.WriteLine(i.ToString() + factor.ToString() + " -- " + verts[i].ToString());
            }
            area *= 6.0f;
            factor = 1 / area;
            cx *= factor;
            cy *= factor;
            res.X = cx;
            res.Y = cy;
            return res;
        }

        public float GetMomentOfInertia() {
            Vertices verts = new Vertices(this);
            verts.ForceCounterClockWiseOrder();
            Vector2 centroid = verts.GetCentroid();
            verts.Translate(-centroid);

            if (verts.Count == 0) { throw new ArgumentOutOfRangeException("vertexes"); }
            if (verts.Count == 1) { return 0; }

            float denom = 0;
            float numer = 0;
            float a, b, c, d;
            Vector2 v1, v2;
            v1 = verts[verts.Count - 1];
            for (int index = 0; index < verts.Count; index++, v1 = v2)
            {
                v2 = verts[index];
                Vector2.Dot(ref v2, ref v2, out a);
                Vector2.Dot(ref v2, ref v1, out b);
                Vector2.Dot(ref v1, ref v1, out c);
                //Vector2.Cross(ref v1, ref v2, out d);
                Calculator.Cross(ref v1, ref v2, out d);
                d = Math.Abs(d);
                numer += d;
                denom += (a + b + c) * d;
            }
            return denom / (numer * 6);
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
                }else
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
            for (int i = 0; i < this.Count; i++)
            {
                this[i] = Vector2.Add(this[i], vector);
            }
        }

        public static Vertices CreateRectangle(float width, float height) {
            Vertices vertices = new Vertices();
            vertices.Add(new Vector2(-width * .5f, -height * .5f));
            vertices.Add(new Vector2(-width * .5f, -height * .25f));
            vertices.Add(new Vector2(-width * .5f, 0));
            vertices.Add(new Vector2(-width * .5f, height * .25f));
            vertices.Add(new Vector2(-width * .5f, height * .5f));
            vertices.Add(new Vector2(-width * .25f, height * .5f));
            vertices.Add(new Vector2(0, height * .5f));
            vertices.Add(new Vector2(width * .25f, height * .5f));
            vertices.Add(new Vector2(width * .5f, height * .5f));
            vertices.Add(new Vector2(width * .5f, height * .25f));
            vertices.Add(new Vector2(width * .5f, 0));
            vertices.Add(new Vector2(width * .5f, -height * .25f));
            vertices.Add(new Vector2(width * .5f, -height * .5f));
            vertices.Add(new Vector2(width * .25f, -height * .5f));
            vertices.Add(new Vector2(0, -height * .5f));
            vertices.Add(new Vector2(-width * .25f, -height * .5f));
            return vertices;
        }

        public static Vertices CreateCircle(float radius, int numberOfEdges) {
            Vertices vertices = new Vertices();

            float stepSize = Mathematics.Calculator.TwoPi / (float)numberOfEdges;
            vertices.Add(new Vector2(radius, 0));
            for (int i = 1; i < numberOfEdges; i++) {
                vertices.Add(new Vector2(radius * Calculator.Cos(stepSize * i), -radius * Calculator.Sin(stepSize * i)));
            }
            return vertices;
        }

        public override string ToString()
        {
            string toString = "";
            for (int i = 0; i < this.Count; i++)
            {
                toString += this[i].ToString();
                if (i < this.Count - 1)
                {
                    toString += " ";
                }  
            }
            return toString;
        }
    }
}
