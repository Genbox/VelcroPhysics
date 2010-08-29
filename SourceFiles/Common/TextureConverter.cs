using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common
{
    public static class TextureConverter
    {
        //User contribution from Sickbattery

        /// <summary>
        /// TODO:
        /// 1.) Das Array welches ich bekomme am besten in einen bool array verwandeln. Würde die Geschwindigkeit verbessern
        /// </summary>
        private static readonly int[,] ClosePixels = new int[8,2]
                                                         {
                                                             {-1, -1}, {0, -1}, {1, -1}, {1, 0}, {1, 1}, {0, 1}, {-1, 1}
                                                             ,
                                                             {-1, 0}
                                                         };

        public static Vertices CreateVertices(uint[] data, int width, int height)
        {
            PolygonCreationAssistance pca = new PolygonCreationAssistance(data, width, height);
            List<Vertices> verts = CreateVertices(pca);

            return verts[0];
        }

        public static Vertices CreateVertices(uint[] data, int width, int height, bool holeDetection)
        {
            PolygonCreationAssistance pca = new PolygonCreationAssistance(data, width, height);
            pca.HoleDetection = holeDetection;
            List<Vertices> verts = CreateVertices(pca);

            return verts[0];
        }

        public static List<Vertices> CreateVertices(uint[] data, int width, int height, float hullTolerance,
                                                    byte alphaTolerance, bool multiPartDetection, bool holeDetection)
        {
            PolygonCreationAssistance pca = new PolygonCreationAssistance(data, width, height);
            pca.HullTolerance = hullTolerance;
            pca.AlphaTolerance = alphaTolerance;
            pca.MultipartDetection = multiPartDetection;
            pca.HoleDetection = holeDetection;
            return CreateVertices(pca);
        }

        private static List<Vertices> CreateVertices(PolygonCreationAssistance pca)
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
                        polygon = CreateSimplePolygon(pca, Vector2.Zero, Vector2.Zero);

                        if (polygon != null && polygon.Count > 2)
                        {
                            polygonEntrance = GetTopMostVertex(polygon);
                        }
                    }
                    else if (polygonEntrance.HasValue)
                    {
                        polygon = CreateSimplePolygon(pca, polygonEntrance.Value,
                                                      new Vector2(polygonEntrance.Value.X - 1f, polygonEntrance.Value.Y));
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
                                holeEntrance = GetHoleHullEntrance(pca, polygon, holeEntrance);

                                if (holeEntrance.HasValue)
                                {
                                    if (!blackList.Contains(holeEntrance.Value))
                                    {
                                        blackList.Add(holeEntrance.Value);
                                        holePolygon = CreateSimplePolygon(pca, holeEntrance.Value,
                                                                          new Vector2(holeEntrance.Value.X + 1,
                                                                                      holeEntrance.Value.Y));

                                        if (holePolygon != null && holePolygon.Count > 2)
                                        {
                                            holePolygon.Add(holePolygon[0]);

                                            int vertex2Index;
                                            int vertex1Index;
                                            if (SplitPolygonEdge(polygon, EdgeAlignment.Vertical, holeEntrance.Value,
                                                                 out vertex1Index, out vertex2Index))
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
                            while (GetNextHullEntrance(pca, polygonEntrance.Value, out polygonEntrance))
                            {
                                bool inPolygon = false;

                                for (int i = 0; i < polygons.Count; i++)
                                {
                                    polygon = polygons[i];

                                    if (InPolygon(pca, ref polygon, polygonEntrance.Value))
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
                throw new Exception(
                    "Sizes don't match: Color array must contain texture width * texture height elements.");
            }

            return polygons;
        }

        private static Vector2? GetHoleHullEntrance(PolygonCreationAssistance pca, Vertices polygon,
                                                    Vector2? startVertex)
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
                    startLine = (int) startVertex.Value.Y;
                }
                else
                {
                    startLine = (int) GetTopMostCoord(polygon);
                }
                endLine = (int) GetBottomMostCoord(polygon);

                if (startLine > 0 && startLine < pca.Height && endLine > 0 && endLine < pca.Height)
                {
                    // go from top to bottom of the polygon
                    for (int y = startLine; y <= endLine; y += pca.HoleDetectionLineStepSize)
                    {
                        // get x-coord of every polygon edge which crosses y
                        edges = GetCrossingEdges(polygon, EdgeAlignment.Vertical, y);

                        // we need an even number of crossing edges
                        if (edges.Count > 1 && edges.Count % 2 == 0)
                        {
                            for (int i = 0; i < edges.Count; i += 2)
                            {
                                foundSolid = false;
                                foundTransparent = false;

                                for (int x = (int) edges[i].CrossingPoint.X;
                                     x <= (int) edges[i + 1].CrossingPoint.X;
                                     x++)
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

                                            if (DistanceToHullAcceptable(pca, polygon, entrance.Value, true))
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

        private static bool DistanceToHullAcceptable(PolygonCreationAssistance pca, Vertices polygon, Vector2 point,
                                                     bool higherDetail)
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

                        if (LineTools.DistanceBetweenPointAndLineSegment(ref point, ref edgeVertex1, ref edgeVertex2) <=
                            pca.HullTolerance ||
                            LineTools.DistanceBetweenPointAndPoint(ref point, ref edgeVertex1) <= pca.HullTolerance)
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

                        if (LineTools.DistanceBetweenPointAndLineSegment(ref point, ref edgeVertex1, ref edgeVertex2) <=
                            pca.HullTolerance)
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

        private static bool InPolygon(PolygonCreationAssistance pca, ref Vertices polygon, Vector2 point)
        {
            bool inPolygon = !DistanceToHullAcceptable(pca, polygon, point, true);

            if (!inPolygon)
            {
                List<CrossingEdgeInfo> edges = GetCrossingEdges(polygon, EdgeAlignment.Vertical, (int) point.Y);

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

            return true;
        }

        private static Vector2? GetTopMostVertex(Vertices vertices)
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

        private static float GetTopMostCoord(Vertices vertices)
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

        private static float GetBottomMostCoord(Vertices vertices)
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

        private static List<CrossingEdgeInfo> GetCrossingEdges(Vertices polygon, EdgeAlignment edgeAlign, int checkLine)
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

                            if ((edgeVertex1.Y >= checkLine && edgeVertex2.Y <= checkLine) ||
                                (edgeVertex1.Y <= checkLine && edgeVertex2.Y >= checkLine))
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
                                        crossingPoint =
                                            new Vector2((checkLine - edgeVertex1.Y) / slope.Y * slope.X + edgeVertex1.X,
                                                        checkLine);
                                        edges.Add(new CrossingEdgeInfo(edgeVertex1, edgeVertex2, crossingPoint,
                                                                       edgeAlign));
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

        private static bool SplitPolygonEdge(Vertices polygon, EdgeAlignment edgeAlign, Vector2 coordInsideThePolygon,
                                             out int vertex1Index, out int vertex2Index)
        {
            List<CrossingEdgeInfo> edges;

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
                    edges = GetCrossingEdges(polygon, EdgeAlignment.Vertical, (int) coordInsideThePolygon.Y);

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
                                distance = LineTools.DistanceBetweenPointAndLineSegment(ref foundEdgeCoord,
                                                                                        ref tempVector1, ref tempVector2);
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
                                distance = LineTools.DistanceBetweenPointAndPoint(ref tempVector, ref foundEdgeCoord);

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

        private static Vertices CreateSimplePolygon(PolygonCreationAssistance pca, Vector2 entrance, Vector2 last)
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
                entranceFound = GetHullEntrance(pca, out entrance);

                if (entranceFound)
                {
                    current = new Vector2(entrance.X - 1f, entrance.Y);
                }
            }
            else
            {
                if (pca.IsSolid(entrance))
                {
                    if (IsNearPixel(pca, entrance, last))
                    {
                        current = last;
                        entranceFound = true;
                    }
                    else
                    {
                        Vector2 temp;
                        if (SearchNearPixels(pca, false, entrance, out temp))
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
                    if (SearchForOutstandingVertex(hullArea, pca.HullTolerance, out outstanding))
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
                    if (GetNextHullPoint(pca, ref last, ref current, out next))
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
                } while (true);
            }

            return polygon;
        }

        private static bool SearchNearPixels(PolygonCreationAssistance pca, bool searchingForSolidPixel, Vector2 current,
                                             out Vector2 foundPixel)
        {
            int x;
            int y;

            for (int i = 0; i < 8; i++)
            {
                x = (int) current.X + ClosePixels[i, 0];
                y = (int) current.Y + ClosePixels[i, 1];

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

        private static bool IsNearPixel(PolygonCreationAssistance pca, Vector2 current, Vector2 near)
        {
            for (int i = 0; i < 8; i++)
            {
                int x = (int) current.X + ClosePixels[i, 0];
                int y = (int) current.Y + ClosePixels[i, 1];

                if (x >= 0 && x <= pca.Width && y >= 0 && y <= pca.Height)
                {
                    if (x == (int) near.X && y == (int) near.Y)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool GetHullEntrance(PolygonCreationAssistance pca, out Vector2 entrance)
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

        private static bool GetNextHullEntrance(PolygonCreationAssistance pca, Vector2 start, out Vector2? entrance)
        {
            // Search for first solid pixel.
            int size = pca.Height * pca.Width;
            int x;

            bool foundTransparent = false;

            for (int i = (int) start.X + (int) start.Y * pca.Width; i <= size; i++)
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

        private static bool GetNextHullPoint(PolygonCreationAssistance pca, ref Vector2 last, ref Vector2 current,
                                             out Vector2 next)
        {
            int x;
            int y;

            int indexOfFirstPixelToCheck = GetIndexOfFirstPixelToCheck(last, current);
            int indexOfPixelToCheck;

            const int pixelsToCheck = 8; // _closePixels.Length;

            for (int i = 0; i < pixelsToCheck; i++)
            {
                indexOfPixelToCheck = (indexOfFirstPixelToCheck + i) % pixelsToCheck;

                x = (int) current.X + ClosePixels[indexOfPixelToCheck, 0];
                y = (int) current.Y + ClosePixels[indexOfPixelToCheck, 1];

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

        private static bool SearchForOutstandingVertex(Vertices hullArea, float hullTolerance, out Vector2 outstanding)
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
                    if (
                        LineTools.DistanceBetweenPointAndLineSegment(ref tempVector1, ref tempVector2, ref tempVector3) >=
                        hullTolerance)
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
    }

    public enum EdgeAlignment
    {
        Vertical = 0,
        Horizontal = 1
    }

    public sealed class CrossingEdgeInfo : IComparable
    {
        #region Attributes

        private EdgeAlignment _alignment;
        private Vector2 _crossingPoint;
        private Vector2 _edgeVertex2;
        private Vector2 _egdeVertex1;

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

        public CrossingEdgeInfo(Vector2 edgeVertex1, Vector2 edgeVertex2, Vector2 crossingPoint,
                                EdgeAlignment checkLineAlignment)
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
            CrossingEdgeInfo cei = (CrossingEdgeInfo) obj;
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
    public sealed class PolygonCreationAssistance
    {
        private byte _alphaTolerance;
        private uint _alphaToleranceRealValue;
        private int _holeDetectionLineStepSize;
        private float _hullTolerance;

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

        private uint[] Data { get; set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public byte AlphaTolerance
        {
            get { return _alphaTolerance; }
            set
            {
                _alphaTolerance = value;
                _alphaToleranceRealValue = (uint) value << 24;
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
            private set
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

        public bool IsSolid(Vector2 pixel)
        {
            return IsSolid((int) pixel.X, (int) pixel.Y);
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
    }
}