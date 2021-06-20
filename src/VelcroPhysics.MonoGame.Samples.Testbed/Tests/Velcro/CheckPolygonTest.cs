using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework.Input;
using Genbox.VelcroPhysics.Shared;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests.Velcro
{
    public class CheckPolygonTest : Test
    {
        private readonly Vertices _vertices = new Vertices();

        private CheckPolygonTest() { }

        public override void Mouse(MouseManager manager)
        {
            Vector2 worldPosition = GameInstance.ConvertScreenToWorld(manager.NewPosition);

            if (manager.IsNewButtonClick(MouseButton.Left))
                _vertices.Add(worldPosition);

            base.Mouse(manager);
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
                DrawString("Polygon is supported in Velcro Physics");
            else
                DrawString("Polygon is NOT supported in Velcro Physics. Reason: " + returnCode);

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