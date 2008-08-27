using System;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerPhysicsDemos.ScreenSystem
{
    internal class PauseScreen : MenuScreen
    {
        private readonly string details = "Details";
        private readonly Color panelColor = new Color(100, 100, 100, 200);
        private readonly Color textColor = Color.White;
        private readonly Color textPanelColor = new Color(100, 100, 100, 220);
        private readonly string title = "Title";
        private SpriteFont detailsFont;
        private const int leftBorder = 20;
        private const int panelHeight = 512;

        private Texture2D panelTexture;
        private const int panelWidth = 640;
        private const int textLeftBorder = 20;
        private const int textPanelHeight = 512;
        private const int textPanelLeftBorder = 200;

        private Texture2D textPanelTexture;
        private const int textPanelWidth = 440;
        private const int textTopBorder = 20;

        public PauseScreen(string title, string details)
        {
            IsPopup = true;
            this.title = title;
            this.details = details;
            TransitionOnTime = TimeSpan.FromSeconds(.2f);
            TransitionOffTime = TimeSpan.FromSeconds(.2f);
        }

        public override void Initialize()
        {
            base.Initialize();
            MenuEntries.Add("Resume Demo");
            MenuEntries.Add("Quit Demo");
        }

        protected override void OnSelectEntry(int entryIndex)
        {
            switch (entryIndex)
            {
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
            panelTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, panelWidth, panelHeight,
                                                                panelColor);
            LeftBorder = ScreenManager.ScreenCenter.X - panelWidth/2f + leftBorder;

            textPanelTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, textPanelWidth,
                                                                    textPanelHeight, textPanelColor);

            detailsFont = ScreenManager.ContentManager.Load<SpriteFont>(@"Content\Fonts\detailsFont");
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            Vector2 panelOrigin = new Vector2(panelTexture.Width/2f, panelTexture.Height/2f);
            ScreenManager.SpriteBatch.Draw(panelTexture, ScreenManager.ScreenCenter, null, panelColor, 0, panelOrigin,
                                           Vector2.One, SpriteEffects.None, 0);

            Vector2 textPanelTexturePosition =
                new Vector2(ScreenManager.ScreenCenter.X - panelWidth/2f + textPanelLeftBorder,
                            ScreenManager.ScreenCenter.Y - textPanelHeight/2f);
            ScreenManager.SpriteBatch.Draw(textPanelTexture, textPanelTexturePosition, null, textPanelColor, 0,
                                           Vector2.Zero, Vector2.One, SpriteEffects.None, 0);

            Vector2 titlePosition = textPanelTexturePosition + new Vector2(textLeftBorder, textTopBorder);
            ScreenManager.SpriteBatch.DrawString(ScreenManager.MenuSpriteFont, title, titlePosition, textColor);

            Vector2 detailsPosition = titlePosition + new Vector2(0, 75);
            ScreenManager.SpriteBatch.DrawString(detailsFont, details, detailsPosition, textColor);

            base.Draw(gameTime);
            ScreenManager.SpriteBatch.End();
        }

        public override void HandleInput(InputState input)
        {
            if (input.MenuCancel)
            {
                ExitScreen();
            }
            base.HandleInput(input);
        }

        protected override void OnCancel()
        {
            ScreenManager.AddScreen(new MainMenuScreen());
        }
    }
}