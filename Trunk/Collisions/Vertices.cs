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
            //calc centroid on counter clockwise verts.
            Vertices verts = new Vertices(this);
            verts.ForceCounterClockWiseOrder();

            float cx = 0, cy = 0;
            int i;

            float factor;
            //Debug.WriteLine(verts.ToString());
            for (i = 0; i < Count; i++)
            {
                int j = (i + 1) % Count;

                factor = -(verts[i].X * verts[j].Y - verts[j].X * verts[i].Y);
                cx += (verts[i].X + verts[j].X) * factor;
                cy += (verts[i].Y + verts[j].Y) * factor;
                //Debug.WriteLine(i.ToString() + factor.ToString() + " -- " + verts[i].ToString());
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
        public void Translate(Vector2 vector)
        {
            for (int i = 0; i < Count; i++)
            {
                this[i] = Vector2.Add(this[i], vector);
            }
        }

        /// <summary>
        /// Scales the vertices with the specified vector.
        /// </summary>
        /// <param name="value">The Value.</param>
        public void Scale(Vector2 value)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                this[i] *= value;
            }
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
            {
                this[i] = Vector2.Transform(this[i], rotationMatrix);
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
            vertices.Add(new Vector2(-width * .5f, -height * .5f));
            //vertices.Add(new Vector2(-width * .5f, -height * .25f));
            //vertices.Add(new Vector2(-width * .5f, 0));
            //vertices.Add(new Vector2(-width * .5f, height * .25f));
            vertices.Add(new Vector2(-width * .5f, height * .5f));
            //vertices.Add(new Vector2(-width * .25f, height * .5f));
            //vertices.Add(new Vector2(0, height * .5f));
            //vertices.Add(new Vector2(width * .25f, height * .5f));
            vertices.Add(new Vector2(width * .5f, height * .5f));
            //vertices.Add(new Vector2(width * .5f, height * .25f));
            //vertices.Add(new Vector2(width * .5f, 0));
            //vertices.Add(new Vector2(width * .5f, -height * .25f));
            vertices.Add(new Vector2(width * .5f, -height * .5f));
            //vertices.Add(new Vector2(width * .25f, -height * .5f));
            //vertices.Add(new Vector2(0, -height * .5f));
            //vertices.Add(new Vector2(-width * .25f, -height * .5f));

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

            float stepSize = MathHelper.TwoPi / numberOfEdges;
            vertices.Add(new Vector2(radius, 0));
            for (int i = 1; i < numberOfEdges; i++)
            {
                vertices.Add(new Vector2(radius * Calculator.Cos(stepSize * i), -radius * Calculator.Sin(stepSize * i)));
            }
            return vertices;
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
            {
                vertices.Add(new Vector2(xRadius * Calculator.Cos(stepSize * i), -yRadius * Calculator.Sin(stepSize * i)));
            }
            return vertices;
        }

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

                //vertices.Add(new Vector2((radius) * Calculator.Cos((stepSize * i) + (toothAngleStepSize + toothTipStepSize) * 2f),
                //    -(radius) * Calculator.Sin((stepSize * i) + (toothAngleStepSize + toothTipStepSize) * 2f)));

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

        #region Sickbattery's Extension

        /// <summary>
        /// TODO:
        /// 1.) Das Array welches ich bekomme am besten in einen bool array verwandeln. Würde die Geschwindigkeit verbessern
        /// </summary>
        private static readonly int[,] _closePixels = new int[8, 2] { { -1, -1 }, { 0, -1 }, { 1, -1 }, { 1, 0 }, { 1, 1 }, { 0, 1 }, { -1, 1 }, { -1, 0 } };

        public static List<Vertices> CreatePolygon(ref PolygonCreationAssistance pca)
        {
            List<Vertices> polygons = new List<Vertices>();

            Vertices polygon;
            Vertices holePolygon;

            int vertex1Index = 0;
            int vertex2Index = 0;

            Vector2? holeEntrance = null;
            Vector2? polygonEntrance = null;

            List<Vector2> blackList = new List<Vector2>();

            bool inPolygon = false;
            bool searchOn = false;

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

                        if (Calculator.DistanceBetweenPointAndLineSegment(point, edgeVertex1, edgeVertex2) <= pca.HullTolerance ||
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

                        if (Calculator.DistanceBetweenPointAndLineSegment(point, edgeVertex1, edgeVertex2) <= pca.HullTolerance)
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

        #region Functions to get top and bottom most vertex of a polygon.
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

        private static Vector2? GetBottomMostVertex(ref Vertices vertices)
        {
            float bottomMostValue = float.MinValue;
            Vector2? bottomMost = null;

            for (int i = 0; i < vertices.Count; i++)
            {
                if (bottomMostValue < vertices[i].Y)
                {
                    bottomMostValue = vertices[i].Y;
                    bottomMost = vertices[i];
                }
            }

            return bottomMost;
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
        #endregion

        public static List<CrossingEdgeInfo> GetCrossingEdges(ref Vertices polygon, EdgeAlignment edgeAlign, int checkLine)
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

                            for (edgeVertex1Index = 0; edgeVertex1Index < polygon.Count; edgeVertex1Index++)
                            {
                                distance = Calculator.DistanceBetweenPointAndLineSegment(foundEdgeCoord,
                                                                                         polygon[edgeVertex1Index],
                                                                                         polygon[edgeVertex2Index]);
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

                                distance = Calculator.DistanceBetweenPointAndPoint(polygon[nearestEdgeVertex1Index],
                                                                                   foundEdgeCoord);

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

            // Search between the first and last hull point.
            for (int i = 1; i < hullAreaLastPoint; i++)
            {
                // Check if the distance is over the one that's tolerable.
                if (Calculator.DistanceBetweenPointAndLineSegment(hullArea[i], hullArea[0], hullArea[hullAreaLastPoint]) >= hullTolerance)
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
        /// Static variable to store the last error.
        /// </summary>
        //public static PolyUnionError UnionError = PolyUnionError.None;

        /// <summary>
        /// Offsets a list of vertices by the specified offset.
        /// </summary>
        /// <param name="polygon">The Vertices to offset.</param>
        /// <param name="offset">A Vector2 to offset vertices by.</param>
        /// <returns>A new set of Vertices that have been offset.</returns>
        //public Vertices Offset(Vector2 offset)
        //{
        //    Vertices offsetPolygon = new Vertices();
        //    foreach (Vector2 point in this)
        //    {
        //        offsetPolygon.Add(Vector2.Add(point,offset));
        //    }

        //    return offsetPolygon;
        //}

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

                    // _defaultFloatTolerance = .00001f (Perhaps this should be made available publically from CollisionHelper?

                    // Check if the edges intersect
                    if (CollisionHelper.LineIntersect(p1, p2, p3, p4, true, true, 0.00001f, out point))
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
            {
                returnPoly.Add(new Vector2((float)Math.Round(polygon[i].X, 0), (float)Math.Round(polygon[i].Y, 0)));
            }
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
            //We cant simplify polygons under 3 vertices
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
                {
                    simplified.Add(roundPolygon[curr]);
                }
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
            for (int i = 1; i < topEdges - 1; i++)
            {
                vertices.Add(new Vector2(topRadius * Calculator.Cos(stepSize * i), topRadius * Calculator.Sin(stepSize * i) + newHeight));
            }

            vertices.Add(new Vector2(-topRadius, newHeight));

            // bottom
            vertices.Add(new Vector2(-bottomRadius, -newHeight));

            stepSize = MathHelper.Pi / bottomEdges;
            for (int i = 1; i < bottomEdges - 1; i++)
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
                set
                {
                    //perform the index wrapping
                    while (index < 0)
                        index = Count + index;
                    if (index >= Count)
                        index %= Count;

                    base[index] = value;
                }
            }

            public CyclicalList() { }

            public CyclicalList(IEnumerable<T> collection)
                : base(collection)
            {
            }

            public new void RemoveAt(int index)
            {
                Remove(this[index]);
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

        public uint AlphaToleranceRealValue
        {
            get { return _alphaToleranceRealValue; }
            set
            {
                _alphaTolerance = (byte)(value >> 24);
                _alphaToleranceRealValue = value;
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
            {
                return ((_data[x + y * _width] & 0xFF000000) >= _alphaToleranceRealValue);
            }
            else
            {
                return false;
            }
        }

        public bool IsSolid(int index)
        {
            if (index >= 0 && index < _width * _height)
            {
                return ((_data[index] & 0xFF000000) >= _alphaToleranceRealValue);
            }
            else
            {
                return false;
            }
        }

        public bool InBounds(Vector2 coord)
        {
            return (coord.X >= 0f && coord.X < _width && coord.Y >= 0f && coord.Y < _height);
        }

        public bool IsValid()
        {
            if (_data != null && _data.Length > 0)
            {
                return _data.Length == _width * _height;
            }
            else
            {
                return false;
            }
        }

        ~PolygonCreationAssistance()
        {
            _data = null;
        }
    }
    #endregion

    #region SAT Extensions

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