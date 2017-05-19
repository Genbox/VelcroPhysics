using System.Diagnostics;
using Microsoft.Xna.Framework;
using VelcroPhysics.Collision.Shapes;
using VelcroPhysics.Shared;

namespace VelcroPhysics.Collision.Narrowphase
{
    /// <summary>
    /// A distance proxy is used by the GJK algorithm.
    /// It encapsulates any shape.
    /// </summary>
    public class DistanceProxy
    {
        internal float Radius;
        internal Vertices Vertices = new Vertices();

        /// <summary>
        /// Initialize the proxy using the given shape. The shape
        /// must remain in scope while the proxy is in use.
        /// </summary>
        public void Set(Shape shape, int index)
        {
            switch (shape.ShapeType)
            {
                case ShapeType.Circle:
                    {
                        CircleShape circle = (CircleShape)shape;
                        Vertices.Clear();
                        Vertices.Add(circle.Position);
                        Radius = circle.Radius;
                    }
                    break;

                case ShapeType.Polygon:
                    {
                        PolygonShape polygon = (PolygonShape)shape;
                        Vertices.Clear();
                        for (int i = 0; i < polygon.Vertices.Count; i++)
                        {
                            Vertices.Add(polygon.Vertices[i]);
                        }
                        Radius = polygon.Radius;
                    }
                    break;

                case ShapeType.Chain:
                    {
                        ChainShape chain = (ChainShape)shape;
                        Debug.Assert(0 <= index && index < chain.Vertices.Count);
                        Vertices.Clear();
                        Vertices.Add(chain.Vertices[index]);
                        Vertices.Add(index + 1 < chain.Vertices.Count ? chain.Vertices[index + 1] : chain.Vertices[0]);

                        Radius = chain.Radius;
                    }
                    break;

                case ShapeType.Edge:
                    {
                        EdgeShape edge = (EdgeShape)shape;
                        Vertices.Clear();
                        Vertices.Add(edge.Vertex1);
                        Vertices.Add(edge.Vertex2);
                        Radius = edge.Radius;
                    }
                    break;

                default:
                    Debug.Assert(false);
                    break;
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
            for (int i = 1; i < Vertices.Count; ++i)
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

        /// <summary>
        /// Get the supporting vertex in the given direction.
        /// </summary>
        /// <param name="direction">The direction.</param>
        public Vector2 GetSupportVertex(Vector2 direction)
        {
            int bestIndex = 0;
            float bestValue = Vector2.Dot(Vertices[0], direction);
            for (int i = 1; i < Vertices.Count; ++i)
            {
                float value = Vector2.Dot(Vertices[i], direction);
                if (value > bestValue)
                {
                    bestIndex = i;
                    bestValue = value;
                }
            }

            return Vertices[bestIndex];
        }
    }
}