using System.Collections.Generic;
using System.Text;
using FarseerPhysics.Common;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SimpleSamplesXNA.Demo2
{
    internal class Demo2Screen : GameScreen
    {
        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Demo2: Two Bodies with one fixture";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a single body with no geometry");
            sb.AppendLine("attached. Note that it does not collide with the borders.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  -Rotate: left and right triggers");
            sb.AppendLine("  -Move: left thumbstick");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate: left and right arrows");
            sb.AppendLine("  -Move: A,S,D,W");
            return sb.ToString();
        }

        #endregion

        private List<Fixture> _rectangles;

        public override void Initialize()
        {
            World = new World(new Vector2(0, 0));

            base.Initialize();

            DebugViewEnabled = true;
        }

        public override void LoadContent()
        {
            Vertices rect1 = PolygonTools.CreateRectangle(2, 2);
            Vertices rect2 = PolygonTools.CreateRectangle(2, 2);

            Vector2 trans = new Vector2(-2,0);
            rect1.Translate(ref trans);
            trans = new Vector2(2,0);
            rect2.Translate(ref trans);

            List<Vertices> vertices = new List<Vertices>(2);
            vertices.Add(rect1);
            vertices.Add(rect2);

            _rectangles = FixtureFactory.CreateCompundPolygon(World, vertices, 1);
            _rectangles[0].Body.BodyType = BodyType.Dynamic;

            base.LoadContent();
        }

        public override void HandleInput(InputState input)
        {
            if (input.CurrentGamePadState.IsConnected)
            {
                HandleGamePadInput(input);
            }
            else
            {
                HandleKeyboardInput(input);
            }

            base.HandleInput(input);
        }

        private void HandleGamePadInput(InputState input)
        {
            Vector2 force = 100 * input.CurrentGamePadState.ThumbSticks.Left;
            _rectangles[0].Body.ApplyForce(force);

            float rotation = 200 * input.CurrentGamePadState.Triggers.Left;
            _rectangles[0].Body.ApplyTorque(rotation);

            rotation = -200 * input.CurrentGamePadState.Triggers.Right;
            _rectangles[0].Body.ApplyTorque(rotation);
        }

        private void HandleKeyboardInput(InputState input)
        {
            const float forceAmount = 100;
            Vector2 force = Vector2.Zero;
            force.Y = -force.Y;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.A)) { force += new Vector2(-forceAmount, 0); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.S)) { force += new Vector2(0, -forceAmount); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.D)) { force += new Vector2(forceAmount, 0); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.W)) { force += new Vector2(0, forceAmount); }

            _rectangles[0].Body.ApplyForce(force);

            const float torqueAmount = 200;
            float torque = 0;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.Left)) { torque += torqueAmount; }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Right)) { torque -= torqueAmount; }

            _rectangles[0].Body.ApplyTorque(torque);
        }
    }
}