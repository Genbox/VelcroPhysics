using System;
using System.Globalization;
using System.Windows.Controls;
using FarseerPhysics.ScreenSystem;

namespace FarseerPhysics.Components
{
    /// <summary>
    /// Displays the FPS
    /// </summary>
    public class FrameRateCounter : DrawableGameComponent
    {
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private NumberFormatInfo _format;
        private int _frameCounter;
        private int _frameRate;
        private ScreenManager _screenManager;
        private TextBlock _txtFPS;

        public FrameRateCounter(ScreenManager screenManager, TextBlock txtFPS)
            : base(screenManager.Game)
        {
            _screenManager = screenManager;
            _format = new NumberFormatInfo();
            _format.NumberDecimalSeparator = ".";
            _txtFPS = txtFPS;
        }


        public override void Update(GameTime gameTime)
        {
            _elapsedTime += gameTime.ElapsedGameTime;

            if (_elapsedTime <= TimeSpan.FromSeconds(1)) return;

            _elapsedTime -= TimeSpan.FromSeconds(1);
            _frameRate = _frameCounter;
            _frameCounter = 0;
        }

        public override void Draw(GameTime gameTime)
        {
            _frameCounter++;

            _txtFPS.Text = string.Format(_format, "{0} fps", _frameRate);
        }
    }
}