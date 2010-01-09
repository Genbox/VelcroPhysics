using System.Text;
using System.Collections.Generic;
using DemoBaseXNA;
using DemoBaseXNA.DrawingSystem;
using DemoBaseXNA.ScreenSystem;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.SimpleSamplesXNA.Demo1
{
    public class Demo1Screen : GameScreen
    {
        private int texture;
        private Body _rectangleBody;
        private float size = 2;

        public override void Initialize()
        {
            PhysicsSimulator = new World(new Vector2(0, 0), true);

            base.Initialize();
        }

        public override void LoadContent()
        {
            //load texture that will visually represent the physics body
            texture = ScreenManager.QuadRenderEngine.Submit(DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 100, 100, 1, Color.Gold, Color.Black), true);

            //use the body factory to create the physics body
            _rectangleBody = PhysicsSimulator.CreateBody();
            _rectangleBody.Position = Vector2.Zero;
            _rectangleBody.BodyType = BodyType.Dynamic;
            Vertices box = PolygonTools.CreateBox(size, size);
            PolygonShape shape = new PolygonShape(box, 5);
            _rectangleBody.CreateFixture(shape);

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.QuadRenderEngine.Submit(new Quad(_rectangleBody.Position, _rectangleBody.Rotation, (size + 0.0055f) * 2, (size + 0.0055f) * 2, texture));

            ScreenManager.QuadRenderEngine.Render(ScreenManager.Camera.Projection, ScreenManager.Camera.View);

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
            Vector2 force = 1 * input.CurrentGamePadState.ThumbSticks.Left;
            force.Y = -force.Y;
            _rectangleBody.ApplyForce(force, _rectangleBody.Position);

            float rotation = -1 * input.CurrentGamePadState.Triggers.Left;
            _rectangleBody.ApplyTorque(rotation);

            rotation = 1 * input.CurrentGamePadState.Triggers.Right;
            _rectangleBody.ApplyTorque(rotation);
        }

        private void HandleKeyboardInput(InputState input)
        {
            const float forceAmount = 1000;
            Vector2 force = Vector2.Zero;
            force.Y = -force.Y;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.A)) { force += new Vector2(-forceAmount, 0); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.S)) { force += new Vector2(0, -forceAmount); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.D)) { force += new Vector2(forceAmount, 0); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.W)) { force += new Vector2(0, forceAmount); }

            _rectangleBody.ApplyForce(force, _rectangleBody.Position);

            const float torqueAmount = 1000;
            float torque = 0;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.Left)) { torque += torqueAmount; }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Right)) { torque -= torqueAmount; }

            _rectangleBody.ApplyTorque(torque);
        }

        public static string GetTitle()
        {
            return "Demo1: A Single Body";
        }

        public static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a single body with geometry");
            sb.AppendLine("attached.");
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