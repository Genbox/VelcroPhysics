using System.Text;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.Demos.Prefabs;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem.Graphics;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.Screens;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Demo.Demos
{
    internal class D10_Ragdoll : PhysicsDemoScreen
    {
        private Sprite _obstacle;
        private Body[] _obstacles;
        private Ragdoll _ragdoll;

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = new Vector2(0f, 20f);

            _ragdoll = new Ragdoll(World, new Vector2(-20f, -10f));

            _obstacles = new Body[9];
            Vector2 stairStart = new Vector2(-23f, 0f);
            Vector2 stairDelta = new Vector2(2.5f, 1.65f);

            for (int i = 0; i < 9; i++)
            {
                _obstacles[i] = BodyFactory.CreateRectangle(World, 5f, 1.5f, 1f, stairStart + stairDelta * i);
                _obstacles[i].BodyType = BodyType.Static;
            }

            // create sprite based on body
            _obstacle = new Sprite(Managers.TextureManager.TextureFromShape(_obstacles[0].FixtureList[0].Shape, "Stripe", Colors.Red, Colors.Black, Colors.Black, 1.5f));

            SetUserAgent(_ragdoll.Body, 1000f, 400f);
        }

        public override void Draw()
        {
            Sprites.Begin(0, null, null, null, null, null, Camera.View);

            for (int i = 0; i < 9; i++)
            {
                Sprites.Draw(_obstacle.Image, ConvertUnits.ToDisplayUnits(_obstacles[i].Position), null, Color.White, _obstacles[i].Rotation, _obstacle.Origin, 1f, SpriteEffects.None, 0f);
            }

            _ragdoll.Draw(Sprites);
            Sprites.End();

            base.Draw();
        }

        public override string GetTitle()
        {
            return "Ragdoll";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to combine bodies to create a ragdoll.");
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