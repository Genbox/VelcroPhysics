using System.Text;
using FarseerPhysics.DebugViews;
using FarseerPhysics.DemoBaseXNA;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.SimpleSamplesXNA
{
    internal class Demo1Screen : PhysicsGameScreen, IDemoScreen
    {
        private Fixture _rectangle;

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Demo1: A Single Fixture";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a single fixture.");
            sb.AppendLine("A fixture is a combination of a body and a shape.");
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

        public override void LoadContent()
        {
            World = new World(Vector2.Zero);
            base.LoadContent();

            DebugMaterial material = new DebugMaterial(MaterialType.Circles)
                                         {
                                             Color = Color.Gold,
                                             Scale = 2.5f
                                         };

            _rectangle = FixtureFactory.CreateRectangle(World, 5, 5, 1, material);
            _rectangle.Body.BodyType = BodyType.Dynamic;
        }

        public override void HandleGamePadInput(InputHelper input)
        {
            Vector2 force = 50*input.CurrentGamePadState.ThumbSticks.Left;
            _rectangle.Body.ApplyForce(force);

            float rotation = 40*input.CurrentGamePadState.Triggers.Left;
            _rectangle.Body.ApplyTorque(rotation);

            rotation = -40*input.CurrentGamePadState.Triggers.Right;
            _rectangle.Body.ApplyTorque(rotation);

            base.HandleGamePadInput(input);
        }

        public override void HandleKeyboardInput(InputHelper input)
        {
            const float forceAmount = 60;
            Vector2 force = Vector2.Zero;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.A))
            {
                force += new Vector2(-forceAmount, 0);
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.S))
            {
                force += new Vector2(0, -forceAmount);
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.D))
            {
                force += new Vector2(forceAmount, 0);
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.W))
            {
                force += new Vector2(0, forceAmount);
            }

            _rectangle.Body.ApplyForce(force);

            const float torqueAmount = 40;
            float torque = 0;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.Q))
            {
                torque += torqueAmount;
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.E))
            {
                torque -= torqueAmount;
            }

            _rectangle.Body.ApplyTorque(torque);

            base.HandleKeyboardInput(input);
        }
    }
}