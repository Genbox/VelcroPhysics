using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VelcroPhysics.Samples.Demo.MediaSystem;

namespace VelcroPhysics.Samples.Demo.ScreenSystem
{
    /// <summary>
    /// Helper class represents a single entry in a MenuScreen.
    /// </summary>
    public sealed class MenuEntry : IComparable
    {
        private const float MaxTranslation = 10f;
        private const double HighlightTime = 0.3;
        private const double FadeTime = 0.4;

        private static float _targetHiddenX;
        private static float _targetVisibleX;

        private readonly PhysicsDemoScreen _screen;
        private Vector2 _currentPosition;

        private double _hoverFade;
        private double _selectionFade;

        private float _targetY;
        private double _visibleFade;

        /// <summary>
        /// Constructs a new menu entry with the specified text.
        /// </summary>
        public MenuEntry(string text, PhysicsDemoScreen screen, Texture2D preview)
        {
            Text = text;
            _screen = screen;
            Preview = preview;

            _hoverFade = 0.0;
            _selectionFade = 0.0;

            SpriteFont font = ContentWrapper.GetFont("MenuFont");
            Size = font.MeasureString(text);
        }

        public string Text { get; }

        public Vector2 Position
        {
            get { return _currentPosition; }
        }

        public float Target
        {
            set { _targetY = value; }
        }

        public Vector2 Origin
        {
            get { return Size / 2f; }
        }

        public Vector2 Size { get; }

        public float Fade
        {
            get { return (float)_selectionFade; }
        }

        public GameScreen Screen
        {
            get { return _screen; }
        }

        public Texture2D Preview { get; }

        public Color TextColor { get; private set; }

        public Color TileColor { get; private set; }

        public float Scale
        {
            get { return 0.9f + 0.1f * (float)_hoverFade; }
        }

        public bool Visible { get; set; }

        public float Alpha
        {
            get { return (float)_visibleFade; }
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            MenuEntry entry = obj as MenuEntry;
            if (entry == null)
            {
                return 0;
            }
            return _screen.GetType().ToString().CompareTo(entry._screen.GetType().ToString());
        }

        #endregion

        public static void InitializeEntries(float hiddenX, float visibleX)
        {
            _targetHiddenX = hiddenX;
            _targetVisibleX = visibleX;
        }

        public void InitializePosition(float target, bool visible)
        {
            Visible = visible;
            _visibleFade = visible ? 1.0 : 0.0;
            _currentPosition.X = visible ? _targetVisibleX : _targetHiddenX;
            _currentPosition.Y = _targetY = target;
        }

        /// <summary>
        /// Updates the menu entry.
        /// </summary>
        public void Update(bool isSelected, bool isHovered, GameTime gameTime)
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

            TextColor = Color.Lerp(ContentWrapper.Beige, ContentWrapper.Gold, (float)_selectionFade);
            TileColor = Color.Lerp(ContentWrapper.Sky * 0.6f, ContentWrapper.Grey * 0.6f, (float)Math.Max(_selectionFade, _hoverFade));

            if (Visible)
            {
                _visibleFade = Math.Min(_visibleFade + gameTime.ElapsedGameTime.TotalSeconds / FadeTime, 1.0);
            }
            else
            {
                _visibleFade = Math.Max(_visibleFade - gameTime.ElapsedGameTime.TotalSeconds / FadeTime, 0.0);
            }
            _currentPosition.X = MathHelper.SmoothStep(_targetHiddenX, _targetVisibleX, (float)_visibleFade);

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