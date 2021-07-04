using System.Text;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem.Graphics;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.Screens;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Demo.Demos
{
    internal class D06_Restitution : PhysicsDemoScreen
    {
        private readonly Body[] _circle = new Body[6];
        private Sprite _circleSprite;

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = new Vector2(0f, 20f);

            Vector2 position = new Vector2(-15f, -8f);
            float restitution = 0f;

            for (int i = 0; i < 6; ++i)
            {
                _circle[i] = BodyFactory.CreateCircle(World, 1.5f, 1f, position);
                _circle[i].BodyType = BodyType.Dynamic;
                _circle[i].Restitution = restitution;
                position.X += 6f;
                restitution += 0.2f;
            }

            // create sprite based on body
            _circleSprite = new Sprite(Managers.TextureManager.TextureFromShape(_circle[0].FixtureList[0].Shape, "Square", Colors.Green, Colors.Lime, Colors.Black, 1f));
        }

        public override void Draw()
        {
            Sprites.Begin(0, null, null, null, null, null, Camera.View);

            for (int i = 0; i < 6; ++i)
            {
                Sprites.Draw(_circleSprite.Image, ConvertUnits.ToDisplayUnits(_circle[i].Position), null, Color.White, _circle[i].Rotation, _circleSprite.Origin, 1f, SpriteEffects.None, 0f);
            }

            Sprites.End();

            base.Draw();
        }

        public override string GetTitle()
        {
            return "Restitution";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows several bodies with varying restitution.");
#if WINDOWS
            sb.AppendLine();
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Exit to demo selection: Escape");
            sb.AppendLine();
            sb.AppendLine("Mouse:");
            sb.AppendLine("  - Grab object (beneath cursor): Left click");
            sb.AppendLine("  - Drag grabbed object: Move mouse");
#elif XBOX
            sb.AppendLine();
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Move cursor: Left thumbstick");
            sb.AppendLine("  - Grab object (beneath cursor): A button");
            sb.AppendLine("  - Drag grabbed object: Left thumbstick");
            sb.AppendLine("  - Exit to demo selection: Back button");
#endif
            return sb.ToString();
        }
    }
}