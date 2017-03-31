using System;
using FarseerPhysics.Samples.MediaSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.Samples.ScreenSystem
{
    /// <summary>
    /// Helper class represents a single entry in a OptionScreen.
    /// </summary>
    public sealed class OptionEntry
    {
        private const double HighlightTime = 0.3;
        private const double FadeTime = 0.1;

        private Vector2 _position;
        private Vector2 _size;
        private Color _color;
        private Color _textColor;

        private bool _isChecked;

        private double _hoverFade;
        private double _checkedFade;

        private string _text;

        /// <summary>
        /// Constructs a new option entry with the specified text.
        /// </summary>
        public OptionEntry(string text, bool isChecked)
        {
            _text = text;
            _isChecked = isChecked;

            _hoverFade = 0.0;
            _checkedFade = 0.0;

            SpriteFont font = ContentWrapper.GetFont("MenuFont");
            _size = font.MeasureString(text);
        }

        public void Switch()
        {
            _isChecked = !_isChecked;
        }

        public void InitializePosition(Vector2 target)
        {
            _position = target;
        }

        public string Text
        {
            get { return _text; }
        }

        public Vector2 Position
        {
            get { return _position; }
        }

        public bool IsChecked
        {
            get { return _isChecked; }
        }

        public Vector2 Origin
        {
            get { return _size / 2f; }
        }

        public Vector2 Size
        {
            get { return _size; }
        }

        public Color TextColor
        {
            get { return _textColor; }
        }

        public Color TileColor
        {
            get { return _color; }
        }

        public float CheckedFade
        {
            get { return (float)_checkedFade; }
        }

        public float Scale
        {
            get { return 0.9f + 0.1f * (float)_hoverFade; }
        }

        /// <summary>
        /// Updates the menu entry.
        /// </summary>
        public void Update(bool isHovered, GameTime gameTime)
        {
            if (isHovered)
            {
                _hoverFade = Math.Min(_hoverFade + (gameTime.ElapsedGameTime.TotalSeconds / HighlightTime), 1.0);
            }
            else
            {
                _hoverFade = Math.Max(_hoverFade - (gameTime.ElapsedGameTime.TotalSeconds / HighlightTime), 0.0);
            }
            if (_isChecked)
            {
                _checkedFade = Math.Min(_checkedFade + (gameTime.ElapsedGameTime.TotalSeconds / HighlightTime), 1.0);
            }
            else
            {
                _checkedFade = Math.Max(_checkedFade - (gameTime.ElapsedGameTime.TotalSeconds / HighlightTime), 0.0);
            }

            _textColor = Color.Lerp(ContentWrapper.Beige, ContentWrapper.Gold, (float)_hoverFade);
            _color = Color.Lerp(ContentWrapper.Sky * 0.6f, ContentWrapper.Cyan * 0.6f, (float)_hoverFade);
        }
    }
}