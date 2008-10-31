using System.Text;
using FarseerGames.AdvancedSamples.DrawingSystem;
using FarseerGames.AdvancedSamples.ScreenSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerGames.AdvancedSamples.Demos.DemoShare;

namespace FarseerGames.AdvancedSamples.Demos.Demo5
{
    public class Demo5Screen : GameScreen
    {
        private Texture2D _chainTexture;
        private Vector2 _chainOrigin;
        private Path _chain;
        private Border _border;
        private LineBrush _lineBrush = new LineBrush(1, Color.Black); //used to draw spring on mouse grab
        private FixedLinearSpring _mousePickSpring;
        private Geom _pickedGeom;

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 150));
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);

            base.Initialize();
        }

        public override void LoadContent()
        {
            _chainTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 20, 20, Color.White,
                                                                 Color.Black);
            _chainOrigin = new Vector2(_chainTexture.Width / 2f, _chainTexture.Height / 2f);
            _border = new Border(ScreenManager.ScreenWidth, ScreenManager.ScreenHeight, 30, ScreenManager.ScreenCenter);
            _border.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);

            _chain = ComplexFactory.Instance.CreateChain(PhysicsSimulator, new Vector2(500, 300), new Vector2(500, 500), 20.0f, 10.0f, 1, 2);
            _chain.CreateGeoms();

            //Pinning the chain to world.
            JointFactory.Instance.CreateFixedRevoluteJoint(PhysicsSimulator, _chain.Bodies[0], _chain.Bodies[0].Position);

            _lineBrush.Load(ScreenManager.GraphicsDevice);

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            foreach (Body body in _chain.Bodies)
            {
                ScreenManager.SpriteBatch.Draw(_chainTexture, body.Position, null, Color.White, body.Rotation, _chainOrigin, 1, SpriteEffects.None, 1);
            }

            _border.Draw(ScreenManager.SpriteBatch);
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
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails(), this));
                FirstRun = false;
            }
            if (input.PauseGame)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails(), this));
            }
            else
            {
                HandleMouseInput(input);
            }
            base.HandleInput(input);
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


        public static string GetTitle()
        {
            return "Demo5: Chains factory";
        }

        public static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to use the chain");
            sb.AppendLine("factory with the path generator.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate : left and right arrows");
            sb.AppendLine("  -Move : A,S,D,W");
            return sb.ToString();
        }
    }
}
