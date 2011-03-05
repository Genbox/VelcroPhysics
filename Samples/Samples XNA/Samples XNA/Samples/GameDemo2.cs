using System.Text;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.SamplesFramework
{
    internal class GameDemo2 : PhysicsGameScreen, IDemoScreen
    {
        private Body _ball;
        private RevoluteJoint _leftJoint;
        private RevoluteJoint _rightJoint;

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Pinball";
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

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = new Vector2(0f, 10f);
            HasCursor = false;

            Body ground;
            {
                ground = BodyFactory.CreateBody(World);

                Vertices vertices = new Vertices(5);
                vertices.Add(new Vector2(0.0f, -2.0f));
                vertices.Add(new Vector2(8.0f, 6.0f));
                vertices.Add(new Vector2(8.0f, 20.0f));
                vertices.Add(new Vector2(-8.0f, 20.0f));
                vertices.Add(new Vector2(-8.0f, 6.0f));
                Vector2 flip = new Vector2(1, -1);
                vertices.Scale(ref flip);
                LoopShape loop = new LoopShape(vertices);
                ground.CreateFixture(loop);
            }

            Vector2 p1 = new Vector2(-2.0f, 0f);
            Vector2 p2 = new Vector2(2.0f, 0f);

            Body leftFlipper = BodyFactory.CreateBody(World, p1);
            leftFlipper.BodyType = BodyType.Dynamic;
            Body rightFlipper = BodyFactory.CreateBody(World, p2);
            rightFlipper.BodyType = BodyType.Dynamic;

            PolygonShape box = new PolygonShape(1);
            box.SetAsBox(1.75f, 0.1f);

            leftFlipper.CreateFixture(box);
            rightFlipper.CreateFixture(box);

            _leftJoint = new RevoluteJoint(ground, leftFlipper, p1, Vector2.Zero);
            _leftJoint.MaxMotorTorque = 1000.0f;
            _leftJoint.LimitEnabled = true;
            _leftJoint.MotorEnabled = true;
            _leftJoint.MotorSpeed = 0.0f;
            _leftJoint.LowerLimit = -30.0f * Settings.Pi / 180.0f;
            _leftJoint.UpperLimit = 5.0f * Settings.Pi / 180.0f;
            World.AddJoint(_leftJoint);

            _rightJoint = new RevoluteJoint(ground, rightFlipper, p2, Vector2.Zero);
            _rightJoint.MaxMotorTorque = 1000.0f;
            _rightJoint.LimitEnabled = true;
            _rightJoint.MotorEnabled = true;
            _rightJoint.MotorSpeed = 0.0f;
            _rightJoint.LowerLimit = -5.0f * Settings.Pi / 180.0f;
            _rightJoint.UpperLimit = 30.0f * Settings.Pi / 180.0f;
            World.AddJoint(_rightJoint);

            // Circle character
            {
                _ball = BodyFactory.CreateBody(World, new Vector2(1.0f, 15.0f));
                _ball.BodyType = BodyType.Dynamic;
                _ball.IsBullet = true;
                _ball.CreateFixture(new CircleShape(0.2f, 1.0f));
            }
        }

        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            if (input.KeyboardState.IsKeyDown(Keys.A))
            {
                _leftJoint.MotorSpeed = 20.0f;
                _rightJoint.MotorSpeed = -20.0f;
            }
            if (input.KeyboardState.IsKeyUp(Keys.A))
            {
                _leftJoint.MotorSpeed = -10.0f;
                _rightJoint.MotorSpeed = 10.0f;
            }

            base.HandleInput(input, gameTime);
        }
    }
}
