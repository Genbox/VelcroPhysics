using System.Text;
using System;
using System.Collections.Generic;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.GettingStarted.Demos.DemoShare;
using FarseerGames.GettingStarted.DrawingSystem;
using FarseerGames.GettingStarted.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.GettingStarted.Demos.Demo9
{
    public class Demo9Screen : GameScreen
    {
        private Agent _agent;
        private Border _border;
        private LineBrush _lineBrush = new LineBrush(1, Color.Black); //used to draw spring on mouse grab
        private FixedLinearSpring _mousePickSpring;
        private Geom _pickedGeom;
        private Vertices _convexPolygon;
        private GeomList _polyList;
        private BodyList _bodyList;
        private List<RevoluteJoint> _revJointList;

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 50));
            PhysicsSimulator.MaxContactsToDetect = 4;
            PhysicsSimulator.MaxContactsToResolve = 2;
            //for stacked objects, simultaneous collision are the bottlenecks so limit them to 2 per geometric pair.
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);

            _convexPolygon = new Vertices();
            _polyList = new GeomList();
            _bodyList = new BodyList();
            _revJointList = new List<RevoluteJoint>();

            base.Initialize();
        }

        public override void LoadContent()
        {
            _lineBrush.Load(ScreenManager.GraphicsDevice);

            int borderWidth = (int)(ScreenManager.ScreenHeight * .05f);
            _border = new Border(ScreenManager.ScreenWidth, ScreenManager.ScreenHeight, borderWidth,
                                 ScreenManager.ScreenCenter);
            _border.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);

            _agent = new Agent(ScreenManager.ScreenCenter);
            _agent.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            _border.Draw(ScreenManager.SpriteBatch);
            _agent.Draw(ScreenManager.SpriteBatch);

            if (_mousePickSpring != null)
            {
                _lineBrush.Draw(ScreenManager.SpriteBatch,
                                _mousePickSpring.Body.GetWorldPosition(_mousePickSpring.BodyAttachPoint),
                                _mousePickSpring.WorldAttachPoint);
            }

            if (_convexPolygon.Count >= 2)
            {
                for (int i = 0; i < _convexPolygon.Count - 1; i++)
                {
                    _lineBrush.Draw(ScreenManager.SpriteBatch,
                                    _convexPolygon[i],
                                    _convexPolygon[i + 1]);
                }
                _lineBrush.Draw(ScreenManager.SpriteBatch,
                                    _convexPolygon[0],
                                    _convexPolygon[_convexPolygon.Count - 1]);
            }

            foreach (Geom g in _polyList)
            {
                for (int i = 0; i < g.WorldVertices.Count - 1; i++)
                {
                    _lineBrush.Draw(ScreenManager.SpriteBatch,
                                       g.WorldVertices[i],
                                       g.WorldVertices[i + 1]);
                }
                _lineBrush.Draw(ScreenManager.SpriteBatch,
                                    g.WorldVertices[0],
                                    g.WorldVertices[g.WorldVertices.Count - 1]);
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
            Vector2 force = 1000 * input.CurrentGamePadState.ThumbSticks.Left;
            force.Y = -force.Y;
            _agent.ApplyForce(force);

            float rotation = -14000 * input.CurrentGamePadState.Triggers.Left;
            _agent.ApplyTorque(rotation);

            rotation = 14000 * input.CurrentGamePadState.Triggers.Right;
            _agent.ApplyTorque(rotation);
        }

        private void HandleKeyboardInput(InputState input)
        {
            const float forceAmount = 1000;
            Vector2 force = Vector2.Zero;
            force.Y = -force.Y;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.A)) { force += new Vector2(-forceAmount, 0); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.S)) { force += new Vector2(0, forceAmount); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.D)) { force += new Vector2(forceAmount, 0); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.W)) { force += new Vector2(0, -forceAmount); }

            _agent.ApplyForce(force);

            const float torqueAmount = 14000;
            float torque = 0;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.Left)) { torque -= torqueAmount; }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Right)) { torque += torqueAmount; }

            _agent.ApplyTorque(torque);

            if (input.CurrentKeyboardState.IsKeyDown(Keys.Space))
            {
                if (_convexPolygon.Count >= 3)
                {
                    _bodyList.Add(BodyFactory.Instance.CreatePolygonBody(PhysicsSimulator, _convexPolygon, 1));
                    _polyList.Add(GeomFactory.Instance.CreatePolygonGeom(PhysicsSimulator, _bodyList[_bodyList.Count - 1], new Vertices (_convexPolygon)));
                    _polyList[_polyList.Count - 1].CollisionGroup = 1;
                    _bodyList[_bodyList.Count - 1].Position = new Vector2(400, 200);

                    _convexPolygon.Clear();
                }
            }
        }

        int count = 0;

#if !XBOX
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

            if (input.CurrentMouseState.MiddleButton == ButtonState.Pressed)
            {
                count++;
                if (count > 10)
                {
                    _convexPolygon.Add(new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y));
                    count = 0;
                }
            }

            if (input.CurrentMouseState.RightButton == ButtonState.Pressed)
            {
                count++;
                if (count > 20)
                {
                    List<Geom> list = PhysicsSimulator.CollideAll(new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y));
                    if (list.Count >= 2)
                        _revJointList.Add(JointFactory.Instance.CreateRevoluteJoint(PhysicsSimulator, list[0].Body, list[1].Body, new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y)));
                    count = 0;
                }
            }
        }
#endif

        public static string GetTitle()
        {
            return "Demo9: Realtime Geometry Creation";
        }

        public static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo simply stress tests broad phase collision");
            sb.AppendLine("In this demo:");
            sb.AppendLine("Narrow phase collision is disabled between");
            sb.AppendLine(" all balls.");
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