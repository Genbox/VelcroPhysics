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
    internal class D15_TextureToShapes : PhysicsDemoScreen
    {
        private Border _border;
        private Body _compound;
        private Sprite _objectSprite;

        #region Demo description
        public override string GetTitle()
        {
            return "Texture to collision shapes";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to create collision shapes from a texture.");
            sb.AppendLine("These are added to a single body with multiple fixtures.");
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

            List<Vertices> tracedObject = Framework.Content.Load<List<Vertices>>("Pipeline/Object");

            // Create a single body with multiple fixtures
            _compound = BodyFactory.CreateCompoundPolygon(World, tracedObject, 1f, Vector2.Zero, 0, BodyType.Dynamic);

            SetUserAgent(_compound, 200f, 200f);
            _objectSprite = new Sprite(ContentWrapper.GetTexture("Logo"), ContentWrapper.CalculateOrigin(_compound));
        }

        public override void Draw(GameTime gameTime)
        {
            Sprites.Begin(0, null, null, null, null, null, Camera.View);
            Sprites.Draw(_objectSprite.Image, ConvertUnits.ToDisplayUnits(_compound.Position), null, Color.White, _compound.Rotation, _objectSprite.Origin, 1f, SpriteEffects.None, 0f);
            Sprites.End();

            _border.Draw(Camera.SimProjection, Camera.SimView);

            base.Draw(gameTime);
        }
    }
}