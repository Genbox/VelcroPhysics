using System;
using System.Collections.Generic;
using System.Text;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerGames.FarseerPhysics.Dynamics;

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

        /// <summary>
        /// Gets an array of vertices.
        /// </summary>
        /// <returns></returns>
        public Vector2[] GetVerticesArray()
        {
            return ToArray();
        }

        /// <summary>
        /// Gets the next index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
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
        /// Gets the edge.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public Vector2 GetEdge(int index)
        {
            int nextIndex = NextIndex(index);
            _vectorTemp2 = this[nextIndex];
            _vectorTemp3 = this[index];
            Vector2.Subtract(ref _vectorTemp2, ref _vectorTemp3, out _vectorTemp1);
            return _vectorTemp1;
        }

        /// <summary>
        /// Gets the edge.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="edge">The edge.</param>
        public void GetEdge(int index, out Vector2 edge)
        {
            int nextIndex = NextIndex(index);
            _vectorTemp2 = this[nextIndex];
            _vectorTemp3 = this[index];
            Vector2.Subtract(ref _vectorTemp2, ref _vectorTemp3, out edge);
        }

        /// <summary>
        /// Gets the edge mid point.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public Vector2 GetEdgeMidPoint(int index)
        {
            GetEdge(index, out _vectorTemp1);
            Vector2.Multiply(ref _vectorTemp1, .5f, out _vectorTemp2);

            _vectorTemp3 = this[index];
            Vector2.Add(ref _vectorTemp3, ref _vectorTemp2, out _vectorTemp1);

            return _vectorTemp1;
        }

        /// <summary>
        /// Gets the edge mid point.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="midPoint">The mid point.</param>
        public void GetEdgeMidPoint(int index, out Vector2 midPoint)
        {
            GetEdge(index, out _vectorTemp1);
            Vector2.Multiply(ref _vectorTemp1, .5f, out _vectorTemp2);
            _vectorTemp3 = this[index];
            Vector2.Add(ref _vectorTemp3, ref _vectorTemp2, out midPoint);
        }

        /// <summary>
        /// Gets the edge normal.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public Vector2 GetEdgeNormal(int index)
        {
            GetEdge(index, out _vectorTemp1);

            _vectorTemp2.X = -_vectorTemp1.Y;
            _vectorTemp2.Y = _vectorTemp1.X;

            Vector2.Normalize(ref _vectorTemp2, out _vectorTemp3);

            return _vectorTemp3;
        }

        /// <summary>
        /// Gets the edge normal.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="edgeNormal">The edge normal.</param>
        public void GetEdgeNormal(int index, out Vector2 edgeNormal)
        {
            GetEdge(index, out _vectorTemp4);
            _vectorTemp5.X = -_vectorTemp4.Y;
            _vectorTemp5.Y = _vectorTemp4.X;
            Vector2.Normalize(ref _vectorTemp5, out edgeNormal);
        }

        /// <summary>
        /// Gets the vertex normal.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public Vector2 GetVertexNormal(int index)
        {
            GetEdgeNormal(index, out _vectorTemp1);

            int prevIndex = PreviousIndex(index);
            GetEdgeNormal(prevIndex, out _vectorTemp2);

            Vector2.Add(ref _vectorTemp1, ref _vectorTemp2, out _vectorTemp3);

            Vector2.Normalize(ref _vectorTemp3, out _vectorTemp1);

            return _vectorTemp1;
        }

        /// <summary>
        /// Gets the vertex normal.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="vertexNormal">The vertex normal.</param>
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
                    double edgeCount = Math.Ceiling(edgeLength / (double)maxEdgeLength);

                    for (int j = 0; j < edgeCount - 1; j++)
                    {
                        Vector2 vert = Vector2.Lerp(vertA, vertB, (j + 1) / (float)edgeCount);
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

        /// <summary>
        /// Forces counter clock wise order.
        /// </summary>
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
        /// Gets the moment of inertia from the vertices
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Can't calculate MOI on zero vertices</exception>
        public float GetMomentOfInertia()
        {
            Vertices verts = new Vertices(this);

            //Make sure that the vertices are in counter clockwise order.
            verts.ForceCounterClockWiseOrder();

            //Get the centroid and center the vertices around the centroid.
            Vector2 centroid = verts.GetCentroid();

            Vector2.Multiply(ref centroid, -1, out centroid);

            verts.Translate(ref centroid);

            if (verts.Count == 0)
                throw new ArgumentException("Can't calculate MOI on zero vertices");

            if (verts.Count == 1)
                return 0;

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
                denom += (a + b + c) * d;
            }
            return denom / (numer * 6);
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
        /// Determines whether this instance is convex.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is convex; otherwise, <c>false</c>.
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

        /// <summary>
        /// Creates a rectangle with the specified width and height
        /// with automatic subdivsion.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>The vertices that define a rectangle</returns>
        public static Vertices CreateRectangle(float width, float height)
        {
            //Note: The rectangle has vertices along the edges. This is to support the distance grid better.
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

        /// <summary>
        /// Creates a rectangle with the specified width and height
        /// with no subdivision.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>The vertices that define a rectangle</returns>
        public static Vertices CreateSimpleRectangle(float width, float height)
        {
            Vertices vertices = new Vertices();
            vertices.Add(new Vector2(-width * .5f, -height * .5f));
            vertices.Add(new Vector2(-width * .5f, height * .5f));
            vertices.Add(new Vector2(width * .5f, height * .5f));
            vertices.Add(new Vector2(width * .5f, -height * .5f));

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
            return CreateEllipse(radius, radius, numberOfEdges);
        }

        /// <summary>
        /// Creates a ellipse with the specified width, height and number of edges.
        /// </summary>
        /// <param name="xRadius">Width of the ellipse.</param>
        /// <param name="yRadius">Height of the ellipse.</param>
        /// <param name="numberOfEdges">The number of edges. The more edges, the more it resembles an ellipse</param>
        /// <returns></returns>
        public static Vertices CreateEllipse(float xRadius, float yRadius, int numberOfEdges)
        {
            Vertices vertices = new Vertices();

            float stepSize = MathHelper.TwoPi / numberOfEdges;

            vertices.Add(new Vector2(xRadius, 0));
            for (int i = 1; i < numberOfEdges; i++)
                vertices.Add(new Vector2(xRadius * Calculator.Cos(stepSize * i), -yRadius * Calculator.Sin(stepSize * i)));

            return vertices;
        }

        /// <summary>
        /// Creates a gear shape with the specified radius and number of teeth.
        /// </summary>
        /// <param name="radius">The radius.</param>
        /// <param name="numberOfTeeth">The number of teeth.</param>
        /// <param name="tipPercentage">The tip percentage.</param>
        /// <param name="toothHeight">Height of the tooth.</param>
        /// <returns></returns>
        public static Vertices CreateGear(float radius, int numberOfTeeth, float tipPercentage, float toothHeight)
        {
            Vertices vertices = new Vertices();

            float stepSize = MathHelper.TwoPi / numberOfTeeth;

            float toothTipStepSize = (stepSize / 2f) * tipPercentage;

            float toothAngleStepSize = (stepSize - (toothTipStepSize * 2f)) / 2f;

            for (int i = 0; i < numberOfTeeth; i++)
            {
                vertices.Add(new Vector2((radius) * Calculator.Cos(stepSize * i),
                    -(radius) * Calculator.Sin(stepSize * i)));

                vertices.Add(new Vector2((radius + toothHeight) * Calculator.Cos((stepSize * i) + toothAngleStepSize),
                    -(radius + toothHeight) * Calculator.Sin((stepSize * i) + toothAngleStepSize)));

                vertices.Add(new Vector2((radius + toothHeight) * Calculator.Cos((stepSize * i) + toothAngleStepSize + toothTipStepSize),
                    -(radius + toothHeight) * Calculator.Sin((stepSize * i) + toothAngleStepSize + toothTipStepSize)));

                vertices.Add(new Vector2((radius) * Calculator.Cos((stepSize * i) + (toothAngleStepSize * 2f) + toothTipStepSize),
                    -(radius) * Calculator.Sin((stepSize * i) + (toothAngleStepSize * 2f) + toothTipStepSize)));
            }

            return vertices;
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

        #region General purpose static tools
        /// <summary>
        /// Finds the mid-point of two Vector2.
        /// </summary>
        /// <param name="firstVector">First Vector2.</param>
        /// <param name="secondVector">Other Vector2.</param>
        /// <returns>Mid-point Vector2.</returns>
        public static Vector2 FindMidpoint(Vector2 firstVector, Vector2 secondVector)
        {
            float midDeltaX, midDeltaY;

            if (firstVector.X < secondVector.X)
                midDeltaX = Math.Abs((firstVector.X - secondVector.X) * 0.5f); // find x axis midpoint
            else
                midDeltaX = (secondVector.X - firstVector.X) * 0.5f; // find x axis midpoint
            if (firstVector.Y < secondVector.Y)
                midDeltaY = Math.Abs((firstVector.Y - secondVector.Y) * 0.5f); // find y axis midpoint
            else
                midDeltaY = (secondVector.Y - firstVector.Y) * 0.5f; // find y axis midpoint

            return (new Vector2(firstVector.X + midDeltaX, firstVector.Y + midDeltaY)); // return mid point
        }

        /// <summary>
        /// Finds the normal from two vectors
        /// </summary>
        /// <param name="first">The first vector</param>
        /// <param name="second">The second vector</param>
        /// <returns></returns>
        public static Vector2 FindEdgeNormal(Vector2 first, Vector2 second)
        {
            //Xbox360 need this variable to be initialized to Vector2.Zero
            Vector2 normal = Vector2.Zero;

            Vector2 temp = new Vector2(first.X - second.X, first.Y - second.Y);

            normal.X = -temp.Y; // get 2D normal
            normal.Y = temp.X; // works only on counter clockwise polygons

            normal.Normalize();

            return normal;
        }

        public static Vector2 FindVertexNormal(Vector2 first, Vector2 second, Vector2 c)
        {
            Vector2 temp;
            Vector2 one = FindEdgeNormal(first, second);
            Vector2 two = FindEdgeNormal(second, c);

            Vector2.Add(ref one, ref two, out temp);
            return temp;
        }

        /// <summary>
        /// Finds the angle of the vector.
        /// </summary>
        /// <returns>Angle of the vector.</returns>
        public static float FindNormalAngle(Vector2 n)
        {
            if ((n.Y > 0.0f) && (n.X > 0.0f))
                return (float)Math.Atan(n.X / -n.Y);

            if ((n.Y < 0.0f) && (n.X > 0.0f))
                return (float)Math.Atan(n.X / -n.Y); // good

            if ((n.Y > 0.0f) && (n.X < 0.0f))
                return (float)Math.Atan(-n.X / n.Y);

            if ((n.Y < 0.0f) && (n.X < 0.0f))
                return (float)Math.Atan(-n.X / n.Y); // good

            return 0.0f;
        }

        #endregion

        #region Sickbattery's Extension

        /// <summary>
        /// TODO:
        /// 1.) Das Array welches ich bekomme am besten in einen bool array verwandeln. Würde die Geschwindigkeit verbessern
        /// </summary>
        private static readonly int[,] _closePixels = new int[8, 2] { { -1, -1 }, { 0, -1 }, { 1, -1 }, { 1, 0 }, { 1, 1 }, { 0, 1 }, { -1, 1 }, { -1, 0 } };

        /// <summary>
        /// Creates vertices from the texture data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        public static Vertices CreatePolygon(uint[] data, int width, int height)
        {
            PolygonCreationAssistance pca = new PolygonCreationAssistance(data, width, height);
            List<Vertices> verts = CreatePolygon(ref pca);

            return verts[0];
        }

        /// <summary>
        /// Creates a list of vertices from the texture data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="hullTolerance">The hull tolerance. This argument controls the amount of details found in the detection.</param>
        /// <param name="alphaTolerance">The alpha tolerance.</param>
        /// <param name="multiPartDetection">if set to <c>true</c> [multi part detection].</param>
        /// <param name="holeDetection">if set to <c>true</c> [hole detection].</param>
        /// <returns></returns>
        public static List<Vertices> CreatePolygon(uint[] data, int width, int height, float hullTolerance, byte alphaTolerance, bool multiPartDetection, bool holeDetection)
        {
            PolygonCreationAssistance pca = new PolygonCreationAssistance(data, width, height);
            pca.HullTolerance = hullTolerance;
            pca.AlphaTolerance = alphaTolerance;
            pca.MultipartDetection = multiPartDetection;
            pca.HoleDetection = holeDetection;
            return CreatePolygon(ref pca);
        }

        /// <summary>
        /// Creates a list of vertices. Create a PolygonCreationAssistance that contains all the data needed for detection.
        /// </summary>
        /// <param name="pca">The pca.</param>
        /// <returns></returns>
        public static List<Vertices> CreatePolygon(ref PolygonCreationAssistance pca)
        {
            List<Vertices> polygons = new List<Vertices>();

            Vertices polygon;
            Vertices holePolygon;

            int vertex1Index;
            int vertex2Index;

            Vector2? holeEntrance = null;
            Vector2? polygonEntrance = null;

            List<Vector2> blackList = new List<Vector2>();

            bool inPolygon;
            bool searchOn;

            // First of all: Check the array you just got.
            if (pca.IsValid())
            {
                do
                {
                    if (polygons.Count == 0)
                    {
                        polygon = CreateSimplePolygon(ref pca, Vector2.Zero, Vector2.Zero);

                        if (polygon != null && polygon.Count > 2)
                        {
                            polygonEntrance = GetTopMostVertex(ref polygon);
                        }
                    }
                    else if (polygonEntrance.HasValue)
                    {
                        polygon = CreateSimplePolygon(ref pca, polygonEntrance.Value, new Vector2(polygonEntrance.Value.X - 1f, polygonEntrance.Value.Y));
                    }
                    else
                    {
                        break;
                    }

                    searchOn = false;

                    if (polygon != null && polygon.Count > 2)
                    {
                        if (pca.HoleDetection)
                        {
                            do
                            {
                                holeEntrance = GetHoleHullEntrance(ref pca, ref polygon, holeEntrance);

                                if (holeEntrance.HasValue)
                                {
                                    if (!blackList.Contains(holeEntrance.Value))
                                    {
                                        blackList.Add(holeEntrance.Value);
                                        holePolygon = CreateSimplePolygon(ref pca, holeEntrance.Value, new Vector2(holeEntrance.Value.X + 1, holeEntrance.Value.Y));

                                        if (holePolygon != null && holePolygon.Count > 2)
                                        {
                                            holePolygon.Add(holePolygon[0]);

                                            if (SplitPolygonEdge(ref polygon, EdgeAlignment.Vertical, holeEntrance.Value, out vertex1Index, out vertex2Index))
                                            {
                                                polygon.InsertRange(vertex2Index, holePolygon);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    break;
                                }

                            } while (true);
                        }

                        polygons.Add(polygon);

                        if (pca.MultipartDetection)
                        {
                            /// 1:  95 / 151
                            /// 2: 232 / 252
                            /// 
                            while (GetNextHullEntrance(ref pca, polygonEntrance.Value, out polygonEntrance))
                            {
                                inPolygon = false;

                                for (int i = 0; i < polygons.Count; i++)
                                {
                                    polygon = polygons[i];

                                    if (InPolygon(ref pca, ref polygon, polygonEntrance.Value))
                                    {
                                        inPolygon = true;
                                        break;
                                    }
                                }

                                if (!inPolygon)
                                {
                                    searchOn = true;
                                    break;
                                }
                            }
                        }
                    }

                } while (searchOn);
            }
            else
            {
                throw new Exception("Sizes don't match: Color array must contain texture width * texture height elements.");
            }

            return polygons;
        }

        private static Vector2? GetHoleHullEntrance(ref PolygonCreationAssistance pca, ref Vertices polygon, Vector2? startVertex)
        {
            List<CrossingEdgeInfo> edges = new List<CrossingEdgeInfo>();
            Vector2? entrance;

            int startLine;
            int endLine;

            int lastSolid = 0;
            bool foundSolid;
            bool foundTransparent;

            if (polygon != null && polygon.Count > 0)
            {
                if (startVertex.HasValue)
                {
                    startLine = (int)startVertex.Value.Y;
                }
                else
                {
                    startLine = (int)GetTopMostCoord(ref polygon);
                }
                endLine = (int)GetBottomMostCoord(ref polygon);

                if (startLine > 0 && startLine < pca.Height && endLine > 0 && endLine < pca.Height)
                {
                    // go from top to bottom of the polygon
                    for (int y = startLine; y <= endLine; y += pca.HoleDetectionLineStepSize)
                    {
                        // get x-coord of every polygon edge which crosses y
                        edges = GetCrossingEdges(ref polygon, EdgeAlignment.Vertical, y);

                        // we need an even number of crossing edges
                        if (edges.Count > 1 && edges.Count % 2 == 0)
                        {
                            for (int i = 0; i < edges.Count; i += 2)
                            {
                                foundSolid = false;
                                foundTransparent = false;

                                for (int x = (int)edges[i].CrossingPoint.X; x <= (int)edges[i + 1].CrossingPoint.X; x++)
                                {
                                    if (pca.IsSolid(x, y))
                                    {
                                        if (!foundTransparent)
                                        {
                                            foundSolid = true;
                                            lastSolid = x;
                                        }

                                        if (foundSolid && foundTransparent)
                                        {
                                            entrance = new Vector2(lastSolid, y);

                                            if (DistanceToHullAcceptable(ref pca, ref polygon, entrance.Value, true))
                                            {
                                                return entrance;
                                            }
                                            entrance = null;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (foundSolid)
                                        {
                                            foundTransparent = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        private static bool DistanceToHullAcceptable(ref PolygonCreationAssistance pca, ref Vertices polygon, Vector2 point, bool higherDetail)
        {
            Vector2 edgeVertex1;
            Vector2 edgeVertex2;

            if (polygon != null && polygon.Count > 2)
            {
                edgeVertex2 = polygon[polygon.Count - 1];

                if (higherDetail)
                {
                    for (int i = 0; i < polygon.Count; i++)
                    {
                        edgeVertex1 = polygon[i];

                        if (Calculator.DistanceBetweenPointAndLineSegment(ref point, ref edgeVertex1, ref edgeVertex2) <= pca.HullTolerance ||
                            Calculator.DistanceBetweenPointAndPoint(ref point, ref edgeVertex1) <= pca.HullTolerance)
                        {
                            return false;
                        }

                        edgeVertex2 = polygon[i];
                    }

                    return true;
                }
                else
                {
                    for (int i = 0; i < polygon.Count; i++)
                    {
                        edgeVertex1 = polygon[i];

                        if (Calculator.DistanceBetweenPointAndLineSegment(ref point, ref edgeVertex1, ref edgeVertex2) <= pca.HullTolerance)
                        {
                            return false;
                        }

                        edgeVertex2 = polygon[i];
                    }

                    return true;
                }
            }

            return false;
        }

        private static bool InPolygon(ref PolygonCreationAssistance pca, ref Vertices polygon, Vector2 point)
        {
            bool inPolygon = !DistanceToHullAcceptable(ref pca, ref polygon, point, true);

            if (!inPolygon)
            {
                List<CrossingEdgeInfo> edges = GetCrossingEdges(ref polygon, EdgeAlignment.Vertical, (int)point.Y);

                if (edges.Count > 0 && edges.Count % 2 == 0)
                {
                    for (int i = 0; i < edges.Count; i += 2)
                    {
                        if (edges[i].CrossingPoint.X <= point.X && edges[i + 1].CrossingPoint.X >= point.X)
                        {
                            return true;
                        }
                    }

                    return false;
                }
                return false;
            }

            return inPolygon;
        }

        private static Vector2? GetTopMostVertex(ref Vertices vertices)
        {
            float topMostValue = float.MaxValue;
            Vector2? topMost = null;

            for (int i = 0; i < vertices.Count; i++)
            {
                if (topMostValue > vertices[i].Y)
                {
                    topMostValue = vertices[i].Y;
                    topMost = vertices[i];
                }
            }

            return topMost;
        }

        private static float GetTopMostCoord(ref Vertices vertices)
        {
            float returnValue = float.MaxValue;

            for (int i = 0; i < vertices.Count; i++)
            {
                if (returnValue > vertices[i].Y)
                {
                    returnValue = vertices[i].Y;
                }
            }

            return returnValue;
        }

        private static float GetBottomMostCoord(ref Vertices vertices)
        {
            float returnValue = float.MinValue;

            for (int i = 0; i < vertices.Count; i++)
            {
                if (returnValue < vertices[i].Y)
                {
                    returnValue = vertices[i].Y;
                }
            }

            return returnValue;
        }

        private static List<CrossingEdgeInfo> GetCrossingEdges(ref Vertices polygon, EdgeAlignment edgeAlign, int checkLine)
        {
            List<CrossingEdgeInfo> edges = new List<CrossingEdgeInfo>();

            Vector2 slope;
            Vector2 edgeVertex1;
            Vector2 edgeVertex2;

            Vector2 slopePreview;
            Vector2 edgeVertexPreview;

            Vector2 crossingPoint;
            bool addCrossingPoint;

            if (polygon.Count > 1)
            {
                edgeVertex2 = polygon[polygon.Count - 1];

                switch (edgeAlign)
                {
                    case EdgeAlignment.Vertical:
                        for (int i = 0; i < polygon.Count; i++)
                        {
                            edgeVertex1 = polygon[i];

                            if ((edgeVertex1.Y >= checkLine && edgeVertex2.Y <= checkLine) || (edgeVertex1.Y <= checkLine && edgeVertex2.Y >= checkLine))
                            {
                                if (edgeVertex1.Y != edgeVertex2.Y)
                                {
                                    addCrossingPoint = true;
                                    slope = edgeVertex2 - edgeVertex1;

                                    if (edgeVertex1.Y == checkLine)
                                    {
                                        edgeVertexPreview = polygon[(i + 1) % polygon.Count];
                                        slopePreview = edgeVertex1 - edgeVertexPreview;

                                        if (slope.Y > 0)
                                        {
                                            addCrossingPoint = (slopePreview.Y <= 0);
                                        }
                                        else
                                        {
                                            addCrossingPoint = (slopePreview.Y >= 0);
                                        }
                                    }

                                    if (addCrossingPoint)
                                    {
                                        crossingPoint = new Vector2((checkLine - edgeVertex1.Y) / slope.Y * slope.X + edgeVertex1.X, (float)checkLine);
                                        edges.Add(new CrossingEdgeInfo(edgeVertex1, edgeVertex2, crossingPoint, edgeAlign));
                                    }
                                }
                            }
                            edgeVertex2 = edgeVertex1;
                        }
                        break;

                    case EdgeAlignment.Horizontal:
                        throw new Exception("EdgeAlignment.Horizontal isn't implemented yet. Sorry.");
                }
            }

            edges.Sort();
            return edges;
        }

        private static bool SplitPolygonEdge(ref Vertices polygon, EdgeAlignment edgeAlign, Vector2 coordInsideThePolygon, out int vertex1Index, out int vertex2Index)
        {
            List<CrossingEdgeInfo> edges = new List<CrossingEdgeInfo>();

            Vector2 slope;
            int edgeVertex1Index;
            int edgeVertex2Index;
            int nearestEdgeVertex1Index = 0;
            int nearestEdgeVertex2Index = 0;
            bool edgeFound = false;

            float distance;
            float shortestDistance = float.MaxValue;

            bool edgeCoordFound = false;
            Vector2 foundEdgeCoord = Vector2.Zero;

            vertex1Index = 0;
            vertex2Index = 0;

            switch (edgeAlign)
            {
                case EdgeAlignment.Vertical:
                    edges = GetCrossingEdges(ref polygon, EdgeAlignment.Vertical, (int)coordInsideThePolygon.Y);

                    foundEdgeCoord.Y = coordInsideThePolygon.Y;

                    if (edges != null && edges.Count > 1 && edges.Count % 2 == 0)
                    {
                        for (int i = 0; i < edges.Count; i++)
                        {
                            if (edges[i].CrossingPoint.X < coordInsideThePolygon.X)
                            {
                                distance = coordInsideThePolygon.X - edges[i].CrossingPoint.X;

                                if (distance < shortestDistance)
                                {
                                    shortestDistance = distance;
                                    foundEdgeCoord.X = edges[i].CrossingPoint.X;

                                    edgeCoordFound = true;
                                }
                            }
                        }

                        if (edgeCoordFound)
                        {
                            shortestDistance = float.MaxValue;

                            edgeVertex2Index = polygon.Count - 1;

                            Vector2 tempVector1;
                            Vector2 tempVector2;

                            for (edgeVertex1Index = 0; edgeVertex1Index < polygon.Count; edgeVertex1Index++)
                            {
                                tempVector1 = polygon[edgeVertex1Index];
                                tempVector2 = polygon[edgeVertex2Index];
                                distance = Calculator.DistanceBetweenPointAndLineSegment(ref foundEdgeCoord, ref tempVector1, ref tempVector2);
                                if (distance < shortestDistance)
                                {
                                    shortestDistance = distance;

                                    nearestEdgeVertex1Index = edgeVertex1Index;
                                    nearestEdgeVertex2Index = edgeVertex2Index;

                                    edgeFound = true;
                                }

                                edgeVertex2Index = edgeVertex1Index;
                            }

                            if (edgeFound)
                            {
                                slope = polygon[nearestEdgeVertex2Index] - polygon[nearestEdgeVertex1Index];
                                slope.Normalize();

                                Vector2 tempVector = polygon[nearestEdgeVertex1Index];
                                distance = Calculator.DistanceBetweenPointAndPoint(ref tempVector, ref foundEdgeCoord);

                                vertex1Index = nearestEdgeVertex1Index;
                                vertex2Index = nearestEdgeVertex1Index + 1;

                                polygon.Insert(nearestEdgeVertex1Index, distance * slope + polygon[vertex1Index]);
                                polygon.Insert(nearestEdgeVertex1Index, distance * slope + polygon[vertex2Index]);

                                return true;
                            }
                        }
                    }
                    break;

                case EdgeAlignment.Horizontal:
                    throw new Exception("EdgeAlignment.Horizontal isn't implemented yet. Sorry.");
            }

            return false;
        }

        private static Vertices CreateSimplePolygon(ref PolygonCreationAssistance pca, Vector2 entrance, Vector2 last)
        {
            bool entranceFound = false;

            Vertices polygon = new Vertices();
            Vertices hullArea = new Vertices();

            Vector2 current = Vector2.Zero;
            Vector2 next;

            #region Entrance check
            // Get the entrance point. //todo: alle möglichkeiten testen
            if (entrance == Vector2.Zero || !pca.InBounds(entrance))
            {
                entranceFound = GetHullEntrance(ref pca, out entrance);

                if (entranceFound)
                {
                    current = new Vector2(entrance.X - 1f, entrance.Y);
                }
            }
            else
            {
                if (pca.IsSolid(entrance))
                {
                    if (IsNearPixel(ref pca, entrance, last))
                    {
                        current = last;
                        entranceFound = true;
                    }
                    else
                    {
                        Vector2 temp;
                        if (SearchNearPixels(ref pca, false, entrance, out temp))
                        {
                            current = temp;
                            entranceFound = true;
                        }
                        else
                        {
                            entranceFound = false;
                        }
                    }
                }
            }
            #endregion

            if (entranceFound)
            {

                // next has to be set to entrance so it'll be added as the
                // first point in the list.
                next = entrance;

                // Fast bugfix XD. The entrance point of course has to be added first to the polygon XD. Damn I forgot that!
                polygon.Add(entrance);

                do
                {
                    Vector2 outstanding;

                    // Add the vertex to a hull pre vision list.
                    hullArea.Add(next);

                    // Search in the pre vision list for an outstanding point.
                    if (SearchForOutstandingVertex(ref hullArea, pca.HullTolerance, out outstanding))
                    {
                        // Add it and remove all vertices that don't matter anymore
                        // (all the vertices before the outstanding).
                        polygon.Add(outstanding);
                        hullArea.RemoveRange(0, hullArea.IndexOf(outstanding));
                    }

                    // Last point gets current and current gets next. Our little spider is moving forward on the hull ;).
                    last = current;
                    current = next;

                    // Get the next point on hull.
                    if (!GetNextHullPoint(ref pca, ref last, ref current, out next))
                    {
                        next = entrance;
                    }
                } // Exit loop if next piont is the entrance point. The hull is complete now!
                while (next != entrance);
            }

            return polygon;
        }

        private static bool SearchNearPixels(ref PolygonCreationAssistance pca, bool searchingForSolidPixel, Vector2 current, out Vector2 foundPixel)
        {
            int x;
            int y;

            for (int i = 0; i < 8; i++)
            {
                x = (int)current.X + _closePixels[i, 0];
                y = (int)current.Y + _closePixels[i, 1];

                if (!searchingForSolidPixel ^ pca.IsSolid(x, y))
                {
                    foundPixel = new Vector2(x, y);
                    return true;
                }
            }

            // Nothing found.
            foundPixel = Vector2.Zero;
            return false;
        }

        private static bool IsNearPixel(ref PolygonCreationAssistance pca, Vector2 current, Vector2 near)
        {
            for (int i = 0; i < 8; i++)
            {
                int x = (int)current.X + _closePixels[i, 0];
                int y = (int)current.Y + _closePixels[i, 1];

                if (x >= 0 && x <= pca.Width && y >= 0 && y <= pca.Height)
                {
                    if (x == (int)near.X && y == (int)near.Y)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool GetHullEntrance(ref PolygonCreationAssistance pca, out Vector2 entrance)
        {
            // Search for first solid pixel.
            for (int y = 0; y <= pca.Height; y++)
            {
                for (int x = 0; x <= pca.Width; x++)
                {
                    if (pca.IsSolid(x, y))
                    {
                        entrance = new Vector2(x, y);
                        return true;
                    }
                }
            }

            // If there are no solid pixels.
            entrance = Vector2.Zero;
            return false;
        }

        private static bool GetNextHullEntrance(ref PolygonCreationAssistance pca, Vector2 start, out Vector2? entrance)
        {
            // Search for first solid pixel.
            int size = pca.Height * pca.Width;
            int x;

            bool foundTransparent = false;

            for (int i = (int)start.X + (int)start.Y * pca.Width; i <= size; i++)
            {
                if (pca.IsSolid(i))
                {
                    if (foundTransparent)
                    {
                        x = i % pca.Width;

                        entrance = new Vector2(x, (i - x) / pca.Width);
                        return true;
                    }
                }
                else
                {
                    foundTransparent = true;
                }
            }

            // If there are no solid pixels.
            entrance = null;
            return false;
        }

        private static bool GetNextHullPoint(ref PolygonCreationAssistance pca, ref Vector2 last, ref Vector2 current, out Vector2 next)
        {
            int x;
            int y;

            int indexOfFirstPixelToCheck = GetIndexOfFirstPixelToCheck(last, current);
            int indexOfPixelToCheck;

            const int pixelsToCheck = 8;// _closePixels.Length;

            for (int i = 0; i < pixelsToCheck; i++)
            {
                indexOfPixelToCheck = (indexOfFirstPixelToCheck + i) % pixelsToCheck;

                x = (int)current.X + _closePixels[indexOfPixelToCheck, 0];
                y = (int)current.Y + _closePixels[indexOfPixelToCheck, 1];

                if (x >= 0 && x < pca.Width && y >= 0 && y <= pca.Height)
                {
                    if (pca.IsSolid(x, y)) //todo
                    {
                        next = new Vector2(x, y);
                        return true;
                    }
                }
            }

            next = Vector2.Zero;
            return false;
        }

        private static bool SearchForOutstandingVertex(ref Vertices hullArea, float hullTolerance, out Vector2 outstanding)
        {
            int hullAreaLastPoint = hullArea.Count - 1;

            Vector2 outstandingResult = Vector2.Zero;
            bool found = false;

            Vector2 tempVector1;
            Vector2 tempVector2;
            Vector2 tempVector3;

            // Search between the first and last hull point.
            for (int i = 1; i < hullAreaLastPoint; i++)
            {
                tempVector1 = hullArea[i];
                tempVector2 = hullArea[0];
                tempVector3 = hullArea[hullAreaLastPoint];
                // Check if the distance is over the one that's tolerable.
                if (Calculator.DistanceBetweenPointAndLineSegment(ref tempVector1, ref  tempVector2, ref tempVector3) >= hullTolerance)
                {
                    outstandingResult = hullArea[i];
                    found = true;
                    break;
                }
            }

            outstanding = outstandingResult;
            return found;
        }

        private static int GetIndexOfFirstPixelToCheck(Vector2 last, Vector2 current)
        {
            /// .: pixel
            /// l: last position
            /// c: current position
            /// f: first pixel for next search

            /// f . .
            /// l c .
            /// . . .

            //Calculate in which direction the last move went and decide over the next first pixel.
            switch ((int)(current.X - last.X))
            {
                case 1:
                    switch ((int)(current.Y - last.Y))
                    {
                        case 1:
                            return 1;

                        case 0:
                            return 0;

                        case -1:
                            return 7;
                    }
                    break;

                case 0:
                    switch ((int)(current.Y - last.Y))
                    {
                        case 1:
                            return 2;

                        case -1:
                            return 6;
                    }
                    break;

                case -1:
                    switch ((int)(current.Y - last.Y))
                    {
                        case 1:
                            return 3;

                        case 0:
                            return 4;

                        case -1:
                            return 5;
                    }
                    break;
            }

            return 0;
        }
        #endregion

        #region DrDeth's Extension

        /// <summary>
        /// Merges two polygons, given that they intersect.
        /// </summary>
        /// <param name="polygon1">The first polygon.</param>
        /// <param name="polygon2">The second polygon.</param>
        /// <param name="error">The error returned from union</param>
        /// <returns>The union of the two polygons, or null if there was an error.</returns>
        public static Vertices Union(Vertices polygon1, Vertices polygon2, out PolyUnionError error)
        {
            Vertices poly1;
            Vertices poly2;
            List<EdgeIntersectInfo> intersections;

            int startingIndex = PreparePolygons(polygon1, polygon2, out poly1, out poly2, out intersections, out error);

            if (startingIndex == -1)
            {
                switch (error)
                {
                    case PolyUnionError.NoIntersections:
                        return null;

                    case PolyUnionError.Poly1InsidePoly2:
                        return polygon2;
                }
            }

            Vertices union = new Vertices();
            Vertices currentPoly = poly1;
            Vertices otherPoly = poly2;

            // Store the starting vertex so we can refer to it later.
            Vector2 startingVertex = poly1[startingIndex];
            int currentIndex = startingIndex;

            do
            {
                // Add the current vertex to the final union
                union.Add(currentPoly[currentIndex]);

                foreach (EdgeIntersectInfo intersect in intersections)
                {
                    // If the current point is an intersection point
                    if (currentPoly[currentIndex] == intersect.IntersectionPoint)
                    {
                        // Make sure we want to swap polygons here.
                        int otherIndex = otherPoly.IndexOf(intersect.IntersectionPoint);

                        // If the next vertex, if we do swap, is not inside the current polygon,
                        // then its safe to swap, otherwise, just carry on with the current poly.
                        if (!PointInPolygonAngle(otherPoly[otherPoly.NextIndex(otherIndex)], currentPoly))
                        {
                            // switch polygons
                            if (currentPoly == poly1)
                            {
                                currentPoly = poly2;
                                otherPoly = poly1;
                            }
                            else
                            {
                                currentPoly = poly1;
                                otherPoly = poly2;
                            }

                            // set currentIndex
                            currentIndex = otherIndex;

                            // Stop checking intersections for this point.
                            break;
                        }
                    }
                }

                // Move to next index
                currentIndex = currentPoly.NextIndex(currentIndex);
            } while ((currentPoly[currentIndex] != startingVertex) && (union.Count <= (poly1.Count + poly2.Count)));


            // If the number of vertices in the union is more than the combined vertices
            // of the input polygons, then something is wrong and the algorithm will
            // loop forever. Luckily, we check for that.
            if (union.Count > (poly1.Count + poly2.Count))
            {
                error = PolyUnionError.InfiniteLoop;
            }

            return union;
        }

        /// <summary>
        /// Subtracts one polygon from another.
        /// </summary>
        /// <param name="polygon1">The base polygon.</param>
        /// <param name="polygon2">The polygon to subtract from the base.</param>
        /// <param name="error">The error.</param>
        /// <returns>
        /// The result of the polygon subtraction, or null if there was an error.
        /// </returns>
        public static Vertices Subtract(Vertices polygon1, Vertices polygon2, out PolyUnionError error)
        {
            Vertices poly1;
            Vertices poly2;
            List<EdgeIntersectInfo> intersections;

            int startingIndex = PreparePolygons(polygon1, polygon2, out poly1, out poly2, out intersections, out error);

            if (startingIndex == -1)
            {
                switch (error)
                {
                    case PolyUnionError.NoIntersections:
                        return null;

                    case PolyUnionError.Poly1InsidePoly2:
                        return null;
                }
            }

            Vertices subtract = new Vertices();
            Vertices currentPoly = poly1;
            Vertices otherPoly = poly2;

            // Store the starting vertex so we can refer to it later.
            Vector2 startingVertex = poly1[startingIndex];
            int currentIndex = startingIndex;

            // Trace direction
            bool forward = true;

            do
            {
                // Add the current vertex to the final union
                subtract.Add(currentPoly[currentIndex]);

                foreach (EdgeIntersectInfo intersect in intersections)
                {
                    // If the current point is an intersection point
                    if (currentPoly[currentIndex] == intersect.IntersectionPoint)
                    {
                        // Make sure we want to swap polygons here.
                        int otherIndex = otherPoly.IndexOf(intersect.IntersectionPoint);

                        Vector2 otherVertex;
                        if (forward)
                        {
                            otherVertex = otherPoly[otherPoly.PreviousIndex(otherIndex)];

                            // If the next vertex, if we do swap, is inside the current polygon,
                            // then its safe to swap, otherwise, just carry on with the current poly.
                            if (PointInPolygonAngle(otherVertex, currentPoly))
                            {
                                // switch polygons
                                if (currentPoly == poly1)
                                {
                                    currentPoly = poly2;
                                    otherPoly = poly1;
                                }
                                else
                                {
                                    currentPoly = poly1;
                                    otherPoly = poly2;
                                }

                                // set currentIndex
                                currentIndex = otherIndex;

                                // Reverse direction
                                forward = !forward;

                                // Stop checking intersections for this point.
                                break;
                            }
                        }
                        else
                        {
                            otherVertex = otherPoly[otherPoly.NextIndex(otherIndex)];

                            // If the next vertex, if we do swap, is outside the current polygon,
                            // then its safe to swap, otherwise, just carry on with the current poly.
                            if (!PointInPolygonAngle(otherVertex, currentPoly))
                            {
                                // switch polygons
                                if (currentPoly == poly1)
                                {
                                    currentPoly = poly2;
                                    otherPoly = poly1;
                                }
                                else
                                {
                                    currentPoly = poly1;
                                    otherPoly = poly2;
                                }

                                // set currentIndex
                                currentIndex = otherIndex;

                                // Reverse direction
                                forward = !forward;

                                // Stop checking intersections for this point.
                                break;
                            }
                        }
                    }
                }

                if (forward)
                {
                    // Move to next index
                    currentIndex = currentPoly.NextIndex(currentIndex);
                }
                else
                {
                    currentIndex = currentPoly.PreviousIndex(currentIndex);
                }
            } while ((currentPoly[currentIndex] != startingVertex) &&
                     (subtract.Count <= (poly1.Count + poly2.Count)));


            // If the number of vertices in the union is more than the combined vertices
            // of the input polygons, then something is wrong and the algorithm will
            // loop forever. Luckily, we check for that.
            if (subtract.Count > (poly1.Count + poly2.Count))
            {
                error = PolyUnionError.InfiniteLoop;
            }

            return subtract;
        }

        /// <summary>
        /// Finds the intersection between two polygons.
        /// </summary>
        /// <param name="polygon1">The first polygon.</param>
        /// <param name="polygon2">The second polygon.</param>
        /// <param name="error">The error.</param>
        /// <returns>
        /// The intersection of the two polygons, or null if there was an error.
        /// </returns>
        public static Vertices Intersect(Vertices polygon1, Vertices polygon2, out PolyUnionError error)
        {
            error = PolyUnionError.None;

            Vertices poly1;
            Vertices poly2;
            List<EdgeIntersectInfo> intersections;

            PolyUnionError gotError;
            int startingIndex = PreparePolygons(polygon1, polygon2, out poly1, out poly2, out intersections, out gotError);

            if (startingIndex == -1)
            {
                switch (gotError)
                {
                    case PolyUnionError.NoIntersections:
                        return null;

                    case PolyUnionError.Poly1InsidePoly2:
                        return polygon2;
                }
            }

            Vertices intersectOut = new Vertices();
            Vertices currentPoly = poly1;
            Vertices otherPoly = poly2;

            // Store the starting vertex so we can refer to it later.            
            int currentIndex = poly1.IndexOf(intersections[0].IntersectionPoint);
            Vector2 startingVertex = poly1[currentIndex];

            do
            {
                // Add the current vertex to the final union
                intersectOut.Add(currentPoly[currentIndex]);

                foreach (EdgeIntersectInfo intersect in intersections)
                {
                    // If the current point is an intersection point
                    if (currentPoly[currentIndex] == intersect.IntersectionPoint)
                    {
                        // Make sure we want to swap polygons here.
                        int otherIndex = otherPoly.IndexOf(intersect.IntersectionPoint);

                        // If the next vertex, if we do swap, is inside the current polygon,
                        // then its safe to swap, otherwise, just carry on with the current poly.
                        if (PointInPolygonAngle(otherPoly[otherPoly.NextIndex(otherIndex)], currentPoly))
                        {
                            // switch polygons
                            if (currentPoly == poly1)
                            {
                                currentPoly = poly2;
                                otherPoly = poly1;
                            }
                            else
                            {
                                currentPoly = poly1;
                                otherPoly = poly2;
                            }

                            // set currentIndex
                            currentIndex = otherIndex;

                            // Stop checking intersections for this point.
                            break;
                        }
                    }
                }

                // Move to next index
                currentIndex = currentPoly.NextIndex(currentIndex);
            } while ((currentPoly[currentIndex] != startingVertex) &&
                     (intersectOut.Count <= (poly1.Count + poly2.Count)));


            // If the number of vertices in the union is more than the combined vertices
            // of the input polygons, then something is wrong and the algorithm will
            // loop forever. Luckily, we check for that.
            if (intersectOut.Count > (poly1.Count + poly2.Count))
            {
                error = PolyUnionError.InfiniteLoop;
            }

            return intersectOut;
        }

        /// <summary>
        /// Prepares the polygons.
        /// </summary>
        /// <param name="polygon1">The polygon1.</param>
        /// <param name="polygon2">The polygon2.</param>
        /// <param name="poly1">The poly1.</param>
        /// <param name="poly2">The poly2.</param>
        /// <param name="intersections">The intersections.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        private static int PreparePolygons(Vertices polygon1, Vertices polygon2, out Vertices poly1, out Vertices poly2,
                                    out List<EdgeIntersectInfo> intersections, out PolyUnionError error)
        {
            error = PolyUnionError.None;

            // Make a copy of the polygons so that we dont modify the originals, and
            // force vertices to integer (pixel) values.
            poly1 = Round(polygon1);

            poly2 = Round(polygon2);

            // Find intersection points
            intersections = new List<EdgeIntersectInfo>();
            if (!VerticesIntersect(poly1, poly2, ref intersections))
            {
                // No intersections found - polygons do not overlap.
                error = PolyUnionError.NoIntersections;
                return -1;
            }

            // Add intersection points to original polygons, ignoring existing points.
            foreach (EdgeIntersectInfo intersect in intersections)
            {
                if (!poly1.Contains(intersect.IntersectionPoint))
                {
                    poly1.Insert(poly1.IndexOf(intersect.EdgeOne.EdgeStart) + 1, intersect.IntersectionPoint);
                }

                if (!poly2.Contains(intersect.IntersectionPoint))
                {
                    poly2.Insert(poly2.IndexOf(intersect.EdgeTwo.EdgeStart) + 1, intersect.IntersectionPoint);
                }
            }

            // Find starting point on the edge of polygon1 
            // that is outside of the intersected area
            // to begin polygon trace.
            int startingIndex = -1;
            int currentIndex = 0;
            do
            {
                if (!PointInPolygonAngle(poly1[currentIndex], poly2))
                {
                    startingIndex = currentIndex;
                    break;
                }
                currentIndex = poly1.NextIndex(currentIndex);
            } while (currentIndex != 0);

            // If we dont find a point on polygon1 thats outside of the
            // intersect area, the polygon1 must be inside of polygon2,
            // in which case, polygon2 IS the union of the two.
            if (startingIndex == -1)
            {
                error = PolyUnionError.Poly1InsidePoly2;
            }

            return startingIndex;
        }

        /// <summary>
        /// Check and return polygon intersections
        /// </summary>
        /// <param name="polygon1"></param>
        /// <param name="polygon2"></param>
        /// <param name="intersections"></param>
        /// <returns></returns>
        private static bool VerticesIntersect(Vertices polygon1, Vertices polygon2,
                                       ref List<EdgeIntersectInfo> intersections)
        {
            // Make sure the output is clear before we start.
            intersections.Clear();

            // Iterate through polygon1's edges
            for (int i = 0; i < polygon1.Count; i++)
            {
                // Get edge vertices
                Vector2 p1 = polygon1[i];
                Vector2 p2 = polygon1[polygon1.NextIndex(i)];

                // Get intersections between this edge and polygon2
                for (int j = 0; j < polygon2.Count; j++)
                {
                    Vector2 point;

                    Vector2 p3 = polygon2[j];
                    Vector2 p4 = polygon2[polygon2.NextIndex(j)];

                    // _defaultFloatTolerance = .00001f (Perhaps this should be made available publically from RayHelper?

                    // Check if the edges intersect
                    if (RayHelper.LineIntersect(p1, p2, p3, p4, true, true, 0.00001f, out point))
                    {
                        // Here, we round the returned intersection point to its nearest whole number.
                        // This prevents floating point anomolies where 99.9999-> is returned instead of 100.
                        point = new Vector2((float)Math.Round(point.X, 0), (float)Math.Round(point.Y, 0));
                        // Record the intersection
                        intersections.Add(new EdgeIntersectInfo(new Edge(p1, p2), new Edge(p3, p4), point));
                    }
                }
            }

            // true if any intersections were found.
            return (intersections.Count > 0);
        }

        /// <summary>
        /// * ref: http://ozviz.wasp.uwa.edu.au/~pbourke/geometry/insidepoly/  - Solution 2 
        /// * Compute the sum of the angles made between the test point and each pair of points making up the polygon. 
        /// * If this sum is 2pi then the point is an interior point, if 0 then the point is an exterior point. 
        /// </summary>
        private static bool PointInPolygonAngle(Vector2 point, Vertices polygon)
        {
            double angle = 0;

            // Iterate through polygon's edges
            for (int i = 0; i < polygon.Count; i++)
            {
                /*
                p1.h = polygon[i].h - p.h;
                p1.v = polygon[i].v - p.v;
                p2.h = polygon[(i + 1) % n].h - p.h;
                p2.v = polygon[(i + 1) % n].v - p.v;
                */
                // Get points
                Vector2 p1 = polygon[i] - point;
                Vector2 p2 = polygon[polygon.NextIndex(i)] - point;

                angle += VectorAngle(p1, p2);
            }

            if (Math.Abs(angle) < Math.PI)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Return the angle between two vectors on a plane
        /// The angle is from vector 1 to vector 2, positive anticlockwise
        /// The result is between -pi -> pi
        /// </summary>
        private static double VectorAngle(Vector2 p1, Vector2 p2)
        {
            double theta1 = Math.Atan2(p1.Y, p1.X);
            double theta2 = Math.Atan2(p2.Y, p2.X);
            double dtheta = theta2 - theta1;
            while (dtheta > Math.PI)
                dtheta -= (2 * Math.PI);
            while (dtheta < -Math.PI)
                dtheta += (2 * Math.PI);

            return (dtheta);
        }

        /// <summary>
        /// Rounds vertices X and Y values to whole numbers.
        /// </summary>
        /// <param name="polygon">The polygon whose vertices should be rounded.</param>
        /// <returns>A new polygon with rounded vertices.</returns>
        public static Vertices Round(Vertices polygon)
        {
            Vertices returnPoly = new Vertices();
            for (int i = 0; i < polygon.Count; i++)
                returnPoly.Add(new Vector2((float)Math.Round(polygon[i].X, 0), (float)Math.Round(polygon[i].Y, 0)));

            return returnPoly;
        }

        /// <summary>
        /// Determines if three vertices are collinear (ie. on a straight line)
        /// </summary>
        /// <param name="p1">Vertex 1</param>
        /// <param name="p2">Vertex 2</param>
        /// <param name="p3">Vertex 3</param>
        /// <returns></returns>
        private static bool VerticesAreCollinear(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            double collinearity = (p3.X - p1.X) * (p2.Y - p1.Y) + (p3.Y - p1.Y) * (p1.X - p2.X);
            return (collinearity == 0);
        }

        /// <summary>
        /// Simple polygon simplification.
        /// </summary>
        /// <param name="polygon">The polygon that needs simplification.</param>
        /// <param name="bias">The distance bias (in pixels) between points. Points closer than this will be 'joined'.</param>
        /// <returns>A simplified polygon.</returns>
        public static Vertices Simplify(Vertices polygon, int bias)
        {
            //We can't simplify polygons under 3 vertices
            if (polygon.Count < 3)
                return polygon;

            Vertices simplified = new Vertices();
            Vertices roundPolygon = Round(polygon);

            for (int curr = 0; curr < roundPolygon.Count; curr++)
            {
                int prev = roundPolygon.PreviousIndex(curr);
                int next = roundPolygon.NextIndex(curr);

                if ((roundPolygon[prev] - roundPolygon[curr]).Length() <= bias)
                    continue;

                if (!VerticesAreCollinear(roundPolygon[prev], roundPolygon[curr], roundPolygon[next]))
                    simplified.Add(roundPolygon[curr]);
            }

            return simplified;
        }

        /// <summary>
        /// Simple polygon simplification.
        /// </summary>
        /// <param name="polygon">The polygon that needs simplification.</param>
        /// <returns>A simplified polygon.</returns>
        public static Vertices Simplify(Vertices polygon)
        {
            return Simplify(polygon, 0);
        }

        #endregion

        #region Yobiv's Extension
        /// <summary>
        /// Creates an capsule with the specified height, radius and number of edges.
        /// A capsule has the same form as a pill capsule.
        /// </summary>
        /// <param name="height">Height (inner height + 2 * radius) of the capsule.</param>
        /// <param name="endRadius">Radius of the capsule ends.</param>
        /// <param name="edges">The number of edges of the capsule ends. The more edges, the more it resembles an capsule</param>
        /// <returns></returns>
        public static Vertices CreateCapsule(float height, float endRadius, int edges)
        {
            if (endRadius >= height / 2)
                throw new ArgumentException("The radius must be lower than height / 2. Higher values of radius would create a circle, and not a half circle.", "endRadius");

            return CreateCapsule(height, endRadius, edges, endRadius, edges);
        }

        /// <summary>
        /// Creates an capsule with the specified  height, radius and number of edges.
        /// A capsule has the same form as a pill capsule.
        /// </summary>
        /// <param name="height">Height (inner height + radii) of the capsule.</param>
        /// <param name="topRadius">Radius of the top.</param>
        /// <param name="topEdges">The number of edges of the top. The more edges, the more it resembles an capsule</param>
        /// <param name="bottomRadius">Radius of bottom.</param>
        /// <param name="bottomEdges">The number of edges of the bottom. The more edges, the more it resembles an capsule</param>
        /// <returns></returns>
        public static Vertices CreateCapsule(float height, float topRadius, int topEdges, float bottomRadius, int bottomEdges)
        {
            if (height <= 0)
                throw new ArgumentException("Height must be longer than 0", "height");

            if (topRadius <= 0)
                throw new ArgumentException("The top radius must be more than 0", "topRadius");

            if (topEdges <= 0)
                throw new ArgumentException("Top edges must be more than 0", "topEdges");

            if (bottomRadius <= 0)
                throw new ArgumentException("The bottom radius must be more than 0", "bottomRadius");

            if (bottomEdges <= 0)
                throw new ArgumentException("Bottom edges must be more than 0", "bottomEdges");

            if (topRadius >= height / 2)
                throw new ArgumentException("The top radius must be lower than height / 2. Higher values of top radius would create a circle, and not a half circle.", "topRadius");

            if (bottomRadius >= height / 2)
                throw new ArgumentException("The bottom radius must be lower than height / 2. Higher values of bottom radius would create a circle, and not a half circle.", "bottomRadius");

            Vertices vertices = new Vertices();

            float newHeight = (height - topRadius - bottomRadius) * 0.5f;

            // top
            vertices.Add(new Vector2(topRadius, newHeight));

            float stepSize = MathHelper.Pi / topEdges;
            for (int i = 1; i < topEdges; i++)
            {
                vertices.Add(new Vector2(topRadius * Calculator.Cos(stepSize * i), topRadius * Calculator.Sin(stepSize * i) + newHeight));
            }

            vertices.Add(new Vector2(-topRadius, newHeight));

            // bottom
            vertices.Add(new Vector2(-bottomRadius, -newHeight));

            stepSize = MathHelper.Pi / bottomEdges;
            for (int i = 1; i < bottomEdges; i++)
            {
                vertices.Add(new Vector2(-bottomRadius * Calculator.Cos(stepSize * i), -bottomRadius * Calculator.Sin(stepSize * i) - newHeight));
            }

            vertices.Add(new Vector2(bottomRadius, -newHeight));

            return vertices;
        }
        #endregion

        #region Matt Bettcher's Extension

        #region Fields

        static readonly IndexableCyclicalLinkedList<Vertex> polygonVertices = new IndexableCyclicalLinkedList<Vertex>();
        static readonly IndexableCyclicalLinkedList<Vertex> earVertices = new IndexableCyclicalLinkedList<Vertex>();
        static readonly CyclicalList<Vertex> convexVertices = new CyclicalList<Vertex>();
        static readonly CyclicalList<Vertex> reflexVertices = new CyclicalList<Vertex>();

        #endregion

        #region Vertex

        struct Vertex
        {
            public readonly Vector2 Position;
            public readonly short Index;

            public Vertex(Vector2 position, short index)
            {
                Position = position;
                Index = index;
            }

            public override bool Equals(object obj)
            {
                if (obj.GetType() != typeof(Vertex))
                    return false;
                return Equals((Vertex)obj);
            }

            public bool Equals(Vertex obj)
            {
                return obj.Position.Equals(Position) && obj.Index == Index;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Position.GetHashCode() * 397) ^ Index;
                }
            }

            public override string ToString()
            {
                return string.Format("{0} ({1})", Position, Index);
            }
        }
        #endregion

        #region LineSegment

        struct LineSegment
        {
            public Vertex A;
            public Vertex B;

            public LineSegment(Vertex a, Vertex b)
            {
                A = a;
                B = b;
            }

            public float? IntersectsWithRay(Vector2 origin, Vector2 direction)
            {
                float largestDistance = MathHelper.Max(A.Position.X - origin.X, B.Position.X - origin.X) * 2f;
                LineSegment raySegment = new LineSegment(new Vertex(origin, 0), new Vertex(origin + (direction * largestDistance), 0));

                Vector2? intersection = FindIntersection(this, raySegment);
                float? value = null;

                if (intersection != null)
                    value = Vector2.Distance(origin, intersection.Value);

                return value;
            }

            public static Vector2? FindIntersection(LineSegment a, LineSegment b)
            {
                float x1 = a.A.Position.X;
                float y1 = a.A.Position.Y;
                float x2 = a.B.Position.X;
                float y2 = a.B.Position.Y;
                float x3 = b.A.Position.X;
                float y3 = b.A.Position.Y;
                float x4 = b.B.Position.X;
                float y4 = b.B.Position.Y;

                float denom = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);

                float uaNum = (x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3);
                float ubNum = (x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3);

                float ua = uaNum / denom;
                float ub = ubNum / denom;

                if (MathHelper.Clamp(ua, 0f, 1f) != ua || MathHelper.Clamp(ub, 0f, 1f) != ub)
                    return null;

                return a.A.Position + (a.B.Position - a.A.Position) * ua;
            }
        }

        #endregion

        #region Triangle

        /// <summary>
        /// A basic triangle structure that holds the three vertices that make up a given triangle.
        /// </summary>
        struct Triangle
        {
            public readonly Vertex A;
            public readonly Vertex B;
            public readonly Vertex C;

            public Triangle(Vertex a, Vertex b, Vertex c)
            {
                A = a;
                B = b;
                C = c;
            }

            public bool ContainsPoint(Vertex point)
            {
                //return true if the point to test is one of the vertices
                if (point.Equals(A) || point.Equals(B) || point.Equals(C))
                    return true;

                bool oddNodes = false;

                if (checkPointToSegment(C, A, point))
                    oddNodes = !oddNodes;
                if (checkPointToSegment(A, B, point))
                    oddNodes = !oddNodes;
                if (checkPointToSegment(B, C, point))
                    oddNodes = !oddNodes;

                return oddNodes;
            }

            public static bool ContainsPoint(Vertex a, Vertex b, Vertex c, Vertex point)
            {
                return new Triangle(a, b, c).ContainsPoint(point);
            }

            static bool checkPointToSegment(Vertex sA, Vertex sB, Vertex point)
            {
                if ((sA.Position.Y < point.Position.Y && sB.Position.Y >= point.Position.Y) ||
                    (sB.Position.Y < point.Position.Y && sA.Position.Y >= point.Position.Y))
                {
                    float x =
                        sA.Position.X +
                        (point.Position.Y - sA.Position.Y) /
                        (sB.Position.Y - sA.Position.Y) *
                        (sB.Position.X - sA.Position.X);

                    if (x < point.Position.X)
                        return true;
                }

                return false;
            }

            public override bool Equals(object obj)
            {
                if (obj.GetType() != typeof(Triangle))
                    return false;
                return Equals((Triangle)obj);
            }

            public bool Equals(Triangle obj)
            {
                return obj.A.Equals(A) && obj.B.Equals(B) && obj.C.Equals(C);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int result = A.GetHashCode();
                    result = (result * 397) ^ B.GetHashCode();
                    result = (result * 397) ^ C.GetHashCode();
                    return result;
                }
            }
        }
        #endregion

        #region CyclicalList
        /// <summary>
        /// Implements a List structure as a cyclical list where indices are wrapped.
        /// </summary>
        /// <typeparam name="T">The Type to hold in the list.</typeparam>
        class CyclicalList<T> : List<T>
        {
            public new T this[int index]
            {
                get
                {
                    //perform the index wrapping
                    while (index < 0)
                        index = Count + index;
                    if (index >= Count)
                        index %= Count;

                    return base[index];
                }
            }
        }
        #endregion

        #region IndexableCyclicalLinkedList
        /// <summary>
        /// Implements a LinkedList that is both indexable as well as cyclical. Thus
        /// indexing into the list with an out-of-bounds index will automatically cycle
        /// around the list to find a valid node.
        /// </summary>
        class IndexableCyclicalLinkedList<T> : LinkedList<T>
        {
            /// <summary>
            /// Gets the LinkedListNode at a particular index.
            /// </summary>
            /// <param name="index">The index of the node to retrieve.</param>
            /// <returns>The LinkedListNode found at the index given.</returns>
            public LinkedListNode<T> this[int index]
            {
                get
                {
                    //perform the index wrapping
                    while (index < 0)
                        index = Count + index;
                    if (index >= Count)
                        index %= Count;

                    //find the proper node
                    LinkedListNode<T> node = First;
                    for (int i = 0; i < index; i++)
                        node = node.Next;

                    return node;
                }
            }

            /// <summary>
            /// Removes the node at a given index.
            /// </summary>
            /// <param name="index">The index of the node to remove.</param>
            public void RemoveAt(int index)
            {
                Remove(this[index]);
            }

            /// <summary>
            /// Finds the index of a given item.
            /// </summary>
            /// <param name="item">The item to find.</param>
            /// <returns>The index of the item if found; -1 if the item is not found.</returns>
            public int IndexOf(T item)
            {
                for (int i = 0; i < Count; i++)
                    if (this[i].Value.Equals(item))
                        return i;

                return -1;
            }
        }
        #endregion

        #region Public Methods

        #region Triangulate

        /// <summary>
        /// Triangulates a 2D polygon produced the indexes required to render the points as a triangle list.
        /// </summary>
        /// <param name="inputVertices">The polygon vertices in counter-clockwise winding order.</param>
        /// <param name="desiredWindingOrder">The desired output winding order.</param>
        /// <param name="outputVertices">The resulting vertices that include any reversals of winding order and holes.</param>
        /// <param name="indices">The resulting indices for rendering the shape as a triangle list.</param>
        public static void Triangulate(
            Vector2[] inputVertices,
            WindingOrder desiredWindingOrder,
            out Vector2[] outputVertices,
            out short[] indices)
        {
            //Log("\nBeginning triangulation...");

            List<Triangle> triangles = new List<Triangle>();

            //make sure we have our vertices wound properly
            if (DetermineWindingOrder(inputVertices) == WindingOrder.Clockwise)
                outputVertices = ReverseWindingOrder(inputVertices);
            else
                outputVertices = (Vector2[])inputVertices.Clone();

            //clear all of the lists
            polygonVertices.Clear();
            earVertices.Clear();
            convexVertices.Clear();
            reflexVertices.Clear();

            //generate the cyclical list of vertices in the polygon
            for (int i = 0; i < outputVertices.Length; i++)
                polygonVertices.AddLast(new Vertex(outputVertices[i], (short)i));

            //categorize all of the vertices as convex, reflex, and ear
            FindConvexAndReflexVertices();
            FindEarVertices();

            //clip all the ear vertices
            while (polygonVertices.Count > 3 && earVertices.Count > 0)
                ClipNextEar(triangles);

            //if there are still three points, use that for the last triangle
            if (polygonVertices.Count == 3)
                triangles.Add(new Triangle(
                    polygonVertices[0].Value,
                    polygonVertices[1].Value,
                    polygonVertices[2].Value));

            //add all of the triangle indices to the output array
            indices = new short[triangles.Count * 3];

            //move the if statement out of the loop to prevent all the
            //redundant comparisons
            if (desiredWindingOrder == WindingOrder.CounterClockwise)
            {
                for (int i = 0; i < triangles.Count; i++)
                {
                    indices[(i * 3)] = triangles[i].A.Index;
                    indices[(i * 3) + 1] = triangles[i].B.Index;
                    indices[(i * 3) + 2] = triangles[i].C.Index;
                }
            }
            else
            {
                for (int i = 0; i < triangles.Count; i++)
                {
                    indices[(i * 3)] = triangles[i].C.Index;
                    indices[(i * 3) + 1] = triangles[i].B.Index;
                    indices[(i * 3) + 2] = triangles[i].A.Index;
                }
            }
        }

        #endregion

        #region CutHoleInShape

        /// <summary>
        /// Cuts a hole into a shape.
        /// </summary>
        /// <param name="shapeVerts">An array of vertices for the primary shape.</param>
        /// <param name="holeVerts">An array of vertices for the hole to be cut. It is assumed that these vertices lie completely within the shape verts.</param>
        /// <returns>The new array of vertices that can be passed to Triangulate to properly triangulate the shape with the hole.</returns>
        public static Vector2[] CutHoleInShape(Vector2[] shapeVerts, Vector2[] holeVerts)
        {
            Log("\nCutting hole into shape...");

            //make sure the shape vertices are wound counter clockwise and the hole vertices clockwise
            shapeVerts = EnsureWindingOrder(shapeVerts, WindingOrder.CounterClockwise);
            holeVerts = EnsureWindingOrder(holeVerts, WindingOrder.Clockwise);

            //clear all of the lists
            polygonVertices.Clear();
            earVertices.Clear();
            convexVertices.Clear();
            reflexVertices.Clear();

            //generate the cyclical list of vertices in the polygon
            for (int i = 0; i < shapeVerts.Length; i++)
                polygonVertices.AddLast(new Vertex(shapeVerts[i], (short)i));

            CyclicalList<Vertex> holePolygon = new CyclicalList<Vertex>();
            for (int i = 0; i < holeVerts.Length; i++)
                holePolygon.Add(new Vertex(holeVerts[i], (short)(i + polygonVertices.Count)));

#if DEBUG
            StringBuilder vString = new StringBuilder();
            foreach (Vertex v in polygonVertices)
                vString.Append(string.Format("{0}, ", v));
            Log("Shape Vertices: {0}", vString);

            vString = new StringBuilder();
            foreach (Vertex v in holePolygon)
                vString.Append(string.Format("{0}, ", v));
            Log("Hole Vertices: {0}", vString);
#endif

            FindConvexAndReflexVertices();
            FindEarVertices();

            //find the hole vertex with the largest X value
            Vertex rightMostHoleVertex = holePolygon[0];
            foreach (Vertex v in holePolygon)
                if (v.Position.X > rightMostHoleVertex.Position.X)
                    rightMostHoleVertex = v;

            //construct a list of all line segments where at least one vertex
            //is to the right of the rightmost hole vertex with one vertex
            //above the hole vertex and one below
            List<LineSegment> segmentsToTest = new List<LineSegment>();
            for (int i = 0; i < polygonVertices.Count; i++)
            {
                Vertex a = polygonVertices[i].Value;
                Vertex b = polygonVertices[i + 1].Value;

                if ((a.Position.X > rightMostHoleVertex.Position.X || b.Position.X > rightMostHoleVertex.Position.X) &&
                    ((a.Position.Y >= rightMostHoleVertex.Position.Y && b.Position.Y <= rightMostHoleVertex.Position.Y) ||
                    (a.Position.Y <= rightMostHoleVertex.Position.Y && b.Position.Y >= rightMostHoleVertex.Position.Y)))
                    segmentsToTest.Add(new LineSegment(a, b));
            }

            //now we try to find the closest intersection point heading to the right from
            //our hole vertex.
            float? closestPoint = null;
            LineSegment closestSegment = new LineSegment();
            foreach (LineSegment segment in segmentsToTest)
            {
                float? intersection = segment.IntersectsWithRay(rightMostHoleVertex.Position, Vector2.UnitX);
                if (intersection != null)
                {
                    if (closestPoint == null || closestPoint.Value > intersection.Value)
                    {
                        closestPoint = intersection;
                        closestSegment = segment;
                    }
                }
            }

            //if closestPoint is null, there were no collisions (likely from improper input data),
            //but we'll just return without doing anything else
            if (closestPoint == null)
                return shapeVerts;

            //otherwise we can find our mutually visible vertex to split the polygon
            Vector2 I = rightMostHoleVertex.Position + Vector2.UnitX * closestPoint.Value;
            Vertex P = (closestSegment.A.Position.X > closestSegment.B.Position.X)
                ? closestSegment.A
                : closestSegment.B;

            //construct triangle MIP
            Triangle mip = new Triangle(rightMostHoleVertex, new Vertex(I, 1), P);

            //see if any of the reflex vertices lie inside of the MIP triangle
            List<Vertex> interiorReflexVertices = new List<Vertex>();
            foreach (Vertex v in reflexVertices)
                if (mip.ContainsPoint(v))
                    interiorReflexVertices.Add(v);

            //if there are any interior reflex vertices, find the one that, when connected
            //to our rightMostHoleVertex, forms the line closest to Vector2.UnitX
            if (interiorReflexVertices.Count > 0)
            {
                float closestDot = -1f;
                foreach (Vertex v in interiorReflexVertices)
                {
                    //compute the dot product of the vector against the UnitX
                    Vector2 d = Vector2.Normalize(v.Position - rightMostHoleVertex.Position);
                    float dot = Vector2.Dot(Vector2.UnitX, d);

                    //if this line is the closest we've found
                    if (dot > closestDot)
                    {
                        //save the value and save the vertex as P
                        closestDot = dot;
                        P = v;
                    }
                }
            }

            //now we just form our output array by injecting the hole vertices into place
            //we know we have to inject the hole into the main array after point P going from
            //rightMostHoleVertex around and then back to P.
            int mIndex = holePolygon.IndexOf(rightMostHoleVertex);
            int injectPoint = polygonVertices.IndexOf(P);

            Log("Inserting hole at injection point {0} starting at hole vertex {1}.",
                P,
                rightMostHoleVertex);
            for (int i = mIndex; i <= mIndex + holePolygon.Count; i++)
            {
                Log("Inserting vertex {0} after vertex {1}.", holePolygon[i], polygonVertices[injectPoint].Value);
                polygonVertices.AddAfter(polygonVertices[injectPoint++], holePolygon[i]);
            }
            polygonVertices.AddAfter(polygonVertices[injectPoint], P);

#if DEBUG
            vString = new StringBuilder();
            foreach (Vertex v in polygonVertices)
                vString.Append(string.Format("{0}, ", v));
            Log("New Shape Vertices: {0}\n", vString);
#endif

            //finally we write out the new polygon vertices and return them out
            Vector2[] newShapeVerts = new Vector2[polygonVertices.Count];
            for (int i = 0; i < polygonVertices.Count; i++)
                newShapeVerts[i] = polygonVertices[i].Value.Position;

            return newShapeVerts;
        }

        #endregion

        #region EnsureWindingOrder

        /// <summary>
        /// Ensures that a set of vertices are wound in a particular order, reversing them if necessary.
        /// </summary>
        /// <param name="vertices">The vertices of the polygon.</param>
        /// <param name="windingOrder">The desired winding order.</param>
        /// <returns>A new set of vertices if the winding order didn't match; otherwise the original set.</returns>
        public static Vector2[] EnsureWindingOrder(Vector2[] vertices, WindingOrder windingOrder)
        {
            //Log("\nEnsuring winding order of {0}...", windingOrder);
            if (DetermineWindingOrder(vertices) != windingOrder)
            {
                //Log("Reversing vertices...");
                return ReverseWindingOrder(vertices);
            }

            //Log("No reversal needed.");
            return vertices;
        }

        #endregion

        #region ReverseWindingOrder

        /// <summary>
        /// Reverses the winding order for a set of vertices.
        /// </summary>
        /// <param name="vertices">The vertices of the polygon.</param>
        /// <returns>The new vertices for the polygon with the opposite winding order.</returns>
        public static Vector2[] ReverseWindingOrder(Vector2[] vertices)
        {
            //Log("\nReversing winding order...");
            Vector2[] newVerts = new Vector2[vertices.Length];

#if DEBUG
            //StringBuilder vString = new StringBuilder();
            //foreach (Vector2 v in vertices)
            //	vString.Append(string.Format("{0}, ", v));
            //Log("Original Vertices: {0}", vString);
#endif

            newVerts[0] = vertices[0];
            for (int i = 1; i < newVerts.Length; i++)
                newVerts[i] = vertices[vertices.Length - i];

#if DEBUG
            //vString = new StringBuilder();
            //foreach (Vector2 v in newVerts)
            //	vString.Append(string.Format("{0}, ", v));
            //Log("New Vertices After Reversal: {0}\n", vString);
#endif

            return newVerts;
        }

        #endregion

        #region DetermineWindingOrder

        /// <summary>
        /// Determines the winding order of a polygon given a set of vertices.
        /// </summary>
        /// <param name="vertices">The vertices of the polygon.</param>
        /// <returns>The calculated winding order of the polygon.</returns>
        public static WindingOrder DetermineWindingOrder(Vector2[] vertices)
        {
            int clockWiseCount = 0;
            int counterClockWiseCount = 0;
            Vector2 p1 = vertices[0];

            for (int i = 1; i < vertices.Length; i++)
            {
                Vector2 p2 = vertices[i];
                Vector2 p3 = vertices[(i + 1) % vertices.Length];

                Vector2 e1 = p1 - p2;
                Vector2 e2 = p3 - p2;

                if (e1.X * e2.Y - e1.Y * e2.X >= 0)
                    clockWiseCount++;
                else
                    counterClockWiseCount++;

                p1 = p2;
            }

            return (clockWiseCount > counterClockWiseCount)
                ? WindingOrder.Clockwise
                : WindingOrder.CounterClockwise;
        }

        #endregion

        #endregion

        #region Private Methods

        #region ClipNextEar

        private static void ClipNextEar(ICollection<Triangle> triangles)
        {
            //find the triangle
            Vertex ear = earVertices[0].Value;
            Vertex prev = polygonVertices[polygonVertices.IndexOf(ear) - 1].Value;
            Vertex next = polygonVertices[polygonVertices.IndexOf(ear) + 1].Value;
            triangles.Add(new Triangle(ear, next, prev));

            //remove the ear from the shape
            earVertices.RemoveAt(0);
            polygonVertices.RemoveAt(polygonVertices.IndexOf(ear));
            //Log("\nRemoved Ear: {0}", ear);

            //validate the neighboring vertices
            ValidateAdjacentVertex(prev);
            ValidateAdjacentVertex(next);

            //write out the states of each of the lists
#if DEBUG
            /*StringBuilder rString = new StringBuilder();
            foreach (Vertex v in reflexVertices)
                rString.Append(string.Format("{0}, ", v.Index));
            Log("Reflex Vertices: {0}", rString);

            StringBuilder cString = new StringBuilder();
            foreach (Vertex v in convexVertices)
                cString.Append(string.Format("{0}, ", v.Index));
            Log("Convex Vertices: {0}", cString);

            StringBuilder eString = new StringBuilder();
            foreach (Vertex v in earVertices)
                eString.Append(string.Format("{0}, ", v.Index));
            Log("Ear Vertices: {0}", eString);*/
#endif
        }

        #endregion

        #region ValidateAdjacentVertex

        private static void ValidateAdjacentVertex(Vertex vertex)
        {
            //Log("Validating: {0}...", vertex);

            if (reflexVertices.Contains(vertex))
            {
                if (IsConvex(vertex))
                {
                    reflexVertices.Remove(vertex);
                    convexVertices.Add(vertex);
                    //Log("Vertex: {0} now convex", vertex);
                }
                else
                {
                    //Log("Vertex: {0} still reflex", vertex);
                }
            }

            if (convexVertices.Contains(vertex))
            {
                bool wasEar = earVertices.Contains(vertex);
                bool isEar = IsEar(vertex);

                if (wasEar && !isEar)
                {
                    earVertices.Remove(vertex);
                    //Log("Vertex: {0} no longer ear", vertex);
                }
                else if (!wasEar && isEar)
                {
                    earVertices.AddFirst(vertex);
                    //Log("Vertex: {0} now ear", vertex);
                }
                else
                {
                    //Log("Vertex: {0} still ear", vertex);
                }
            }
        }

        #endregion

        #region FindConvexAndReflexVertices

        private static void FindConvexAndReflexVertices()
        {
            for (int i = 0; i < polygonVertices.Count; i++)
            {
                Vertex v = polygonVertices[i].Value;

                if (IsConvex(v))
                {
                    convexVertices.Add(v);
                    //Log("Convex: {0}", v);
                }
                else
                {
                    reflexVertices.Add(v);
                    //Log("Reflex: {0}", v);
                }
            }
        }

        #endregion

        #region FindEarVertices

        private static void FindEarVertices()
        {
            for (int i = 0; i < convexVertices.Count; i++)
            {
                Vertex c = convexVertices[i];

                if (IsEar(c))
                {
                    earVertices.AddLast(c);
                    //Log("Ear: {0}", c);
                }
            }
        }

        #endregion

        #region IsEar

        private static bool IsEar(Vertex c)
        {
            Vertex p = polygonVertices[polygonVertices.IndexOf(c) - 1].Value;
            Vertex n = polygonVertices[polygonVertices.IndexOf(c) + 1].Value;

            //Log("Testing vertex {0} as ear with triangle {1}, {0}, {2}...", c, p, n);

            foreach (Vertex t in reflexVertices)
            {
                if (t.Equals(p) || t.Equals(c) || t.Equals(n))
                    continue;

                if (Triangle.ContainsPoint(p, c, n, t))
                {
                    //Log("\tTriangle contains vertex {0}...", t);
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region IsConvex

        private static bool IsConvex(Vertex c)
        {
            Vertex p = polygonVertices[polygonVertices.IndexOf(c) - 1].Value;
            Vertex n = polygonVertices[polygonVertices.IndexOf(c) + 1].Value;

            Vector2 d1 = Vector2.Normalize(c.Position - p.Position);
            Vector2 d2 = Vector2.Normalize(n.Position - c.Position);
            Vector2 n2 = new Vector2(-d2.Y, d2.X);

            return (Vector2.Dot(d1, n2) <= 0f);
        }

        #endregion

        #region IsReflex

        private static bool IsReflex(Vertex c)
        {
            return !IsConvex(c);
        }

        #endregion

        #region Log

        //[Conditional("DEBUG")]
        private static void Log(string format, params object[] parameters)
        {
            Console.WriteLine(format, parameters);
        }

        #endregion

        #endregion

        #region WindingOrder

        /// <summary>
        /// Specifies a desired winding order for the shape vertices.
        /// </summary>
        public enum WindingOrder
        {
            Clockwise,
            CounterClockwise
        }

        #endregion

        #endregion

        #region SAT Extensions

        /// <summary>
        /// Decomposes a set of vertices into a set of Geoms all
        /// attached to one body.
        /// </summary>
        /// <param name="vertices">Vertices to decompose.</param>
        /// <param name="body">Body to attach too.</param>
        /// <param name="maxPolysToFind">Maximum Geoms to return.</param>
        /// <returns>A list of Geoms.</returns>
        public static List<Geom> DecomposeGeom(Vertices vertices, Body body, int maxPolysToFind)
        {
            Vertices[] verts = Polygon.DecomposeVertices(vertices, maxPolysToFind);

            List<Geom> geomList = new List<Geom>();

            Vector2 mainCentroid = vertices.GetCentroid();

            foreach (Vertices v in verts)
            {
                //Vector2 subCentroid = v.GetCentroid();
                geomList.Add(new Geom(body, v, -mainCentroid, 0, 1.0f));
            }

            return geomList;
        }

        #endregion

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

    #region Sickbattery's Extension - Enums & Classes
    public enum EdgeAlignment
    {
        Vertical = 0,
        Horizontal = 1
    }

    public class CrossingEdgeInfo : IComparable
    {
        #region Attributes
        private Vector2 _egdeVertex1;
        private Vector2 _edgeVertex2;

        private EdgeAlignment _alignment;
        private Vector2 _crossingPoint;
        #endregion

        #region Properties
        public Vector2 EdgeVertex1
        {
            get { return _egdeVertex1; }
            set { _egdeVertex1 = value; }
        }

        public Vector2 EdgeVertex2
        {
            get { return _edgeVertex2; }
            set { _edgeVertex2 = value; }
        }

        public EdgeAlignment CheckLineAlignment
        {
            get { return _alignment; }
            set { _alignment = value; }
        }

        public Vector2 CrossingPoint
        {
            get { return _crossingPoint; }
            set { _crossingPoint = value; }
        }
        #endregion

        #region Constructor
        public CrossingEdgeInfo(Vector2 edgeVertex1, Vector2 edgeVertex2, Vector2 crossingPoint, EdgeAlignment checkLineAlignment)
        {
            _egdeVertex1 = edgeVertex1;
            _edgeVertex2 = edgeVertex2;

            _alignment = checkLineAlignment;
            _crossingPoint = crossingPoint;
        }
        #endregion

        #region IComparable Member
        public int CompareTo(object obj)
        {
            CrossingEdgeInfo cei = (CrossingEdgeInfo)obj;
            int result = 0;

            switch (_alignment)
            {
                case EdgeAlignment.Vertical:
                    if (_crossingPoint.X < cei.CrossingPoint.X)
                    {
                        result = -1;
                    }
                    else if (_crossingPoint.X > cei.CrossingPoint.X)
                    {
                        result = 1;
                    }
                    break;

                case EdgeAlignment.Horizontal:
                    if (_crossingPoint.Y < cei.CrossingPoint.Y)
                    {
                        result = -1;
                    }
                    else if (_crossingPoint.Y > cei.CrossingPoint.Y)
                    {
                        result = 1;
                    }
                    break;
            }

            return result;
        }
        #endregion
    }

    /// <summary>
    /// Class used as a data container and helper for the texture-to-vertices code.
    /// </summary>
    public class PolygonCreationAssistance
    {
        private uint[] _data;
        private int _width;
        private int _height;
        private byte _alphaTolerance;
        private uint _alphaToleranceRealValue;
        private float _hullTolerance;
        private int _holeDetectionLineStepSize;
        private bool _holeDetection;
        private bool _multipartDetection;

        public uint[] Data
        {
            get { return _data; }
        }

        public int Width
        {
            get { return _width; }
        }

        public int Height
        {
            get { return _height; }
        }

        public byte AlphaTolerance
        {
            get { return _alphaTolerance; }
            set
            {
                _alphaTolerance = value;
                _alphaToleranceRealValue = (uint)value << 24;
            }
        }

        public float HullTolerance
        {
            get { return _hullTolerance; }
            set
            {
                float hullTolerance = value;

                if (hullTolerance > 4f) hullTolerance = 4f;
                if (hullTolerance < 0.9f) hullTolerance = 0.9f;

                _hullTolerance = hullTolerance;
            }
        }

        public int HoleDetectionLineStepSize
        {
            get { return _holeDetectionLineStepSize; }
            set
            {
                if (value < 1)
                {
                    _holeDetectionLineStepSize = 1;
                }
                else
                {
                    if (value > 10)
                    {
                        _holeDetectionLineStepSize = 10;
                    }
                    else
                    {
                        _holeDetectionLineStepSize = value;
                    }
                }
            }
        }

        public bool HoleDetection
        {
            get { return _holeDetection; }
            set { _holeDetection = value; }
        }

        public bool MultipartDetection
        {
            get { return _multipartDetection; }
            set { _multipartDetection = value; }
        }

        public PolygonCreationAssistance(uint[] data, int width, int height)
        {
            _data = data;
            _width = width;
            _height = height;

            AlphaTolerance = 20;
            HullTolerance = 1.5f;

            HoleDetectionLineStepSize = 1;

            _holeDetection = false;
            _multipartDetection = false;
        }

        public bool IsSolid(Vector2 pixel)
        {
            return IsSolid((int)pixel.X, (int)pixel.Y);
        }

        public bool IsSolid(int x, int y)
        {
            if (x >= 0 && x < _width && y >= 0 && y < _height)
                return ((_data[x + y * _width] & 0xFF000000) >= _alphaToleranceRealValue);

            return false;
        }

        public bool IsSolid(int index)
        {
            if (index >= 0 && index < _width * _height)
                return ((_data[index] & 0xFF000000) >= _alphaToleranceRealValue);

            return false;
        }

        public bool InBounds(Vector2 coord)
        {
            return (coord.X >= 0f && coord.X < _width && coord.Y >= 0f && coord.Y < _height);
        }

        public bool IsValid()
        {
            if (_data != null && _data.Length > 0)
                return _data.Length == _width * _height;

            return false;
        }

        ~PolygonCreationAssistance()
        {
            _data = null;
        }
    }
    #endregion

    #region SAT Extensions

    /*
     * C# Version Ported by Matt Bettcher 2009
     * 
     * Original C++ Version Copyright (c) 2007 Eric Jordan
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

    internal class Triangle
    {
        public float[] x;
        public float[] y;

        //Constructor automatically fixes orientation to ccw
        public Triangle(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            x = new float[3];
            y = new float[3];
            float dx1 = x2 - x1;
            float dx2 = x3 - x1;
            float dy1 = y2 - y1;
            float dy2 = y3 - y1;
            float cross = dx1 * dy2 - dx2 * dy1;
            bool ccw = (cross > 0);
            if (ccw)
            {
                x[0] = x1; x[1] = x2; x[2] = x3;
                y[0] = y1; y[1] = y2; y[2] = y3;
            }
            else
            {
                x[0] = x1; x[1] = x3; x[2] = x2;
                y[0] = y1; y[1] = y3; y[2] = y2;
            }
        }

        //public Triangle()
        //{
        //    x = new float[3];
        //    y = new float[3];
        //}

        public Triangle(Triangle t)
        {
            x = new float[3];
            y = new float[3];

            x[0] = t.x[0]; x[1] = t.x[1]; x[2] = t.x[2];
            y[0] = t.y[0]; y[1] = t.y[1]; y[2] = t.y[2];
        }

        //public void Set(ref Triangle toMe)
        //{
        //    for (int i = 0; i < 3; ++i)
        //    {
        //        x[i] = toMe.x[i];
        //        y[i] = toMe.y[i];
        //    }
        //}

        public bool IsInside(float _x, float _y)
        {
            if (_x < x[0] && _x < x[1] && _x < x[2]) return false;
            if (_x > x[0] && _x > x[1] && _x > x[2]) return false;
            if (_y < y[0] && _y < y[1] && _y < y[2]) return false;
            if (_y > y[0] && _y > y[1] && _y > y[2]) return false;

            float vx2 = _x - x[0]; float vy2 = _y - y[0];
            float vx1 = x[1] - x[0]; float vy1 = y[1] - y[0];
            float vx0 = x[2] - x[0]; float vy0 = y[2] - y[0];

            float dot00 = vx0 * vx0 + vy0 * vy0;
            float dot01 = vx0 * vx1 + vy0 * vy1;
            float dot02 = vx0 * vx2 + vy0 * vy2;
            float dot11 = vx1 * vx1 + vy1 * vy1;
            float dot12 = vx1 * vx2 + vy1 * vy2;
            float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            return ((u > 0) && (v > 0) && (u + v < 1));
        }
    }

    internal class Polygon
    {
        private const int maxVerticesPerPolygon = 32;
        private const float angularSlop = 1.0f / 180.0f * (float)Math.PI; // 1 degrees

        private float[] x; //vertex arrays
        private float[] y;
        private int nVertices;
        private float area;

        private Polygon(float[] _x, float[] _y, int nVert)
        {
            nVertices = nVert;
            x = new float[nVertices];
            y = new float[nVertices];
            for (int i = 0; i < nVertices; ++i)
            {
                x[i] = _x[i];
                y[i] = _y[i];
            }
        }

        private Polygon(Vector2[] v, int nVert)
        {
            nVertices = nVert;
            x = new float[nVertices];
            y = new float[nVertices];
            for (int i = 0; i < nVertices; ++i)
            {
                x[i] = v[i].X;
                y[i] = v[i].Y;

            }
        }

        private Polygon()
        {
            x = null;
            y = null;
            nVertices = 0;
        }

        private float GetArea()
        {
            area = 0.0f;

            //First do wraparound
            area += x[nVertices - 1] * y[0] - x[0] * y[nVertices - 1];
            for (int i = 0; i < nVertices - 1; ++i)
            {
                area += x[i] * y[i + 1] - x[i + 1] * y[i];
            }
            area *= .5f;
            return area;
        }

        private bool IsCCW()
        {
            return (GetArea() > 0.0f);
        }

        private void MergeParallelEdges(float tolerance)
        {
            if (nVertices <= 3) return;             //Can't do anything useful here to a triangle
            bool[] mergeMe = new bool[nVertices];
            int newNVertices = nVertices;
            for (int i = 0; i < nVertices; ++i)
            {
                int lower = (i == 0) ? (nVertices - 1) : (i - 1);
                int middle = i;
                int upper = (i == nVertices - 1) ? (0) : (i + 1);
                float dx0 = x[middle] - x[lower];
                float dy0 = y[middle] - y[lower];
                float dx1 = x[upper] - x[middle];
                float dy1 = y[upper] - y[middle];
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
            if (newNVertices == nVertices || newNVertices == 0)
            {
                return;
            }
            float[] newx = new float[newNVertices];
            float[] newy = new float[newNVertices];
            int currIndex = 0;
            for (int i = 0; i < nVertices; ++i)
            {
                if (mergeMe[i] || newNVertices == 0 || currIndex == newNVertices) continue;

                //b2Assert(currIndex < newNVertices);
                newx[currIndex] = x[i];
                newy[currIndex] = y[i];
                ++currIndex;
            }

            x = newx;
            y = newy;
            nVertices = newNVertices;
            //	printf("%d \n", newNVertices);
        }

        private Polygon(Triangle t)
        {
            nVertices = 3;
            x = new float[nVertices];
            y = new float[nVertices];
            for (int i = 0; i < nVertices; ++i)
            {
                x[i] = t.x[i];
                y[i] = t.y[i];
            }
        }

        private Polygon(Polygon p)
        {
            nVertices = p.nVertices;
            x = new float[nVertices];
            y = new float[nVertices];
            for (int i = 0; i < nVertices; ++i)
            {
                x[i] = p.x[i];
                y[i] = p.y[i];
            }
        }

        private void Set(Polygon p)
        {
            if (nVertices != p.nVertices)
            {
                nVertices = p.nVertices;

                x = new float[nVertices];
                y = new float[nVertices];
            }

            for (int i = 0; i < nVertices; ++i)
            {
                x[i] = p.x[i];
                y[i] = p.y[i];
            }
        }

        /// <summary>
        /// Assuming the polygon is simple, checks if it is convex.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is convex; otherwise, <c>false</c>.
        /// </returns>
        private bool IsConvex()
        {
            bool isPositive = false;
            for (int i = 0; i < nVertices; ++i)
            {
                int lower = (i == 0) ? (nVertices - 1) : (i - 1);
                int middle = i;
                int upper = (i == nVertices - 1) ? (0) : (i + 1);
                float dx0 = x[middle] - x[lower];
                float dy0 = y[middle] - y[lower];
                float dx1 = x[upper] - x[middle];
                float dy1 = y[upper] - y[middle];
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

        /// <summary>
        /// Tries to add a triangle to the polygon. Returns null if it can't connect
        /// properly, otherwise returns a pointer to the new Polygon. Assumes bitwise
        /// equality of joined vertex positions.
        ///
        /// For internal use.
        /// </summary>
        /// <param name="t">The triangle to add.</param>
        /// <returns></returns>
        private Polygon Add(Triangle t)
        {
            //		float32 equalTol = .001f;
            // First, find vertices that connect
            int firstP = -1;
            int firstT = -1;
            int secondP = -1;
            int secondT = -1;
            for (int i = 0; i < nVertices; i++)
            {
                if (t.x[0] == x[i] && t.y[0] == y[i])
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
                else if (t.x[1] == x[i] && t.y[1] == y[i])
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
                else if (t.x[2] == x[i] && t.y[2] == y[i])
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
            if (firstP == 0 && secondP == nVertices - 1)
            {
                firstP = nVertices - 1;
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

            float[] newx = new float[nVertices + 1];
            float[] newy = new float[nVertices + 1];
            int currOut = 0;
            for (int i = 0; i < nVertices; i++)
            {
                newx[currOut] = x[i];
                newy[currOut] = y[i];
                if (i == firstP)
                {
                    ++currOut;
                    newx[currOut] = t.x[tipT];
                    newy[currOut] = t.y[tipT];
                }
                ++currOut;
            }
            Polygon result = new Polygon(newx, newy, nVertices + 1);

            return result;
        }

        /// <summary>
        /// Finds and fixes "pinch points," points where two polygon
        /// vertices are at the same point.
        /// If a pinch point is found, pin is broken up into poutA and poutB
        /// and true is returned; otherwise, returns false.
        /// Mostly for internal use.
        /// </summary>
        /// <param name="pin">The pin.</param>
        /// <param name="poutA">The pout A.</param>
        /// <param name="poutB">The pout B.</param>
        /// <returns></returns>
        private static bool ResolvePinchPoint(Polygon pin, out Polygon poutA, out Polygon poutB)
        {
            poutA = new Polygon();
            poutB = new Polygon();

            if (pin.nVertices < 3) return false;
            const float tol = .001f;
            bool hasPinchPoint = false;
            int pinchIndexA = -1;
            int pinchIndexB = -1;
            for (int i = 0; i < pin.nVertices; ++i)
            {
                for (int j = i + 1; j < pin.nVertices; ++j)
                {
                    //Don't worry about pinch points where the points
                    //are actually just dupe neighbors
                    if (Math.Abs(pin.x[i] - pin.x[j]) < tol && Math.Abs(pin.y[i] - pin.y[j]) < tol && j != i + 1)
                    {
                        pinchIndexA = i;
                        pinchIndexB = j;
                        //printf("pinch: %f, %f == %f, %f\n",pin.x[i],pin.y[i],pin.x[j],pin.y[j]);
                        //printf("at indexes %d, %d\n",i,j);
                        hasPinchPoint = true;
                        break;
                    }
                }
                if (hasPinchPoint) break;
            }
            if (hasPinchPoint)
            {
                //printf("Found pinch point\n");
                int sizeA = pinchIndexB - pinchIndexA;
                if (sizeA == pin.nVertices) return false;//has dupe points at wraparound, not a problem here
                float[] xA = new float[sizeA];
                float[] yA = new float[sizeA];
                for (int i = 0; i < sizeA; ++i)
                {
                    int ind = Remainder(pinchIndexA + i, pin.nVertices);             // is this right
                    xA[i] = pin.x[ind];
                    yA[i] = pin.y[ind];
                }
                Polygon tempA = new Polygon(xA, yA, sizeA);
                poutA.Set(tempA);

                int sizeB = pin.nVertices - sizeA;
                float[] xB = new float[sizeB];
                float[] yB = new float[sizeB];
                for (int i = 0; i < sizeB; ++i)
                {
                    int ind = Remainder(pinchIndexB + i, pin.nVertices);          // is this right    
                    xB[i] = pin.x[ind];
                    yB[i] = pin.y[ind];
                }
                Polygon tempB = new Polygon(xB, yB, sizeB);
                poutB.Set(tempB);
                //printf("Size of a: %d, size of b: %d\n",sizeA,sizeB);
            }
            return hasPinchPoint;
        }

        /// <summary>
        /// Triangulates a polygon using simple ear-clipping algorithm. Returns
        /// size of Triangle array unless the polygon can't be triangulated.
        /// This should only happen if the polygon self-intersects,
        /// though it will not _always_ return null for a bad polygon - it is the
        /// caller's responsibility to check for self-intersection, and if it
        /// doesn't, it should at least check that the return value is non-null
        /// before using. You're warned!
        ///
        /// Triangles may be degenerate, especially if you have identical points
        /// in the input to the algorithm.  Check this before you use them.
        ///
        /// This is totally unoptimized, so for large polygons it should not be part
        /// of the simulation loop.
        ///
        /// Returns:
        /// -1 if algorithm fails (self-intersection most likely)
        /// 0 if there are not enough vertices to triangulate anything.
        /// Number of triangles if triangulation was successful.
        ///
        /// results will be filled with results - ear clipping always creates vNum - 2
        /// or fewer (due to pinch point polygon snipping), so allocate an array of
        /// this size.
        /// </summary>
        /// <param name="xv">The xv.</param>
        /// <param name="yv">The yv.</param>
        /// <param name="vNum">The v num.</param>
        /// <param name="results">The results.</param>
        /// <returns></returns>
        private static int TriangulatePolygon(float[] xv, float[] yv, int vNum, out Triangle[] results)
        {
            results = new Triangle[175];

            if (vNum < 3)
                return 0;

            //Recurse and split on pinch points
            Polygon pA, pB;
            Polygon pin = new Polygon(xv, yv, vNum);
            if (ResolvePinchPoint(pin, out pA, out pB))
            {
                Triangle[] mergeA = new Triangle[pA.nVertices];
                Triangle[] mergeB = new Triangle[pB.nVertices];
                int nA = TriangulatePolygon(pA.x, pA.y, pA.nVertices, out mergeA);
                int nB = TriangulatePolygon(pB.x, pB.y, pB.nVertices, out mergeB);
                if (nA == -1 || nB == -1)
                {
                    return -1;
                }
                for (int i = 0; i < nA; ++i)
                {
                    results[i] = new Triangle(mergeA[i]);
                }
                for (int i = 0; i < nB; ++i)
                {
                    results[nA + i] = new Triangle(mergeB[i]);
                }
                return (nA + nB);
            }

            Triangle[] buffer = new Triangle[vNum - 2];
            int bufferSize = 0;
            float[] xrem = new float[vNum];
            float[] yrem = new float[vNum];
            for (int i = 0; i < vNum; ++i)
            {
                xrem[i] = xv[i];
                yrem[i] = yv[i];
            }

            while (vNum > 3)
            {
                // Find an ear
                int earIndex = -1;
                //float32 earVolume = -1.0f;
                float earMaxMinCross = -10.0f;
                for (int i = 0; i < vNum; ++i)
                {
                    if (IsEar(i, xrem, yrem, vNum))
                    {
                        int lower = Remainder(i - 1, vNum);
                        int upper = Remainder(i + 1, vNum);
                        Vector2 d1 = new Vector2(xrem[upper] - xrem[i], yrem[upper] - yrem[i]);
                        Vector2 d2 = new Vector2(xrem[i] - xrem[lower], yrem[i] - yrem[lower]);
                        Vector2 d3 = new Vector2(xrem[lower] - xrem[upper], yrem[lower] - yrem[upper]);

                        d1.Normalize();
                        d2.Normalize();
                        d3.Normalize();
                        float cross12 = Math.Abs(Calculator.Cross(ref d1, ref d2));
                        float cross23 = Math.Abs(Calculator.Cross(ref d2, ref d3));
                        float cross31 = Math.Abs(Calculator.Cross(ref d3, ref d1));
                        //Find the maximum minimum angle
                        float minCross = Math.Min(cross12, Math.Min(cross23, cross31));
                        if (minCross > earMaxMinCross)
                        {
                            earIndex = i;
                            earMaxMinCross = minCross;
                        }

                        /*//This bit chooses the ear with greatest volume first
                        float32 testVol = b2Abs( d1.x*d2.y-d2.x*d1.y );
                        if (testVol > earVolume){
                            earIndex = i;
                            earVolume = testVol;
                        }*/
                    }
                }

                // If we still haven't found an ear, we're screwed.
                // Note: sometimes this is happening because the
                // remaining points are collinear.  Really these
                // should just be thrown out without halting triangulation.
                if (earIndex == -1)
                {
                    for (int i = 0; i < bufferSize; i++)
                    {
                        results[i] = new Triangle(buffer[i]);
                    }

                    if (bufferSize > 0)
                        return bufferSize;

                    return -1;
                }

                // Clip off the ear:
                // - remove the ear tip from the list

                --vNum;
                float[] newx = new float[vNum];
                float[] newy = new float[vNum];
                int currDest = 0;
                for (int i = 0; i < vNum; ++i)
                {
                    if (currDest == earIndex) ++currDest;
                    newx[i] = xrem[currDest];
                    newy[i] = yrem[currDest];
                    ++currDest;
                }

                // - add the clipped triangle to the triangle list
                int under = (earIndex == 0) ? (vNum) : (earIndex - 1);
                int over = (earIndex == vNum) ? 0 : (earIndex + 1);
                Triangle toAdd = new Triangle(xrem[earIndex], yrem[earIndex], xrem[over], yrem[over], xrem[under], yrem[under]);
                buffer[bufferSize] = new Triangle(toAdd);
                ++bufferSize;

                // - replace the old list with the new one
                xrem = newx;
                yrem = newy;
            }

            Triangle tooAdd = new Triangle(xrem[1], yrem[1], xrem[2], yrem[2],
                                      xrem[0], yrem[0]);
            buffer[bufferSize] = new Triangle(tooAdd);
            ++bufferSize;

            //b2Assert(bufferSize == xremLength-2);

            for (int i = 0; i < bufferSize; i++)
            {
                results[i] = new Triangle(buffer[i]);
            }

            return bufferSize;
        }

        /// <summary>
        /// Fix for obnoxious behavior for the % operator for negative numbers...
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="modulus">The modulus.</param>
        /// <returns></returns>
        private static int Remainder(int x, int modulus)
        {
            int rem = x % modulus;
            while (rem < 0)
            {
                rem += modulus;
            }
            return rem;
        }

        /// <summary>
        /// Turns a list of triangles into a list of convex polygons. Very simple
        /// method - start with a seed triangle, keep adding triangles to it until
        /// you can't add any more without making the polygon non-convex.
        ///
        /// Returns an integer telling how many polygons were created.  Will fill
        /// polys array up to polysLength entries, which may be smaller or larger
        /// than the return value.
        /// 
        /// Takes O(N///P) where P is the number of resultant polygons, N is triangle
        /// count.
        /// 
        /// The final polygon list will not necessarily be minimal, though in
        /// practice it works fairly well.
        /// </summary>
        /// <param name="triangulated">The triangulated.</param>
        /// <param name="triangulatedLength">Length of the triangulated.</param>
        /// <param name="polys">The polys.</param>
        /// <param name="polysLength">Length of the polys.</param>
        /// <returns></returns>
        private static int PolygonizeTriangles(Triangle[] triangulated, int triangulatedLength, out Polygon[] polys, int polysLength)
        {
            int polyIndex = 0;
            polys = new Polygon[50];

            if (triangulatedLength <= 0)
            {
                return 0;
            }
            bool[] covered = new bool[triangulatedLength];
            for (int i = 0; i < triangulatedLength; ++i)
            {
                covered[i] = false;
                //Check here for degenerate triangles
                if (((triangulated[i].x[0] == triangulated[i].x[1]) && (triangulated[i].y[0] == triangulated[i].y[1]))
                     || ((triangulated[i].x[1] == triangulated[i].x[2]) && (triangulated[i].y[1] == triangulated[i].y[2]))
                     || ((triangulated[i].x[0] == triangulated[i].x[2]) && (triangulated[i].y[0] == triangulated[i].y[2])))
                {
                    covered[i] = true;
                }
            }

            bool notDone = true;
            while (notDone)
            {
                int currTri = -1;
                for (int i = 0; i < triangulatedLength; ++i)
                {
                    if (covered[i])
                        continue;
                    currTri = i;
                    break;
                }
                if (currTri == -1)
                {
                    notDone = false;
                }
                else
                {
                    Polygon poly = new Polygon(triangulated[currTri]);
                    covered[currTri] = true;
                    int index = 0;
                    for (int i = 0; i < 2 * triangulatedLength; ++i, ++index)
                    {
                        while (index >= triangulatedLength) index -= triangulatedLength;
                        if (covered[index])
                        {
                            continue;
                        }
                        Polygon newP = poly.Add(triangulated[index]);
                        if (newP == null)
                        {                                 // is this right
                            continue;
                        }
                        if (newP.nVertices > maxVerticesPerPolygon)
                        {
                            newP = null;
                            continue;
                        }
                        if (newP.IsConvex())
                        { //Or should it be IsUsable?  Maybe re-write IsConvex to apply the angle threshold from Box2d
                            poly = new Polygon(newP);
                            newP = null;
                            covered[index] = true;
                        }
                        else
                        {
                            newP = null;
                        }
                    }
                    if (polyIndex < polysLength)
                    {
                        poly.MergeParallelEdges(angularSlop);
                        //If identical points are present, a triangle gets
                        //borked by the MergeParallelEdges function, hence
                        //the vertex number check
                        if (poly.nVertices >= 3) polys[polyIndex] = new Polygon(poly);
                        //else printf("Skipping corrupt poly\n");
                    }
                    if (poly.nVertices >= 3) polyIndex++; //Must be outside (polyIndex < polysLength) test
                }
            }
            return polyIndex;
        }

        /// <summary>
        /// Checks if vertex i is the tip of an ear in polygon defined by xv[] and
        /// yv[].
        ///
        /// Assumes clockwise orientation of polygon...ick
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="xv">The xv.</param>
        /// <param name="yv">The yv.</param>
        /// <param name="xvLength">Length of the xv.</param>
        /// <returns>
        /// 	<c>true</c> if the specified i is ear; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsEar(int i, float[] xv, float[] yv, int xvLength)
        {
            float dx0, dy0, dx1, dy1;
            if (i >= xvLength || i < 0 || xvLength < 3)
            {
                return false;
            }
            int upper = i + 1;
            int lower = i - 1;
            if (i == 0)
            {
                dx0 = xv[0] - xv[xvLength - 1];
                dy0 = yv[0] - yv[xvLength - 1];
                dx1 = xv[1] - xv[0];
                dy1 = yv[1] - yv[0];
                lower = xvLength - 1;
            }
            else if (i == xvLength - 1)
            {
                dx0 = xv[i] - xv[i - 1];
                dy0 = yv[i] - yv[i - 1];
                dx1 = xv[0] - xv[i];
                dy1 = yv[0] - yv[i];
                upper = 0;
            }
            else
            {
                dx0 = xv[i] - xv[i - 1];
                dy0 = yv[i] - yv[i - 1];
                dx1 = xv[i + 1] - xv[i];
                dy1 = yv[i + 1] - yv[i];
            }
            float cross = dx0 * dy1 - dx1 * dy0;
            if (cross > 0)
                return false;
            Triangle myTri = new Triangle(xv[i], yv[i], xv[upper], yv[upper],
                                      xv[lower], yv[lower]);
            for (int j = 0; j < xvLength; ++j)
            {
                if (j == i || j == lower || j == upper)
                    continue;
                if (myTri.IsInside(xv[j], yv[j]))
                    return false;
            }
            return true;
        }

        private static void ReversePolygon(float[] x, float[] y, int n)
        {
            if (n == 1)
                return;
            int low = 0;
            int high = n - 1;
            while (low < high)
            {
                float buffer = x[low];
                x[low] = x[high];
                x[high] = buffer;
                buffer = y[low];
                y[low] = y[high];
                y[high] = buffer;
                ++low;
                --high;
            }
        }

        /// <summary>
        /// Decomposes a non-convex polygon into a number of convex polygons, up
        /// to maxPolys (remaining pieces are thrown out, but the total number
        /// is returned, so the return value can be greater than maxPolys).
        ///
        /// Each resulting polygon will have no more than maxVerticesPerPolygon
        /// vertices (set to b2MaxPolyVertices by default, though you can change
        /// this).
        /// 
        /// Returns -1 if operation fails (usually due to self-intersection of
        /// polygon).
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="results">The results.</param>
        /// <param name="maxPolys">The max polys.</param>
        /// <returns></returns>
        private static int DecomposeConvex(Polygon p, out Polygon[] results, int maxPolys)
        {
            results = new Polygon[1];

            if (p.nVertices < 3) return 0;

            Triangle[] triangulated = new Triangle[p.nVertices - 2];
            int nTri;
            if (p.IsCCW())
            {
                //printf("It is ccw \n");
                Polygon tempP = new Polygon(p);
                ReversePolygon(tempP.x, tempP.y, tempP.nVertices);
                nTri = TriangulatePolygon(tempP.x, tempP.y, tempP.nVertices, out triangulated);
                //			ReversePolygon(p->x, p->y, p->nVertices); //reset orientation
            }
            else
            {
                //printf("It is not ccw \n");
                nTri = TriangulatePolygon(p.x, p.y, p.nVertices, out triangulated);
            }
            if (nTri < 1)
            {
                //Still no luck?  Oh well...
                return -1;
            }
            int nPolys = PolygonizeTriangles(triangulated, nTri, out results, maxPolys);
            return nPolys;
        }

        public static Vertices[] DecomposeVertices(Vertices v, int max)
        {
            Polygon p = new Polygon(v.ToArray(), v.Count);      // convert the vertices to a polygon

            Polygon[] output;

            DecomposeConvex(p, out output, max);

            Vertices[] verticesOut = new Vertices[output.Length];

            int i;

            for (i = 0; i < output.Length; i++)
            {
                if (output[i] != null)
                {
                    verticesOut[i] = new Vertices();

                    for (int j = 0; j < output[i].nVertices; j++)
                        verticesOut[i].Add(new Vector2(output[i].x[j], output[i].y[j]));
                }
                else
                    break;
            }

            Vertices[] verts = new Vertices[i];
            for (int k = 0; k < i; k++)
            {
                verts[k] = new Vertices(verticesOut[k]);
            }

            return verts;
        }
    }

    #endregion

    #region DrDeth's Extension
    /// <summary>
    /// Enumerator to specify errors with Polygon functions.
    /// </summary>
    public enum PolyUnionError
    {
        None,
        NoIntersections,
        Poly1InsidePoly2,
        InfiniteLoop
    }

    public class Edge
    {
        public Edge(Vector2 edgeStart, Vector2 edgeEnd)
        {
            EdgeStart = edgeStart;
            EdgeEnd = edgeEnd;
        }

        public Vector2 EdgeStart { get; private set; }
        public Vector2 EdgeEnd { get; private set; }
    }

    public class EdgeIntersectInfo
    {
        public EdgeIntersectInfo(Edge edgeOne, Edge edgeTwo, Vector2 intersectionPoint)
        {
            EdgeOne = edgeOne;
            EdgeTwo = edgeTwo;
            IntersectionPoint = intersectionPoint;
        }

        public Edge EdgeOne { get; private set; }
        public Edge EdgeTwo { get; private set; }
        public Vector2 IntersectionPoint { get; private set; }
    }
    #endregion
}
