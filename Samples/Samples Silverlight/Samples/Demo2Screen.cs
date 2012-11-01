using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.ScreenSystem;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Samples
{
    internal class Demo2Screen : GameScreen, IDemoScreen
    {
        private Body _rectangles;

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Demo2: Two fixtures with one body";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows two shapes attached to a single body");
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
            Vertices rect1 = PolygonTools.CreateRectangle(2, 2);
            Vertices rect2 = PolygonTools.CreateRectangle(2, 2);

            Vector2 trans = new Vector2(-2, 0);
            rect1.Translate(ref trans);
            trans = new Vector2(2, 0);
            rect2.Translate(ref trans);

            List<Vertices> vertices = new List<Vertices>(2);
            vertices.Add(rect1);
            vertices.Add(rect2);

            _rectangles = BodyFactory.CreateCompoundPolygon(World, vertices, 1);
            _rectangles.BodyType = BodyType.Dynamic;

            base.LoadContent();
        }

        public override void HandleInput(InputState input)
        {
            HandleKeyboardInput(input);

            base.HandleInput(input);
        }

        private void HandleKeyboardInput(InputState input)
        {
            const float forceAmount = 100;
            Vector2 force = Vector2.Zero;
            force.Y = -force.Y;

            if (input.CurrentKeyboardState.IsKeyDown(Key.A))
            {
                force += new Vector2(-forceAmount, 0);
            }
            if (input.CurrentKeyboardState.IsKeyDown(Key.S))
            {
                force += new Vector2(0, -forceAmount);
            }
            if (input.CurrentKeyboardState.IsKeyDown(Key.D))
            {
                force += new Vector2(forceAmount, 0);
            }
            if (input.CurrentKeyboardState.IsKeyDown(Key.W))
            {
                force += new Vector2(0, forceAmount);
            }

            _rectangles.ApplyForce(force);

            const float torqueAmount = 200;
            float torque = 0;

            if (input.CurrentKeyboardState.IsKeyDown(Key.Left))
            {
                torque += torqueAmount;
            }
            if (input.CurrentKeyboardState.IsKeyDown(Key.Right))
            {
                torque -= torqueAmount;
            }

            _rectangles.ApplyTorque(torque);
        }
    }
}