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

namespace FarseerGames.SimpleSamplesXNA.Demo1
{
    public class Demo1Screen : GameScreen
    {
        private RectangleBrush _rectangleBrush;
        private Body _rectangleBody;

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 0));
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);

            base.Initialize();
        }

        public override void LoadContent()
        {
            //load texture that will visually represent the physics body
            _rectangleBrush = new RectangleBrush(128, 128, Color.Gold, Color.Black);
            _rectangleBrush.Load(ScreenManager.GraphicsDevice);

            //use the body factory to create the physics body
            _rectangleBody = BodyFactory.Instance.CreateRectangleBody(PhysicsSimulator, 128, 128, 1);
            _rectangleBody.Position = ScreenManager.ScreenCenter;

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
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
            return "Demo1: A Single Body";
        }

        public static string GetDetails()
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
    }
}