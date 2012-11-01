using System.Text;
using System.Windows.Input;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.ScreenSystem;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Samples
{
    internal class Demo1Screen : GameScreen, IDemoScreen
    {
        private Body _rectangle;

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Demo1: A Single Fixture";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a single fixture.");
            sb.AppendLine("A fixture is a combination of a body and a shape.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate: left and right arrows");
            sb.AppendLine("  -Move: A,S,D,W");
            return sb.ToString();
        }

        #endregion

        public override void Initialize()
        {
            World = new World(Vector2.Zero);

            base.Initialize();
        }

        public override void LoadContent()
        {
            _rectangle = BodyFactory.CreateRectangle(World, 5, 5, 1);
            _rectangle.BodyType = BodyType.Dynamic;

            base.LoadContent();
        }

        public override void HandleInput(InputState input)
        {
            const float forceAmount = 60;
            Vector2 force = Vector2.Zero;
            force.Y = -force.Y;

            if (input.IsKeyPressed(Key.A))
            {
                force += new Vector2(-forceAmount, 0);
            }
            if (input.IsKeyPressed(Key.S))
            {
                force += new Vector2(0, -forceAmount);
            }
            if (input.IsKeyPressed(Key.D))
            {
                force += new Vector2(forceAmount, 0);
            }
            if (input.IsKeyPressed(Key.W))
            {
                force += new Vector2(0, forceAmount);
            }

            _rectangle.ApplyForce(force);

            const float torqueAmount = 40;
            float torque = 0;

            if (input.IsKeyPressed(Key.Left))
            {
                torque += torqueAmount;
            }
            if (input.IsKeyPressed(Key.Right))
            {
                torque -= torqueAmount;
            }

            _rectangle.ApplyTorque(torque);

            base.HandleInput(input);
        }
    }
}