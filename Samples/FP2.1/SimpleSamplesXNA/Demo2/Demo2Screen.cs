using System.Text;
using DemoBaseXNA;
using DemoBaseXNA.DrawingSystem;
using DemoBaseXNA.ScreenSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.SimpleSamplesXNA.Demo2
{
    public class Demo2Screen : GameScreen
    {
        private Body _circleBody;
        private CircleBrush _circleBrush;

        private Body _rectangleBody;
        private RectangleBrush _rectangleBrush;

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 0));
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);

            base.Initialize();
        }

        public override void LoadContent()
        {
            //We create the brush that will be used to draw the circle
            _circleBrush = new CircleBrush(64, Color.White, Color.Black);
            _circleBrush.Load(ScreenManager.GraphicsDevice);

            //A body is used to simulate forces and impulses
            _circleBody = BodyFactory.Instance.CreateCircleBody(PhysicsSimulator, 64, 1);
            _circleBody.Position = new Vector2(725, 384);

            //A geometry is needed to get collision detection on the body
            GeomFactory.Instance.CreateCircleGeom(PhysicsSimulator, _circleBody, 64, 20);

            _rectangleBody = BodyFactory.Instance.CreateRectangleBody(PhysicsSimulator, 128, 128, 1);
            _rectangleBody.Position = new Vector2(256, 384);
            GeomFactory.Instance.CreateRectangleGeom(PhysicsSimulator, _rectangleBody, 128, 128);

            _rectangleBrush = new RectangleBrush(128, 128, Color.Gold, Color.Black);
            _rectangleBrush.Load(ScreenManager.GraphicsDevice);

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            _circleBrush.Draw(ScreenManager.SpriteBatch, _circleBody.Position);
            _rectangleBrush.Draw(ScreenManager.SpriteBatch, _rectangleBody.Position, _rectangleBody.Rotation);

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
            Vector2 force = 50 * input.CurrentGamePadState.ThumbSticks.Left;
            force.Y = -force.Y;
            _rectangleBody.ApplyForce(force);

            float rotation = -1000 * input.CurrentGamePadState.Triggers.Left;
            _rectangleBody.ApplyTorque(rotation);

            rotation = 1000 * input.CurrentGamePadState.Triggers.Right;
            _rectangleBody.ApplyTorque(rotation);
        }

        private void HandleKeyboardInput(InputState input)
        {
            const float forceAmount = 50;
            Vector2 force = Vector2.Zero;
            force.Y = -force.Y;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.A)) { force += new Vector2(-forceAmount, 0); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.S)) { force += new Vector2(0, forceAmount); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.D)) { force += new Vector2(forceAmount, 0); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.W)) { force += new Vector2(0, -forceAmount); }

            _rectangleBody.ApplyForce(force);

            const float torqueAmount = 1000;
            float torque = 0;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.Left)) { torque -= torqueAmount; }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Right)) { torque += torqueAmount; }

            _rectangleBody.ApplyTorque(torque);
        }

        public static string GetTitle()
        {
            return "Demo2: Two Bodies With Geom";
        }

        private static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows two bodies each with a single geometry");
            sb.AppendLine("object attached.");
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