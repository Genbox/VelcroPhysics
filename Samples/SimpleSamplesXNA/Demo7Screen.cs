using System.Text;
using FarseerPhysics.DebugViews;
using FarseerPhysics.DemoBaseXNA;
using FarseerPhysics.DemoBaseXNA.DemoShare;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.SimpleSamplesXNA
{
    internal class Demo7Screen : PhysicsGameScreen, IDemoScreen
    {
        private Ragdoll _ragdoll;

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Demo7: Ragdoll";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to combine physics objects");
            sb.AppendLine("to create a ragdoll.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  -Rotate: left and right triggers");
            sb.AppendLine("  -Move: left thumbstick");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse");
            sb.AppendLine("  -Hold down left button and drag");
            return sb.ToString();
        }

        #endregion

        public override void LoadContent()
        {
            World = new World(new Vector2(0, -20));
            base.LoadContent();

            _ragdoll = new Ragdoll(World, Vector2.Zero);
            CreateObstacles();
        }

        private void CreateObstacles()
        {
            DebugMaterial material = new DebugMaterial(MaterialType.Dots)
                                         {
                                             Color = Color.SandyBrown,
                                             Scale = 8f
                                         };

            Fixture[] rect = new Fixture[4];

            for (int i = 0; i < 4; i++)
            {
                rect[i] = FixtureFactory.CreateRectangle(World, 6, 1.5f, 1, material);
            }
            rect[0].Body.Position = new Vector2(-9, -5);
            rect[1].Body.Position = new Vector2(-8, 7);
            rect[2].Body.Position = new Vector2(9, -7);
            rect[3].Body.Position = new Vector2(7, 5);
        }

        public override void HandleGamePadInput(InputHelper input)
        {
            Vector2 force = 1000*input.CurrentGamePadState.ThumbSticks.Left;
            _ragdoll.Body.ApplyForce(force);

            float rotation = 400*input.CurrentGamePadState.Triggers.Left;
            _ragdoll.Body.ApplyTorque(rotation);

            rotation = -400*input.CurrentGamePadState.Triggers.Right;
            _ragdoll.Body.ApplyTorque(rotation);

            base.HandleGamePadInput(input);
        }

        public override void HandleKeyboardInput(InputHelper input)
        {
            const float forceAmount = 1000;
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

            _ragdoll.Body.ApplyForce(force);

            const float torqueAmount = 400;
            float torque = 0;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.Q))
            {
                torque += torqueAmount;
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.E))
            {
                torque -= torqueAmount;
            }

            _ragdoll.Body.ApplyTorque(torque);

            base.HandleKeyboardInput(input);
        }
    }
}