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
    internal class Demo3Screen : PhysicsGameScreen, IDemoScreen
    {
        private Agent _agent;
        private Fixture[] _obstacles = new Fixture[5];

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Demo3: Multiple fixtures and static bodies";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a single body with multiple shapes");
            sb.AppendLine("attached.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("This demo also shows the use of static bodies.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  -Rotate: left and right triggers");
            sb.AppendLine("  -Move: left thumbstick");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate: left and right arrows");
            sb.AppendLine("  -Move: A,S,D,W");
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

            _agent = new Agent(World, new Vector2(5, 10));

            LoadObstacles();
        }

        private void LoadObstacles()
        {
            DebugMaterial material = new DebugMaterial(MaterialType.Dots)
                                         {
                                             Color = Color.SandyBrown,
                                             Scale = 8f
                                         };

            for (int i = 0; i < 5; i++)
            {
                _obstacles[i] = FixtureFactory.CreateRectangle(World, 8, 1.5f, 1, material);
                _obstacles[i].Body.IsStatic = true;

                if (i == 0)
                {
                    _obstacles[i].Restitution = .2f;
                    _obstacles[i].Friction = .2f;
                }
            }

            _obstacles[0].Body.Position = new Vector2(-5, -15);
            _obstacles[1].Body.Position = new Vector2(15, -10);
            _obstacles[2].Body.Position = new Vector2(10, 5);
            _obstacles[3].Body.Position = new Vector2(-10, 15);
            _obstacles[4].Body.Position = new Vector2(-17, 0);
        }

        public override void HandleGamePadInput(InputHelper input)
        {
            Vector2 force = 1000*input.CurrentGamePadState.ThumbSticks.Left;
            _agent.Body.ApplyForce(force);

            float rotation = 400*input.CurrentGamePadState.Triggers.Left;
            _agent.Body.ApplyTorque(rotation);

            rotation = -400*input.CurrentGamePadState.Triggers.Right;
            _agent.Body.ApplyTorque(rotation);

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

            _agent.Body.ApplyForce(force);

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

            _agent.Body.ApplyTorque(torque);

            base.HandleKeyboardInput(input);
        }
    }
}