using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerGames.FarseerPhysicsDemos.DrawingSystem;

namespace FarseerGames.FarseerPhysicsDemos.ScreenSystem {
    class PauseScreen : MenuScreen  {
        string title = "Title";
        string details = "Details";

        Texture2D panelTexture;
        int panelWidth = 640;
        int panelHeight = 512;
        int leftBorder = 20;
        Color panelColor = new Color(100, 100, 100, 200);

        Texture2D textPanelTexture;
        int textPanelWidth = 440;
        int textPanelHeight = 512;
        int textLeftBorder = 20;
        int textTopBorder = 20;
        int textPanelLeftBorder = 200;
        Color textPanelColor = new Color(100, 100,100, 220);

        Color textColor = Color.White;
        SpriteFont detailsFont;

        public PauseScreen(string title, string details) {
            this.IsPopup = true;
            this.title = title;
            this.details = details;
            TransitionOnTime = TimeSpan.FromSeconds(.2f);
            TransitionOffTime = TimeSpan.FromSeconds(.2f);
        }

        public override void Initialize() {
            base.Initialize();
            MenuEntries.Add("Resume Demo");
            MenuEntries.Add("Quit Demo");
        }

        protected override void OnSelectEntry(int entryIndex) {
            switch (entryIndex) {
                case 0:
                    ExitScreen();

                    break;

                case 1:
                    ExitScreen();
                    ScreenManager.AddScreen(new MainMenuScreen());
                    break;
            }
        }

        public override void LoadContent()
        {
            panelTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, panelWidth, panelHeight, panelColor);
            LeftBorder = ScreenManager.ScreenCenter.X - panelWidth / 2 + leftBorder;

            textPanelTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, textPanelWidth, textPanelHeight, textPanelColor);

            detailsFont = ScreenManager.ContentManager.Load<SpriteFont>(@"Content\Fonts\detailsFont");
            base.LoadContent();
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime) {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            Vector2 panelOrigin = new Vector2(panelTexture.Width/2,panelTexture.Height/2);
            ScreenManager.SpriteBatch.Draw(panelTexture, ScreenManager.ScreenCenter, null, panelColor, 0, panelOrigin, Vector2.One,SpriteEffects.None, 0);

            Vector2 textPanelTexturePosition = new Vector2(ScreenManager.ScreenCenter.X - panelWidth / 2 + textPanelLeftBorder, ScreenManager.ScreenCenter.Y- textPanelHeight / 2);
            ScreenManager.SpriteBatch.Draw(textPanelTexture, textPanelTexturePosition, null, textPanelColor, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);

            Vector2 titlePosition = textPanelTexturePosition + new Vector2(textLeftBorder, textTopBorder);
            ScreenManager.SpriteBatch.DrawString(ScreenManager.MenuSpriteFont, title, titlePosition, textColor);

            Vector2 detailsPosition = titlePosition + new Vector2(0, 75);
            ScreenManager.SpriteBatch.DrawString(detailsFont, details, detailsPosition, textColor);
            
            base.Draw(gameTime);
            ScreenManager.SpriteBatch.End();
        }

        public override void HandleInput(InputState input) {
            if (input.MenuCancel) {
                ExitScreen();
            }
            base.HandleInput(input);
        }

        protected override void OnCancel() {
            ScreenManager.AddScreen(new MainMenuScreen());
        }
    }
}
