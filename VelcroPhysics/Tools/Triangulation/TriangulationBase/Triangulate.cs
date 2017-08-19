using System;
using System.Collections.Generic;
using System.Diagnostics;
using VelcroPhysics.Shared;
using VelcroPhysics.Tools.ConvexHull.GiftWrap;
using VelcroPhysics.Tools.Triangulation.Bayazit;
using VelcroPhysics.Tools.Triangulation.Delaunay;
using VelcroPhysics.Tools.Triangulation.Earclip;
using VelcroPhysics.Tools.Triangulation.FlipCode;
using VelcroPhysics.Tools.Triangulation.Seidel;

namespace VelcroPhysics.Tools.Triangulation.TriangulationBase
{
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
                        Debug.Assert(!vertices.IsCounterClockWise(), "The Ear-clip algorithm expects the polygon to be clockwise.");
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
                    throw new ArgumentOutOfRangeException(nameof(algorithm));
            }

            if (discardAndFixInvalid)
            {
                for (int i = results.Count - 1; i >= 0; i--)
                {
                    Vertices polygon = results[i];

                    if (!ValidatePolygon(polygon))
                        results.RemoveAt(i);
                }
            }

            return results;
        }

        private static bool ValidatePolygon(Vertices polygon)
        {
            PolygonError errorCode = polygon.CheckPolygon();

            if (errorCode == PolygonError.InvalidAmountOfVertices || errorCode == PolygonError.AreaTooSmall || errorCode == PolygonError.SideTooSmall || errorCode == PolygonError.NotSimple)
                return false;

            if (errorCode == PolygonError.NotCounterClockWise) //NotCounterCloseWise is the last check in CheckPolygon(), thus we don't need to call ValidatePolygon again.
                polygon.Reverse();

            if (errorCode == PolygonError.NotConvex)
            {
                polygon = GiftWrap.GetConvexHull(polygon);
                return ValidatePolygon(polygon);
            }

            return true;
        }
    }
}