using System.Collections.Generic;
using FarseerPhysics.Common.PolygonManipulation;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common.Decomposition
{
    //From phed rev 36

    /// <summary>
    /// Convex decomposition algorithm created by Mark Bayazit (http://mnbayazit.com/)
    /// For more information about this algorithm, see http://mnbayazit.com/406/bayazit
    /// </summary>
    public static class BayazitDecomposer
    {
        private static Vector2 At(int i, Vertices vertices)
        {
            int s = vertices.Count;
            return vertices[i < 0 ? s - (-i % s) : i % s];
        }

        private static Vertices Copy(int i, int j, Vertices vertices)
        {
            Vertices p = new Vertices();
            while (j < i) j += vertices.Count;
            //p.reserve(j - i + 1);
            for (; i <= j; ++i)
            {
                p.Add(At(i, vertices));
            }
            return p;
        }

        /// <summary>
        /// Precondition: Counter Clockwise polygon
        /// Decompose the polygon into several smaller non-concave polygon.
        /// If the polygon is already convex, it will return the original polygon, unless it is over Settings.MaxPolygonVertices.
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public static List<Vertices> ConvexPartition(Vertices vertices)
        {
            List<Vertices> list = new List<Vertices>();
            float d, dist1, dist2;
            Vector2 ip;
            Vector2 ip1 = new Vector2();
            Vector2 ip2 = new Vector2(); // intersection points
            int ind1 = 0, ind2 = 0;
            Vertices poly1, poly2;

            for (int i = 0; i < vertices.Count; ++i)
            {
                if (Reflex(i, vertices))
                {
                    dist1 = dist2 = float.MaxValue; // std::numeric_limits<qreal>::max();
                    for (int j = 0; j < vertices.Count; ++j)
                    {
                        if (Left(At(i - 1, vertices), At(i, vertices), At(j, vertices)) &&
                            RightOn(At(i - 1, vertices), At(i, vertices), At(j - 1, vertices)))
                        {
                            // if ray (i-1)->(i) intersects with edge (j, j-1)
                            //QLineF(at(i - 1), at(i)).intersect(QLineF(at(j), at(j - 1)), ip);
                            ip = LineTools.LineIntersect(At(i - 1, vertices), At(i, vertices), At(j, vertices),
                                                         At(j - 1, vertices));
                            if (Right(At(i + 1, vertices), At(i, vertices), ip))
                            {
                                // intersection point isn't caused by backwards ray
                                d = SquareDist(At(i, vertices), ip);
                                if (d < dist1)
                                {
                                    // take the closest intersection so we know it isn't blocked by another edge
                                    dist1 = d;
                                    ind1 = j;
                                    ip1 = ip;
                                }
                            }
                        }
                        if (Left(At(i + 1, vertices), At(i, vertices), At(j + 1, vertices)) &&
                            RightOn(At(i + 1, vertices), At(i, vertices), At(j, vertices)))
                        {
                            // if ray (i+1)->(i) intersects with edge (j+1, j)
                            //QLineF(at(i + 1), at(i)).intersect(QLineF(at(j), at(j + 1)), ip);
                            ip = LineTools.LineIntersect(At(i + 1, vertices), At(i, vertices), At(j, vertices),
                                                         At(j + 1, vertices));
                            if (Left(At(i - 1, vertices), At(i, vertices), ip))
                            {
                                d = SquareDist(At(i, vertices), ip);
                                if (d < dist2)
                                {
                                    dist2 = d;
                                    ind2 = j;
                                    ip2 = ip;
                                }
                            }
                        }
                    }
                    if (ind1 == (ind2 + 1) % vertices.Count)
                    {
                        // no vertices in range
                        Vector2 sp = ((ip1 + ip2) / 2);
                        poly1 = Copy(i, ind2, vertices);
                        poly1.Add(sp);
                        poly2 = Copy(ind1, i, vertices);
                        poly2.Add(sp);
                    }
                    else
                    {
                        double highestScore = 0, bestIndex = ind1;
                        while (ind2 < ind1) ind2 += vertices.Count;
                        for (int j = ind1; j <= ind2; ++j)
                        {
                            if (CanSee(i, j, vertices))
                            {
                                double score = 1 / (SquareDist(At(i, vertices), At(j, vertices)) + 1);
                                if (Reflex(j, vertices))
                                {
                                    if (RightOn(At(j - 1, vertices), At(j, vertices), At(i, vertices)) &&
                                        LeftOn(At(j + 1, vertices), At(j, vertices), At(i, vertices)))
                                    {
                                        score += 3;
                                    }
                                    else
                                    {
                                        score += 2;
                                    }
                                }
                                else
                                {
                                    score += 1;
                                }
                                if (score > highestScore)
                                {
                                    bestIndex = j;
                                    highestScore = score;
                                }
                            }
                        }
                        poly1 = Copy(i, (int) bestIndex, vertices);
                        poly2 = Copy((int) bestIndex, i, vertices);
                    }
                    list.AddRange(ConvexPartition(poly1));
                    list.AddRange(ConvexPartition(poly2));
                    return list;
                }
            }

            // polygon is already convex
            if (vertices.Count > Settings.MaxPolygonVertices)
            {
                poly1 = Copy(0, vertices.Count / 2, vertices);
                poly2 = Copy(vertices.Count / 2, 0, vertices);
                list.AddRange(ConvexPartition(poly1));
                list.AddRange(ConvexPartition(poly2));
            }
            else
                list.Add(vertices);

            //The polygons are not guaranteed to be without collinear points. We remove
            //them to be sure.
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = SimplifyTools.CollinearSimplify(list[i], 0);
            }

            return list;
        }

        private static bool CanSee(int i, int j, Vertices vertices)
        {
            if (Reflex(i, vertices))
            {
                if (LeftOn(At(i, vertices), At(i - 1, vertices), At(j, vertices)) &&
                    RightOn(At(i, vertices), At(i + 1, vertices), At(j, vertices))) return false;
            }
            else
            {
                if (RightOn(At(i, vertices), At(i + 1, vertices), At(j, vertices)) ||
                    LeftOn(At(i, vertices), At(i - 1, vertices), At(j, vertices))) return false;
            }
            if (Reflex(j, vertices))
            {
                if (LeftOn(At(j, vertices), At(j - 1, vertices), At(i, vertices)) &&
                    RightOn(At(j, vertices), At(j + 1, vertices), At(i, vertices))) return false;
            }
            else
            {
                if (RightOn(At(j, vertices), At(j + 1, vertices), At(i, vertices)) ||
                    LeftOn(At(j, vertices), At(j - 1, vertices), At(i, vertices))) return false;
            }
            for (int k = 0; k < vertices.Count; ++k)
            {
                if ((k + 1) % vertices.Count == i || k == i || (k + 1) % vertices.Count == j || k == j)
                {
                    continue; // ignore incident edges
                }
                //if(QLineF(at(i), at(j)).intersect(QLineF(at(k), at(k + 1)), NULL) == QLineF::BoundedIntersection) {
                if (LineTools.LineIntersect(At(i, vertices), At(j, vertices), At(k, vertices), At(k + 1, vertices)) !=
                    Vector2.Zero)
                {
                    return false;
                }
            }
            return true;
        }

        // precondition: ccw
        private static bool Reflex(int i, Vertices vertices)
        {
            return Right(i, vertices);
        }

        private static bool Right(int i, Vertices vertices)
        {
            return Right(At(i - 1, vertices), At(i, vertices), At(i + 1, vertices));
        }

        private static bool Left(Vector2 a, Vector2 b, Vector2 c)
        {
            return MathUtils.Area(ref a, ref b, ref c) > 0;
        }

        private static bool LeftOn(Vector2 a, Vector2 b, Vector2 c)
        {
            return MathUtils.Area(ref a, ref b, ref c) >= 0;
        }

        private static bool Right(Vector2 a, Vector2 b, Vector2 c)
        {
            return MathUtils.Area(ref a, ref b, ref c) < 0;
        }

        private static bool RightOn(Vector2 a, Vector2 b, Vector2 c)
        {
            return MathUtils.Area(ref a, ref b, ref c) <= 0;
        }

        private static float SquareDist(Vector2 a, Vector2 b)
        {
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;
            return dx * dx + dy * dy;
        }
    }
}