using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using FarseerGames.FarseerPhysicsDemos.DrawingSystem;

namespace FarseerGames.FarseerPhysicsDemos.ScreenSystem
{
    public class DiagnosticsScreen : GameScreen
    {
        Texture2D panelTexture;
        Vector2 panelTextureSize = new Vector2(200, 100);
        Vector2 panelTexturePosition = new Vector2(50, 50);
        public DiagnosticsScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0);
            TransitionOffTime = TimeSpan.FromSeconds(0);
        }

        public override void LoadContent() {
            panelTexture = DrawingSystem.DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, (int)panelTextureSize.X, (int)panelTextureSize.Y, new Color(new Vector4(0, 0, 0, .5f)));
            base.LoadContent();
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            byte fade = TransitionAlpha;

            ScreenManager.SpriteBatch.End();
        }
    }
}
