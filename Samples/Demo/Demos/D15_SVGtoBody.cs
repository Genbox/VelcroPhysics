using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Samples.Demo.Demos.Prefabs;
using VelcroPhysics.Samples.Demo.MediaSystem;
using VelcroPhysics.Samples.Demo.ScreenSystem;
using VelcroPhysics.Templates;
using VelcroPhysics.Utilities;

namespace VelcroPhysics.Samples.Demo.Demos
{
    internal class D15_SVGtoBody : PhysicsDemoScreen
    {
        private BodyContainer _body;
        private Border _border;
        private Sprite _club;
        private Body _clubBody;
        private Sprite _diamond;
        private Body _diamondBody;
        private Sprite _heart;
        private Body _heartBody;
        private Sprite _spade;
        private Body _spadeBody;

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = new Vector2(0f, 10f);
            _border = new Border(World, Lines, Framework.GraphicsDevice);

            _body = Framework.Content.Load<BodyContainer>("Pipeline/Body");

            _heartBody = _body["Heart"].Create(World);
            _heart = new Sprite(ContentWrapper.GetTexture("Heart"), ContentWrapper.CalculateOrigin(_heartBody));

            _clubBody = _body["Club"].Create(World);
            _club = new Sprite(ContentWrapper.GetTexture("Club"), ContentWrapper.CalculateOrigin(_clubBody));

            _spadeBody = _body["Spade"].Create(World);
            _spade = new Sprite(ContentWrapper.GetTexture("Spade"), ContentWrapper.CalculateOrigin(_spadeBody));

            _diamondBody = _body["Diamond"].Create(World);
            _diamond = new Sprite(ContentWrapper.GetTexture("Diamond"), ContentWrapper.CalculateOrigin(_diamondBody));
        }

        public override void Draw(GameTime gameTime)
        {
            Sprites.Begin(0, null, null, null, null, null, Camera.View);
            Sprites.Draw(_heart.Image, ConvertUnits.ToDisplayUnits(_heartBody.Position), null, Color.White, _heartBody.Rotation, _heart.Origin, 1f, SpriteEffects.None, 0f);
            Sprites.Draw(_club.Image, ConvertUnits.ToDisplayUnits(_clubBody.Position), null, Color.White, _clubBody.Rotation, _club.Origin, 1f, SpriteEffects.None, 0f);
            Sprites.Draw(_spade.Image, ConvertUnits.ToDisplayUnits(_spadeBody.Position), null, Color.White, _spadeBody.Rotation, _spade.Origin, 1f, SpriteEffects.None, 0f);
            Sprites.Draw(_diamond.Image, ConvertUnits.ToDisplayUnits(_diamondBody.Position), null, Color.White, _diamondBody.Rotation, _diamond.Origin, 1f, SpriteEffects.None, 0f);
            Sprites.End();

            _border.Draw(Camera.SimProjection, Camera.SimView);
            base.Draw(gameTime);
        }

        #region Demo description

        public override string GetTitle()
        {
            return "SVG Importer to bodies";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to load bodies from a SVG.");
            sb.AppendLine();
            sb.AppendLine("GamePad:");
            sb.Append("  - Exit to demo selection: Back button");
#if WINDOWS
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Exit to demo selection: Escape");
#endif
            return sb.ToString();
        }

        #endregion
    }
}