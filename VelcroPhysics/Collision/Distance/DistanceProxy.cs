using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using VelcroPhysics.Collision.Shapes;

namespace VelcroPhysics.Collision.Distance
{
    /// <summary>
    /// A distance proxy is used by the GJK algorithm.
    /// It encapsulates any shape.
    /// </summary>
    public struct DistanceProxy
    {
        internal readonly float Radius;
        internal readonly Vector2[] Vertices;

        public DistanceProxy(Shape shape, int index)
        {
            switch (shape.ShapeType)
            {
                case ShapeType.Circle:
                    {
                        CircleShape circle = (CircleShape)shape;
                        Vertices = new Vector2[1];
                        Vertices[0] = circle.Position;
                        Radius = circle.Radius;
                    }
                    break;

                case ShapeType.Polygon:
                    {
                        PolygonShape polygon = (PolygonShape)shape;
                        Vertices = new Vector2[polygon.Vertices.Count];

                        for (int i = 0; i < polygon.Vertices.Count; i++)
                        {
                            Vertices[i] = polygon.Vertices[i];
                        }

                        Radius = polygon.Radius;
                    }
                    break;

                case ShapeType.Chain:
                    {

                        ChainShape chain = (ChainShape)shape;
                        Debug.Assert(0 <= index && index < chain.Vertices.Count);

                        Vertices = new Vector2[2];
                        Vertices[0] = chain.Vertices[index];
                        Vertices[1] = index + 1 < chain.Vertices.Count ? chain.Vertices[index + 1] : chain.Vertices[0];

                        Radius = chain.Radius;
                    }
                    break;

                case ShapeType.Edge:
                    {
                        EdgeShape edge = (EdgeShape)shape;
                        Vertices = new Vector2[2];
                        Vertices[0] = edge.Vertex1;
                        Vertices[1] = edge.Vertex2;
                        Radius = edge.Radius;
                    }
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Get the supporting vertex index in the given direction.
        /// </summary>
        /// <param name="direction">The direction.</param>
        public int GetSupport(Vector2 direction)
        {
            int bestIndex = 0;
            float bestValue = Vector2.Dot(Vertices[0], direction);
            for (int i = 1; i < Vertices.Length; ++i)
            {
                float value = Vector2.Dot(Vertices[i], direction);
                if (value > bestValue)
                {
                    bestIndex = i;
                    bestValue = value;
                }
            }

            return bestIndex;
        }
    }
}