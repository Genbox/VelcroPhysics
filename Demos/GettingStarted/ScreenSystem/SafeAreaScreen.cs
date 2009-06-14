using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.FarseerPhysicsDemos.ScreenSystem
{
    public class SafeAreaScreen : GameScreen
    {
        Texture2D tex; // Holds a 1x1 texture containing a single white texel
        int width; // Viewport width
        int height; // Viewport height
        int dx; // 5% of width
        int dy; // 5% of height
        Color notActionSafeColor = new Color(0, 0, 0, 50); // Red, 50% opacity
        Color notTitleSafeColor = new Color(0, 0, 0, 25); // Yellow, 50% opacity
        bool enabled = true;

        public SafeAreaScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0);
            TransitionOffTime = TimeSpan.FromSeconds(0);
        }

        public override void LoadContent() {
            base.LoadContent();
            tex = new Texture2D(ScreenManager.GraphicsDevice, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            Color[] texData = new Color[1];
            texData[0] = Color.White;
            tex.SetData<Color>(texData);
            width = ScreenManager.GraphicsDevice.Viewport.Width;
            height = ScreenManager.GraphicsDevice.Viewport.Height;
            dx = (int)(width * 0.05);
            dy = (int)(height * 0.05);
        }

        public override void HandleInput(InputState input) {
            //enabled = input.CurrentKeyboardState.IsKeyDown(Keys.F1);
            base.HandleInput(input);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (!enabled) return;
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            // Tint the non-action-safe area red
            ScreenManager.SpriteBatch.Draw(tex, new Rectangle(0, 0, width, dy), notActionSafeColor);
            ScreenManager.SpriteBatch.Draw(tex, new Rectangle(0, height - dy, width, dy), notActionSafeColor);
            ScreenManager.SpriteBatch.Draw(tex, new Rectangle(0, dy, dx, height - 2 * dy), notActionSafeColor);
            ScreenManager.SpriteBatch.Draw(tex, new Rectangle(width - dx, dy, dx, height - 2 * dy), notActionSafeColor);

            // Tint the non-title-safe area yellow
            ScreenManager.SpriteBatch.Draw(tex, new Rectangle(dx, dy, width - 2 * dx, dy), notTitleSafeColor);
            ScreenManager.SpriteBatch.Draw(tex, new Rectangle(dx, height - 2 * dy, width - 2 * dx, dy), notTitleSafeColor);
            ScreenManager.SpriteBatch.Draw(tex, new Rectangle(dx, 2 * dy, dx, height - 4 * dy), notTitleSafeColor);
            ScreenManager.SpriteBatch.Draw(tex, new Rectangle(width - 2 * dx, 2 * dy, dx, height - 4 * dy), notTitleSafeColor);

            ScreenManager.SpriteBatch.End();
        }
    }
}


