using System;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Demo.Screens
{
    /// <summary>Helper class represents a single entry in a OptionScreen.</summary>
    public sealed class OptionEntry
    {
        private const double HighlightTime = 0.3;
        private double _checkedFade;
        private double _hoverFade;

        public OptionEntry(string text, bool isChecked)
        {
            Text = text;
            IsChecked = isChecked;

            _hoverFade = 0.0;
            _checkedFade = 0.0;

            SpriteFont font = Managers.FontManager.GetFont("MenuFont");
            Size = font.MeasureString(text);
        }

        public string Text { get; }

        public Vector2 Position { get; private set; }

        public bool IsChecked { get; private set; }

        public Vector2 Origin => Size / 2f;

        public Vector2 Size { get; }

        public Color TextColor { get; private set; }

        public Color TileColor { get; private set; }

        public float CheckedFade => (float)_checkedFade;

        public float Scale => 0.9f + 0.1f * (float)_hoverFade;

        public void Switch()
        {
            IsChecked = !IsChecked;
        }

        public void InitializePosition(Vector2 target)
        {
            Position = target;
        }

        public void Update(bool isHovered, GameTime gameTime)
        {
            if (isHovered)
                _hoverFade = Math.Min(_hoverFade + gameTime.ElapsedGameTime.TotalSeconds / HighlightTime, 1.0);
            else
                _hoverFade = Math.Max(_hoverFade - gameTime.ElapsedGameTime.TotalSeconds / HighlightTime, 0.0);
            if (IsChecked)
                _checkedFade = Math.Min(_checkedFade + gameTime.ElapsedGameTime.TotalSeconds / HighlightTime, 1.0);
            else
                _checkedFade = Math.Max(_checkedFade - gameTime.ElapsedGameTime.TotalSeconds / HighlightTime, 0.0);

            TextColor = Color.Lerp(Colors.Beige, Colors.Gold, (float)_hoverFade);
            TileColor = Color.Lerp(Colors.Sky * 0.6f, Colors.Cyan * 0.6f, (float)_hoverFade);
        }
    }
}