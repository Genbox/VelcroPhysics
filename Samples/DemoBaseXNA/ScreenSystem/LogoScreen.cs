using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.DemoBaseXNA.ScreenSystem
{
    /// <summary>
    /// The background screen sits behind all the other menu screens.
    /// It draws a background image that remains fixed in place regardless
    /// of whatever transitions the screens on top of it may be doing.
    /// </summary>
    public class LogoScreen : GameScreen
    {
        private Texture2D _farseerLogoTexture;
        private Vector2 _origin;

        public LogoScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.75f);
            TransitionOffTime = TimeSpan.FromSeconds(1.5f);
        }

        public override void LoadContent()
        {
            _farseerLogoTexture = ScreenManager.ContentManager.Load<Texture2D>("Common/logo");
            _origin = new Vector2(_farseerLogoTexture.Width / 2f, _farseerLogoTexture.Height / 2f);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                    bool coveredByOtherScreen)
        {
            if (TransitionPosition == 0)
            {
                ExitScreen();
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(Color.White);

            ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Color tint = new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha, TransitionAlpha);
            ScreenManager.SpriteBatch.Draw(_farseerLogoTexture, ScreenManager.Camera.ScreenCenter, null, tint, 0, _origin,
                                           Vector2.One, SpriteEffects.None, 0);
            ScreenManager.SpriteBatch.End();
        }
    }
}