using System;
using System.Text;
using FarseerPhysics.DebugViews;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision.Shapes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.SamplesFramework
{
    internal class GameDemo1 : PhysicsGameScreen, IDemoScreen
    {
        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Racing Car";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("TODO: Add sample description!");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Exit to menu: Back button");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Exit to menu: Escape");
            return sb.ToString();
        }

        #endregion

        private Body _car;
        private Body _wheelBack;
        private Body _wheelFront;

        private LineJoint _springBack;
        private LineJoint _springFront;
        private float _hz;
        private float _zeta;
        private float _maxSpeed;
        private float _acceleration;

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = new Vector2(0f, 10f);

            HasCursor = false;

            _hz = 4.0f;
            _zeta = 0.7f;
            _maxSpeed = 50.0f;

            // terrain
            Body ground = new Body(World);
            {
                Vertices terrain = new Vertices();
                terrain.Add(new Vector2(-20f, -5f));
                terrain.Add(new Vector2(-20f, 0f));
                terrain.Add(new Vector2(20f, 0f));
                terrain.Add(new Vector2(25f, -0.25f));
                terrain.Add(new Vector2(30f, -1f));
                terrain.Add(new Vector2(35f, -4f));
                terrain.Add(new Vector2(40f, 0f));
                terrain.Add(new Vector2(45f, 0f));
                terrain.Add(new Vector2(50f, 1f));
                terrain.Add(new Vector2(55f, 2f));
                terrain.Add(new Vector2(60f, 2f));
                terrain.Add(new Vector2(65f, 1.25f));
                terrain.Add(new Vector2(70f, 0f));
                terrain.Add(new Vector2(75f, -0.3f));
                terrain.Add(new Vector2(80f, -1.5f));
                terrain.Add(new Vector2(85f, -3.5f));
                terrain.Add(new Vector2(90f, 0f));
                terrain.Add(new Vector2(95f, 0.5f));
                terrain.Add(new Vector2(100f, 1f));
                terrain.Add(new Vector2(105f, 2f));
                terrain.Add(new Vector2(110f, 2.5f));
                terrain.Add(new Vector2(115f, 1.3f));
                terrain.Add(new Vector2(120f, 0f));
                terrain.Add(new Vector2(160f, 0f));
                terrain.Add(new Vector2(159f, 10f));
                terrain.Add(new Vector2(201f, 10f));
                terrain.Add(new Vector2(200f, 0f));
                terrain.Add(new Vector2(240f, 0f));
                terrain.Add(new Vector2(250f, -5f));
                terrain.Add(new Vector2(250f, 10f));
                terrain.Add(new Vector2(270f, 10f));
                terrain.Add(new Vector2(270f, 0));
                terrain.Add(new Vector2(310f, 0));
                terrain.Add(new Vector2(310f, -5));

                for (int i = 0; i < terrain.Count - 1; ++i)
                {
                    FixtureFactory.AttachEdge(terrain[i], terrain[i + 1], ground);
                }

                ground.Friction = 0.6f;
            }

            // teeter board
            {
                Body board = new Body(World);
                board.BodyType = BodyType.Dynamic;
                board.Position = new Vector2(140.0f, -1.0f);

                PolygonShape box = new PolygonShape(1f);
                box.SetAsBox(10.0f, 0.25f);
                board.CreateFixture(box);

                RevoluteJoint teeterAxis = JointFactory.CreateRevoluteJoint(ground, board, Vector2.Zero);
                teeterAxis.LowerLimit = -8.0f * Settings.Pi / 180.0f;
                teeterAxis.UpperLimit = 8.0f * Settings.Pi / 180.0f;
                teeterAxis.LimitEnabled = true;
                World.AddJoint(teeterAxis);

                board.ApplyAngularImpulse(-100.0f);
            }

            // bridge
            {
                const int segmentCount = 20;
                PolygonShape shape = new PolygonShape(1f);
                shape.SetAsBox(1.0f, 0.125f);

                Body prevBody = ground;
                for (int i = 0; i < segmentCount; ++i)
                {
                    Body body = new Body(World);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = new Vector2(161f + 2f * i, 0.125f);
                    Fixture fix = body.CreateFixture(shape);
                    fix.Friction = 0.6f;
                    JointFactory.CreateRevoluteJoint(World, prevBody, body, -Vector2.UnitX);

                    prevBody = body;
                }
                JointFactory.CreateRevoluteJoint(World, ground, prevBody, Vector2.UnitX);
            }

            // boxes
            {
                PolygonShape box = new PolygonShape(1f);
                box.SetAsBox(0.5f, 0.5f);

                Body body = new Body(World);
                body.BodyType = BodyType.Dynamic;
                body.Position = new Vector2(220f, -0.5f);
                body.CreateFixture(box);

                body = new Body(World);
                body.BodyType = BodyType.Dynamic;
                body.Position = new Vector2(220f, -1.5f);
                body.CreateFixture(box);

                body = new Body(World);
                body.BodyType = BodyType.Dynamic;
                body.Position = new Vector2(220f, -2.5f);
                body.CreateFixture(box);
            }

            // car
            {
                Vertices vertices = new Vertices(6);
                vertices.Add(new Vector2(-1.5f, -0.2f));
                vertices.Add(new Vector2(-1f, -0.7f));
                vertices.Add(new Vector2(0f, -0.8f));
                vertices.Add(new Vector2(1.5f, 0f));
                vertices.Add(new Vector2(1.5f, 0.5f));
                vertices.Add(new Vector2(-1.5f, 0.5f));

                PolygonShape chassis = new PolygonShape(vertices, 2f);

                _car = new Body(World);
                _car.BodyType = BodyType.Dynamic;
                _car.Position = new Vector2(0.0f, -1.0f);
                _car.CreateFixture(chassis);

                _wheelBack = new Body(World);
                _wheelBack.BodyType = BodyType.Dynamic;
                _wheelBack.Position = new Vector2(-1.0f, -0.35f);
                Fixture fix = _wheelBack.CreateFixture(new CircleShape(0.4f, 0.8f));
                fix.Friction = 0.9f;

                _wheelFront = new Body(World);
                _wheelFront.BodyType = BodyType.Dynamic;
                _wheelFront.Position = new Vector2(1.0f, -0.4f);
                _wheelFront.CreateFixture(new CircleShape(0.4f, 1f));

                Vector2 axis = new Vector2(0.0f, -1.0f);
                _springBack = new LineJoint(_car, _wheelBack, _wheelBack.Position, axis);
                _springBack.MotorSpeed = 0.0f;
                _springBack.MaxMotorTorque = 20.0f;
                _springBack.MotorEnabled = true;
                _springBack.Frequency = _hz;
                _springBack.DampingRatio = _zeta;
                World.AddJoint(_springBack);

                _springFront = new LineJoint(_car, _wheelFront, _wheelFront.Position, axis);
                _springFront.MotorSpeed = 0.0f;
                _springFront.MaxMotorTorque = 10.0f;
                _springFront.MotorEnabled = false;
                _springFront.Frequency = _hz;
                _springFront.DampingRatio = _zeta;
                World.AddJoint(_springFront);
            }

            Camera.MinRotation = -0.05f;
            Camera.MaxRotation = 0.05f;

            Camera.TrackingBody = _car;
            Camera.EnableTracking = true;
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            _springBack.MotorSpeed = Math.Sign(_acceleration) * MathHelper.SmoothStep(0f, _maxSpeed, Math.Abs(_acceleration));
            if (Math.Abs(_springBack.MotorSpeed) < _maxSpeed * 0.06f)
            {
                _springBack.MotorEnabled = false;
            }
            else
            {
                _springBack.MotorEnabled = true;
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            if (input.KeyboardState.IsKeyDown(Keys.S))
            {
                _acceleration = 0f;
            }
            else if (input.KeyboardState.IsKeyDown(Keys.D))
            {
                _acceleration = Math.Min(_acceleration + (float)(2.0 * gameTime.ElapsedGameTime.TotalSeconds), 1f);
            }
            else if (input.KeyboardState.IsKeyDown(Keys.A))
            {
                _acceleration = Math.Max(_acceleration - (float)(2.0 * gameTime.ElapsedGameTime.TotalSeconds), -1f);
            }
            else
            {
                _acceleration -= Math.Sign(_acceleration) * (float)(2.0 * gameTime.ElapsedGameTime.TotalSeconds);
            }

            base.HandleInput(input, gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            // TODO
            base.Draw(gameTime);
        }
    }
}
