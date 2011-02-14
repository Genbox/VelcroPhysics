using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.SamplesFramework
{
    public class SimpleDemo6 : PhysicsGameScreen, IDemoScreen
    {
        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Dynamic Angle Joints";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo demonstrates the use of revolute joints combined");
            sb.AppendLine("with angle joints that have a dynamic target angle.");
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

        private Agent _agent;
        private Spider[] _spiders;

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = new Vector2(0f, -20f);

            new Border(World, ScreenManager.GraphicsDevice.Viewport);
            
            _agent = new Agent(World, new Vector2(0f, -12f));
            _spiders = new Spider[8];

            for (int i = 0; i < _spiders.Length; i++)
            {
                _spiders[i] = new Spider(World, new Vector2(0f, ((i + 1) * 2f) - 10f));
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (IsActive)
            {
                for (int i = 0; i < _spiders.Length; i++)
                {
                    _spiders[i].Update(gameTime);
                }
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
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