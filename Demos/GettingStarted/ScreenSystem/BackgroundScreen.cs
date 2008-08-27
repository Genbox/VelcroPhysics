#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using FarseerGames.FarseerPhysicsDemos.ScreenSystem;
#endregion

namespace FarseerGames.FarseerPhysicsDemos.ScreenSystem
{
    /// <summary>
    /// The background screen sits behind all the other menu screens.
    /// It draws a background image that remains fixed in place regardless
    /// of whatever transitions the screens on top of it may be doing.
    /// </summary>
    class BackgroundScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        Texture2D backgroundTexture;

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public BackgroundScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(.5f);
            TransitionOffTime = TimeSpan.FromSeconds(.5f);
        }


        /// <summary>
        /// Loads graphics content for this screen. The background texture is quite
        /// big, so we use our own local ContentManager to load it. This allows us
        /// to unload before going from the menus into the game itself, wheras if we
        /// used the shared ContentManager provided by the ScreenManager, the content
        /// would remain loaded forever.
        /// </summary>
        public override void LoadContent()
        {
                if (content == null)
                    content = new ContentManager(ScreenManager.Game.Services);
             
                backgroundTexture = content.Load<Texture2D>("Content/Common/background");
        }


        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public override void UnloadContent()
        {
                content.Unload();
        }

        /// <summary>
        /// Updates the background screen. Unlike most screens, this should not
        /// transition off even if it has been covered by another screen: it is
        /// supposed to be covered, after all! This overload forces the
        /// coveredByOtherScreen parameter to false in order to stop the base
        /// Update method wanting to transition off.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);
        }


        /// <summary>
        /// Draws the background screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);

            byte fade = TransitionAlpha;

            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            //ScreenManager.SpriteBatch.Draw(backgroundTexture, fullscreen,new Color(fade, fade, fade));

            ScreenManager.SpriteBatch.End();
        }
    }
}
