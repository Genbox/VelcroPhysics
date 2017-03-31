using System.Text;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Samples.Demos.Prefabs;
using FarseerPhysics.Samples.MediaSystem;
using FarseerPhysics.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.Samples.Demos
{
    internal class D06_Restitution : PhysicsDemoScreen
    {
        private Border _border;
        private Body[] _circle = new Body[6];
        private Sprite _circleSprite;

        #region Demo description
        public override string GetTitle()
        {
            return "Restitution";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows several bodies with varying restitution.");
            sb.AppendLine();
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Move cursor: Left thumbstick");
            sb.AppendLine("  - Grab object (beneath cursor): A button");
            sb.AppendLine("  - Drag grabbed object: Left thumbstick");
            sb.Append("  - Exit to demo selection: Back button");
#if WINDOWS
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("Keyboard:");
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

            World.Gravity = new Vector2(0f, 20f);

            _border = new Border(World, Lines, Framework.GraphicsDevice);

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
            _circleSprite = new Sprite(ContentWrapper.TextureFromShape(_circle[0].FixtureList[0].Shape, "Square", ContentWrapper.Green, ContentWrapper.Lime, ContentWrapper.Black, 1f));
        }

        public override void Draw(GameTime gameTime)
        {
            Sprites.Begin(0, null, null, null, null, null, Camera.View);
            
            for (int i = 0; i < 6; ++i)
            {
                Sprites.Draw(_circleSprite.Image, ConvertUnits.ToDisplayUnits(_circle[i].Position), null, Color.White, _circle[i].Rotation, _circleSprite.Origin, 1f, SpriteEffects.None, 0f);
            }

            Sprites.End();

            _border.Draw(Camera.SimProjection, Camera.SimView);

            base.Draw(gameTime);
        }
    }
}