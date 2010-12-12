using System;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.DemoBaseXNA.Screens
{
    /// <summary>
    /// The background screen sits behind all the other menu screens.
    /// It draws a background image that remains fixed in place regardless
    /// of whatever transitions the screens on top of it may be doing.
    /// </summary>
    public class LogoScreen : GameScreen
    {
        private const float LogoScreenHeightRatio = 13f / 30f;
        private const float LogoWidthHeightRatio = 2f;
        private Texture2D _farseerLogoTexture;
        private Texture2D _blankTexture;
        private Vector2 _origin;

        public LogoScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.75f);
            TransitionOffTime = TimeSpan.FromSeconds(1.5f);
        }

        public override void LoadContent()
        {
            _farseerLogoTexture = ScreenManager.ContentManager.Load<Texture2D>("Common/logo");
            _blankTexture = ScreenManager.ContentManager.Load<Texture2D>("Common/blank");
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
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Vector2 logoSize = new Vector2();
            logoSize.Y = viewport.Height * LogoScreenHeightRatio;
            logoSize.X = logoSize.Y * LogoWidthHeightRatio;
            Vector2 logoPosition = ScreenManager.Camera.ScreenCenter - logoSize / 2f;
            Rectangle destination = new Rectangle((int)logoPosition.X, (int)logoPosition.Y, (int)logoSize.X, (int)logoSize.Y);

            if (ScreenState != ScreenSystem.ScreenState.TransitionOff)
            {
                ScreenManager.GraphicsDevice.Clear(Color.White);
            }

            ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            if (ScreenState == ScreenSystem.ScreenState.TransitionOff)
            {
                ScreenManager.SpriteBatch.Draw(_blankTexture, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.White * TransitionAlpha);
            }
            ScreenManager.SpriteBatch.Draw(_farseerLogoTexture, destination, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha, TransitionAlpha));
            ScreenManager.SpriteBatch.End();
        }
    }
}