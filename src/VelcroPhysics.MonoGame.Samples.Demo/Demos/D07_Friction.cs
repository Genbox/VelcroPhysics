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
    internal class D07_Friction : PhysicsDemoScreen
    {
        private readonly Body[] _rectangle = new Body[5];
        private Body _ramps;
        private Sprite _rectangleSprite;

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = new Vector2(0f, 20f);

            _ramps = BodyFactory.CreateBody(World);
            FixtureFactory.AttachEdge(new Vector2(-20f, -11.2f), new Vector2(10f, -3.8f), _ramps);
            FixtureFactory.AttachEdge(new Vector2(12f, -5.6f), new Vector2(12f, -3.2f), _ramps);

            FixtureFactory.AttachEdge(new Vector2(-10f, 4.4f), new Vector2(20f, -1.4f), _ramps);
            FixtureFactory.AttachEdge(new Vector2(-12f, 2.6f), new Vector2(-12f, 5f), _ramps);

            FixtureFactory.AttachEdge(new Vector2(-20f, 6.8f), new Vector2(10f, 11.5f), _ramps);

            float[] friction = { 0.75f, 0.45f, 0.28f, 0.17f, 0.0f };
            for (int i = 0; i < 5; i++)
            {
                _rectangle[i] = BodyFactory.CreateRectangle(World, 1.5f, 1.5f, 1f);
                _rectangle[i].BodyType = BodyType.Dynamic;
                _rectangle[i].Position = new Vector2(-18f + 5.2f * i, -13.0f + 1.282f * i);
                _rectangle[i].Friction = friction[i];
            }

            // create sprite based on body
            _rectangleSprite = new Sprite(Managers.TextureManager.TextureFromShape(_rectangle[0].FixtureList[0].Shape, "Square", Colors.Green, Colors.Lime, Colors.Black, 1f));
        }

        public override void Draw()
        {
            Sprites.Begin(0, null, null, null, null, null, Camera.View);

            for (int i = 0; i < 5; ++i)
            {
                Sprites.Draw(_rectangleSprite.Image, ConvertUnits.ToDisplayUnits(_rectangle[i].Position), null,
                    Color.White, _rectangle[i].Rotation, _rectangleSprite.Origin, 1f, SpriteEffects.None, 0f);
            }

            Sprites.End();
            Lines.Begin(ref Camera.SimProjection, ref Camera.SimView);

            foreach (Fixture f in _ramps.FixtureList)
            {
                Lines.DrawLineShape(f.Shape, Colors.Teal);
            }

            Lines.End();

            base.Draw();
        }

        public override string GetTitle()
        {
            return "Friction";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows several bodies with varying friction.");
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