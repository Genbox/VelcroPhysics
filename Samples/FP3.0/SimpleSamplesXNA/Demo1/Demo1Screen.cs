using System.Text;
using FarseerPhysics.DemoBaseXNA;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SimpleSamplesXNA.Demo1
{
    internal class Demo1Screen : GameScreen, IDemoScreen
    {
        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Demo1: A Single Body";
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

        private Fixture _rectangle;

        public override void Initialize()
        {
            World = new World(new Vector2(0, 0));

            base.Initialize();

            DebugViewEnabled = true;
        }

        public override void LoadContent()
        {
            _rectangle = FixtureFactory.CreateRectangle(World, 5, 5, 1);
            _rectangle.Body.BodyType = BodyType.Dynamic;

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
            Vector2 force = 50 * input.CurrentGamePadState.ThumbSticks.Left;
            _rectangle.Body.ApplyForce(force);

            float rotation = 40 * input.CurrentGamePadState.Triggers.Left;
            _rectangle.Body.ApplyTorque(rotation);

            rotation = -40 * input.CurrentGamePadState.Triggers.Right;
            _rectangle.Body.ApplyTorque(rotation);
        }

        private void HandleKeyboardInput(InputState input)
        {
            const float forceAmount = 60;
            Vector2 force = Vector2.Zero;
            force.Y = -force.Y;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.A)) { force += new Vector2(-forceAmount, 0); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.S)) { force += new Vector2(0, -forceAmount); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.D)) { force += new Vector2(forceAmount, 0); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.W)) { force += new Vector2(0, forceAmount); }

            _rectangle.Body.ApplyForce(force);

            const float torqueAmount = 40;
            float torque = 0;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.Left)) { torque += torqueAmount; }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Right)) { torque -= torqueAmount; }

            _rectangle.Body.ApplyTorque(torque);
        }
    }
}