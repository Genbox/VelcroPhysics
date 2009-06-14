using System.Text;
using System.Collections.Generic;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Interfaces;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.SimpleSamples.Demos.DemoShare;
using FarseerGames.SimpleSamples.DrawingSystem;
using FarseerGames.SimpleSamples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.SimpleSamples.Demos.Demo9
{
    public class Demo9Screen : GameScreen
    {
        private Agent _agent;

        private Body[] _obstacleBodies;
        private Geom[] _obstacleGeoms;
        private RectangleBrush _obstacleBrush;

        private Weld _weld;

        private List<Body> _weldedBody;
        private List<Geom> _weldedGeom;

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 100));
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);
            PhysicsSimulator.MaxContactsToDetect = 6;
            PhysicsSimulator.MaxContactsToResolve = 3;
            PhysicsSimulator.BroadPhaseCollider = new BruteForceCollider(PhysicsSimulator);
            PhysicsSimulator.Iterations = 20;

            PhysicsSimulatorView.EnableContactView = false;
            PhysicsSimulatorView.EnableEdgeView = true;
            PhysicsSimulatorView.EnableAABBView = false;
            PhysicsSimulatorView.EnableVerticeView = false;

            base.Initialize();
        }

        public override void LoadContent()
        {
            _agent = new Agent(new Vector2(500, 500));
            _agent.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);

            //LoadObstacles();

            _weldedBody = new List<Body>();
            _weldedGeom = new List<Geom>();

            for (int i = 0; i < 128; i++)
            {
                _weldedBody.Add(BodyFactory.Instance.CreateRectangleBody(25, 25, 10.0f));
                _weldedGeom.Add(GeomFactory.Instance.CreateRectangleGeom(_weldedBody[i], 25, 25));
                _weldedGeom[i].RestitutionCoefficient = 0.0001f;
                _weldedGeom[i].FrictionCoefficient = 0.999f;
                PhysicsSimulator.Add(_weldedBody[i]);
                PhysicsSimulator.Add(_weldedGeom[i]);
            }
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    // Width*y + x
                    _weldedBody[8 * y + x].Position = new Vector2((x * 30) + 80, (y * 30) + 80);
                }
            }
            _weld = new Weld(PhysicsSimulator, _weldedGeom);


            base.LoadContent();
        }

        public void LoadObstacles()
        {

            _obstacleBrush = new RectangleBrush(128, 32, Color.White, Color.Black);
            _obstacleBrush.Load(ScreenManager.GraphicsDevice);

            _obstacleBodies = new Body[5];
            _obstacleGeoms = new Geom[5];
            for (int i = 0; i < _obstacleBodies.Length; i++)
            {
                _obstacleBodies[i] = BodyFactory.Instance.CreateRectangleBody(PhysicsSimulator, 128, 32, 1);
                _obstacleBodies[i].IsStatic = true;

                if (i == 0)
                {
                    _obstacleGeoms[i] = GeomFactory.Instance.CreateRectangleGeom(PhysicsSimulator, _obstacleBodies[i], 128,
                                                                                32);
                    _obstacleGeoms[i].RestitutionCoefficient = .2f;
                    _obstacleGeoms[i].FrictionCoefficient = .2f;
                }
                else
                {
                    _obstacleGeoms[i] = GeomFactory.Instance.CreateGeom(PhysicsSimulator, _obstacleBodies[i],
                                                                       _obstacleGeoms[0]);
                }
            }

            _obstacleBodies[0].Position = ScreenManager.ScreenCenter + new Vector2(-50, -200);
            _obstacleBodies[1].Position = ScreenManager.ScreenCenter + new Vector2(150, -100);
            _obstacleBodies[2].Position = ScreenManager.ScreenCenter + new Vector2(100, 50);
            _obstacleBodies[3].Position = ScreenManager.ScreenCenter + new Vector2(-100, 200);
            _obstacleBodies[4].Position = ScreenManager.ScreenCenter + new Vector2(-170, 0);
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            //DrawObstacles();

            _agent.Draw(ScreenManager.SpriteBatch);

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawObstacles()
        {
            for (int i = 0; i < _obstacleBodies.Length; i++)
            {
                _obstacleBrush.Draw(ScreenManager.SpriteBatch, _obstacleBodies[i].Position, _obstacleBodies[i].Rotation);
            }
        }
        int count = 0;
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

            MouseState ms = Mouse.GetState();

            if (ms.RightButton == ButtonState.Pressed && count > 50)
            {
                _weld.Fracture();
                count = 0;
            }
            else if (ms.MiddleButton == ButtonState.Pressed && count > 50)
            {
                //_weld = new Weld(PhysicsSimulator, geoms);
                count = 0;
            }
            count++;

            base.HandleInput(input);
        }

        private void HandleGamePadInput(InputState input)
        {
            Vector2 force = 800 * input.CurrentGamePadState.ThumbSticks.Left;
            force.Y = -force.Y;
            _agent.Body.ApplyForce(force);

            float rotation = -8000 * input.CurrentGamePadState.Triggers.Left;
            _agent.Body.ApplyTorque(rotation);

            rotation = 8000 * input.CurrentGamePadState.Triggers.Right;
            _agent.Body.ApplyTorque(rotation);
        }

        private void HandleKeyboardInput(InputState input)
        {
            const float forceAmount = 800;
            Vector2 force = Vector2.Zero;
            force.Y = -force.Y;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.A)) { force += new Vector2(-forceAmount, 0); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.S)) { force += new Vector2(0, forceAmount); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.D)) { force += new Vector2(forceAmount, 0); }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.W)) { force += new Vector2(0, -forceAmount); }

            _agent.Body.ApplyForce(force);

            const float torqueAmount = 8000;
            float torque = 0;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.Left)) { torque -= torqueAmount; }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Right)) { torque += torqueAmount; }

            _agent.Body.ApplyTorque(torque);
        }

        public static string GetTitle()
        {
            return "Demo9: Multiple geometries and weld joints.";
        }

        public static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a single body with multiple geometry");
            sb.AppendLine("objects attached.  The yellow circles are offset");
            sb.AppendLine("from the bodies center. The body itself is created");
            sb.AppendLine("using 'CreateRectangleBody' so that it's moment of");
            sb.AppendLine("inertia is that of a rectangle.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("This demo also shows the use of static bodies.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  -Rotate: left and right triggers");
            sb.AppendLine("  -Move: left thumbstick");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate: left and right arrows");
            sb.AppendLine("  -Move: A,S,D,W");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse");
            sb.AppendLine("  -Hold down left button and drag");
            return sb.ToString();
        }
    }
}