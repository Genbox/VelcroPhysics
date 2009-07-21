using System.Text;
using DemoBaseXNA;
using DemoBaseXNA.DemoShare;
using DemoBaseXNA.ScreenSystem;
using FarseerGames.FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.SimpleSamplesXNA.Demo8
{
    public class Demo8Screen : GameScreen
    {
        private Agent _agent;
        private Circles[] _blackCircles;
        private Circles[] _blueCircles;
        private Circles[] _greenCircles;
        private Circles[] _redCircles;

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 0));
            PhysicsSimulator.MaxContactsToDetect = 2;
            //for stacked objects, simultaneous collision are the bottlenecks so limit them to 2 per geometric pair.
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);

            base.Initialize();
        }

        public override void LoadContent()
        {
            _agent = new Agent(ScreenManager.ScreenCenter);
            _agent.CollisionCategory = CollisionCategory.Cat5;
            _agent.CollidesWith = CollisionCategory.All & ~CollisionCategory.Cat4;
            //collide with all but Cat4 (black)
            _agent.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);

            LoadCircles();

            base.LoadContent();
        }

        private void LoadCircles()
        {
            _redCircles = new Circles[10];
            _blueCircles = new Circles[10];
            _blackCircles = new Circles[10];
            _greenCircles = new Circles[10];


            Vector2 startPosition = new Vector2(50, 50);
            Vector2 endPosition = new Vector2(ScreenManager.ScreenWidth - 50, 50);

#if XBOX
            const int balls = 20;
#else
            const int balls = 40;
#endif

            const float ySpacing = 12;
            for (int i = 0; i < _redCircles.Length; i++)
            {
                _redCircles[i] = new Circles(startPosition, endPosition, balls, 5, new Color(200, 0, 0, 175),
                                             Color.Black);
                _redCircles[i].CollisionCategories = (CollisionCategory.Cat1);
                _redCircles[i].CollidesWith = (CollisionCategory.Cat5);
                _redCircles[i].Load(ScreenManager.GraphicsDevice, PhysicsSimulator);
                startPosition.Y += ySpacing;
                endPosition.Y += ySpacing;
            }

            startPosition.Y += 2 * ySpacing;
            endPosition.Y += 2 * ySpacing;

            for (int i = 0; i < _blueCircles.Length; i++)
            {
                _blueCircles[i] = new Circles(startPosition, endPosition, balls, 5, new Color(0, 0, 200, 175),
                                              Color.Black);
                _blueCircles[i].CollisionCategories = (CollisionCategory.Cat3);
                _blueCircles[i].CollidesWith = (CollisionCategory.Cat5);
                _blueCircles[i].Load(ScreenManager.GraphicsDevice, PhysicsSimulator);
                startPosition.Y += ySpacing;
                endPosition.Y += ySpacing;
            }

            startPosition.Y += 12 * ySpacing;
            endPosition.Y += 12 * ySpacing;

            for (int i = 0; i < _greenCircles.Length; i++)
            {
                _greenCircles[i] = new Circles(startPosition, endPosition, balls, 5, new Color(0, 200, 0, 175),
                                               Color.Black);
                _greenCircles[i].CollisionCategories = (CollisionCategory.Cat2);
                _greenCircles[i].CollidesWith = (CollisionCategory.Cat5);
                _greenCircles[i].Load(ScreenManager.GraphicsDevice, PhysicsSimulator);
                startPosition.Y += ySpacing;
                endPosition.Y += ySpacing;
            }

            startPosition.Y += 2 * ySpacing;
            endPosition.Y += 2 * ySpacing;

            for (int i = 0; i < _blackCircles.Length; i++)
            {
                _blackCircles[i] = new Circles(startPosition, endPosition, balls, 5, new Color(0, 0, 0, 175),
                                               Color.Black);
                _blackCircles[i].CollisionCategories = (CollisionCategory.Cat4);
                _blackCircles[i].CollidesWith = (CollisionCategory.Cat5);
                _blackCircles[i].Load(ScreenManager.GraphicsDevice, PhysicsSimulator);
                startPosition.Y += ySpacing;
                endPosition.Y += ySpacing;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            _agent.Draw(ScreenManager.SpriteBatch);

            for (int i = 0; i < _redCircles.Length; i++)
            {
                _redCircles[i].Draw(ScreenManager.SpriteBatch);
            }

            for (int i = 0; i < _blueCircles.Length; i++)
            {
                _blueCircles[i].Draw(ScreenManager.SpriteBatch);
            }


            for (int i = 0; i < _blueCircles.Length; i++)
            {
                _greenCircles[i].Draw(ScreenManager.SpriteBatch);
            }


            for (int i = 0; i < _blueCircles.Length; i++)
            {
                _blackCircles[i].Draw(ScreenManager.SpriteBatch);
            }

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
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
            Vector2 force = 1000 * input.CurrentGamePadState.ThumbSticks.Left;
            force.Y = -force.Y;
            _agent.ApplyForce(force);

            float rotation = -14000 * input.CurrentGamePadState.Triggers.Left;
            _agent.ApplyTorque(rotation);

            rotation = 14000 * input.CurrentGamePadState.Triggers.Right;
            _agent.ApplyTorque(rotation);
        }

        private void HandleKeyboardInput(InputState input)
        {
            const float forceAmount = 1000;
            Vector2 force = Vector2.Zero;
            force.Y = -force.Y;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.A)) { force += new Vector2(-forceAmount, 0); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.S)) { force += new Vector2(0, forceAmount); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.D)) { force += new Vector2(forceAmount, 0); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.W)) { force += new Vector2(0, -forceAmount); }

            _agent.ApplyForce(force);

            const float torqueAmount = 14000;
            float torque = 0;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.Left)) { torque -= torqueAmount; }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Right)) { torque += torqueAmount; }

            _agent.ApplyTorque(torque);
        }

        public static string GetTitle()
        {
            return "Demo8: Broad Phase Collision Stress Test";
        }

        private static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo simply stress tests broad phase collision");
            sb.AppendLine("In this demo:");
            sb.AppendLine("Narrow phase collision is disabled between");
            sb.AppendLine(" all balls.");
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