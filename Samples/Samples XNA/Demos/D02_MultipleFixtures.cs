using System.Collections.Generic;
using System.Text;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Samples.Demos.Prefabs;
using FarseerPhysics.Samples.MediaSystem;
using FarseerPhysics.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.Samples.Demos
{
    internal class D02_MultipleFixtures : PhysicsDemoScreen
    {
        private Border _border;
        private Sprite _rectangleSprite;
        private Body _rectangles;
        private Vector2 _offset;

        #region Demo description
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
            sb.AppendLine();
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Rotate object: Left and right trigger");
            sb.AppendLine("  - Move object: Right thumbstick");
            sb.AppendLine("  - Move cursor: Left thumbstick");
            sb.AppendLine("  - Grab object (beneath cursor): A button");
            sb.AppendLine("  - Drag grabbed object: Left thumbstick");
            sb.Append("  - Exit to demo selection: Back button");
#if WINDOWS
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Rotate object: Q, E");
            sb.AppendLine("  - Move object: W, S, A, D");
            sb.AppendLine("  - Exit to demo selection: Escape");
            sb.AppendLine();
            sb.AppendLine("Mouse");
            sb.AppendLine("  - Grab object (beneath cursor): Left click");
            sb.Append("  - Drag grabbed object: Move mouse");
#endif
            return sb.ToString();
        }
        #endregion

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = Vector2.Zero;

            _border = new Border(World, Lines, Framework.GraphicsDevice);

            Vertices rectangle1 = PolygonTools.CreateRectangle(2f, 2f);
            Vertices rectangle2 = PolygonTools.CreateRectangle(2f, 2f);

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
            _rectangleSprite = new Sprite(ContentWrapper.PolygonTexture(rectangle1, "Square", ContentWrapper.Blue, ContentWrapper.Gold, ContentWrapper.Black, 1f));
            _offset = new Vector2(ConvertUnits.ToDisplayUnits(2f), 0f);
        }

        public override void Draw(GameTime gameTime)
        {
            Sprites.Begin(0, null, null, null, null, null, Camera.View);
            // draw first rectangle
            Sprites.Draw(_rectangleSprite.Image, ConvertUnits.ToDisplayUnits(_rectangles.Position), null, Color.White, _rectangles.Rotation, _rectangleSprite.Origin + _offset, 1f, SpriteEffects.None, 0f);

            // draw second rectangle
            Sprites.Draw(_rectangleSprite.Image, ConvertUnits.ToDisplayUnits(_rectangles.Position), null, Color.White, _rectangles.Rotation, _rectangleSprite.Origin - _offset, 1f, SpriteEffects.None, 0f);
            Sprites.End();

            _border.Draw(Camera.SimProjection, Camera.SimView);

            base.Draw(gameTime);
        }
    }
}