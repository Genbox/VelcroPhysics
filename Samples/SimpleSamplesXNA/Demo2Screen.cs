using System.Collections.Generic;
using System.Text;
using FarseerPhysics.Common;
using FarseerPhysics.DebugViews;
using FarseerPhysics.DemoBaseXNA;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.SimpleSamplesXNA
{
    internal class Demo2Screen : PhysicsGameScreen, IDemoScreen
    {
        private List<Fixture> _rectangles;

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
            sb.AppendLine("GamePad:");
            sb.AppendLine("  -Rotate: left and right triggers");
            sb.AppendLine("  -Move: left thumbstick");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate: left and right arrows");
            sb.AppendLine("  -Move: A,S,D,W");
            return sb.ToString();
        }

        #endregion

        public override void LoadContent()
        {
            World = new World(Vector2.Zero);
            base.LoadContent();

            Vertices rect1 = PolygonTools.CreateRectangle(2, 2);
            Vertices rect2 = PolygonTools.CreateRectangle(2, 2);

            Vector2 trans = new Vector2(-2, 0);
            rect1.Translate(ref trans);
            trans = new Vector2(2, 0);
            rect2.Translate(ref trans);

            List<Vertices> vertices = new List<Vertices>(2);
            vertices.Add(rect1);
            vertices.Add(rect2);

            DebugMaterial material = new DebugMaterial(MaterialType.Circles)
                                         {
                                             Color = Color.Gold,
                                             Scale = 2.5f
                                         };

            _rectangles = FixtureFactory.CreateCompoundPolygon(World, vertices, 1, material);
            _rectangles[0].Body.BodyType = BodyType.Dynamic;
        }

        public override void HandleGamePadInput(InputHelper input)
        {
            Vector2 force = 100*input.CurrentGamePadState.ThumbSticks.Left;
            _rectangles[0].Body.ApplyForce(force);

            float rotation = 200*input.CurrentGamePadState.Triggers.Left;
            _rectangles[0].Body.ApplyTorque(rotation);

            rotation = -200*input.CurrentGamePadState.Triggers.Right;
            _rectangles[0].Body.ApplyTorque(rotation);

            base.HandleGamePadInput(input);
        }

        public override void HandleKeyboardInput(InputHelper input)
        {
            const float forceAmount = 100;
            Vector2 force = Vector2.Zero;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.A))
            {
                force += new Vector2(-forceAmount, 0);
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.S))
            {
                force += new Vector2(0, -forceAmount);
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.D))
            {
                force += new Vector2(forceAmount, 0);
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.W))
            {
                force += new Vector2(0, forceAmount);
            }

            _rectangles[0].Body.ApplyForce(force);

            const float torqueAmount = 200;
            float torque = 0;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.E))
            {
                torque += torqueAmount;
            }
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Q))
            {
                torque -= torqueAmount;
            }

            _rectangles[0].Body.ApplyTorque(torque);

            base.HandleKeyboardInput(input);
        }
    }
}