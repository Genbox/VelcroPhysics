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

namespace FarseerGames.AdvancedSamples.Demos.Demo6
{
    public class Demo6Screen : GameScreen
    {
        private Texture2D _chainTexture;
        private Texture2D _wheelTexture;
        private Vector2 _chainOrigin;
        private Vector2 _wheelOrigin;
        private Path _track;
        private Vertices _controlPoints;
        private Body _wheel1;
        private Body _wheel2;
        private Geom _wheelg;
        private LinearSpring _spring;

        private BodyList _obstacles;
        private GeomList _obstaclesg;
        private Texture2D _obstaclesTexture;
        private Vector2 _obstaclesOrigin;

        private LineBrush _lineBrush = new LineBrush(1, Color.Black); //used to draw spring on mouse grab
        private FixedLinearSpring _mousePickSpring;
        private Geom _pickedGeom;

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 100));
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);

            base.Initialize();
        }

        public override void LoadContent()
        {
            _chainTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 20, 10, Color.White,
                                                                 Color.Black);

            _obstaclesTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 40, 40, Color.Brown,
                                                                 Color.Black);

            _wheelTexture = DrawingHelper.CreateCircleTexture(ScreenManager.GraphicsDevice, 45, 2, Color.White, Color.Black);

            _chainOrigin = new Vector2(_chainTexture.Width / 2f, _chainTexture.Height / 2f);
            _wheelOrigin = new Vector2(_wheelTexture.Width / 2f, _wheelTexture.Height / 2f);
            _obstaclesOrigin = new Vector2(_obstaclesTexture.Width / 2f, _obstaclesTexture.Height / 2f);
            Vector2 center = new Vector2(400, 500);

            _controlPoints = new Vertices();
            _controlPoints.Add(new Vector2(-15, -50) + center);
            _controlPoints.Add(new Vector2(-50, -50) + center);
            _controlPoints.Add(new Vector2(-100, -25) + center);
            _controlPoints.Add(new Vector2(-100, 25) + center);
            _controlPoints.Add(new Vector2(-50, 50) + center);
            _controlPoints.Add(new Vector2(50, 50) + center);
            _controlPoints.Add(new Vector2(100, 25) + center);
            _controlPoints.Add(new Vector2(100, -25) + center);
            _controlPoints.Add(new Vector2(50, -50) + center);
            _controlPoints.Add(new Vector2(-10, -50) + center);

            _track = ComplexFactory.Instance.CreateTrack(PhysicsSimulator, _controlPoints, 20.0f, 10.0f, 3.0f, true, 2, LinkType.RevoluteJoint);

            foreach (Geom g in _track.Geoms)
                g.FrictionCoefficient = 1.0f;

            _wheel1 = BodyFactory.Instance.CreateCircleBody(PhysicsSimulator, 45, 30);
            _wheel1.Position = new Vector2(-50, 0) + center;
            _wheel2 = BodyFactory.Instance.CreateCircleBody(PhysicsSimulator, 45, 30);
            _wheel2.Position = new Vector2(50, 0) + center;

            _wheelg = GeomFactory.Instance.CreateCircleGeom(PhysicsSimulator, _wheel1, 45, 36);
            _wheelg.FrictionCoefficient = 1.0f;
            _wheelg = GeomFactory.Instance.CreateCircleGeom(PhysicsSimulator, _wheel2, 45, 36);
            _wheelg.FrictionCoefficient = 1.0f;

            _spring = SpringFactory.Instance.CreateLinearSpring(PhysicsSimulator, _wheel1, new Vector2(), _wheel2, new Vector2(), 1200, 250);
            _spring.RestLength += 20;

            _lineBrush.Load(ScreenManager.GraphicsDevice);

            _obstacles = new BodyList();
            _obstaclesg = new GeomList();

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            foreach (Body body in _track.Bodies)
            {
                ScreenManager.SpriteBatch.Draw(_chainTexture, body.Position, null, Color.White, body.Rotation, _chainOrigin, 1, SpriteEffects.None, 1);
            }

            ScreenManager.SpriteBatch.Draw(_wheelTexture, _wheel1.Position, null, Color.White, _wheel1.Rotation, _wheelOrigin, 1, SpriteEffects.None, 1);
            ScreenManager.SpriteBatch.Draw(_wheelTexture, _wheel2.Position, null, Color.White, _wheel2.Rotation, _wheelOrigin, 1, SpriteEffects.None, 1);

            foreach (Body b in _obstacles)
                ScreenManager.SpriteBatch.Draw(_obstaclesTexture, b.Position, null, Color.White, b.Rotation, _obstaclesOrigin, 1, SpriteEffects.None, 1);

            if (_mousePickSpring != null)
            {
                _lineBrush.Draw(ScreenManager.SpriteBatch,
                                _mousePickSpring.Body.GetWorldPosition(_mousePickSpring.BodyAttachPoint),
                                _mousePickSpring.WorldAttachPoint);
            }

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }

        float _torque;

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
            else
            {
                HandleMouseInput(input);
            }

            // do some keyboard torque stuff
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Left))
            {
                _torque = -3500;
            }
            else if (input.CurrentKeyboardState.IsKeyDown(Keys.Right))
            {
                _torque = 3500;
            }
            else
            {
                _torque = 0;
                _wheel1.ClearTorque();
                _wheel2.ClearTorque();
            }

            _wheel1.ApplyAngularImpulse(_torque);
            _wheel2.ApplyAngularImpulse(_torque);
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
                                                                                      point, 150, 10);
                }
                else
                {
                    _obstacles.Add(BodyFactory.Instance.CreateRectangleBody(PhysicsSimulator, 40, 40, 25));
                    _obstacles[_obstacles.Count - 1].Position = new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y);
                    _obstaclesg.Add(GeomFactory.Instance.CreateRectangleGeom(PhysicsSimulator, _obstacles[_obstacles.Count - 1], 40, 40));
                    _obstaclesg[_obstaclesg.Count - 1].FrictionCoefficient = 1.0f;
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
            return "Demo6: Path factory";
        }

        public static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to use the Path");
            sb.AppendLine("factory with the path generator.");
            sb.AppendLine("Click to add obstacles.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Move : left and right arrows");
            return sb.ToString();
        }
    }
}
