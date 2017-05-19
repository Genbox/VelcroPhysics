using System;
using Microsoft.Xna.Framework;
using VelcroPhysics.Samples.Demo.MediaSystem;

namespace VelcroPhysics.Samples.Demo.ScreenSystem
{
    public sealed class MenuSlider
    {
        private const float MaxTranslation = 15f;
        private const double HighlightTime = 0.3;
        private Vector2 _currentPosition;

        private double _hoverFade;

        private float _positionX;
        private double _selectionFade;

        private float _targetY;

        /// <summary>
        /// Constructs a new menu slider.
        /// </summary>
        public MenuSlider(Vector2 position)
        {
            _currentPosition = position;
            _positionX = _currentPosition.X;
            _targetY = _currentPosition.Y;
        }

        public Vector2 Position
        {
            get { return _currentPosition; }
        }

        public float Target
        {
            set { _targetY = value; }
        }

        public Color TileColor { get; private set; }

        /// <summary>
        /// Updates the menu slider.
        /// </summary>
        public void Update(bool isHovered, bool isSelected, GameTime gameTime)
        {
            if (isHovered)
            {
                _hoverFade = Math.Min(_hoverFade + gameTime.ElapsedGameTime.TotalSeconds / HighlightTime, 1.0);
            }
            else
            {
                _hoverFade = Math.Max(_hoverFade - gameTime.ElapsedGameTime.TotalSeconds / HighlightTime, 0.0);
            }

            if (isSelected)
            {
                _selectionFade = Math.Min(_selectionFade + gameTime.ElapsedGameTime.TotalSeconds / HighlightTime, 1.0);
            }
            else
            {
                _selectionFade = Math.Max(_selectionFade - gameTime.ElapsedGameTime.TotalSeconds / HighlightTime, 0.0);
            }

            TileColor = Color.Lerp(ContentWrapper.Sky * 0.6f, ContentWrapper.Grey * 0.6f, (float)_hoverFade);
            TileColor = Color.Lerp(TileColor, ContentWrapper.Gold, (float)_selectionFade);

            float deltaY = _targetY - _currentPosition.Y;
            if (Math.Abs(deltaY) > MaxTranslation)
            {
                _currentPosition.Y += MaxTranslation * Math.Sign(deltaY);
            }
            else
            {
                _currentPosition.Y += deltaY;
            }
        }
    }
}