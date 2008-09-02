using System.Text;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysicsDemos.Demos.DemoShare;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;
using FarseerGames.FarseerPhysicsDemos.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.FarseerPhysicsDemos.Demos.Demo4
{
    public class Demo4Screen : GameScreen
    {
        private const int pyramidBaseBodyCount = 16;
        private readonly LineBrush _lineBrush = new LineBrush(1, Color.Black); //used to draw spring on mouse grab

        private readonly PhysicsSimulator _physicsSimulator;
        private readonly PhysicsSimulatorView _physicsSimulatorView;

        private Agent _agent;
        private ContentManager _contentManager;
        private bool _debugViewEnabled;
        private bool _firstRun;
        private Floor _floor;
        private FixedLinearSpring _mousePickSpring;
        private Geom _pickedGeom;
        private Pyramid _pyramid;
        private Body _rectangleBody;
        private Geom _rectangleGeom;
        private Texture2D _rectangleTexture;
        public bool updatedOnce;

        public Demo4Screen()
        {
            _physicsSimulator = new PhysicsSimulator(new Vector2(0, 50));
            _physicsSimulator.BiasFactor = .4f;
            _physicsSimulator.MaxContactsToDetect = 2;
            //for stacked objects, simultaneous collision are the bottlenecks so limit them to 2 per geometric pair.
            _physicsSimulatorView = new PhysicsSimulatorView(_physicsSimulator);
        }

        public override void LoadContent()
        {
            if (_contentManager == null) _contentManager = new ContentManager(ScreenManager.Game.Services);
            _lineBrush.Load(ScreenManager.GraphicsDevice);
            _physicsSimulatorView.LoadContent(ScreenManager.GraphicsDevice, _contentManager);

            _rectangleTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 32, 32, 2, 0, 0,
                                                                    Color.White, Color.Black);

            _rectangleBody = BodyFactory.Instance.CreateRectangleBody(32, 32, 1f); //template              
            _rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(_rectangleBody, 32, 32); //template
            _rectangleGeom.FrictionCoefficient = .4f;
            _rectangleGeom.RestitutionCoefficient = 0f;

            //create the _pyramid near the bottom of the screen.
            _pyramid = new Pyramid(_rectangleBody, _rectangleGeom, 32f/3f, 32f/3f, 32, 32, pyramidBaseBodyCount,
                                  new Vector2(ScreenManager.ScreenCenter.X - pyramidBaseBodyCount*.5f*(32 + 32/3),
                                              ScreenManager.ScreenHeight - 125));
            _pyramid.Load(_physicsSimulator);

            _floor = new Floor(ScreenManager.ScreenWidth, 100,
                              new Vector2(ScreenManager.ScreenCenter.X, ScreenManager.ScreenHeight - 50));
            _floor.Load(ScreenManager.GraphicsDevice, _physicsSimulator);

            _agent = new Agent(ScreenManager.ScreenCenter - new Vector2(320, 300));
            _agent.Load(ScreenManager.GraphicsDevice, _physicsSimulator);
        }

        public override void UnloadContent()
        {
            _contentManager.Unload();
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
            if (!updatedOnce)
            {
                return;
            }
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            _pyramid.Draw(ScreenManager.SpriteBatch, _rectangleTexture);
            _floor.Draw(ScreenManager.SpriteBatch);
            _agent.Draw(ScreenManager.SpriteBatch);

            if (_mousePickSpring != null)
            {
                _lineBrush.Draw(ScreenManager.SpriteBatch,
                               _mousePickSpring.Body.GetWorldPosition(_mousePickSpring.BodyAttachPoint),
                               _mousePickSpring.WorldAttachPoint);
            }
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
#if !XBOX
                HandleMouseInput(input);
#endif
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

            Vector2 force = 1000*input.CurrentGamePadState.ThumbSticks.Left;
            force.Y = -force.Y;
            _agent.ApplyForce(force);

            float rotation = -14000*input.CurrentGamePadState.Triggers.Left;
            _agent.ApplyTorque(rotation);

            rotation = 14000*input.CurrentGamePadState.Triggers.Right;
            _agent.ApplyTorque(rotation);
        }

        private void HandleKeyboardInput(InputState input)
        {
            if (!input.LastKeyboardState.IsKeyDown(Keys.F1) && input.CurrentKeyboardState.IsKeyDown(Keys.F1))
            {
                _debugViewEnabled = !_debugViewEnabled;
                _physicsSimulator.EnableDiagnostics = _debugViewEnabled;
            }

            const float forceAmount = 1000;
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

            _agent.ApplyForce(force);

            const float torqueAmount = 14000;
            float torque = 0;
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Left))
            {
                torque -= torqueAmount;
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Right))
            {
                torque += torqueAmount;
            }
            _agent.ApplyTorque(torque);
        }

#if !XBOX
        private void HandleMouseInput(InputState input)
        {
            Vector2 point = new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y);
            if (input.LastMouseState.LeftButton == ButtonState.Released &&
                input.CurrentMouseState.LeftButton == ButtonState.Pressed)
            {
                //create mouse spring
                _pickedGeom = _physicsSimulator.Collide(point);
                if (_pickedGeom != null)
                {
                    _mousePickSpring = ControllerFactory.Instance.CreateFixedLinearSpring(_physicsSimulator,
                                                                                         _pickedGeom.Body,
                                                                                         _pickedGeom.Body.
                                                                                             GetLocalPosition(point),
                                                                                         point, 20, 10);
                }
            }
            else if (input.LastMouseState.LeftButton == ButtonState.Pressed &&
                     input.CurrentMouseState.LeftButton == ButtonState.Released)
            {
                //destroy mouse spring
                if (_mousePickSpring != null && _mousePickSpring.IsDisposed == false)
                {
                    _mousePickSpring.Dispose();
                    _mousePickSpring = null;
                }
            }

            //move anchor point
            if (input.CurrentMouseState.LeftButton == ButtonState.Pressed && _mousePickSpring != null)
            {
                _mousePickSpring.WorldAttachPoint = point;
            }
        }
#endif

        public string GetTitle()
        {
            return "Stacked Objects";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows the stacking stability of the engine.");
            sb.AppendLine("It shows a stack of rectangular bodies stacked in");
            sb.AppendLine("the shape of a _pyramid.");
            sb.AppendLine("");
            sb.AppendLine("GamePad:");
            sb.AppendLine("  -Rotate : left and right triggers");
            sb.AppendLine("  -Move : left thumbstick");
            sb.AppendLine("");
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate : left and right arrows");
            sb.AppendLine("  -Move : A,S,D,W");
            sb.AppendLine("");
            sb.AppendLine("Mouse");
            sb.AppendLine("  -Hold down left button and drag");
            return sb.ToString();
        }
    }
}