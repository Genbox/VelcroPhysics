using System;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Demo.Screens
{
    public sealed class MenuSlider
    {
        private const float MaxTranslation = 15f;
        private const double HighlightTime = 0.3;
        private Vector2 _currentPosition;
        private double _hoverFade;
        private double _selectionFade;
        private float _targetY;

        public MenuSlider(Vector2 position)
        {
            _currentPosition = position;
            _targetY = _currentPosition.Y;
        }

        public Vector2 Position => _currentPosition;

        public float Target
        {
            set => _targetY = value;
        }

        public Color TileColor { get; private set; }

        public void Update(bool isHovered, bool isSelected, GameTime gameTime)
        {
            if (isHovered)
                _hoverFade = Math.Min(_hoverFade + gameTime.ElapsedGameTime.TotalSeconds / HighlightTime, 1.0);
            else
                _hoverFade = Math.Max(_hoverFade - gameTime.ElapsedGameTime.TotalSeconds / HighlightTime, 0.0);

            if (isSelected)
                _selectionFade = Math.Min(_selectionFade + gameTime.ElapsedGameTime.TotalSeconds / HighlightTime, 1.0);
            else
                _selectionFade = Math.Max(_selectionFade - gameTime.ElapsedGameTime.TotalSeconds / HighlightTime, 0.0);

            TileColor = Color.Lerp(Colors.Sky * 0.6f, Colors.Grey * 0.6f, (float)_hoverFade);
            TileColor = Color.Lerp(TileColor, Colors.Gold, (float)_selectionFade);

            float deltaY = _targetY - _currentPosition.Y;
            if (Math.Abs(deltaY) > MaxTranslation)
                _currentPosition.Y += MaxTranslation * Math.Sign(deltaY);
            else
                _currentPosition.Y += deltaY;
        }
    }
}