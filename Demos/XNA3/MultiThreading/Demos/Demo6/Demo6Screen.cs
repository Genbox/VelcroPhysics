using System;
using System.Text;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysicsDemos.Demos.DemoShare;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;
using FarseerGames.FarseerPhysicsDemos.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.FarseerPhysicsDemos.Demos.Demo6
{
    public class Demo6Screen : GameScreen
    {
        private const float platform1HeightRatio = .6f;
        private const float platform1WidthRatio = .1f;
        private const float platform2HeightRatio = .7f;
        private const float platform2WidthRatio = .1f;
        private const float platform3HeightRatio = .6f;
        private const float platform3WidthRatio = .1f;
        private readonly LineBrush _lineBrush = new LineBrush(1, Color.Black); //used to draw spring on mouse grab
        private Agent _agent;
        private AngularSpringLever _angularSpringLever1;
        private AngularSpringLever _angularSpringLever2;
        private AngularSpringLever _angularSpringLever3;
        private AngularSpringLever _angularSpringLever4;
        private Border _border;
        private bool _firstRun = true;
        private RectanglePlatform _floor;
        private Body _hangingBody;
        private Geom _hangingGeom;
        private Texture2D _hangingTexture;
        private FixedLinearSpring _mousePickSpring;
        private Geom _pickedGeom;
        private RectanglePlatform _platform1;
        private RectanglePlatform _platform2;
        private RectanglePlatform _platform3;
        private SpringRectangleRope _springRectangleRope1;
        private SpringRectangleRope _springRectangleRope2;
        public bool updatedOnce;

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 200));
            PhysicsSimulator.MaxContactsToDetect = 2;
                //for stacked objects, simultaneous collision are the bottlenecks so limit them to 2 per geometric pair.
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);
            PhysicsSimulatorView.EnableGridView = false;
            PhysicsSimulatorView.EnableEdgeView = false;
            PhysicsSimulatorView.EnableVerticeView = false;
            PhysicsSimulatorView.EnableAABBView = true;
            PhysicsSimulatorView.EnableCoordinateAxisView = true;

            base.Initialize();
        }

        public override void LoadContent()
        {
            _lineBrush.Load(ScreenManager.GraphicsDevice);

            int borderWidth = (int) (ScreenManager.ScreenHeight*.05f);
            _border = new Border(ScreenManager.ScreenWidth + borderWidth*2, ScreenManager.ScreenHeight + borderWidth*2,
                                 borderWidth, ScreenManager.ScreenCenter);
            _border.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);

            _agent = new Agent(new Vector2(ScreenManager.ScreenCenter.X, 100));
            _agent.CollisionCategory = CollisionCategories.Cat5;
            _agent.CollidesWith = CollisionCategories.All & ~CollisionCategories.Cat4;
            //collide with all but Cat4 (black)
            _agent.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);

            LoadPlatforms();

            base.LoadContent();
        }

        public void LoadPlatforms()
        {
            //_platform1
            int width = Convert.ToInt32(ScreenManager.ScreenWidth*platform1WidthRatio);
            int height = Convert.ToInt32(ScreenManager.ScreenHeight*platform1HeightRatio);
            Vector2 position = new Vector2(-5 + width/2f, 5 + ScreenManager.ScreenHeight - height/2f);

            _platform1 = new RectanglePlatform(width, height, position, Color.White, Color.Black, 100);
            _platform1.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);

            _angularSpringLever1 = new AngularSpringLever();
            _angularSpringLever1.AttachPoint = 0;
            _angularSpringLever1.RectangleWidth = 200;
            _angularSpringLever1.RectangleHeight = 20;
            _angularSpringLever1.SpringConstant = 1000000;
            _angularSpringLever1.DampningConstant = 5000;
            _angularSpringLever1.CollisionGroup = 100;
            Vector2 springPosition = position + new Vector2(width/2f, -height/2f) +
                                     new Vector2(-_angularSpringLever1.RectangleHeight - 5, .4f*height);
            _angularSpringLever1.Position = springPosition;
            _angularSpringLever1.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);

            //platform 2
            width = Convert.ToInt32(ScreenManager.ScreenWidth*platform2WidthRatio);
            height = Convert.ToInt32(ScreenManager.ScreenHeight*platform2HeightRatio);
            position = new Vector2(ScreenManager.ScreenCenter.X, 5 + ScreenManager.ScreenHeight - height/2f);

            _platform2 = new RectanglePlatform(width, height, position, Color.White, Color.Black, 100);
            _platform2.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);

            _angularSpringLever2 = new AngularSpringLever();
            _angularSpringLever2.AttachPoint = 2;
            _angularSpringLever2.RectangleWidth = 200;
            _angularSpringLever2.RectangleHeight = 20;
            _angularSpringLever2.SpringConstant = 1000000;
            _angularSpringLever2.DampningConstant = 5000;
            _angularSpringLever2.CollisionGroup = 100;
            springPosition = position + new Vector2(-width/2f, -height/2f) +
                             new Vector2(_angularSpringLever2.RectangleHeight + 5, .2f*height);
            _angularSpringLever2.Position = springPosition;
            _angularSpringLever2.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);

            _angularSpringLever3 = new AngularSpringLever();
            _angularSpringLever3.AttachPoint = 0;
            _angularSpringLever3.RectangleWidth = 150;
            _angularSpringLever3.RectangleHeight = 20;
            _angularSpringLever3.SpringConstant = 10000000;
            _angularSpringLever3.DampningConstant = 1000;
            _angularSpringLever3.CollisionGroup = 100;
            springPosition = position + new Vector2(width/2f, -height/2f) +
                             new Vector2(-_angularSpringLever3.RectangleHeight - 5, .1f*height);
            _angularSpringLever3.Position = springPosition;
            _angularSpringLever3.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);

            _springRectangleRope1 = new SpringRectangleRope();
            _springRectangleRope1.Position = springPosition + new Vector2(_angularSpringLever3.RectangleWidth - 5, 25);
            _springRectangleRope1.RectangleCount = 20;
            _springRectangleRope1.RectangleWidth = 15;
            _springRectangleRope1.RectangleHeight = 15;
            _springRectangleRope1.RectangleMass = .5f;
            _springRectangleRope1.SpringLength = 15;
            _springRectangleRope1.SpringConstant = 400;
            _springRectangleRope1.DampningConstant = 3f;
            _springRectangleRope1.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);
            ControllerFactory.Instance.CreateLinearSpring(PhysicsSimulator, _angularSpringLever3.Body,
                                                          new Vector2(
                                                              _angularSpringLever3.RectangleWidth/2f, 0),
                                                          _springRectangleRope1.FirstBody, Vector2.Zero,
                                                          400, 3);

            //platform 3
            width = Convert.ToInt32(ScreenManager.ScreenWidth*platform3WidthRatio);
            height = Convert.ToInt32(ScreenManager.ScreenHeight*platform3HeightRatio);
            position = new Vector2(ScreenManager.ScreenWidth + 5 - width/2f, 5 + ScreenManager.ScreenHeight - height/2f);
            _platform3 = new RectanglePlatform(width, height, position, Color.White, Color.Black, 100);
            _platform3.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);

            _hangingTexture = DrawingHelper.CreateCircleTexture(ScreenManager.GraphicsDevice, 40, Color.White,
                                                                Color.Black);
            _hangingBody = BodyFactory.Instance.CreateCircleBody(PhysicsSimulator, 40, 1);
            _hangingBody.Position = new Vector2(position.X - 200, 200);
            _hangingGeom = GeomFactory.Instance.CreateCircleGeom(PhysicsSimulator, _hangingBody, 40, 20);
            ControllerFactory.Instance.CreateFixedLinearSpring(PhysicsSimulator, _hangingBody,
                                                               new Vector2(0, -35),
                                                               new Vector2(position.X - 200, 100),
                                                               2, .1f);

            _angularSpringLever4 = new AngularSpringLever();
            _angularSpringLever4.AttachPoint = 2;
            _angularSpringLever4.RectangleWidth = 200;
            _angularSpringLever4.RectangleHeight = 20;
            _angularSpringLever4.SpringConstant = 1000000;
            _angularSpringLever4.DampningConstant = 1000;
            _angularSpringLever4.CollisionGroup = 100;
            springPosition = position + new Vector2(-width/2f, -height/2f) +
                             new Vector2(_angularSpringLever4.RectangleHeight + 5, .7f*height);
            _angularSpringLever4.Position = springPosition;
            _angularSpringLever4.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);

            height = (int) (ScreenManager.ScreenHeight*.05f);
            _floor = new RectanglePlatform(ScreenManager.ScreenWidth + 10, height,
                                           new Vector2(ScreenManager.ScreenCenter.X,
                                                       ScreenManager.ScreenHeight + 5 - height/2), Color.White,
                                           Color.Black, 0);
            _floor.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);

            _springRectangleRope2 = new SpringRectangleRope();
            _springRectangleRope2.Position = new Vector2(ScreenManager.ScreenCenter.X, 100);
            _springRectangleRope2.RectangleCount = 20;
            _springRectangleRope2.RectangleWidth = 10;
            _springRectangleRope2.RectangleHeight = 10;
            _springRectangleRope2.RectangleMass = .2f;
            _springRectangleRope2.SpringLength = 10;
            _springRectangleRope2.SpringConstant = 200;
            _springRectangleRope2.DampningConstant = 4f;
            _springRectangleRope2.CollisionGroup = 1; //same as _agent collision group
            _springRectangleRope2.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);
            ControllerFactory.Instance.CreateLinearSpring(PhysicsSimulator, _agent.Body, Vector2.Zero,
                                                          _springRectangleRope2.FirstBody, Vector2.Zero,
                                                          200, 4f);
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            _agent.Draw(ScreenManager.SpriteBatch);
            _springRectangleRope1.Draw(ScreenManager.SpriteBatch);
            _springRectangleRope2.Draw(ScreenManager.SpriteBatch);
            _platform1.Draw(ScreenManager.SpriteBatch);
            _platform2.Draw(ScreenManager.SpriteBatch);
            _platform3.Draw(ScreenManager.SpriteBatch);
            _floor.Draw(ScreenManager.SpriteBatch);
            _angularSpringLever1.Draw(ScreenManager.SpriteBatch);
            _angularSpringLever2.Draw(ScreenManager.SpriteBatch);
            _angularSpringLever3.Draw(ScreenManager.SpriteBatch);
            _angularSpringLever4.Draw(ScreenManager.SpriteBatch);

            ScreenManager.SpriteBatch.Draw(_hangingTexture, _hangingGeom.Position, null, Color.White,
                                           _hangingGeom.Rotation,
                                           new Vector2(_hangingTexture.Width/2f, _hangingTexture.Height/2f), 1,
                                           SpriteEffects.None, 0);

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
            if (_firstRun)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
                _firstRun = false;
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
            const float forceAmount = 3000;
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
                    _mousePickSpring = ControllerFactory.Instance.CreateFixedLinearSpring(PhysicsSimulator,
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
            return "Linear and Angular Springs";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows the use of angular and linear");
            sb.AppendLine("springs");
            sb.AppendLine(string.Empty);
            sb.AppendLine("The levers are connected to the walls using");
            sb.AppendLine("revolute joints and they each have an angular");
            sb.AppendLine("spring attached.");
            sb.AppendLine("The hanging squares are connected by linear");
            sb.AppendLine("springs.");
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