using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FarseerPhysics.ScreenSystem
{
    public class PauseScreen : MenuScreen
    {
        private const int panelHeight = 380;
        private const int panelWidth = 440;
        private string _details = "Details";
        private Color _panelColor = Color.FromArgb(200, 100, 100, 100);
        private Color _textColor = Colors.White;
        private string _title = "Title";

        public PauseScreen(string title, string details)
        {
            IsPopup = true;
            _title = title;
            _details = details;
            TransitionOnTime = TimeSpan.FromSeconds(0.5f);
            TransitionOffTime = TimeSpan.FromSeconds(0.5f);
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
                    ScreenManager.GoToMainMenu();
                    break;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            //Draw Rectangle
            Rectangle rect = new Rectangle();
            rect.Fill = new SolidColorBrush(_panelColor);
            rect.Width = panelWidth;
            rect.Height = panelHeight;
            Canvas.SetLeft(rect, 30);
            Canvas.SetTop(rect, 80);

            if (DebugCanvas != null)
                DebugCanvas.Children.Add(rect);

            // Draw text
            TextBlock txt = new TextBlock();
            txt.Text = _title;
            txt.Foreground = new SolidColorBrush(_textColor);
            txt.FontSize = 16;
            Canvas.SetLeft(txt, 50);
            Canvas.SetTop(txt, 150);

            if (DebugCanvas != null)
                DebugCanvas.Children.Add(txt);

            txt = new TextBlock();
            txt.Text = _details;
            txt.Foreground = new SolidColorBrush(_textColor);
            txt.FontSize = 14;
            Canvas.SetLeft(txt, 50);
            Canvas.SetTop(txt, 180);

            if (DebugCanvas != null)
                DebugCanvas.Children.Add(txt);

            base.Draw(gameTime);
        }
    }
}