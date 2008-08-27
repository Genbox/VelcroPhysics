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
        private readonly ContentManager content;
        private readonly NumberFormatInfo format;
        private TimeSpan elapsedTime = TimeSpan.Zero;
        private int frameCounter;
        private int frameRate;
        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;


        public FrameRateCounter(Game game)
            : base(game)
        {
            content = new ContentManager(game.Services);

            format = new NumberFormatInfo();
            format.NumberDecimalSeparator = ".";
        }


        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = content.Load<SpriteFont>(@"Content\Fonts\FrameRateCounterFont");
        }

        protected override void UnloadContent()
        {
            content.Unload();
        }

        public override void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }
        }


        public override void Draw(GameTime gameTime)
        {
            frameCounter++;

            string fps = string.Format(format, "fps: {0}", frameRate);

            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, fps, new Vector2(100, 80), Color.White);
            spriteBatch.End();
        }
    }
}