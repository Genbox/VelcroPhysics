using System.Text;
using FarseerPhysics.DemoBaseXNA;
using FarseerPhysics.DemoBaseXNA.DemoShare;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace SimpleSamplesXNA
{
    internal class Demo7Screen : GameScreen, IDemoScreen
    {
        private Ragdoll _ragdoll;

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Demo7: Ragdoll";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to combine physics objects");
            sb.AppendLine("to create a ragdoll.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  -Rotate: left and right triggers");
            sb.AppendLine("  -Move: left thumbstick");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse");
            sb.AppendLine("  -Hold down left button and drag");
            return sb.ToString();
        }

        #endregion

        public override void Initialize()
        {
            World = new World(new Vector2(0, -20));
            base.Initialize();
        }

        public override void LoadContent()
        {
            _ragdoll = new Ragdoll(World, new Vector2(0, 0));
            CreateObstacles();
            base.LoadContent();
        }

        private void CreateObstacles()
        {
            Fixture[] rect = new Fixture[4];

            for (int i = 0; i < 4; i++)
            {
                rect[i] = FixtureFactory.CreateRectangle(World, 6, 1.5f, 1);
            }
            rect[0].Body.Position = new Vector2(-9, -5);
            rect[1].Body.Position = new Vector2(-8, 7);
            rect[2].Body.Position = new Vector2(9, -7);
            rect[3].Body.Position = new Vector2(7, 5);
        }

        public override void HandleInput(InputState input)
        {
            if (input.CurrentGamePadState.IsConnected)
            {
                Vector2 force = 1000 * input.CurrentGamePadState.ThumbSticks.Left;
                _ragdoll.Body.ApplyForce(force);

                float rotation = 4000 * input.CurrentGamePadState.Triggers.Left;
                _ragdoll.Body.ApplyTorque(rotation);

                rotation = -4000 * input.CurrentGamePadState.Triggers.Right;
                _ragdoll.Body.ApplyTorque(rotation);
            }

            base.HandleInput(input);
        }
    }
}