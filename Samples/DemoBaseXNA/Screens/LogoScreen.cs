using System;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.DemoBaseXNA.Screens
{
    /// <summary>
    /// The background screen sits behind all the other menu screens.
    /// It draws a background image that remains fixed in place regardless
    /// of whatever transitions the screens on top of it may be doing.
    /// </summary>
    public class LogoScreen : GameScreen
    {
        private const float LogoScreenHeightRatio = 4f/6f;
        private const float LogoWidthHeightRatio = 1.4625f;
        private Texture2D _blankTexture;
        private Rectangle _destination;
        private TimeSpan _duration;
        private Texture2D _farseerLogoTexture;

        public LogoScreen(TimeSpan duration)
        {
            _duration = duration;
            TransitionOffTime = TimeSpan.FromSeconds(2.0);
        }

        public override void LoadContent()
        {
            _farseerLogoTexture = ScreenManager.ContentManager.Load<Texture2D>("Common/logo");
            _blankTexture = ScreenManager.ContentManager.Load<Texture2D>("Common/blank");
            UpdateScreen();
            ScreenManager.Camera.ProjectionUpdated += UpdateScreen;
        }

        private void UpdateScreen()
        {
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Vector2 logoSize = new Vector2();
            logoSize.Y = viewport.Height*LogoScreenHeightRatio;
            logoSize.X = logoSize.Y*LogoWidthHeightRatio;
            Vector2 logoPosition = ScreenManager.Camera.ScreenCenter - logoSize/2f;
            _destination = new Rectangle((int) logoPosition.X, (int) logoPosition.Y, (int) logoSize.X, (int) logoSize.Y);
        }

        public override void HandleInput(InputHelper input)
        {
            if (input.CurrentKeyboardState.GetPressedKeys().Length > 0 ||
                input.CurrentGamePadState.IsButtonDown(Buttons.A | Buttons.Start | Buttons.Back) ||
                input.CurrentMouseState.LeftButton == ButtonState.Pressed)
            {
                _duration = TimeSpan.Zero;
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                    bool coveredByOtherScreen)
        {
            _duration -= gameTime.ElapsedGameTime;
            if (_duration <= TimeSpan.Zero)
            {
                ExitScreen();
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(Color.White);

            ScreenManager.SpriteBatch.Begin();
            ScreenManager.SpriteBatch.Draw(_farseerLogoTexture, _destination, Color.White);
            ScreenManager.SpriteBatch.End();
        }
    }
}