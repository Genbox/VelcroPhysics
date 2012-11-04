using FarseerPhysics.Dynamics;
using FarseerPhysics.Fluids;
using Microsoft.Xna.Framework;
using FarseerPhysics.Collision.Shapes;
using System.Diagnostics;
using FarseerPhysics.Common;
using System.Collections.Generic;
using System;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Collision;

namespace FarseerPhysics.Physics.Collisions
{
    public class PolygonCollision : Collision
    {
        public const float GridSize = 2f;

        public class Edge
        {
            public Vector2 P0 = Vector2.Zero;
            public Vector2 P1 = Vector2.Zero;
        }

        public class Cell
        {
            /// <summary>
            /// Distances to the nearest border from the corners of the cell
            /// In top-left, top-right, bottom-right, bottom-left order
            /// </summary>
            public float[] Distances = new float[4];

            /// <summary>
            /// Edges intersecting the cell
            /// </summary>
            public List<Edge> Edges = new List<Edge>();

            /// <summary>
            /// Clipped polygons of the main shape
            /// </summary>
            public List<Vertices> Polygons = new List<Vertices>();
        }

        PolygonShape shape;

        public int GridWidth { get; private set; }

        public int GridHeight { get; private set; }

        private Cell[] _grid;

        public Cell GetCell(int x, int y)
        {
            if (x < 0 || x >= GridWidth || y < 0 || y >= GridHeight)
            {
                return null;
            }
            return _grid[y * GridWidth + x];
        }

        public PolygonCollision(RigidBody rb, Fixture f)
            : base(rb, f)
        {
            GridHeight = 0;
            GridWidth = 0;
            Debug.Assert(f.ShapeType == ShapeType.Polygon);
            shape = f.Shape as PolygonShape;

            if (!RigidOnly)
            {
                GridWidth = (int)Math.Ceiling(aabb.Extents.X * 2.0f / GridSize);
                GridHeight = (int)Math.Ceiling(aabb.Extents.Y * 2.0f / GridSize);

                _grid = new Cell[GridWidth * GridHeight];

                for (int gy = 0; gy < GridHeight; ++gy)
                {
                    for (int gx = 0; gx < GridWidth; ++gx)
                    {
                        Vector2[] corners = new Vector2[] {
                            aabb.LowerBound + new Vector2(gx * GridSize, gy * GridSize),
                            aabb.LowerBound + new Vector2((gx + 1) * GridSize, gy * GridSize),
                            aabb.LowerBound + new Vector2((gx + 1) * GridSize, (gy + 1) * GridSize),
                            aabb.LowerBound + new Vector2(gx * GridSize, (gy + 1) * GridSize)
                        };

                        // Having original vertices is better but we make do with the fixture's ones
                        Vertices vertices = shape.Vertices;

                        Feature[] features = new Feature[] {
                            vertices.GetNearestFeature(ref corners[0]),
                            vertices.GetNearestFeature(ref corners[1]),
                            vertices.GetNearestFeature(ref corners[2]),
                            vertices.GetNearestFeature(ref corners[3])
                        };

                        Cell cell = _grid[gy * GridWidth + gx] = new Cell();

                        // Set distances (for rough direction of nearest edge)
                        cell.Distances[0] = features[0].Distance;
                        cell.Distances[1] = features[1].Distance;
                        cell.Distances[2] = features[2].Distance;
                        cell.Distances[3] = features[3].Distance;

                        // Clip polygon againt cell
                        {
                            Vertices cellVertices = new Vertices
                            {
                                corners[0], corners[1],
                                corners[2], corners[3]
                            };

                            PolyClipError error;
                            cell.Polygons = YuPengClipper.Intersect(vertices, cellVertices, out error);
                            Debug.Assert(error == PolyClipError.None);
                        }

                        // Get edges intersecting cell
                        {
                            AABB cellAABB = new AABB(corners[0], corners[2]);

                            // Allow some slack
                            cellAABB.LowerBound -= new Vector2(0.01f, 0.01f);
                            cellAABB.UpperBound += new Vector2(0.01f, 0.01f);

                            for (int i = 0; i < vertices.Count; ++i)
                            {
                                Vector2 p0 = vertices[i];
                                Vector2 p1 = vertices.NextVertex(i);

                                if (cellAABB.Intersects(ref p0, ref p1))
                                {
                                    cell.Edges.Add(new Edge { P0 = p0, P1 = p1 });
                                }
                            }
                        }
                    }
                }
            }
        }

