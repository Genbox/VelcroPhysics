using System.Text;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.Demos.Prefabs;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.Screens;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Demo.Demos
{
    public class D09_DynamicJoints : PhysicsDemoScreen
    {
        private Agent _agent;
        private JumpySpider[] _spiders;

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = new Vector2(0f, 20f);

            _agent = new Agent(World, new Vector2(0f, 10f));
            _spiders = new JumpySpider[8];

            for (int i = 0; i < _spiders.Length; i++)
            {
                _spiders[i] = new JumpySpider(World, new Vector2(0f, 8f - (i + 1) * 2f));
            }

            SetUserAgent(_agent.Body, 1000f, 400f);
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

        public override void Draw()
        {
            Sprites.Begin(0, null, null, null, null, null, Camera.View);
            _agent.Draw(Sprites);

            for (int i = 0; i < _spiders.Length; i++)
            {
                _spiders[i].Draw(Sprites);
            }

            Sprites.End();

            base.Draw();
        }

        public override string GetTitle()
        {
            return "Revolute & dynamic angle joints";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo demonstrates the use of revolute joints combined");
            sb.AppendLine("with angle joints that have a dynamic target angle.");
#if WINDOWS
            sb.AppendLine();
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Rotate object: Q, E");
            sb.AppendLine("  - Move object: W, S, A, D");
            sb.AppendLine("  - Exit to demo selection: Escape");
            sb.AppendLine();
            sb.AppendLine("Mouse:");
            sb.AppendLine("  - Grab object (beneath cursor): Left click");
            sb.AppendLine("  - Drag grabbed object: Move mouse");
#elif XBOX
            sb.AppendLine();
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Rotate object: Left and right trigger");
            sb.AppendLine("  - Move object: Right thumbstick");
            sb.AppendLine("  - Move cursor: Left thumbstick");
            sb.AppendLine("  - Grab object (beneath cursor): A button");
            sb.AppendLine("  - Drag grabbed object: Left thumbstick");
            sb.AppendLine("  - Exit to demo selection: Back button");
#endif
            return sb.ToString();
        }
    }
}