using System.Text;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.Demos.Prefabs;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem.Graphics;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.Screens;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.ScreenSystem;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Demo.Demos
{
    internal class D13_TheoJansenWalker : PhysicsDemoScreen
    {
        private Body[] _circles;

        private Sprite _grain;
        private TheoJansenWalker _walker;

        public override void LoadContent()
        {
            base.LoadContent();

            HasCursor = false;

            World.Gravity = new Vector2(0, 9.82f);

            CircleShape shape = new CircleShape(0.25f, 1);
            _grain = new Sprite(Managers.TextureManager.CircleTexture(0.25f, Colors.Gold, Colors.Grey));

            _circles = new Body[48];
            for (int i = 0; i < 48; i++)
            {
                _circles[i] = BodyFactory.CreateBody(World);
                _circles[i].BodyType = BodyType.Dynamic;
                _circles[i].Position = new Vector2(-24f + 1f * i, 10f);
                _circles[i].AddFixture(shape);
            }

            _walker = new TheoJansenWalker(World, Vector2.Zero);
        }

        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            if (input.IsNewButtonPress(Buttons.A) || input.IsNewMouseButtonPress(MouseButtons.RightButton) || input.IsNewKeyPress(Keys.Space))
                _walker.Reverse();

            base.HandleInput(input, gameTime);
        }

        public override void Draw()
        {
            Sprites.Begin(0, null, null, null, null, null, Camera.View);
            for (int i = 0; i < 48; i++)
            {
                Sprites.Draw(_grain.Image, ConvertUnits.ToDisplayUnits(_circles[i].Position), null, Color.White, _circles[i].Rotation, _grain.Origin, 1f, SpriteEffects.None, 1f);
            }

            Sprites.End();

            _walker.Draw(Sprites, Lines, Camera);

            base.Draw();
        }

        public override string GetTitle()
        {
            return "Theo Jansen's Strandbeast";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how complex mechanical structures can be realized.");
            sb.AppendLine("http://www.strandbeest.com/");
#if WINDOWS
            sb.AppendLine();
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Switch walker direction: Space");
            sb.AppendLine("  - Exit to demo selection: Escape");
            sb.AppendLine();
            sb.AppendLine("Mouse:");
            sb.AppendLine("  - Switch walker direction: Right click");
#elif XBOX
            sb.AppendLine();
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Switch walker direction: A button");
            sb.AppendLine("  - Exit to demo selection: Back button");
#endif
            return sb.ToString();
        }
    }
}