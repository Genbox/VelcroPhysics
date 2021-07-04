using System.Collections.Generic;
using System.Text;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem.Graphics;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.Screens;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Demo.Demos
{
    internal class D02_MultipleFixtures : PhysicsDemoScreen
    {
        private Vector2 _offset;
        private Body _rectangles;
        private Sprite _rectangleSprite;

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = Vector2.Zero;

            Vertices rectangle1 = PolygonUtils.CreateRectangle(2f, 2f);
            Vertices rectangle2 = PolygonUtils.CreateRectangle(2f, 2f);

            Vector2 translation = new Vector2(-2f, 0f);
            rectangle1.Translate(ref translation);
            translation = new Vector2(2f, 0f);
            rectangle2.Translate(ref translation);

            List<Vertices> vertices = new List<Vertices>(2);
            vertices.Add(rectangle1);
            vertices.Add(rectangle2);

            _rectangles = BodyFactory.CreateCompoundPolygon(World, vertices, 1f);
            _rectangles.BodyType = BodyType.Dynamic;

            SetUserAgent(_rectangles, 200f, 200f);

            // create sprite based on rectangle fixture
            _rectangleSprite = new Sprite(Managers.TextureManager.PolygonTexture(rectangle1, "Square", Colors.Blue, Colors.Gold, Colors.Black, 1f));
            _offset = new Vector2(ConvertUnits.ToDisplayUnits(2f), 0f);
        }

        public override void Draw()
        {
            Sprites.Begin(0, null, null, null, null, null, Camera.View);

            // draw first rectangle
            Sprites.Draw(_rectangleSprite.Image, ConvertUnits.ToDisplayUnits(_rectangles.Position), null, Color.White, _rectangles.Rotation, _rectangleSprite.Origin + _offset, 1f, SpriteEffects.None, 0f);

            // draw second rectangle
            Sprites.Draw(_rectangleSprite.Image, ConvertUnits.ToDisplayUnits(_rectangles.Position), null, Color.White, _rectangles.Rotation, _rectangleSprite.Origin - _offset, 1f, SpriteEffects.None, 0f);
            Sprites.End();

            base.Draw();
        }

        public override string GetTitle()
        {
            return "Single body with two fixtures";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a single body with two attached fixtures and shapes.");
            sb.AppendLine("A fixture binds a shape to a body and adds material properties such");
            sb.AppendLine("as density, friction, and restitution.");
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