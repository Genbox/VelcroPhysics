using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerPhysicsDemos.ScreenSystem
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
            DiagnosticSpriteFont = contentManager.Load<SpriteFont>("Content/Fonts/diagnosticFont");
            MenuSpriteFont = contentManager.Load<SpriteFont>("Content/Fonts/menuFont");
            FrameRateCounterFont = contentManager.Load<SpriteFont>("Content/Fonts/FrameRateCounterFont");
            GameSpriteFont = contentManager.Load<SpriteFont>("Content/Fonts/gamefont");
            DetailsFont = contentManager.Load<SpriteFont>("Content/Fonts/detailsFont");
        }
    }
}