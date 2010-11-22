using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.DemoBaseXNA.ScreenSystem
{
    public class SpriteFonts
    {
        public SpriteFont DetailsFont;
        public SpriteFont DiagnosticSpriteFont;
        public SpriteFont FrameRateCounterFont;
        public SpriteFont GameSpriteFont;
        public SpriteFont MenuSpriteFont;

        public SpriteFonts(ContentManager contentManager)
        {
            DiagnosticSpriteFont = contentManager.Load<SpriteFont>("Fonts/diagnosticFont");
            MenuSpriteFont = contentManager.Load<SpriteFont>("Fonts/menuFont");
            FrameRateCounterFont = contentManager.Load<SpriteFont>("Fonts/frameRateCounterFont");
            GameSpriteFont = contentManager.Load<SpriteFont>("Fonts/gamefont");
            DetailsFont = contentManager.Load<SpriteFont>("Fonts/detailsFont");
        }
    }
}