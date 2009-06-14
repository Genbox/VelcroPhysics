using System;

namespace FarseerGames.SimpleSamplesSilverlight.Components
{
    public partial class Fps
    {
        private int _frameCounter = -1;
        private double _timeSinceLastUpdate;

        public Fps()
        {
            InitializeComponent();
            Page.gameLoop.Update += GameLoop_Update;
        }

        private void GameLoop_Update(TimeSpan elapsedTime)
        {
            if (_frameCounter == -1)
            {
                _timeSinceLastUpdate = 0;
            }
            _frameCounter++;
            _timeSinceLastUpdate += elapsedTime.TotalSeconds;
            if (_frameCounter >= 100)
            {
                text.Text = "FPS: " + (int) (_frameCounter/_timeSinceLastUpdate);
                _frameCounter = 0;
                _timeSinceLastUpdate = 0;
            }
        }
    }
}