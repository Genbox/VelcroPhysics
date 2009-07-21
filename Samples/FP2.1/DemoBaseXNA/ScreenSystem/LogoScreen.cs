using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DemoBaseXNA.ScreenSystem
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
            TransitionOnTime = TimeSpan.FromSeconds(.75);
            TransitionOffTime = TimeSpan.FromSeconds(.75);
        }

        public override void LoadContent()
        {
            _farseerLogoTexture = ScreenManager.ContentManager.Load<Texture2D>("Content/Common/logo");
            _origin = new Vector2(_farseerLogoTexture.Width/2f, _farseerLogoTexture.Height/2f);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                    bool coveredByOtherScreen)
        {
            if (TransitionPosition == 0)
            {
                ExitScreen();
            }
            if (ScreenState == ScreenState.TransitionOff && TransitionPosition > .9f)
            {
                ScreenManager.RemoveScreen(this);
                ScreenManager.GoToMainMenu();
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(Color.White);

            const byte fade = 255; // TransitionAlpha;               
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            Color tint = new Color(fade, fade, fade, fade);
            ScreenManager.SpriteBatch.Draw(_farseerLogoTexture, ScreenManager.ScreenCenter, null, tint, 0, _origin,
                                           Vector2.One, SpriteEffects.None, 0);
            ScreenManager.SpriteBatch.End();
        }

        public override void HandleInput(InputState input)
        {
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Escape)) ScreenManager.Game.Exit();

            base.HandleInput(input);
        }
    }
}