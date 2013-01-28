using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
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
            DebugView.DrawString(50, TextLine, "Use the mouse to create a polygon.");
            TextLine += 30;
            DebugView.DrawString(50, TextLine, "Simple: " + _vertices.IsSimple());
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Convex: " + _vertices.IsConvex());
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "CCW: " + _vertices.IsCounterClockWise());
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Area: " + _vertices.GetArea());
            TextLine += 30;

            string errorMessage;
            int returnCode = _vertices.CheckPolygon(out errorMessage);

            if (returnCode == 0)
                DebugView.DrawString(50, TextLine, "Polygon is supported in FPE");
            else
                DebugView.DrawString(50, TextLine, "Polygon is NOT supported in FPE. Reason: " + errorMessage);

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