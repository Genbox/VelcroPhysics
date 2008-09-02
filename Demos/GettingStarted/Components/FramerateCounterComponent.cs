#region Using Statements

using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace FarseerGames.FarseerPhysicsDemos.Components
{
    public class FrameRateCounter : DrawableGameComponent
    {
        private readonly ContentManager _content;
        private readonly NumberFormatInfo _format;
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private int _frameCounter;
        private int _frameRate;
    // TODO: Use screenmanager.
        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;


        public FrameRateCounter(Game game)
            : base(game)
        {
            _content = new ContentManager(game.Services);

            _format = new NumberFormatInfo();
            _format.NumberDecimalSeparator = ".";
        }


        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteFont = _content.Load<SpriteFont>(@"Content\Fonts\FrameRateCounterFont");
        }

        protected override void UnloadContent()
        {
            _content.Unload();
        }

        public override void Update(GameTime gameTime)
        {
            _elapsedTime += gameTime.ElapsedGameTime;

            if (_elapsedTime > TimeSpan.FromSeconds(1))
            {
                _elapsedTime -= TimeSpan.FromSeconds(1);
                _frameRate = _frameCounter;
                _frameCounter = 0;
            }
        }


        public override void Draw(GameTime gameTime)
        {
            _frameCounter++;

            string fps = string.Format(_format, "fps: {0}", _frameRate);

            _spriteBatch.Begin();
            _spriteBatch.DrawString(_spriteFont, fps, new Vector2(100, 80), Color.White);
            _spriteBatch.End();
        }
    }
}