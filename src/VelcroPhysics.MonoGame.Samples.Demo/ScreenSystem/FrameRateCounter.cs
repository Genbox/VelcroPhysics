using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Demo.ScreenSystem
{
    public class FrameRateCounter
    {
        private readonly NumberFormatInfo _format;
        private readonly Vector2 _position;
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private SpriteFont _font;
        private int _frameCounter;
        private int _frameRate;

        public FrameRateCounter()
        {
            _format = new NumberFormatInfo();
            _format.NumberDecimalSeparator = ".";
            _position = new Vector2(30, 30);
        }

        public void LoadContent()
        {
            _font = Managers.FontManager.GetFont("DetailsFont");
        }

        public void Update(GameTime gameTime)
        {
            _elapsedTime += gameTime.ElapsedGameTime;

            if (_elapsedTime > TimeSpan.FromSeconds(1.0))
            {
                _elapsedTime -= TimeSpan.FromSeconds(1.0);
                _frameRate = _frameCounter;
                _frameCounter = 0;
            }
        }

        public void Draw(SpriteBatch batch)
        {
            _frameCounter++;

            string fps = string.Format(_format, "{0} fps", _frameRate);

            batch.Begin();
            batch.DrawString(_font, fps, _position + Vector2.One, Color.Black);
            batch.DrawString(_font, fps, _position, Color.White);
            batch.End();
        }
    }
}