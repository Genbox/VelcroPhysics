using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.Testbed.Tests
{
    public class CheckPolygonTest : Test
    {
        private Vertices _vertices = new Vertices();

        private CheckPolygonTest()
        {
        }

        public override void Mouse(MouseState state, MouseState oldState)
        {
            Vector2 worldPosition = GameInstance.ConvertScreenToWorld(state.X, state.Y);

            if (state.LeftButton == ButtonState.Pressed && oldState.LeftButton == ButtonState.Released)
                _vertices.Add(worldPosition);

            base.Mouse(state, oldState);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DrawString("Use the mouse to create a polygon.");
            DrawString("Simple: " + _vertices.IsSimple());
            DrawString("Convex: " + _vertices.IsConvex());
            DrawString("CCW: " + _vertices.IsCounterClockWise());
            DrawString("Area: " + _vertices.GetArea());

            PolygonError returnCode = _vertices.CheckPolygon();

            if (returnCode == PolygonError.NoError)
                DrawString("Polygon is supported in Farseer Physics Engine");
            else
                DrawString("Polygon is NOT supported in Farseer Physics Engine. Reason: " + returnCode);

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);

            for (int i = 0; i < _vertices.Count; i++)
            {
                Vector2 currentVertex = _vertices[i];
                Vector2 nextVertex = _vertices.NextVertex(i);

                DebugView.DrawPoint(currentVertex, 0.1f, Color.Yellow);
                DebugView.DrawSegment(currentVertex, nextVertex, Color.Red);
            }

            DebugView.DrawPoint(_vertices.GetCentroid(), 0.1f, Color.Green);

            AABB aabb = _vertices.GetAABB();
            DebugView.DrawAABB(ref aabb, Color.HotPink);

            DebugView.EndCustomDraw();
            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new CheckPolygonTest();
        }
    }
}