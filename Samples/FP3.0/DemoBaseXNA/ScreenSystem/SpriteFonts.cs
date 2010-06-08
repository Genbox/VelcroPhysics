using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.DemoBaseXNA.ScreenSystem
{
    public class SpriteFonts
    {
        private SpriteFont _detailsFont;
        private SpriteFont _diagnosticSpriteFont;
        private SpriteFont _frameRateCounterFont;
        private SpriteFont _gameSpriteFont;
        private SpriteFont _menuSpriteFont;

        public SpriteFonts(ContentManager contentManager)
        {
            _diagnosticSpriteFont = contentManager.Load<SpriteFont>("Fonts/diagnosticFont");
            _menuSpriteFont = contentManager.Load<SpriteFont>("Fonts/menuFont");
            _frameRateCounterFont = contentManager.Load<SpriteFont>("Fonts/frameRateCounterFont");
            _gameSpriteFont = contentManager.Load<SpriteFont>("Fonts/gamefont");
            _detailsFont = contentManager.Load<SpriteFont>("Fonts/detailsFont");
        }

        public SpriteFont DetailsFont
        {
            get { return _detailsFont; }
        }

        public SpriteFont DiagnosticSpriteFont
        {
            get { return _diagnosticSpriteFont; }
        }

        public SpriteFont FrameRateCounterFont
        {
            get { return _frameRateCounterFont; }
        }

        public SpriteFont GameSpriteFont
        {
            get { return _gameSpriteFont; }
        }

        public SpriteFont MenuSpriteFont
        {
            get { return _menuSpriteFont; }
        }
    }
}