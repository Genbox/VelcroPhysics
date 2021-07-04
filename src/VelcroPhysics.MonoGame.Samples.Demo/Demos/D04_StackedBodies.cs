using System.Text;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.Demos.Prefabs;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.Screens;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Demo.Demos
{
    internal class D04_StackedBodies : PhysicsDemoScreen
    {
#if XBOX
        private const int PyramidBaseBodyCount = 10;
#else
        private const int PyramidBaseBodyCount = 14;
#endif

        private Agent _agent;
        private Pyramid _pyramid;

        public override string GetTitle()
        {
            return "Stacked bodies";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows the stacking stability");
            sb.AppendLine("It shows a bunch of rectangular bodies stacked in the shape of a pyramid.");
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

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = new Vector2(0f, 20f);

            _agent = new Agent(World, new Vector2(5f, -10f));

            _pyramid = new Pyramid(World, new Vector2(0f, 15f), PyramidBaseBodyCount, 1f);

            SetUserAgent(_agent.Body, 1000f, 400f);
        }

        public override void Draw()
        {
            Sprites.Begin(0, null, null, null, null, null, Camera.View);
            _agent.Draw(Sprites);
            _pyramid.Draw(Sprites);
            Sprites.End();

            base.Draw();
        }
    }
}