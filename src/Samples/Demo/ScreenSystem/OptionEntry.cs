using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VelcroPhysics.Samples.Demo.MediaSystem;

namespace VelcroPhysics.Samples.Demo.ScreenSystem
{
    /// <summary>
    /// Helper class represents a single entry in a OptionScreen.
    /// </summary>
    public sealed class OptionEntry
    {
        private const double HighlightTime = 0.3;
        private const double FadeTime = 0.1;
        private double _checkedFade;

        private double _hoverFade;

        /// <summary>
        /// Constructs a new option entry with the specified text.
        /// </summary>
        public OptionEntry(string text, bool isChecked)
        {
            Text = text;
            IsChecked = isChecked;

            _hoverFade = 0.0;
            _checkedFade = 0.0;

            SpriteFont font = ContentWrapper.GetFont("MenuFont");
            Size = font.MeasureString(text);
        }

        public string Text { get; }

        public Vector2 Position { get; private set; }

        public bool IsChecked { get; private set; }

        public Vector2 Origin
        {
            get { return Size / 2f; }
        }

        public Vector2 Size { get; }

        public Color TextColor { get; private set; }

        public Color TileColor { get; private set; }

        public float CheckedFade
        {
            get { return (float)_checkedFade; }
        }

        public float Scale
        {
            get { return 0.9f + 0.1f * (float)_hoverFade; }
        }

        public void Switch()
        {
            IsChecked = !IsChecked;
        }

        public void InitializePosition(Vector2 target)
        {
            Position = target;
        }

        /// <summary>
        /// Updates the menu entry.
        /// </summary>
        public void Update(bool isHovered, GameTime gameTime)
        {
            if (isHovered)
            {
                _hoverFade = Math.Min(_hoverFade + gameTime.ElapsedGameTime.TotalSeconds / HighlightTime, 1.0);
            }
            else
            {
                _hoverFade = Math.Max(_hoverFade - gameTime.ElapsedGameTime.TotalSeconds / HighlightTime, 0.0);
            }
            if (IsChecked)
            {
                _checkedFade = Math.Min(_checkedFade + gameTime.ElapsedGameTime.TotalSeconds / HighlightTime, 1.0);
            }
            else
            {
                _checkedFade = Math.Max(_checkedFade - gameTime.ElapsedGameTime.TotalSeconds / HighlightTime, 0.0);
            }

            TextColor = Color.Lerp(ContentWrapper.Beige, ContentWrapper.Gold, (float)_hoverFade);
            TileColor = Color.Lerp(ContentWrapper.Sky * 0.6f, ContentWrapper.Cyan * 0.6f, (float)_hoverFade);
        }
    }
}