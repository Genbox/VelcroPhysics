using System.Text;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SimpleSamplesXNA.Demo3
{
    internal class Demo3Screen : GameScreen
    {
        //private Agent _agent;
        private Fixture[] _obstacles = new Fixture[5];

        public override void Initialize()
        {
            World = new World(new Vector2(0, -200));

            base.Initialize();
        }

        public override void LoadContent()
        {
            //_agent = new Agent(new Vector2(ScreenManager.ScreenCenter.X, 100));
            //_agent.Load(ScreenManager.GraphicsDevice, World);

            LoadObstacles();

            base.LoadContent();
        }

        private void LoadObstacles()
        {
            for (int i = 0; i < 5; i++)
            {
                _obstacles[i] = FixtureFactory.CreateRectangle(World, 10, 2, 1);
                _obstacles[i].Body.IsStatic = true;

                if (i == 0)
                {
                    _obstacles[i].Restitution = .2f;
                    _obstacles[i].Friction = .2f;
                }
            }

            _obstacles[0].Body.Position = ScreenManager.ScreenCenter + new Vector2(-5, -20);
            _obstacles[1].Body.Position = ScreenManager.ScreenCenter + new Vector2(15, -10);
            _obstacles[2].Body.Position = ScreenManager.ScreenCenter + new Vector2(10, 5);
            _obstacles[3].Body.Position = ScreenManager.ScreenCenter + new Vector2(-10, 20);
            _obstacles[4].Body.Position = ScreenManager.ScreenCenter + new Vector2(-17, 0);
        }

        public override void HandleInput(InputState input)
        {
            if (firstRun)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
                firstRun = false;
            }
            if (input.PauseGame)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
            }

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
            Vector2 force = 800 * input.CurrentGamePadState.ThumbSticks.Left;
            force.Y = -force.Y;
            //_agent.Body.ApplyForce(force);

            float rotation = -8000 * input.CurrentGamePadState.Triggers.Left;
            //_agent.Body.ApplyTorque(rotation);

            rotation = 8000 * input.CurrentGamePadState.Triggers.Right;
            //_agent.Body.ApplyTorque(rotation);
        }

        private void HandleKeyboardInput(InputState input)
        {
            const float forceAmount = 800;
            Vector2 force = Vector2.Zero;
            force.Y = -force.Y;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.A)) { force += new Vector2(-forceAmount, 0); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.S)) { force += new Vector2(0, forceAmount); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.D)) { force += new Vector2(forceAmount, 0); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.W)) { force += new Vector2(0, -forceAmount); }

            //_agent.Body.ApplyForce(force);

            const float torqueAmount = 8000;
            float torque = 0;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.Left)) { torque -= torqueAmount; }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Right)) { torque += torqueAmount; }

            //_agent.Body.ApplyTorque(torque);
        }

        public string GetTitle()
        {
            return "Demo3: Multiple geometries and static bodies";
        }

        private string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a single body with multiple geometry");
            sb.AppendLine("objects attached.  The yellow circles are offset");
            sb.AppendLine("from the bodies center. The body itself is created");
            sb.AppendLine("using 'CreateRectangleBody' so that it's moment of");
            sb.AppendLine("inertia is that of a rectangle.");
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
    }
}