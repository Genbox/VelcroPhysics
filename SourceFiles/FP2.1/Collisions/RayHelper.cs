using System.Collections.Generic;

#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Collisions
{
    /// <summary>
    /// Collection of helper methods for misc collisions.
    /// Does float tolerance and line collisions with lines and AABBs.
    /// </summary>
    public static class RayHelper
    {
        private const float _epsilon = .00001f;

        /// <summary>
        /// Checks if a floating point Value is equal to another,
        /// within a certain tolerance.
        /// </summary>
        /// <param name="value1">The first floating point Value.</param>
        /// <param name="value2">The second floating point Value.</param>
        /// <param name="delta">The floating point tolerance.</param>
        /// <returns>True if the values are "equal", false otherwise.</returns>
        public static bool FloatEquals(float value1, float value2, float delta)
        {
            return FloatInRange(value1, value2 - delta, value2 + delta);
        }

        /// <summary>
        /// Checks if a floating point Value is within a specified
        /// range of values (inclusive).
        /// </summary>
        /// <param name="value">The Value to check.</param>
        /// <param name="min">The minimum Value.</param>
        /// <param name="max">The maximum Value.</param>
        /// <returns>True if the Value is within the range specified,
        /// false otherwise.</returns>
        public static bool FloatInRange(float value, float min, float max)
        {
            return (value >= min && value <= max);
        }

        /// <summary>
        /// This method detects if two line segments (or lines) intersect,
        /// and, if so, the point of intersection. Use the <paramref name="firstIsSegment"/> and
        /// <paramref name="secondIsSegment"/> parameters to set whether the intersection point
        /// must be on the first and second line segments. Setting these
        /// both to true means you are doing a line-segment to line-segment
        /// intersection. Setting one of them to true means you are doing a
        /// line to line-segment intersection test, and so on.
        /// Note: If two line segments are coincident, then 
        /// no intersection is detected (there are actually
        /// infinite intersection points).
        /// Author: Jeremy Bell
        /// </summary>
        /// <param name="point1">The first point of the first line segment.</param>
        /// <param name="point2">The second point of the first line segment.</param>
        /// <param name="point3">The first point of the second line segment.</param>
        /// <param name="point4">The second point of the second line segment.</param>
        /// <param name="point">This is set to the intersection
        /// point if an intersection is detected.</param>
        /// <param name="firstIsSegment">Set this to true to require that the 
        /// intersection point be on the first line segment.</param>
        /// <param name="secondIsSegment">Set this to true to require that the
        /// intersection point be on the second line segment.</param>
        /// <param name="floatTolerance">Some of the calculations require
        /// checking if a floating point Value equals another. This is
        /// the tolerance that is used to determine this (ie Value +
        /// or - <paramref name="floatTolerance"/>)</param>
        /// <returns>True if an intersection is detected, false otherwise.</returns>
        public static bool LineIntersect(ref Vector2 point1, ref Vector2 point2, ref Vector2 point3, ref Vector2 point4,
                                 bool firstIsSegment, bool secondIsSegment, float floatTolerance,
                                 out Vector2 point)
        {
            point = new Vector2();

            // these are reused later.
            // each lettered sub-calculation is used twice, except
            // for b and d, which are used 3 times
            float a = point4.Y - point3.Y;
            float b = point2.X - point1.X;
            float c = point4.X - point3.X;
            float d = point2.Y - point1.Y;

            // denominator to solution of linear system
            float denom = (a * b) - (c * d);

            // if denominator is 0, then lines are parallel
            if (!(denom >= -_epsilon && denom <= _epsilon))
            {
                float e = point1.Y - point3.Y;
                float f = point1.X - point3.X;
                float oneOverDenom = 1.0f / denom;

                // numerator of first equation
                float ua = (c * e) - (a * f);
                ua *= oneOverDenom;

                // check if intersection point of the two lines is on line segment 1
                if (!firstIsSegment || ua >= 0.0f && ua <= 1.0f)
                {
                    // numerator of second equation
                    float ub = (b * e) - (d * f);
                    ub *= oneOverDenom;

                    // check if intersection point of the two lines is on line segment 2
                    // means the line segments intersect, since we know it is on
                    // segment 1 as well.
                    if (!secondIsSegment || ub >= 0.0f && ub <= 1.0f)
                    {
                        // check if they are coincident (no collision in this case)
                        if (ua != 0f && ub != 0f)
                        {
                            //There is an intersection
                            point.X = point1.X + ua * b;
                            point.Y = point1.Y + ua * d;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// This method detects if two line segments (or lines) intersect,
        /// and, if so, the point of intersection. Use the <paramref name="firstIsSegment"/> and
        /// <paramref name="secondIsSegment"/> parameters to set whether the intersection point
        /// must be on the first and second line segments. Setting these
        /// both to true means you are doing a line-segment to line-segment
        /// intersection. Setting one of them to true means you are doing a
        /// line to line-segment intersection test, and so on.
        /// Note: If two line segments are coincident, then 
        /// no intersection is detected (there are actually
        /// infinite intersection points).
        /// Author: Jeremy Bell
        /// </summary>
        /// <param name="point1">The first point of the first line segment.</param>
        /// <param name="point2">The second point of the first line segment.</param>
        /// <param name="point3">The first point of the second line segment.</param>
        /// <param name="point4">The second point of the second line segment.</param>
        /// <param name="intersectionPoint">This is set to the intersection
        /// point if an intersection is detected.</param>
        /// <param name="firstIsSegment">Set this to true to require that the 
        /// intersection point be on the first line segment.</param>
        /// <param name="secondIsSegment">Set this to true to require that the
        /// intersection point be on the second line segment.</param>
        /// <param name="floatTolerance">Some of the calculations require
        /// checking if a floating point Value equals another. This is
        /// the tolerance that is used to determine this (ie Value +
        /// or - <paramref name="floatTolerance"/>)</param>
        /// <returns>True if an intersection is detected, false otherwise.</returns>
        public static bool LineIntersect(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, bool firstIsSegment,
                                         bool secondIsSegment, float floatTolerance, out Vector2 intersectionPoint)
        {
            return LineIntersect(ref point1, ref point2, ref point3, ref point4, firstIsSegment, secondIsSegment, floatTolerance,
                                 out intersectionPoint);
        }

        /// <summary>
        /// This method detects if two line segments intersect,
        /// and, if so, the point of intersection. 
        /// Note: If two line segments are coincident, then 
        /// no intersection is detected (there are actually
        /// infinite intersection points).
        /// </summary>
        /// <param name="point1">The first point of the first line segment.</param>
        /// <param name="point2">The second point of the first line segment.</param>
        /// <param name="point3">The first point of the second line segment.</param>
        /// <param name="point4">The second point of the second line segment.</param>
        /// <param name="intersectionPoint">This is set to the intersection
        /// point if an intersection is detected.</param>
        /// <returns>True if an intersection is detected, false otherwise.</returns>
        public static bool LineIntersect(ref Vector2 point1, ref Vector2 point2, ref Vector2 point3, ref Vector2 point4,
                                         out Vector2 intersectionPoint)
        {
            return LineIntersect(ref point1, ref point2, ref point3, ref point4, true, true, _epsilon, out intersectionPoint);
        }

        /// <summary>
        /// This method detects if two line segments intersect,
        /// and, if so, the point of intersection. 
        /// Note: If two line segments are coincident, then 
        /// no intersection is detected (there are actually
        /// infinite intersection points).
        /// </summary>
        /// <param name="point1">The first point of the first line segment.</param>
        /// <param name="point2">The second point of the first line segment.</param>
        /// <param name="point3">The first point of the second line segment.</param>
        /// <param name="point4">The second point of the second line segment.</param>
        /// <param name="intersectionPoint">This is set to the intersection
        /// point if an intersection is detected.</param>
        /// <returns>True if an intersection is detected, false otherwise.</returns>
        public static bool LineIntersect(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, out Vector2 intersectionPoint)
        {
            return LineIntersect(ref point1, ref point2, ref point3, ref point4, true, true, _epsilon, out intersectionPoint);
        }

        /// <summary>
        /// Get all intersections between a line segment and a list of vertices
        /// representing a polygon. The vertices reuse adjacent points, so for example
        /// edges one and two are between the first and second vertices and between the
        /// second and third vertices. The last edge is between vertex vertices.Count - 1
        /// and verts0. (ie, vertices from a Geometry or AABB)
        /// </summary>
        /// <param name="point1">The first point of the line segment to test</param>
        /// <param name="point2">The second point of the line segment to test.</param>
        /// <param name="vertices">The vertices, as described above</param>
        /// <param name="intersectionPoints">An list of intersection points. Any intersection points
        /// found will be added to this list.</param>
        public static void LineSegmentVerticesIntersect(ref Vector2 point1, ref Vector2 point2, Vertices vertices,
                                                        ref List<Vector2> intersectionPoints)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector2 point;
                if (LineIntersect(vertices[i], vertices[vertices.NextIndex(i)],
                                  point1, point2, true, true, _epsilon, out point))
                {
                    intersectionPoints.Add(point);
                }
            }
        }

        /// <summary>
        /// Get all intersections between a line segment and an AABB. 
        /// </summary>
        /// <param name="point1">The first point of the line segment to test</param>
        /// <param name="point2">The second point of the line segment to test.</param>
        /// <param name="aabb">The AABB that is used for testing intersection.</param>
        /// <param name="intersectionPoints">An list of intersection points. Any intersection points found will be added to this list.</param>
        public static void LineSegmentAABBIntersect(ref Vector2 point1, ref Vector2 point2, AABB aabb, ref List<Vector2> intersectionPoints)
        {
            LineSegmentVerticesIntersect(ref point1, ref point2, aabb.GetVertices(), ref intersectionPoints);
        }

        /// <summary>
        /// Detects all collision points between a line and a Geom. If intersections exist
        /// a LineIntersectionInfo  object is created and added to an existing list of such
        /// objects. </summary>
        /// <param name="point1">The first point of the line segment to test</param>
        /// <param name="point2">The second point of the line segment to test</param>
        /// <param name="geom">The geometry to test.</param>
        /// <param name="detectUsingAABB">If true, intersection will be tested using the
        /// Geoms AABB. If false, the Geoms vertices will be used.</param>
        /// <param name="intersectionPoints">An existing points info list to add to
        /// </param>
        public static void LineSegmentGeomIntersect(Vector2 point1, Vector2 point2, Geom geom, bool detectUsingAABB,
                                                    ref List<Vector2> intersectionPoints)
        {
            LineSegmentGeomIntersect(ref point1, ref point2, geom, detectUsingAABB, ref intersectionPoints);
        }

        /// <summary>
        /// Detects all collision points between a line and a Geom. If intersections exist a LineIntersectionInfo 
        /// object is created and added to an existing list of such objects.
        /// </summary>
        /// <param name="point1">The first point of the line segment to test</param>
        /// <param name="point2">The second point of the line segment to test</param>
        /// <param name="geom">The geometry to test.</param>
        /// <param name="detectUsingAABB">If true, intersection will be tested using the Geoms AABB. If false, the Geoms vertices will be used.</param>
        /// <param name="intersectionPoints">An existing point list to add to</param>
        public static void LineSegmentGeomIntersect(ref Vector2 point1, ref Vector2 point2, Geom geom, bool detectUsingAABB,
                                                    ref List<Vector2> intersectionPoints)
        {
            if (detectUsingAABB)
            {
                LineSegmentAABBIntersect(ref point1, ref point2, geom.AABB, ref intersectionPoints);
            }
            else
            {
                LineSegmentVerticesIntersect(ref point1, ref point2, geom.WorldVertices, ref intersectionPoints);
            }
        }

        /// <summary>
        /// Gets a list of geometries and their intersectionpoints that the line intersects.
        /// </summary>
        /// <param name="point1">The point1.</param>
        /// <param name="point2">The point2.</param>
        /// <param name="simulator">The simulator.</param>
        /// <param name="detectUsingAABB">if set to <c>true</c> [detect using AABB].</param>
        /// <returns></returns>
        public static List<GeomPointPair> LineSegmentAllGeomsIntersect(Vector2 point1, Vector2 point2, PhysicsSimulator simulator, bool detectUsingAABB)
        {
            return LineSegmentAllGeomsIntersect(ref point1, ref point2, simulator, detectUsingAABB);
        }

        /// <summary>
        /// Gets a list of geometries and their intersectionpoints that the line intersects.
        /// </summary>
        /// <param name="point1">The point1.</param>
        /// <param name="point2">The point2.</param>
        /// <param name="simulator">The simulator.</param>
        /// <param name="detectUsingAABB">if set to <c>true</c> [detect using AABB].</param>
        /// <returns></returns>
        public static List<GeomPointPair> LineSegmentAllGeomsIntersect(ref Vector2 point1, ref Vector2 point2, PhysicsSimulator simulator, bool detectUsingAABB)
        {
            List<Vector2> intSecPoints = new List<Vector2>();
            List<GeomPointPair> geoms = new List<GeomPointPair>();

            foreach (Geom geom in simulator.GeomList)
            {
                intSecPoints.Clear();

                if (detectUsingAABB)
                {
                    LineSegmentAABBIntersect(ref point1, ref point2, geom.AABB, ref intSecPoints);
                }
                else
                {
                    LineSegmentVerticesIntersect(ref point1, ref point2, geom.WorldVertices, ref intSecPoints);
                }

                if (intSecPoints.Count > 0)
                {
                    _tempPair = new GeomPointPair();
                    _tempPair.Geom = geom;
                    _tempPair.Points = new List<Vector2>(intSecPoints);
                    geoms.Add(_tempPair);
                }
            }

            return geoms;
        }

        private static GeomPointPair _tempPair;
    }

    public struct GeomPointPair
    {
        public Geom Geom;
        public List<Vector2> Points;
    }
}