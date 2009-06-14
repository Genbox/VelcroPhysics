using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.Components
{
    /// <summary>
    /// Displays the FPS
    /// </summary>
    public class FrameRateCounter : DrawableGameComponent
    {
        private ContentManager _content;
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private SpriteFont _font;
        private NumberFormatInfo _format;
        private int _frameCounter;
        private int _frameRate;
        private SpriteBatch _sb;

        public FrameRateCounter(Game game, GraphicsDevice device, ContentManager content)
            : base(game)
        {
            _sb = new SpriteBatch(device);
            _content = content;
            _format = new NumberFormatInfo();
            _format.NumberDecimalSeparator = ".";
        }

        protected override void LoadContent()
        {
            _font = _content.Load<SpriteFont>("Monaco");

            base.LoadContent();
        }


        public override void Update(GameTime gameTime)
        {
            _elapsedTime += gameTime.ElapsedGameTime;

            if (_elapsedTime <= TimeSpan.FromSeconds(1)) return;

            _elapsedTime -= TimeSpan.FromSeconds(1);
            _frameRate = _frameCounter;
            _frameCounter = 0;
        }

        public override void Draw(GameTime gameTime)
        {
            _frameCounter++;

            string fps = string.Format(_format, "fps: {0}", _frameRate);

            _sb.Begin();
            _sb.DrawString(_font, fps, new Vector2(100, 80), Color.Black);
            _sb.End();
        }
    }
}