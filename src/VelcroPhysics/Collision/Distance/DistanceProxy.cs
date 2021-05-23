using System;
using System.Diagnostics;
using Genbox.VelcroPhysics.Collision.Shapes;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Collision.Distance
{
    /// <summary>A distance proxy is used by the GJK algorithm. It encapsulates any shape.</summary>
    public struct DistanceProxy
    {
        internal readonly float _radius;
        internal readonly Vector2[] _vertices;

        public DistanceProxy(Shape shape, int index)
        {
            switch (shape.ShapeType)
            {
                case ShapeType.Circle:
                    {
                        CircleShape circle = (CircleShape)shape;
                        _vertices = new Vector2[1];
                        _vertices[0] = circle._position;
                        _radius = circle._radius;
                    }
                    break;

                case ShapeType.Polygon:
                    {
                        PolygonShape polygon = (PolygonShape)shape;
                        _vertices = new Vector2[polygon._vertices.Count];

                        for (int i = 0; i < polygon._vertices.Count; i++)
                        {
                            _vertices[i] = polygon._vertices[i];
                        }

                        _radius = polygon._radius;
                    }
                    break;

                case ShapeType.Chain:
                    {

                        ChainShape chain = (ChainShape)shape;
                        Debug.Assert(0 <= index && index < chain._vertices.Count);

                        _vertices = new Vector2[2];
                        _vertices[0] = chain._vertices[index];
                        _vertices[1] = index + 1 < chain._vertices.Count ? chain._vertices[index + 1] : chain._vertices[0];

                        _radius = chain._radius;
                    }
                    break;

                case ShapeType.Edge:
                    {
                        EdgeShape edge = (EdgeShape)shape;
                        _vertices = new Vector2[2];
                        _vertices[0] = edge._vertex1;
                        _vertices[1] = edge._vertex2;
                        _radius = edge._radius;
                    }
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        public DistanceProxy(Vector2[] vertices, float radius)
        {
            _vertices = vertices;
            _radius = radius;
        }

        /// <summary>Get the supporting vertex index in the given direction.</summary>
        /// <param name="direction">The direction.</param>
        public int GetSupport(Vector2 direction)
        {
            int bestIndex = 0;
            float bestValue = Vector2.Dot(_vertices[0], direction);
            for (int i = 1; i < _vertices.Length; ++i)
            {
                float value = Vector2.Dot(_vertices[i], direction);
                if (value > bestValue)
                {
                    bestIndex = i;
                    bestValue = value;
                }
            }

            return bestIndex;
        }

        public Vector2 GetVertex(int index)
        {
            Debug.Assert(0 <= index && index < _vertices.Length);
            return _vertices[index];
        }
    }
}