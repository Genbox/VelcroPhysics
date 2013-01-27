using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
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

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            //if (keyboardManager.IsNewKeyPress(Keys.T))
            //    _vertices = _vertices.TraceEdge(_vertices);
            
            base.Keyboard(keyboardManager);
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

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);

            for (int i = 0; i < _vertices.Count; i++)
            {
                int iplus = (i + 1 > _vertices.Count - 1) ? 0 : i + 1;

                Vector2 currentVertex = _vertices[i];
                Vector2 nextVertex = _vertices[iplus];


                DebugView.DrawPoint(currentVertex, 0.1f, Color.Yellow);
                DebugView.DrawSegment(currentVertex, nextVertex, Color.Red);
            }

            DebugView.EndCustomDraw();
            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new CheckPolygonTest();
        }
    }
}