using System.Text;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;
using FarseerGames.FarseerPhysicsDemos.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.FarseerPhysicsDemos.Demos.Demo2
{
    public class Demo2Screen : GameScreen
    {
        private LineBrush _lineBrush = new LineBrush(1, Color.Black); //used to draw spring on mouse grab
        private Body _circleBody;
        private Geom _circleGeom;
        private Vector2 _circleOrigin;
        private Texture2D _circleTexture;
        private FixedLinearSpring _mousePickSpring;
        private Geom _pickedGeom;
        private Body _rectangleBody;
        private Geom _rectangleGeom;
        private Vector2 _rectangleOrigin;
        private Texture2D _rectangleTexture;
        private Vector2 distance;

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 0));

            base.Initialize();
        }

        public override void LoadContent()
        {
            _lineBrush.Load(ScreenManager.GraphicsDevice);

            _rectangleTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 128, 128, Color.Gold,
                                                                     Color.Black);
            _rectangleOrigin = new Vector2(_rectangleTexture.Width / 2f, _rectangleTexture.Height / 2f);

            _circleTexture = DrawingHelper.CreateCircleTexture(ScreenManager.GraphicsDevice, 64, Color.White,
                                                               Color.Black);
            _circleOrigin = new Vector2(_circleTexture.Width / 2f, _circleTexture.Height / 2f);

            _rectangleBody = BodyFactory.Instance.CreateRectangleBody(PhysicsSimulator, 128, 128, 1);
            _rectangleBody.Position = new Vector2(256, 384);
            _rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(PhysicsSimulator, _rectangleBody, 128, 128);

            _circleBody = BodyFactory.Instance.CreateCircleBody(PhysicsSimulator, 64, 1); //fix 
            _circleBody.Position = new Vector2(725, 384);
            _circleGeom = GeomFactory.Instance.CreateCircleGeom(PhysicsSimulator, _circleBody, 64, 20);

            base.LoadContent();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            distance = Vector2.Subtract(_circleBody.Position, _rectangleBody.Position);
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            ScreenManager.SpriteBatch.Draw(_circleTexture, _circleGeom.Position, null, Color.White, _circleGeom.Rotation,
                                           _circleOrigin, 1, SpriteEffects.None, 0);
            ScreenManager.SpriteBatch.Draw(_rectangleTexture, _rectangleGeom.Position, null, Color.White,
                                           _rectangleGeom.Rotation, _rectangleOrigin, 1, SpriteEffects.None, 0);

            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DetailsFont, "Distance: " + distance.Length(), new Vector2(100, 100), Color.Black);

            if (_mousePickSpring != null)
            {
                _lineBrush.Draw(ScreenManager.SpriteBatch,
                                _mousePickSpring.Body.GetWorldPosition(_mousePickSpring.BodyAttachPoint),
                                _mousePickSpring.WorldAttachPoint);
            }
            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }

        public override void HandleInput(InputState input)
        {
            if (FirstRun)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
                FirstRun = false;
            }

            if (input.PauseGame)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
            }

            HandleKeyboardInput(input);
            HandleMouseInput(input);

            base.HandleInput(input);
        }

        private void HandleKeyboardInput(InputState input)
        {
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

        private void HandleMouseInput(InputState input)
        {
            Vector2 point = new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y);
            if (input.LastMouseState.LeftButton == ButtonState.Released &&
                input.CurrentMouseState.LeftButton == ButtonState.Pressed)
            {
                //create mouse spring
                _pickedGeom = PhysicsSimulator.Collide(point);
                if (_pickedGeom != null)
                {
                    _mousePickSpring = SpringFactory.Instance.CreateFixedLinearSpring(PhysicsSimulator,
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

        public string GetTitle()
        {
            return "Two Bodies With Geom";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows two bodies each with a single geometry");
            sb.AppendLine("object attached.");
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