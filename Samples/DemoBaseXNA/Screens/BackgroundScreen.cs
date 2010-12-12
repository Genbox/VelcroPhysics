#region File Description

//-----------------------------------------------------------------------------
// BackgroundScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

using System;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.DemoBaseXNA.Screens
{
    /// <summary>
    /// The background screen sits behind all the other menu screens.
    /// It draws a background image that remains fixed in place regardless
    /// of whatever transitions the screens on top of it may be doing.
    /// </summary>
    public class BackgroundScreen : GameScreen
    {
        private const float LogoScreenHeightRatio = 0.2f;
        private const float LogoScreenBorderRatio = 0.0375f;
        private const float LogoWidthHeightRatio = 2f;

        private Texture2D _backgroundTexture;
        private ContentManager _content;

        /// <summary>
        /// Constructor.
        /// </summary>
        public BackgroundScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        /// <summary>
        /// Loads graphics content for this screen. The background texture is quite
        /// big, so we use our own local ContentManager to load it. This allows us
        /// to unload before going from the menus into the game itself, wheras if we
        /// used the shared ContentManager provided by the Game class, the content
        /// would remain loaded forever.
        /// </summary>
        public override void LoadContent()
        {
            if (_content == null)
                _content = new ContentManager(ScreenManager.Game.Services, "Content");

            _backgroundTexture = _content.Load<Texture2D>("Common/logo");
        }

        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public override void UnloadContent()
        {
            _content.Unload();
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
            Vector2 logoSize = new Vector2();
            logoSize.Y = viewport.Height * LogoScreenHeightRatio;
            logoSize.X = logoSize.Y * LogoWidthHeightRatio;
            float border = viewport.Height * LogoScreenBorderRatio;
            Vector2 logoPosition = new Vector2();
            logoPosition.X = viewport.Width - border - logoSize.X;
            logoPosition.Y = viewport.Height - border - logoSize.Y;
            Rectangle destination = new Rectangle((int)logoPosition.X, (int)logoPosition.Y, (int)logoSize.X, (int)logoSize.Y);

            ScreenManager.SpriteBatch.Begin();
            ScreenManager.SpriteBatch.Draw(_backgroundTexture, destination, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));
            ScreenManager.SpriteBatch.End();
        }
    }
}