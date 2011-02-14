using System.Text;
using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.SamplesFramework
{
    internal class SimpleDemo1 : PhysicsGameScreen, IDemoScreen
    {
        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Body with a single fixture";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a single body with one attached fixture and shape.");
            sb.AppendLine("A fixture binds a shape to a body and adds material");
            sb.AppendLine("properties such as density, friction, and restitution.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Rotate object: left and right triggers");
            sb.AppendLine("  - Move object: right thumbstick");
            sb.AppendLine("  - Move cursor: left thumbstick");
            sb.AppendLine("  - Grab object (beneath cursor): A button");
            sb.AppendLine("  - Drag grabbed object: left thumbstick");
            sb.AppendLine("  - Exit to menu: Back button");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Rotate Object: left and right arrows");
            sb.AppendLine("  - Move Object: A,S,D,W");
            sb.AppendLine("  - Exit to menu: Escape");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse / Touchscreen");
            sb.AppendLine("  - Grab object (beneath cursor): Left click");
            sb.AppendLine("  - Drag grabbed object: move mouse / finger");
            return sb.ToString();
        }

        #endregion

        private Body _rectangle;

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = Vector2.Zero;

            new Border(World, ScreenManager.GraphicsDevice.Viewport);

            _rectangle = BodyFactory.CreateRectangle(World, 5f, 5f, 1f);
            _rectangle.BodyType = BodyType.Dynamic;
        }


        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            Vector2 force = 100f * input.GamePadState.ThumbSticks.Right;
            float torque = 100f * (input.GamePadState.Triggers.Left - input.GamePadState.Triggers.Right);

            _rectangle.ApplyForce(force);
            _rectangle.ApplyTorque(torque);

            const float forceAmount = 60f;
            const float torqueAmount = 100f;

            force = Vector2.Zero;
            torque = 0;

            if (input.KeyboardState.IsKeyDown(Keys.A))
            {
                force += new Vector2(-forceAmount, 0);
            }
            if (input.KeyboardState.IsKeyDown(Keys.S))
            {
                force += new Vector2(0, -forceAmount);
            }
            if (input.KeyboardState.IsKeyDown(Keys.D))
            {
                force += new Vector2(forceAmount, 0);
            }
            if (input.KeyboardState.IsKeyDown(Keys.W))
            {
                force += new Vector2(0, forceAmount);
            }
            if (input.KeyboardState.IsKeyDown(Keys.Q))
            {
                torque += torqueAmount;
            }
            if (input.KeyboardState.IsKeyDown(Keys.E))
            {
                torque -= torqueAmount;
            }

            _rectangle.ApplyForce(force);
            _rectangle.ApplyTorque(torque);

            base.HandleInput(input, gameTime);
        }
    }
}