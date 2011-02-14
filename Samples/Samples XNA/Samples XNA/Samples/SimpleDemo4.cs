using System.Text;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.SamplesFramework
{
    internal class SimpleDemo4 : PhysicsGameScreen, IDemoScreen
    {
        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Stacked Objects";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows the stacking stability of the engine.");
            sb.AppendLine("It shows a stack of rectangular bodies stacked in the shape of a pyramid.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Rotate agent: left and right triggers");
            sb.AppendLine("  - Move agent: right thumbstick");
            sb.AppendLine("  - Move cursor: left thumbstick");
            sb.AppendLine("  - Grab object (beneath cursor): A button");
            sb.AppendLine("  - Drag grabbed object: left thumbstick");
            sb.AppendLine("  - Exit to menu: Back button");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Rotate agent: left and right arrows");
            sb.AppendLine("  - Move agent: A,S,D,W");
            sb.AppendLine("  - Exit to menu: Escape");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse / Touchscreen");
            sb.AppendLine("  - Grab object (beneath cursor): Left click");
            sb.AppendLine("  - Drag grabbed object: move mouse / finger");
            return sb.ToString();
        }
        #endregion

#if XBOX || WINDOWS_PHONE
        //Xbox360 / WP7 can't handle as many geometries
        private const int PyramidBaseBodyCount = 10;
#else
        private const int PyramidBaseBodyCount = 14;
#endif

        private Agent _agent;
        private Pyramid _pyramid;

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = new Vector2(0f, -20f);

            new Border(World, ScreenManager.GraphicsDevice.Viewport);

            _agent = new Agent(World, new Vector2(5f, 10f));

            _pyramid = new Pyramid(World, new Vector2(0f, -15f), PyramidBaseBodyCount, 1f);
        }

        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            Vector2 force = 1000f * input.GamePadState.ThumbSticks.Right;
            float torque = 400f * (input.GamePadState.Triggers.Left - input.GamePadState.Triggers.Right);

            _agent.Body.ApplyForce(force);
            _agent.Body.ApplyTorque(torque);

            const float forceAmount = 600f;
            const float torqueAmount = 400f;

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

            _agent.Body.ApplyForce(force);
            _agent.Body.ApplyTorque(torque);

            base.HandleInput(input, gameTime);
        }
    }
}