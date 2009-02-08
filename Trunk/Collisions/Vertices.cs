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
        /// Creates a rectangle with the specified width and height.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>The vertices that define a rectangle</returns>
        public static Vertices CreateRectangle(float width, float height)
        {
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

        private static readonly int[,] _closePixels = new int[8, 2]
                                                          {
                                                              {-1, -1}, {0, -1}, {1, -1}, {1, 0}, {1, 1}, {0, 1},
                                                              {-1, 1},
                                                              {-1, 0}
                                                          };

        /// <summary>
        /// Creates a list of vertices from unsigned integer (32-Bit; 8 bit for each color) array.
        /// </summary>
        /// <param name="textureBits">Unsigned integer (32-Bit; 8 bit for each color) array.</param>
        /// <param name="textureWidth">Width of texture.</param>
        /// <param name="textureHeight">Height of texture.</param>
        /// <returns>Returns Vertices a Vector2 list.</returns>
        public static Vertices CreatePolygon(uint[] textureBits, int textureWidth, int textureHeight)
        {
            return CreatePolygon(textureBits, textureWidth, textureHeight, Vector2.Zero, 127, 2f);
        }

        /// <summary>
        /// Creates a list of vertices from unsigned integer (32-Bit; 8 bit for each color) array.
        /// </summary>
        /// <param name="textureBits">Unsigned integer (32-Bit; 8 bit for each color) array.</param>
        /// <param name="textureWidth">Width of texture.</param>
        /// <param name="textureHeight">Height of texture.</param>
        /// <param name="textureOrigin">Center of texture.</param>
        /// <returns>Returns Vertices a Vector2 list.</returns>
        public static Vertices CreatePolygon(uint[] textureBits, int textureWidth, int textureHeight,
                                             Vector2 textureOrigin)
        {
            return CreatePolygon(textureBits, textureWidth, textureHeight, textureOrigin, 127, 2f);
        }

        /// <summary>
        /// Creates a list of vertices from unsigned integer (32-Bit; 8 bit for each color) array.
        /// </summary>
        /// <param name="textureBits">Unsigned integer (32-Bit; 8 bit for each color) array.</param>
        /// <param name="textureWidth">Width of texture.</param>
        /// <param name="textureHeight">Height of texture.</param>
        /// <param name="textureOrigin">Center of texture.</param>
        /// <param name="alphaTolerance">Every Value above the specified counts as solid and will be added to the hull.</param>
        /// <returns>Returns Vertices a Vector2 list.</returns>
        public static Vertices CreatePolygon(uint[] textureBits, int textureWidth, int textureHeight,
                                             Vector2 textureOrigin, byte alphaTolerance)
        {
            return CreatePolygon(textureBits, textureWidth, textureHeight, textureOrigin, alphaTolerance, 2f);
        }

        /// <summary>
        /// Creates a list of vertices from unsigned integer (32-Bit; 8 bit for each color) array.
        /// </summary>
        /// <param name="textureBits">Unsigned integer (32-Bit; 8 bit for each color) array.</param>
        /// <param name="textureWidth">Width of texture.</param>
        /// <param name="textureHeight">Height of texture.</param>
        /// <param name="textureOrigin">Center of texture.</param>
        /// <param name="alphaTolerance">Every Value above the specified counts as solid and will be added to the hull.</param>
        /// <param name="hullTolerance">The polygon is a low detailed line around your shape on the texture and here you can specify how much less detailed. 1f is a good Value.</param>
        /// <returns>Returns Vertices a Vector2 list.</returns>
        /// <exception cref="Exception">Sizes don't match: Color array must contain texture width * texture height elements.</exception>
        public static Vertices CreatePolygon(uint[] textureBits, int textureWidth, int textureHeight,
                                             Vector2 textureOrigin, byte alphaTolerance, float hullTolerance)
        {
            Vector2 entrance;
            Vertices polygon = new Vertices();
            Vertices hullArea = new Vertices();

            // Precalculate alpha.
            uint alphaToleranceRealValue = (uint)alphaTolerance << 24;

            // First of all: Check the array you just got.
            if (textureBits.Length == textureWidth * textureHeight)
            {
                // Get the entrance point.
                if (GetHullEntrance(ref textureBits, ref textureWidth, ref alphaToleranceRealValue, out entrance))
                {
                    // The current point has to be the one before entrance.
                    // It will become last in the do..while loop in the first run.
                    Vector2 current = new Vector2(entrance.X - 1f, entrance.Y);

                    // next has to be set to entrance so it'll be added as the
                    // first point in the list.
                    Vector2 next = entrance;

                    // Fast bugfix XD. The entrance point of course has to be added first to the polygon XD. Damn I forgot that!
                    polygon.Add(entrance);

                    do
                    {
                        Vector2 outstanding;

                        // Add the vertex to a hull pre vision list.
                        hullArea.Add(next);

                        // Search in the pre vision list for an outstanding point.
                        if (SearchForOutstandingVertex(ref hullArea, ref hullTolerance, out outstanding))
                        {
                            // Add it and remove all vertices that don't matter anymore
                            // (all the vertices before the outstanding).
                            polygon.Add(outstanding);
                            hullArea.RemoveRange(0, hullArea.IndexOf(outstanding));
                        }

                        // Last point gets current and current gets next. Our little spider is moving forward on the hull ;).
                        Vector2 last = current;
                        current = next;

                        // Get the next point on hull.
                        if (
                            !GetNextHullPoint(ref textureBits, ref textureWidth, ref textureHeight,
                                              ref alphaToleranceRealValue, ref last, ref current, out next))
                        {
                            next = entrance;
                        }
                    } // Exit loop if next piont is the entrance point. The hull is complete now!
                    while (next != entrance);

                    // Center if requested by user.
                    if (textureOrigin != Vector2.Zero)
                    {
                        for (int i = 0; i < polygon.Count; i++)
                        {
                            polygon[i] -= textureOrigin;
                        }
                    }
                }
            }
            else
            {
                throw new Exception(
                    "Sizes don't match: Color array must contain texture width * texture height elements.");
            }

            // Return the heavy compresed polygon ;D.
            return polygon;
        }

        /// <summary>
        /// This function search for the first hull point.
        /// </summary>
        /// <param name="textureBits">A reference to your texture's data.</param>
        /// <param name="textureWidth">Width of your texture.</param>
        /// <param name="alphaTolerance">Alpha tolerance :). Value of 10 will include points with alpha of 11 and greater.</param>
        /// <param name="entrance">The entrance.</param>
        /// <returns>First hull point.</returns>
        private static bool GetHullEntrance(ref uint[] textureBits, ref int textureWidth, ref uint alphaTolerance,
                                            out Vector2 entrance)
        {
            // Search for first solid pixel.
            for (int i = 0; i < textureBits.Length; i++)
            {
                // Move the alpha bits down and check if the pixel is solid.
                if ((textureBits[i] & 0xFF000000) >= alphaTolerance)
                {
                    // Now calculate the coords and return'em.
                    int x = i % textureWidth;
                    int y = (i - x) / textureWidth;

                    entrance = new Vector2(x, y);
                    return true;
                }
            }

            // If there are no solid pixels.
            entrance = Vector2.Zero;
            return false;
        }

        /// <summary>
        /// Searches for the next hull point.
        /// </summary>
        /// <param name="textureBits">A reference to your texture's data.</param>
        /// <param name="textureWidth">Width of your texture.</param>
        /// <param name="textureHeight">Height of your texture.</param>
        /// <param name="alphaTolerance">Alpha tolerance :). Value of 10 will include points with alpha of 10.</param>
        /// <param name="last">Last hull point.</param>
        /// <param name="current">Current hull point.</param>
        /// <param name="next">The next point</param>
        /// <returns>The next hull point.</returns>
        private static bool GetNextHullPoint(ref uint[] textureBits, ref int textureWidth, ref int textureHeight,
                                             ref uint alphaTolerance, ref Vector2 last, ref Vector2 current,
                                             out Vector2 next)
        {
            // Depending on the direction the little spider comes from you have to tell her
            // where to start the search again.
            int indexOfFirstPixelToCheck = GetIndexOfFirstPixelToCheck(last, current);

            const int pixelsToCheck = 8; //8 -> _closePixels.Length -> hardcoded to speed up

            for (int i = 0; i < pixelsToCheck; i++)
            {
                // The little spider starts now to look around ;).
                int indexOfPixelToCheck = (indexOfFirstPixelToCheck + i) % pixelsToCheck;

                int x = (int)current.X + _closePixels[indexOfPixelToCheck, 0];
                int y = (int)current.Y + _closePixels[indexOfPixelToCheck, 1];

                // Check if the coords are in the texture coords.
                if (x >= 0 && x < textureWidth && y >= 0 && y < textureHeight)
                {
                    // Uh! Something sold?
                    if ((textureBits[x + y * textureWidth] & 0xFF000000) >= alphaTolerance)
                    {
                        // Yeah! Return and quit searching.
                        next = new Vector2(x, y);
                        return true;
                    }
                }
            }

            // Nothing found? Wow. I think that can't happen, ...but next must be set. C# screams for it.
            next = Vector2.Zero;
            return false;
        }

        /// <summary>
        /// This function searches for an outstanding pixel. When found it searches on for the most outstanding.
        /// </summary>
        /// <param name="hullArea">Put a peace of the hull in here.</param>
        /// <param name="hullTolerance">How much distance from the actual hull line is allowed? 1f to 2f are good values.</param>
        /// <param name="outstanding">This will give you the most outstanding point in the piece of hull you gave it.</param>
        /// <returns></returns>
        private static bool SearchForOutstandingVertex(ref Vertices hullArea, ref float hullTolerance,
                                                       out Vector2 outstanding)
        {
            int hullAreaLastPoint = hullArea.Count - 1;

            float lastOutstandingDistance = 0f;
            bool searchMostOutstanding = false;

            Vector2 outstandingResult = Vector2.Zero;

            // Search between the first and last hull point.
            for (int i = 1; i < hullAreaLastPoint; i++)
            {
                // Get the distance of the outstanding point.
                float outstandingDistance = Calculator.DistanceBetweenPointAndLineSegment(hullArea[i], hullArea[0],
                                                                                          hullArea[hullAreaLastPoint]);

                if (!searchMostOutstanding)
                {
                    // Check if the distance is over the one that's tolerable.
                    if (outstandingDistance > hullTolerance)
                    {
                        // Ok, next time we search for the most outstanding.
                        searchMostOutstanding = true;
                        lastOutstandingDistance = outstandingDistance;
                        outstandingResult = hullArea[i];
                    }
                }
                else
                {
                    // Is it the most outstanding?
                    if (outstandingDistance > lastOutstandingDistance)
                    {
                        // Indeed :). But lets search to the end.
                        lastOutstandingDistance = outstandingDistance;
                        outstandingResult = hullArea[i];
                    }
                }
            }

            // Return the stuff...
            outstanding = outstandingResult;
            return searchMostOutstanding;
        }

        /// <summary>
        /// This function tells you where to start searching for the next hull point.
        /// Important: Last and next hull points have to be right next to each other.
        /// </summary>
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
        /// Creates an capsule with the specified total height, radius and number of edges.
        /// A capsule has the same form as a pill capsule.
        /// </summary>
        /// <param name="totalHeight">Total height (inner height + 2 * radius) of the capsule.</param>
        /// <param name="radius">Radius of the capsule ends.</param>
        /// <param name="edges">The number of edges of the capsule ends. The more edges, the more it resembles an capsule</param>
        /// <returns></returns>
        public static Vertices CreateCapsule(float totalHeight, float radius, int edges)
        {
            return CreateCapsule(totalHeight, radius, edges, radius, edges);
        }

        /// <summary>
        /// Creates an capsule with the specified total height, radius and number of edges.
        /// A capsule has the same form as a pill capsule.
        /// </summary>
        /// <param name="totalHeight">Total height (inner height + radii) of the capsule.</param>
        /// <param name="topRadius">Radius of the top.</param>
        /// <param name="topEdges">The number of edges of the top. The more edges, the more it resembles an capsule</param>
        /// <param name="bottomRadius">Radius of bottom.</param>
        /// <param name="bottomEdges">The number of edges of the bottom. The more edges, the more it resembles an capsule</param>
        /// <returns></returns>
        public static Vertices CreateCapsule(float totalHeight, float topRadius, int topEdges, float bottomRadius, int bottomEdges)
        {
            Vertices vertices = new Vertices();

            float height = (totalHeight - topRadius - bottomRadius) * 0.5f;

            // top
            vertices.Add(new Vector2(topRadius, height));

            float stepSize = MathHelper.Pi / topEdges;
            for (int i = 1; i < topEdges - 1; i++)
            {
                vertices.Add(new Vector2(topRadius * Calculator.Cos(stepSize * i), topRadius * Calculator.Sin(stepSize * i) + height));
            }

            vertices.Add(new Vector2(-topRadius, height));

            // bottom
            vertices.Add(new Vector2(-bottomRadius, -height));

            stepSize = MathHelper.Pi / bottomEdges;
            for (int i = 1; i < bottomEdges - 1; i++)
            {
                vertices.Add(new Vector2(-bottomRadius * Calculator.Cos(stepSize * i), -bottomRadius * Calculator.Sin(stepSize * i) - height));
            }

            vertices.Add(new Vector2(bottomRadius, -height));

            return vertices;
        }
        #endregion
    }

    #region DeDeth's Extension
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