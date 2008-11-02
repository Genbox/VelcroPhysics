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
                int j = (i + 1)%Count;
                area += this[i].X*this[j].Y;
                area -= this[i].Y*this[j].X;
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
                int j = (i + 1)%Count;
                area += this[i].X*this[j].Y;
                area -= this[i].Y*this[j].X;
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

            float stepSize = MathHelper.TwoPi/numberOfEdges;
            vertices.Add(new Vector2(radius, 0));
            for (int i = 1; i < numberOfEdges; i++)
            {
                vertices.Add(new Vector2(radius*Calculator.Cos(stepSize*i), -radius*Calculator.Sin(stepSize*i)));
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

            float stepSize = MathHelper.TwoPi/numberOfEdges;

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

        private static readonly int[,] _closePixels = new int[8,2]
                                                          {
                                                              {-1, -1}, {0, -1}, {1, -1}, {1, 0}, {1, 1}, {0, 1}, {-1, 1},
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
            uint alphaToleranceRealValue = (uint) alphaTolerance << 24;

            // First of all: Check the array you just got.
            if (textureBits.Length == textureWidth*textureHeight)
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
                    int x = i%textureWidth;
                    int y = (i - x)/textureWidth;

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
                int indexOfPixelToCheck = (indexOfFirstPixelToCheck + i)%pixelsToCheck;

                int x = (int) current.X + _closePixels[indexOfPixelToCheck, 0];
                int y = (int) current.Y + _closePixels[indexOfPixelToCheck, 1];

                // Check if the coords are in the texture coords.
                if (x >= 0 && x < textureWidth && y >= 0 && y < textureHeight)
                {
                    // Uh! Something sold?
                    if ((textureBits[x + y*textureWidth] & 0xFF000000) >= alphaTolerance)
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
            switch ((int) (current.X - last.X))
            {
                case 1:
                    switch ((int) (current.Y - last.Y))
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
                    switch ((int) (current.Y - last.Y))
                    {
                        case 1:
                            return 2;

                        case -1:
                            return 6;
                    }
                    break;

                case -1:
                    switch ((int) (current.Y - last.Y))
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
    }
}