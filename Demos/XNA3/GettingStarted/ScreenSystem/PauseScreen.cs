using System;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerPhysicsDemos.ScreenSystem
{
    internal class PauseScreen : MenuScreen
    {
        private const int leftBorder = 20;
        private const int panelHeight = 512;
        private const int panelWidth = 640;
        private const int textLeftBorder = 20;
        private const int textPanelHeight = 512;
        private const int textPanelLeftBorder = 200;
        private const int textPanelWidth = 440;
        private const int textTopBorder = 20;
        private readonly string _details = "Details";
        private readonly Color _panelColor = new Color(100, 100, 100, 200);
        private readonly Color _textColor = Color.White;
        private readonly Color _textPanelColor = new Color(100, 100, 100, 220);
        private readonly string _title = "Title";
        private Texture2D _panelTexture;
        private Texture2D _textPanelTexture;

        public PauseScreen(string title, string details)
        {
            IsPopup = true;
            _title = title;
            _details = details;
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
            _panelTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, panelWidth, panelHeight,
                                                                 _panelColor);
            LeftBorder = ScreenManager.ScreenCenter.X - panelWidth/2f + leftBorder;

            _textPanelTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, textPanelWidth,
                                                                     textPanelHeight, _textPanelColor);

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            Vector2 panelOrigin = new Vector2(_panelTexture.Width/2f, _panelTexture.Height/2f);
            ScreenManager.SpriteBatch.Draw(_panelTexture, ScreenManager.ScreenCenter, null, _panelColor, 0, panelOrigin,
                                           Vector2.One, SpriteEffects.None, 0);

            Vector2 textPanelTexturePosition =
                new Vector2(ScreenManager.ScreenCenter.X - panelWidth/2f + textPanelLeftBorder,
                            ScreenManager.ScreenCenter.Y - textPanelHeight/2f);
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