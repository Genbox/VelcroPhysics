using System;
using FarseerPhysics.Samples.MediaSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.Samples.ScreenSystem
{
    /// <summary>
    /// A popup message box screen, used to display "are you sure?"
    /// confirmation messages.
    /// </summary>
    public class DescriptionBoxScreen : GameScreen
    {
        private const float HorizontalPadding = 32f;
        private const float VerticalPadding = 16f;

        private string _message;
        private Vector2 _topLeft;
        private Vector2 _bottomRight;
        private Vector2 _textPosition;
        private SpriteFont _font;

        public DescriptionBoxScreen(string message)
        {
            _message = message;

            IsPopup = true;
            HasCursor = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.4);
            TransitionOffTime = TimeSpan.FromSeconds(0.4);
        }

        /// <summary>
        /// Loads graphics content for this screen. This uses the shared ContentManager
        /// provided by the Game class, so the content will remain loaded forever.
        /// Whenever a subsequent MessageBoxScreen tries to load this same content,
        /// it will just get back another reference to the already loaded data.
        /// </summary>
        public override void LoadContent()
        {
            _font = ContentWrapper.GetFont("DetailsFont");

            // Center the message text in the viewport.
            Viewport viewport = Framework.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
            Vector2 textSize = _font.MeasureString(_message);
            _textPosition = (viewportSize - textSize) / 2;

            // The background includes a border somewhat larger than the text itself.
            _topLeft.X = _textPosition.X - HorizontalPadding;
            _topLeft.Y = _textPosition.Y - VerticalPadding;
            _bottomRight.X = _textPosition.X + textSize.X + HorizontalPadding;
            _bottomRight.Y = _textPosition.Y + textSize.Y + VerticalPadding;

            base.LoadContent();
        }

        /// <summary>
        /// Responds to user input, accepting or cancelling the message box.
        /// </summary>
        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            if (input.IsMenuSelect() || input.IsMenuCancel() || input.IsNewKeyPress(Keys.F1) || input.IsNewButtonPress(Buttons.Start))
            {
                ExitScreen();
            }
        }

        /// <summary>
        /// Draws the message box.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            Quads.Begin();
            Quads.Render(_topLeft, _bottomRight, null, true, ContentWrapper.Black, ContentWrapper.Grey * 0.65f);
            Quads.End();

            Sprites.Begin();
            // Draw the message box text.
            Sprites.DrawString(_font, _message, _textPosition + Vector2.One, ContentWrapper.Black);
            Sprites.DrawString(_font, _message, _textPosition, ContentWrapper.Beige);
            Sprites.End();
        }
    }
}