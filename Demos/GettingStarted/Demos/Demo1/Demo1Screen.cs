using System.Text;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;
using FarseerGames.FarseerPhysicsDemos.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.FarseerPhysicsDemos.Demos.Demo1
{
    public class Demo1Screen : GameScreen
    {
        //TODO: Put physicssimulator into engine files and use Contentmanager from screenmanager.
        private readonly PhysicsSimulator _physicsSimulator;
        private readonly PhysicsSimulatorView _physicsSimulatorView;
        private Texture2D _bodyTexture;
        private ContentManager _contentManager;
        private bool _debugViewEnabled;
        private bool _firstRun = true;
        private Vector2 _origin;

        private Body _rectangleBody;
        public bool updatedOnce;

        public Demo1Screen()
        {
            _physicsSimulator = new PhysicsSimulator(new Vector2(0, 0));
            _physicsSimulatorView = new PhysicsSimulatorView(_physicsSimulator);
        }

        public override void LoadContent()
        {
            if (_contentManager == null) _contentManager = new ContentManager(ScreenManager.Game.Services);
            _physicsSimulatorView.LoadContent(ScreenManager.GraphicsDevice, _contentManager);

            //load texture that will visually represent the physics body
            _bodyTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 128, 128, Color.Gold,
                                                               Color.Black);
            _origin = new Vector2(_bodyTexture.Width/2f, _bodyTexture.Height/2f);

            //use the body factory to create the physics body
            _rectangleBody = BodyFactory.Instance.CreateRectangleBody(_physicsSimulator, 128, 128, 1);
            _rectangleBody.Position = ScreenManager.ScreenCenter;
        }

        public override void UnloadContent()
        {
            _physicsSimulator.Clear();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (IsActive)
            {
                _physicsSimulator.Update(gameTime.ElapsedGameTime.Milliseconds*.001f);
                updatedOnce = true;
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            ScreenManager.SpriteBatch.Draw(_bodyTexture, _rectangleBody.Position, null, Color.White,
                                           _rectangleBody.Rotation, _origin, 1, SpriteEffects.None, 0);
            if (_debugViewEnabled)
            {
                _physicsSimulatorView.Draw(ScreenManager.SpriteBatch);
            }
            ScreenManager.SpriteBatch.End();
        }

        public override void HandleInput(InputState input)
        {
            if (_firstRun)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
                _firstRun = false;
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
            if (input.LastGamePadState.Buttons.Y != ButtonState.Pressed &&
                input.CurrentGamePadState.Buttons.Y == ButtonState.Pressed)
            {
                _debugViewEnabled = !_debugViewEnabled;
                _physicsSimulator.EnableDiagnostics = _debugViewEnabled;
            }

            Vector2 force = 50*input.CurrentGamePadState.ThumbSticks.Left;
            force.Y = -force.Y;
            _rectangleBody.ApplyForce(force);

            float rotation = -1000*input.CurrentGamePadState.Triggers.Left;
            _rectangleBody.ApplyTorque(rotation);

            rotation = 1000*input.CurrentGamePadState.Triggers.Right;
            _rectangleBody.ApplyTorque(rotation);
        }

        private void HandleKeyboardInput(InputState input)
        {
            if (!input.LastKeyboardState.IsKeyDown(Keys.F1) && input.CurrentKeyboardState.IsKeyDown(Keys.F1))
            {
                _debugViewEnabled = !_debugViewEnabled;
                _physicsSimulator.EnableDiagnostics = _debugViewEnabled;
            }

            const float forceAmount = 50;
            Vector2 force = Vector2.Zero;
            force.Y = -force.Y;
            if (input.CurrentKeyboardState.IsKeyDown(Keys.A))
            {
                force += new Vector2(-forceAmount, 0);
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.S))
            {
                force += new Vector2(0, forceAmount);
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.D))
            {
                force += new Vector2(forceAmount, 0);
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.W))
            {
                force += new Vector2(0, -forceAmount);
            }

            _rectangleBody.ApplyForce(force);

            const float torqueAmount = 1000;
            float torque = 0;
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Left))
            {
                torque -= torqueAmount;
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Right))
            {
                torque += torqueAmount;
            }
            _rectangleBody.ApplyTorque(torque);
        }

        public string GetTitle()
        {
            return "A Single Body";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a single body with no geometry");
            sb.AppendLine("attached.");
            sb.AppendLine("");
            sb.AppendLine("GamePad:");
            sb.AppendLine("  -Rotate : left and right triggers");
            sb.AppendLine("  -Move : left thumbstick");
            sb.AppendLine("");
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate : left and right arrows");
            sb.AppendLine("  -Move : A,S,D,W");
            return sb.ToString();
        }
    }
}