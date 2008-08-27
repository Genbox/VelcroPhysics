#region Using Statements
using System;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using FarseerGames.FarseerPhysicsDemos.ScreenSystem;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;
#endregion

namespace FarseerGames.FarseerPhysicsDemos.ScreenSystem
{
    /// <summary>
    /// The background screen sits behind all the other menu screens.
    /// It draws a background image that remains fixed in place regardless
    /// of whatever transitions the screens on top of it may be doing.
    /// </summary>
    class LogoScreen : GameScreen
    {
        ContentManager contentManager;
        Texture2D farseerLogoTexture;
        Vector2 origin;

        public LogoScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(.75);
            TransitionOffTime = TimeSpan.FromSeconds(.75);
        }

        public override void LoadContent()
        {
            if (contentManager == null)
                contentManager = new ContentManager(ScreenManager.Game.Services);

            farseerLogoTexture = contentManager.Load<Texture2D>("Content/Common/logo");
            origin = new Vector2(farseerLogoTexture.Width / 2f, farseerLogoTexture.Height / 2f);
        }

        public override void UnloadContent()
        {
            contentManager.Unload();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            if (this.TransitionPosition == 0) {
                ExitScreen();
            }
            if (ScreenState == ScreenState.TransitionOff && TransitionPosition > .9f) {
                ScreenManager.RemoveScreen(this);
                ScreenManager.AddScreen(new BackgroundScreen());
                ScreenManager.AddScreen(new MainMenuScreen());
            }            

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(Color.White);

            byte fade = 255;// TransitionAlpha;               

            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend );

            Color tint = new Color(fade, fade, fade, fade);

            ScreenManager.SpriteBatch.Draw(farseerLogoTexture, ScreenManager.ScreenCenter, null, tint, 0, origin, Vector2.One, SpriteEffects.None, 0);   

            ScreenManager.SpriteBatch.End();
        }

        public override void HandleInput(InputState input)
        {
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Escape)) ScreenManager.Game.Exit();
            if(input.CurrentGamePadState.Buttons.X == ButtonState.Pressed) ScreenManager.Game.Exit();

            base.HandleInput(input);
        }
    }
}
