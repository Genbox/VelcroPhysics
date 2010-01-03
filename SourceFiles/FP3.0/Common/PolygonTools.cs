using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FarseerPhysics
{
    public static class PolygonTools
    {
        /// <summary>
        /// Build vertices to represent an axis-aligned box.
        /// </summary>
        /// <param name="hx">the half-width.</param>
        /// <param name="hy">the half-height.</param>
        public static Vertices CreateBox(float hx, float hy)
        {
            Vertices vertices = new Vertices(4);
            vertices.Add(new Vector2(-hx, -hy));
            vertices.Add(new Vector2(hx, -hy));
            vertices.Add(new Vector2(hx, hy));
            vertices.Add(new Vector2(-hx, hy));

            return vertices;
        }

        /// <summary>
        /// Build vertices to represent an oriented box.
        /// </summary>
        /// <param name="hx">the half-width.</param>
        /// <param name="hy">the half-height.</param>
        /// <param name="center">the center of the box in local coordinates.</param>
        /// <param name="angle">the rotation of the box in local coordinates.</param>
        public static Vertices CreateBox(float hx, float hy, Vector2 center, float angle)
        {
            Vertices vertices = CreateBox(hx, hy);

            Transform xf = new Transform();
            xf.Position = center;
            xf.R.Set(angle);

            // Transform vertices
            for (int i = 0; i < 4; ++i)
            {
                vertices[i] = MathUtils.Multiply(ref xf, vertices[i]);
            }

            return vertices;
        }

        /// <summary>
        /// Set this as a single edge.
        /// </summary>
        /// <param name="v1">The first point.</param>
        /// <param name="v2">The second point.</param>
        public static Vertices CreateEdge(Vector2 v1, Vector2 v2)
        {
            Vertices vertices = new Vertices(2);
            vertices.Add(v1);
            vertices.Add(v2);

            return vertices;
        }

        /// <summary>
        /// Translates the vertices with the specified vector.
        /// </summary>
        /// <param name="vector">The vector.</param>
        public static void Translate(ref Vertices vertices, ref Vector2 vector)
        {
            for (int i = 0; i < vertices.Count; i++)
                vertices[i] = Vector2.Add(vertices[i], vector);
        }

        /// <summary>
        /// Scales the vertices with the specified vector.
        /// </summary>
        /// <param name="value">The Value.</param>
        public static void Scale(ref Vertices vertices, ref Vector2 value)
        {
            for (int i = 0; i < vertices.Count; i++)
                vertices[i] = Vector2.Multiply(vertices[i], value);
        }

        /// <summary>
        /// Rotate the vertices with the defined value in radians.
        /// </summary>
        /// <param name="value">The amount to rotate by in radians.</param>
        public static void Rotate(ref Vertices vertices, float value)
        {
            Matrix rotationMatrix;
            Matrix.CreateRotationZ(value, out rotationMatrix);

            for (int i = 0; i < vertices.Count; i++)
                vertices[i] = Vector2.Transform(vertices[i], rotationMatrix);
        }

        /// <summary>
        /// Determines whether the polygon is convex.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if it is convex; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsConvex(ref Vertices vertices)
        {
            bool isPositive = false;

            for (int i = 0; i < vertices.Count; ++i)
            {
                int lower = (i == 0) ? (vertices.Count - 1) : (i - 1);
                int middle = i;
                int upper = (i == vertices.Count - 1) ? (0) : (i + 1);

                float dx0 = vertices[middle].X - vertices[lower].X;
                float dy0 = vertices[middle].Y - vertices[lower].Y;
                float dx1 = vertices[upper].X - vertices[middle].X;
                float dy1 = vertices[upper].Y - vertices[middle].Y;

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

        public static float DistanceBetweenPointAndPoint(ref Vector2 point1, ref Vector2 point2)
        {
            Vector2 v;
            Vector2.Subtract(ref point1, ref point2, out v);
            return v.Length();
        }

        public static float DistanceBetweenPointAndLineSegment(ref Vector2 point, ref Vector2 lineEndPoint1, ref Vector2 lineEndPoint2)
        {
            Vector2 v = Vector2.Subtract(lineEndPoint2, lineEndPoint1);
            Vector2 w = Vector2.Subtract(point, lineEndPoint1);

            float c1 = Vector2.Dot(w, v);
            if (c1 <= 0) return DistanceBetweenPointAndPoint(ref point, ref lineEndPoint1);

            float c2 = Vector2.Dot(v, v);
            if (c2 <= c1) return DistanceBetweenPointAndPoint(ref point, ref lineEndPoint2);

            float b = c1 / c2;
            Vector2 pointOnLine = Vector2.Add(lineEndPoint1, Vector2.Multiply(v, b));
            return DistanceBetweenPointAndPoint(ref point, ref  pointOnLine);
        }

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

            Vector2? holeEntrance = null;
            Vector2? polygonEntrance = null;

            List<Vector2> blackList = new List<Vector2>();

            // First of all: Check the array you just got.
            if (pca.IsValid())
            {
                bool searchOn;
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

                                            int vertex2Index;
                                            int vertex1Index;
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
                            // 1:  95 / 151
                            // 2: 232 / 252
                            // 
                            while (GetNextHullEntrance(ref pca, polygonEntrance.Value, out polygonEntrance))
                            {
                                bool inPolygon = false;

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
            if (polygon != null && polygon.Count > 2)
            {
                Vector2 edgeVertex2 = polygon[polygon.Count - 1];

                Vector2 edgeVertex1;
                if (higherDetail)
                {
                    for (int i = 0; i < polygon.Count; i++)
                    {
                        edgeVertex1 = polygon[i];

                        if (DistanceBetweenPointAndLineSegment(ref point, ref edgeVertex1, ref edgeVertex2) <= pca.HullTolerance ||
                            DistanceBetweenPointAndPoint(ref point, ref edgeVertex1) <= pca.HullTolerance)
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

                        if (DistanceBetweenPointAndLineSegment(ref point, ref edgeVertex1, ref edgeVertex2) <= pca.HullTolerance)
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
            int nearestEdgeVertex1Index = 0;
            int nearestEdgeVertex2Index = 0;
            bool edgeFound = false;

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
                        float distance;
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

                            int edgeVertex2Index = polygon.Count - 1;

                            int edgeVertex1Index;
                            for (edgeVertex1Index = 0; edgeVertex1Index < polygon.Count; edgeVertex1Index++)
                            {
                                Vector2 tempVector1 = polygon[edgeVertex1Index];
                                Vector2 tempVector2 = polygon[edgeVertex2Index];
                                distance = DistanceBetweenPointAndLineSegment(ref foundEdgeCoord, ref tempVector1, ref tempVector2);
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
                                distance = DistanceBetweenPointAndPoint(ref tempVector, ref foundEdgeCoord);

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
            bool endOfHull = false;

            Vertices polygon = new Vertices();
            Vertices hullArea = new Vertices();
            Vertices endOfHullArea = new Vertices();

            Vector2 current = Vector2.Zero;

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
                polygon.Add(entrance);
                hullArea.Add(entrance);

                Vector2 next = entrance;

                do
                {
                    // Search in the pre vision list for an outstanding point.
                    Vector2 outstanding;
                    if (SearchForOutstandingVertex(ref hullArea, pca.HullTolerance, out outstanding))
                    {
                        if (endOfHull)
                        {
                            // We have found the next pixel, but is it on the last bit of the hull?
                            if (endOfHullArea.Contains(outstanding))
                            {
                                // Indeed.
                                polygon.Add(outstanding);
                            }

                            // That's enough, quit.
                            break;
                        }

                        // Add it and remove all vertices that don't matter anymore
                        // (all the vertices before the outstanding).
                        polygon.Add(outstanding);
                        hullArea.RemoveRange(0, hullArea.IndexOf(outstanding));
                    }

                    // Last point gets current and current gets next. Our little spider is moving forward on the hull ;).
                    last = current;
                    current = next;

                    // Get the next point on hull.
                    if (GetNextHullPoint(ref pca, ref last, ref current, out next))
                    {
                        // Add the vertex to a hull pre vision list.
                        hullArea.Add(next);
                    }
                    else
                    {
                        // Quit
                        break;
                    }

                    if (next == entrance && !endOfHull)
                    {
                        // It's the last bit of the hull, search on and exit at next found vertex.
                        endOfHull = true;
                        endOfHullArea.AddRange(hullArea);
                    }
                }
                while (true);
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
            Vector2 outstandingResult = Vector2.Zero;
            bool found = false;

            if (hullArea.Count > 2)
            {
                int hullAreaLastPoint = hullArea.Count - 1;

                Vector2 tempVector1;
                Vector2 tempVector2 = hullArea[0];
                Vector2 tempVector3 = hullArea[hullAreaLastPoint];

                // Search between the first and last hull point.
                for (int i = 1; i < hullAreaLastPoint; i++)
                {
                    tempVector1 = hullArea[i];

                    // Check if the distance is over the one that's tolerable.
                    if (DistanceBetweenPointAndLineSegment(ref tempVector1, ref  tempVector2, ref tempVector3) >= hullTolerance)
                    {
                        outstandingResult = hullArea[i];
                        found = true;
                        break;
                    }
                }
            }

            outstanding = outstandingResult;
            return found;
        }

        private static int GetIndexOfFirstPixelToCheck(Vector2 last, Vector2 current)
        {
            // .: pixel
            // l: last position
            // c: current position
            // f: first pixel for next search

            // f . .
            // l c .
            // . . .

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
        private byte _alphaTolerance;
        private uint _alphaToleranceRealValue;
        private float _hullTolerance;
        private int _holeDetectionLineStepSize;

        public uint[] Data { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

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

        public bool HoleDetection { get; set; }

        public bool MultipartDetection { get; set; }

        public PolygonCreationAssistance(uint[] data, int width, int height)
        {
            Data = data;
            Width = width;
            Height = height;

            AlphaTolerance = 20;
            HullTolerance = 1.5f;

            HoleDetectionLineStepSize = 1;

            HoleDetection = false;
            MultipartDetection = false;
        }

        public bool IsSolid(Vector2 pixel)
        {
            return IsSolid((int)pixel.X, (int)pixel.Y);
        }

        public bool IsSolid(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                return ((Data[x + y * Width] & 0xFF000000) >= _alphaToleranceRealValue);

            return false;
        }

        public bool IsSolid(int index)
        {
            if (index >= 0 && index < Width * Height)
                return ((Data[index] & 0xFF000000) >= _alphaToleranceRealValue);

            return false;
        }

        public bool InBounds(Vector2 coord)
        {
            return (coord.X >= 0f && coord.X < Width && coord.Y >= 0f && coord.Y < Height);
        }

        public bool IsValid()
        {
            if (Data != null && Data.Length > 0)
                return Data.Length == Width * Height;

            return false;
        }

        ~PolygonCreationAssistance()
        {
            Data = null;
        }
    }
    #endregion
}
