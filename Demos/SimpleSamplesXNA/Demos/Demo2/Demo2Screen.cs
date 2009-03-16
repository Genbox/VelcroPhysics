using System.Text;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.SimpleSamples.DrawingSystem;
using FarseerGames.SimpleSamples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.SimpleSamples.Demos.Demo2
{
    public class Demo2Screen : GameScreen
    {
        private Body _circleBody;
        private Geom _circleGeom;
        private Vector2 _circleOrigin;
        private Texture2D _circleTexture;

        private Body _rectangleBody;
        private Geom _rectangleGeom;
        private Vector2 _rectangleOrigin;
        private Texture2D _rectangleTexture;

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 0));
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);

            base.Initialize();
        }

        public override void LoadContent()
        {

            _rectangleTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 128, 128, Color.Gold,
                                                                     Color.Black);
            _rectangleOrigin = new Vector2(_rectangleTexture.Width / 2f, _rectangleTexture.Height / 2f);

            _circleTexture = DrawingHelper.CreateCircleTexture(ScreenManager.GraphicsDevice, 64, Color.White,
                                                               Color.Black);
            _circleOrigin = new Vector2(_circleTexture.Width / 2f, _circleTexture.Height / 2f);

            _rectangleBody = BodyFactory.Instance.CreateRectangleBody(PhysicsSimulator, 128, 128, 1);
            _rectangleBody.Position = new Vector2(256, 384);
            _rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(PhysicsSimulator, _rectangleBody, 128, 128);

            _circleBody = BodyFactory.Instance.CreateCircleBody(PhysicsSimulator, 64, 1);
            _circleBody.Position = new Vector2(725, 384);
            _circleGeom = GeomFactory.Instance.CreateCircleGeom(PhysicsSimulator, _circleBody, 64, 20);

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            ScreenManager.SpriteBatch.Draw(_circleTexture, _circleGeom.Position, null, Color.White, _circleGeom.Rotation,
                                           _circleOrigin, 1, SpriteEffects.None, 0);
            ScreenManager.SpriteBatch.Draw(_rectangleTexture, _rectangleGeom.Position, null, Color.White,
                                           _rectangleGeom.Rotation, _rectangleOrigin, 1, SpriteEffects.None, 0);


            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }

        public override void HandleInput(InputState input)
        {
            if (firstRun)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails(), this));
                firstRun = false;
            }

            if (input.PauseGame)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails(), this));
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

        public static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows two bodies each with a single geometry");
            sb.AppendLine("object attached.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  -Rotate : left and right triggers");
            sb.AppendLine("  -Move : left thumbstick");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate : left and right arrows");
            sb.AppendLine("  -Move : A,S,D,W");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse");
            sb.AppendLine("  -Hold down left button and drag");
            return sb.ToString();
        }
    }
}