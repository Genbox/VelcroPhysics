using System.Collections.Generic;

#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Collisions
{
    //TODO: This is not used, remove it or use it.
    /// <summary>
    /// TODO: Write documentation
    /// </summary>
    public static class CollisionHelper
    {
        private const float _defaultFloatTolerance = .00001f;

        /// <summary>
        /// Checks if a floating point Value is equal to another,
        /// within a certain tolerance.
        /// </summary>
        /// <param name="a">The first floating point Value.</param>
        /// <param name="b">The second floating point Value.</param>
        /// <param name="delta">The floating point tolerance.</param>
        /// <returns>True if the values are "equal", false otherwise.</returns>
        public static bool FloatEquals(float a, float b, float delta)
        {
            return FloatInRange(a, b - delta, b + delta);
        }

        /// <summary>
        /// Checks if a floating point Value is within a specified
        /// range of values (inclusive).
        /// </summary>
        /// <param name="a">The Value to check.</param>
        /// <param name="min">The minimum Value.</param>
        /// <param name="max">The maximum Value.</param>
        /// <returns>True if the Value is within the range specified,
        /// false otherwise.</returns>
        public static bool FloatInRange(float a, float min, float max)
        {
            return (a >= min && a <= max);
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
        /// <param name="p1">The first point of the first line segment.</param>
        /// <param name="p2">The second point of the first line segment.</param>
        /// <param name="p3">The first point of the second line segment.</param>
        /// <param name="p4">The second point of the second line segment.</param>
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
        public static bool LineIntersect(ref Vector2 p1, ref Vector2 p2, ref Vector2 p3, ref Vector2 p4,
                                         bool firstIsSegment, bool secondIsSegment, float floatTolerance,
                                         out Vector2 point)
        {
            bool ret = false;
            point = new Vector2();

            // these are reused later.
            // each lettered sub-calculation is used twice, except
            // for b and d, which are used 3 times
            float a = p4.Y - p3.Y;
            float b = p2.X - p1.X;
            float c = p4.X - p3.X;
            float d = p2.Y - p1.Y;

            // denominator to solution of linear system
            //float denom = ((p4.Y - p3.Y) * (p2.X - p1.X)) -
            // ((p4.X - p3.X) * (p2.Y - p1.Y));
            float denom = (a*b) - (c*d);

            // if denominator is 0, then lines are parallel
            if (!FloatEquals(denom, 0f, floatTolerance))
            {
                float e = p1.Y - p3.Y;
                float f = p1.X - p3.X;
                float oneOverDenom = 1.0f/denom;

                // numerator of first equation
                //float ua = ((p4.X - p3.X) * (p1.Y - p3.Y)) - 
                // ((p4.Y - p3.Y) * (p1.X - p3.X));
                float ua = (c*e) - (a*f);
                ua *= oneOverDenom;

                // check if intersection point of the two lines is on line segment 1
                if (!firstIsSegment || FloatInRange(ua, 0f, 1f))
                {
                    // numerator of second equation
                    //float ub = ((p2.X - p1.X) * (p1.Y - p3.Y)) - 
                    // ((p2.Y - p1.Y) * (p1.X - p3.X));
                    float ub = (b*e) - (d*f);
                    ub *= oneOverDenom;

                    // check if intersection point of the two lines is on line segment 2
                    // means the line segments intersect, since we know it is on
                    // segment 1 as well.
                    if (!secondIsSegment || FloatInRange(ub, 0f, 1f))
                    {
                        // check if they are coincident (no collision in this case)
                        if (!(FloatEquals(ua, 0f, floatTolerance) &&
                              FloatEquals(ub, 0f, floatTolerance)))
                        {
                            ret = true;
                            //intersectPoint.X = p1.X + ua * (p2.X - p1.X);
                            //intersectPoint.Y = p1.Y + ua * (p2.Y - p1.Y);

                            point.X = p1.X + ua*b;
                            point.Y = p1.Y + ua*d;
                        } // end if
                    } // end if
                } // end if
            }

            return ret;
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
        /// <param name="p1">The first point of the first line segment.</param>
        /// <param name="p2">The second point of the first line segment.</param>
        /// <param name="p3">The first point of the second line segment.</param>
        /// <param name="p4">The second point of the second line segment.</param>
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
        public static bool LineIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, bool firstIsSegment,
                                         bool secondIsSegment, float floatTolerance, out Vector2 point)
        {
            return LineIntersect(ref p1, ref p2, ref p3, ref p4, firstIsSegment, secondIsSegment, floatTolerance,
                                 out point);
        }

        /// <summary>
        /// This method detects if two line segments intersect,
        /// and, if so, the point of intersection. 
        /// Note: If two line segments are coincident, then 
        /// no intersection is detected (there are actually
        /// infinite intersection points).
        /// </summary>
        /// <param name="p1">The first point of the first line segment.</param>
        /// <param name="p2">The second point of the first line segment.</param>
        /// <param name="p3">The first point of the second line segment.</param>
        /// <param name="p4">The second point of the second line segment.</param>
        /// <param name="point">This is set to the intersection
        /// point if an intersection is detected.</param>
        /// <returns>True if an intersection is detected, false otherwise.</returns>
        public static bool LineIntersect(ref Vector2 p1, ref Vector2 p2, ref Vector2 p3, ref Vector2 p4,
                                         out Vector2 point)
        {
            return LineIntersect(ref p1, ref p2, ref p3, ref p4, true, true, _defaultFloatTolerance, out point);
        }

        /// <summary>
        /// This method detects if two line segments intersect,
        /// and, if so, the point of intersection. 
        /// Note: If two line segments are coincident, then 
        /// no intersection is detected (there are actually
        /// infinite intersection points).
        /// </summary>
        /// <param name="p1">The first point of the first line segment.</param>
        /// <param name="p2">The second point of the first line segment.</param>
        /// <param name="p3">The first point of the second line segment.</param>
        /// <param name="p4">The second point of the second line segment.</param>
        /// <param name="point">This is set to the intersection
        /// point if an intersection is detected.</param>
        /// <returns>True if an intersection is detected, false otherwise.</returns>
        public static bool LineIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 point)
        {
            return LineIntersect(ref p1, ref p2, ref p3, ref p4, true, true, _defaultFloatTolerance, out point);
        }

        /// <summary>
        /// Get all intersections between a line segment and a list of vertices
        /// representing a polygon. The vertices reuse adjacent points, so for example
        /// edges one and two are between the first and second vertices and between the
        /// second and third vertices. The last edge is between vertex vertices.Count - 1
        /// and verts0. (ie, vertices from a Geometry or AABB)
        /// </summary>
        /// <param name="p1">The first point of the line segment to test</param>
        /// <param name="p2">The second point of the line segment to test.</param>
        /// <param name="vertices">The vertices, as described above</param>
        /// <param name="points">An list of intersection points. Any intersection points
        /// found will be added to this list.</param>
        public static void LineSegmentVerticesIntersect(ref Vector2 p1, ref Vector2 p2, Vertices vertices,
                                                        ref List<Vector2> points)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector2 point;
                if (LineIntersect(vertices[i], vertices[vertices.NextIndex(i)],
                                  p1, p2, true, true, _defaultFloatTolerance, out point))
                {
                    points.Add(point);
                }
            }
        }

        /// <summary>
        /// Get all intersections between a line segment and an AABB. 
        /// </summary>
        /// <param name="p1">The first point of the line segment to test</param>
        /// <param name="p2">The second point of the line segment to test.</param>
        /// <param name="aabb">The AABB that is used for testing intersection.</param>
        /// <param name="points">An list of intersection points. Any intersection points found will be added to this list.</param>
        public static void LineSegmentAABBIntersect(ref Vector2 p1, ref Vector2 p2, AABB aabb, ref List<Vector2> points)
        {
            LineSegmentVerticesIntersect(ref p1, ref p2, aabb.GetVertices(), ref points);
        }

        /// <summary>
        /// Detects all collision points between a line and a Geom. If intersections exist
        /// a LineIntersectionInfo  object is created and added to an existing list of such
        /// objects. </summary>
        /// <param name="p1">The first point of the line segment to test</param>
        /// <param name="p2">The second point of the line segment to test</param>
        /// <param name="geom">The geometry to test.</param>
        /// <param name="detectUsingAABB">If true, intersection will be tested using the
        /// Geoms AABB. If false, the Geoms vertices will be used.</param>
        /// <param name="lineIntersectInfoList">An existing intersect info list to add to
        /// </param>
        public static void LineSegmentGeomIntersect(Vector2 p1, Vector2 p2, Geom geom, bool detectUsingAABB,
                                                    ref List<LineIntersectInfo> lineIntersectInfoList)
        {
            LineSegmentGeomIntersect(ref p1, ref p2, geom, detectUsingAABB, ref lineIntersectInfoList);
        }

        /// <summary>
        /// Detects all collision points between a line and a Geom. If intersections exist a LineIntersectionInfo 
        /// object is created and added to an existing list of such objects.
        /// </summary>
        /// <param name="p1">The first point of the line segment to test</param>
        /// <param name="p2">The second point of the line segment to test</param>
        /// <param name="geom">The geometry to test.</param>
        /// <param name="detectUsingAABB">If true, intersection will be tested using the Geoms AABB. If false, the Geoms vertices will be used.</param>
        /// <param name="lineIntersectInfoList">An existing intersect info list to add to</param>
        public static void LineSegmentGeomIntersect(ref Vector2 p1, ref Vector2 p2, Geom geom, bool detectUsingAABB,
                                                    ref List<LineIntersectInfo> lineIntersectInfoList)
        {
            List<Vector2> points = new List<Vector2>();

            if (detectUsingAABB)
            {
                LineSegmentAABBIntersect(ref p1, ref p2, geom.AABB, ref points);
                if (points.Count > 0)
                {
                    lineIntersectInfoList.Add(new LineIntersectInfo(geom, points));
                }
            }
            else
            {
                LineSegmentVerticesIntersect(ref p1, ref p2, geom.WorldVertices, ref points);
                if (points.Count > 0)
                {
                    lineIntersectInfoList.Add(new LineIntersectInfo(geom, points));
                }
            }
        }
    }
}