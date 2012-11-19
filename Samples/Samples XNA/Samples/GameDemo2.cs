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
        private Body _jumper;

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
            //HasCursor = false;

            //Bounds
            Vertices bounds = new Vertices();
            bounds.Add(new Vector2(-10, 13));
            bounds.Add(new Vector2(-10, -13));
            bounds.Add(new Vector2(10, -13));
            bounds.Add(new Vector2(10, 13));

            Body body = BodyFactory.CreateLoopShape(World, bounds);

            //Launcher
            FixtureFactory.AttachEdge(new Vector2(9, -3), new Vector2(9f, 12), body);
            FixtureFactory.AttachEdge(new Vector2(9, 12), new Vector2(9.5f, 12), body);
            FixtureFactory.AttachEdge(new Vector2(9.5f, 12), new Vector2(9.5f, -3.6f), body);

            //Jumper
            _jumper = BodyFactory.CreateRectangle(World, 0.43f, 0.2f, 5, new Vector2(9.25f, 10));
            _jumper.BodyType = BodyType.Dynamic;

            Vector2 axis = new Vector2(0.0f, -1.0f);
            FixedPrismaticJoint jumperJoint = JointFactory.CreateFixedPrismaticJoint(World, _jumper, _jumper.Position, axis);
            jumperJoint.MotorSpeed = 50.0f;
            jumperJoint.MaxMotorForce = 100.0f;
            jumperJoint.MotorEnabled = true;
            jumperJoint.LowerLimit = -2.0f;
            jumperJoint.UpperLimit = -0.1f;
            jumperJoint.LimitEnabled = true;

            //Top arc
            FixtureFactory.AttachLineArc(MathHelper.Pi, 50, 9.5f, new Vector2(0, -3), MathHelper.Pi, false, body);

            // Circle character
            _ball = BodyFactory.CreateBody(World, new Vector2(9.25f, 9.0f));
            _ball.BodyType = BodyType.Dynamic;
            _ball.IsBullet = true;
            _ball.CreateFixture(new CircleShape(0.2f, 1.0f));
        }

        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            if (input.KeyboardState.IsKeyDown(Keys.Space))
                _jumper.ApplyForce(new Vector2(0, 100));

            base.HandleInput(input, gameTime);
        }
    }
}
