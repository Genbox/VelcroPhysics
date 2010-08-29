using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common.PolygonManipulation
{
    public static class BooleanTools
    {
        // Boolean polygon operations contributed by DrDeth

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
            int startingIndex = PreparePolygons(polygon1, polygon2, out poly1, out poly2, out intersections,
                                                out gotError);

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
            if (!VerticesIntersect(poly1, poly2, out intersections))
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
                                              out List<EdgeIntersectInfo> intersections)
        {
            intersections = new List<EdgeIntersectInfo>();

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

                    // Check if the edges intersect
                    if (LineTools.LineIntersect(p1, p2, p3, p4, true, true, out point))
                    {
                        // Here, we round the returned intersection point to its nearest whole number.
                        // This prevents floating point anomolies where 99.9999-> is returned instead of 100.
                        point = new Vector2((float) Math.Round(point.X, 0), (float) Math.Round(point.Y, 0));
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

        //TODO: Reenable the rounding if it is necessary for the boolean tools to work.
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
    }

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
}