using System;
using FarseerPhysics.Samples.MediaSystem;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Samples.ScreenSystem
{
    /// <summary>
    /// The background screen sits behind all the other menu screens.
    /// It draws a background image that remains fixed in place regardless
    /// of whatever transitions the screens on top of it may be doing.
    /// </summary>
    public class BackgroundScreen : GameScreen
    {
        private Vector2 _viewportSize;

        /// <summary>
        /// Constructor.
        /// </summary>
        public BackgroundScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        public override void LoadContent()
        {
            _viewportSize = new Vector2(Framework.GraphicsDevice.Viewport.Width, Framework.GraphicsDevice.Viewport.Height);
        }

        /// <summary>
        /// Updates the background screen. Unlike most screens, this should not
        /// transition off even if it has been covered by another screen: it is
        /// supposed to be covered, after all! This overload forces the
        /// coveredByOtherScreen parameter to false in order to stop the base
        /// Update method wanting to transition off.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);
        }

        /// <summary>
        /// Draws the background screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            Quads.Begin();
            Quads.Render(Vector2.Zero, _viewportSize, null, ContentWrapper.Cyan, ContentWrapper.Ocean, ContentWrapper.Cyan, ContentWrapper.Sky);
            Quads.End();
        }
    }
}