        public override bool Intersect(ref Vector2 point, ref Vector2 previousPoint, out Feature result)
        {
            result = Feature.Empty;

            if (_grid == null || !aabb.Contains(ref point))
            {
                return false;
            }

            int x = (int)Math.Floor((point.X - aabb.LowerBound.X) / GridSize);
            int y = (int)Math.Floor((point.Y - aabb.LowerBound.Y) / GridSize);

            // Outside of grid
            if (x < 0 || x >= GridWidth || y < 0 || y >= GridHeight)
            {
                return false;
            }

            Cell cellCurrent = _grid[y * GridWidth + x];
            Cell cellEdge;

            // Get edge cell
            {
                float xRatio = (point.X - (GridSize * x + aabb.LowerBound.X)) / GridSize;
                float yRatio = (point.Y - (GridSize * y + aabb.LowerBound.Y)) / GridSize;

                // Bilinear interpolation of corners for distance
                float dTop = MathHelper.Lerp(cellCurrent.Distances[0], cellCurrent.Distances[1], xRatio);
                float dBottom = MathHelper.Lerp(cellCurrent.Distances[3], cellCurrent.Distances[2], xRatio);
                float d = MathHelper.Lerp(dTop, dBottom, yRatio);

                // Distance gradient for direction to edge
                float dRight = MathHelper.Lerp(cellCurrent.Distances[1], cellCurrent.Distances[2], yRatio);
                float dLeft = MathHelper.Lerp(cellCurrent.Distances[0], cellCurrent.Distances[3], yRatio);

                Vector2 direction = new Vector2(dLeft - dRight, dTop - dBottom);

                if (Math.Abs(direction.X) < float.Epsilon && Math.Abs(direction.Y) < float.Epsilon)
                {
                    return false;
                }

                direction.Normalize();

                Vector2 nearestEdge = point + direction * d;

                int edgeX = (int)Math.Floor((nearestEdge.X - aabb.LowerBound.X) / GridSize);
                int edgeY = (int)Math.Floor((nearestEdge.Y - aabb.LowerBound.Y) / GridSize);

                // Outside of grid
                if (edgeX < 0 || edgeX >= GridWidth || edgeY < 0 || edgeY >= GridHeight)
                {
                    return false;
                }

                cellEdge = _grid[edgeY * GridWidth + edgeX];
            }

            // Compute nearest edge features
            {
                Edge closestEdge = null;
                Vector2 closestPoint = point;
                float closestDistanceSquared = float.PositiveInfinity;

                for (int i = 0; i < cellEdge.Edges.Count; ++i)
                {
                    Edge edge = cellEdge.Edges[i];
                    Vector2 proj = FluidUtils.ProjectOntoLineClamped(edge.P0, edge.P1, point);
                    float distanceSquared = Vector2.DistanceSquared(point, proj);

                    if (distanceSquared < closestDistanceSquared)
                    {
                        closestEdge = edge;
                        closestPoint = proj;
                        closestDistanceSquared = distanceSquared;
                    }
                }

                // Empty cell
                if (closestEdge == null)
                {
                    return false;
                }

                Vector2 normal = closestPoint - point;
                normal.Normalize();

                result = new Feature
                {
                    Distance = (float)Math.Sqrt(closestDistanceSquared),
                    Normal = normal,
                    Position = closestPoint,
                };

                bool exterior = true;

                for (int i = 0; i < cellEdge.Polygons.Count; ++i)
                {
                    if (cellEdge.Polygons[i].PointInPolygon(ref point) >= 0)
                    {
                        exterior = false;
                        break;
                    }
                }

                // Inside the polygon, negative distance
                if (!exterior)
                {
                    result.Distance *= -1.0f;
                    result.Normal *= -1.0f;
                }

                // Previous point
                {
                    int px = (int)Math.Floor((previousPoint.X - aabb.LowerBound.X) / GridSize);
                    int py = (int)Math.Floor((previousPoint.Y - aabb.LowerBound.Y) / GridSize);

                    // Outside of grid
                    if (px >= 0 && px < GridWidth && py >= 0 && py < GridHeight)
                    {
                        Cell cellPrevious = _grid[py * GridWidth + px];

                        for (int i = 0; i < cellPrevious.Polygons.Count; ++i)
                        {
                            if (cellPrevious.Polygons[i].PointInPolygon(ref previousPoint) >= 0)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}