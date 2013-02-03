using System;
using System.Collections.Generic;
using System.Diagnostics;
using FarseerPhysics.Common.ConvexHull;

namespace FarseerPhysics.Common.Decomposition
{
    public enum TriangulationAlgorithm
    {
        Earclip,
        Bayazit,
        Flipcode,
        Seidel,
        SeidelTrapezoids,
        Delauny
    }

    public static class Triangulate
    {
        public static List<Vertices> ConvexPartition(Vertices vertices, TriangulationAlgorithm algorithm, bool discardAndFixInvalid = true, float tolerance = 0.001f)
        {
            if (vertices.Count <= 3)
                return new List<Vertices> { vertices };

            List<Vertices> results;

            switch (algorithm)
            {
                case TriangulationAlgorithm.Earclip:
                    if (Settings.SkipSanityChecks)
                        Debug.Assert(!vertices.IsCounterClockWise(), "The Earclip algorithm expects the polygon to be clockwise.");
                    else
                    {
                        if (vertices.IsCounterClockWise())
                        {
                            Vertices temp = new Vertices(vertices);
                            temp.Reverse();
                            results = EarclipDecomposer.ConvexPartition(temp, tolerance);
                        }
                        else
                            results = EarclipDecomposer.ConvexPartition(vertices, tolerance);
                    }
                    break;
                case TriangulationAlgorithm.Bayazit:
                    if (Settings.SkipSanityChecks)
                        Debug.Assert(vertices.IsCounterClockWise(), "The polygon is not counter clockwise. This is needed for Bayazit to work correctly.");
                    else
                    {
                        if (!vertices.IsCounterClockWise())
                        {
                            Vertices temp = new Vertices(vertices);
                            temp.Reverse();
                            results = BayazitDecomposer.ConvexPartition(temp);
                        }
                        else
                            results = BayazitDecomposer.ConvexPartition(vertices);
                    }
                    break;
                case TriangulationAlgorithm.Flipcode:
                    if (Settings.SkipSanityChecks)
                        Debug.Assert(vertices.IsCounterClockWise(), "The polygon is not counter clockwise. This is needed for Bayazit to work correctly.");
                    else
                    {
                        if (!vertices.IsCounterClockWise())
                        {
                            Vertices temp = new Vertices(vertices);
                            temp.Reverse();
                            results = FlipcodeDecomposer.ConvexPartition(temp);
                        }
                        else
                            results = FlipcodeDecomposer.ConvexPartition(vertices);
                    }
                    break;
                case TriangulationAlgorithm.Seidel:
                    results = SeidelDecomposer.ConvexPartition(vertices, tolerance);
                    break;
                case TriangulationAlgorithm.SeidelTrapezoids:
                    results = SeidelDecomposer.ConvexPartitionTrapezoid(vertices, tolerance);
                    break;
                case TriangulationAlgorithm.Delauny:
                    results = CDTDecomposer.ConvexPartition(vertices);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("algorithm");
            }

            if (discardAndFixInvalid)
            {
                for (int i = results.Count - 1; i >= 0; i--)
                {
                    Vertices polygon = results[i];

                    PolygonError errorCode = polygon.CheckPolygon();

                    if (errorCode == PolygonError.InvalidAmountOfVertices || errorCode == PolygonError.AreaTooSmall || errorCode == PolygonError.SideTooSmall || errorCode == PolygonError.NotSimple)
                        results.RemoveAt(i);
                    else if (errorCode == PolygonError.NotCounterClockWise)
                        polygon.Reverse();
                    else if (errorCode == PolygonError.NotConvex)
                        results[i] = GiftWrap.GetConvexHull(polygon);
                }
            }

            return results;
        }
    }
}
