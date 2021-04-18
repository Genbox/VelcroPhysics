using System.Diagnostics;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    /// <summary>
    /// This tests stacking. It also shows how to use World.Query()
    /// and AABB.TestOverlap().
    /// This callback is called by World.QueryAABB(). We find all the fixtures
    /// that overlap an AABB. Of those, we use AABB.TestOverlap() to determine which fixtures
    /// overlap a circle. Up to 4 overlapped fixtures will be highlighted with a yellow border.
    /// </summary>
    public class PolyShapesCallback
    {
        private const int MaxCount = 4;
        private int _count;

        internal CircleShape Circle = new CircleShape(0, 0);
        internal DebugView.DebugView DebugDraw;
        internal Transform Transform;

        private void DrawFixture(Fixture fixture)
        {
            Color color = new Color(0.95f, 0.95f, 0.6f);
            fixture.Body.GetTransform(out Transform xf);

            switch (fixture.Shape.ShapeType)
            {
                case ShapeType.Circle:
                {
                    CircleShape circle = (CircleShape)fixture.Shape;

                    Vector2 center = MathUtils.Mul(ref xf, circle.Position);
                    float radius = circle.Radius;

                    DebugDraw.DrawSolidCircle(center, radius, Vector2.Zero, color);
                }
                    break;

                case ShapeType.Polygon:
                {
                    PolygonShape poly = (PolygonShape)fixture.Shape;
                    int vertexCount = poly.Vertices.Count;
                    Debug.Assert(vertexCount <= Settings.MaxPolygonVertices);
                    Vector2[] vertices = new Vector2[Settings.MaxPolygonVertices];

                    for (int i = 0; i < vertexCount; ++i)
                    {
                        vertices[i] = MathUtils.Mul(ref xf, poly.Vertices[i]);
                    }

                    DebugDraw.DrawSolidPolygon(vertices, vertexCount, color);
                }
                    break;
            }
        }

        /// <summary>
        /// Called for each fixture found in the query AABB.
        /// </summary>
        /// <param name="fixture">The fixture.</param>
        /// <returns>false to terminate the query.</returns>
        public bool ReportFixture(Fixture fixture)
        {
            if (_count == MaxCount)
                return false;

            Body body = fixture.Body;
            Shape shape = fixture.Shape;

            body.GetTransform(out Transform xf);

            bool overlap = Collision.Narrowphase.Collision.TestOverlap(shape, 0, Circle, 0, ref xf, ref Transform);

            if (overlap)
            {
                DrawFixture(fixture);
                ++_count;
            }

            return true;
        }
    }
}