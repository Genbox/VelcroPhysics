#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace FarseerGames.FarseerPhysicsDemos.Components {

    public class FrameRateCounter : DrawableGameComponent {
        ContentManager content;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        float frameTime = 0;
        int frameRate = 0;
        int frameCounter = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;

        System.Globalization.NumberFormatInfo format;


        public FrameRateCounter(Game game)
            : base(game) {
            content = new ContentManager(game.Services);

            format = new System.Globalization.NumberFormatInfo();
            format.NumberDecimalSeparator = ".";
        }


        protected override void LoadContent() {
                spriteBatch = new SpriteBatch(GraphicsDevice);
                spriteFont = content.Load<SpriteFont>(@"Content\Fonts\FrameRateCounterFont");
        }

        protected override void UnloadContent(){
            content.Unload();
        }


        public override void Update(GameTime gameTime) {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1)) {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }
        }


        public override void Draw(GameTime gameTime) {
            frameCounter++;

            if(frameRate>0){frameTime = 1000f / frameRate;}

            string fps = string.Format(format, "fps: {0}", frameRate);
            string ft = string.Format(format, " ft: {0:F}", frameTime);

            spriteBatch.Begin();

            spriteBatch.DrawString(spriteFont, fps, new Vector2(100, 80), Color.White);
            //spriteBatch.DrawString(spriteFont, ft, new Vector2(100, 96), Color.White);

            spriteBatch.End();
        }
    }
}