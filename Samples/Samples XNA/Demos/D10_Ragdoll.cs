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
    internal class D10_Ragdoll : PhysicsDemoScreen
    {
        private Border _border;
        private Sprite _obstacle;
        private Body[] _obstacles;
        private Ragdoll _ragdoll;

        #region Demo description
        public override string GetTitle()
        {
            return "Ragdoll";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to combine bodies to create a ragdoll.");
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

            World.Gravity = new Vector2(0f, 20f);

            _border = new Border(World, Lines, Framework.GraphicsDevice);

            _ragdoll = new Ragdoll(World, new Vector2(-20f, -10f));

            _obstacles = new Body[9];
            Vector2 stairStart = new Vector2(-23f, 0f);
            Vector2 stairDelta = new Vector2(2.5f, 1.65f);

            for (int i = 0; i < 9; i++)
            {
                _obstacles[i] = BodyFactory.CreateRectangle(World, 5f, 1.5f, 1f, stairStart + stairDelta * i);
                _obstacles[i].IsStatic = true;
            }

            // create sprite based on body
            _obstacle = new Sprite(ContentWrapper.TextureFromShape(_obstacles[0].FixtureList[0].Shape, "Stripe", ContentWrapper.Red, ContentWrapper.Black, ContentWrapper.Black, 1.5f));

            SetUserAgent(_ragdoll.Body, 1000f, 400f);
        }

        public override void Draw(GameTime gameTime)
        {
            Sprites.Begin(0, null, null, null, null, null, Camera.View);
            
            for (int i = 0; i < 9; i++)
            {
                Sprites.Draw(_obstacle.Image, ConvertUnits.ToDisplayUnits(_obstacles[i].Position), null, Color.White, _obstacles[i].Rotation, _obstacle.Origin, 1f, SpriteEffects.None, 0f);
            }

            _ragdoll.Draw(Sprites);
            Sprites.End();

            _border.Draw(Camera.SimProjection, Camera.SimView);

            base.Draw(gameTime);
        }
    }
}