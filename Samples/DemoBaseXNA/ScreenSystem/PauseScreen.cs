using System;
using FarseerPhysics.DemoBaseXNA.DrawingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.DemoBaseXNA.ScreenSystem
{
    public class PauseScreen : MenuScreen
    {
        private const int leftBorder = 20;
        private const int panelHeight = 512;
        private const int panelWidth = 640;
        private const int textLeftBorder = 20;
        private const int textPanelHeight = 512;
        private const int textPanelLeftBorder = 200;
        private const int textPanelWidth = 440;
        private const int textTopBorder = 20;
        private string _details = "Details";
        private Color _panelColor = new Color(100, 100, 100, 200);
        private Texture2D _panelTexture;
        private Color _textColor = Color.White;
        private Color _textPanelColor = new Color(100, 100, 100, 220);
        private Texture2D _textPanelTexture;
        private string _title = "Title";

        public PauseScreen(string title, string details)
        {
            IsPopup = true;
            _title = title;
            _details = details;
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
                    //also remove the screen that called this pausescreen
                    ScreenManager.RemoveScreen(this);
                    break;
            }
        }

        public override void LoadContent()
        {
            _panelTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, panelWidth, panelHeight,
                                                                 _panelColor);
            LeftBorder = ScreenManager.Camera.ScreenCenter.X - panelWidth / 2f + leftBorder;

            _textPanelTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, textPanelWidth,
                                                                     textPanelHeight, _textPanelColor);

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            Vector2 panelOrigin = new Vector2(_panelTexture.Width / 2f, _panelTexture.Height / 2f);
            ScreenManager.SpriteBatch.Draw(_panelTexture, ScreenManager.Camera.ScreenCenter, null, _panelColor, 0, panelOrigin,
                                           Vector2.One, SpriteEffects.None, 0);

            Vector2 textPanelTexturePosition =
                new Vector2(ScreenManager.Camera.ScreenCenter.X - panelWidth / 2f + textPanelLeftBorder,
                            ScreenManager.Camera.ScreenCenter.Y - textPanelHeight / 2f);
            ScreenManager.SpriteBatch.Draw(_textPanelTexture, textPanelTexturePosition, null, _textPanelColor, 0,
                                           Vector2.Zero, Vector2.One, SpriteEffects.None, 0);

            Vector2 titlePosition = textPanelTexturePosition + new Vector2(textLeftBorder, textTopBorder);
            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.MenuSpriteFont, _title, titlePosition,
                                                 _textColor);

            Vector2 detailsPosition = titlePosition + new Vector2(0, 75);
            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DetailsFont, _details, detailsPosition,
                                                 _textColor);

            base.Draw(gameTime);
            ScreenManager.SpriteBatch.End();
        }
    }
}