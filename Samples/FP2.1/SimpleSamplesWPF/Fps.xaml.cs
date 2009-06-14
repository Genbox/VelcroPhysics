using System;
using System.Windows.Controls;

namespace SimpleSamplesWPF
{
    /// <summary>
    /// Interaction logic for Fps.xaml
    /// </summary>
    public partial class Fps : UserControl
    {
        private int frameCounter;
        private double timeSinceLastUpdate;
        private GameLoop gameLoop;

        public Fps()
        {
            InitializeComponent();
        }

        public void SetGameLoop(GameLoop gameLoop)
        {
            if (gameLoop != null)
            {
                //remove old handlers
                gameLoop.Update -= GameLoopUpdate;
                gameLoop.IsRunningChanged -= GameLoopIsRunningChanged;
            }

            this.gameLoop = gameLoop;

            if (gameLoop != null)
            {
                //add handlers
                gameLoop.Update += GameLoopUpdate;
                gameLoop.IsRunningChanged += GameLoopIsRunningChanged;
            }

            Reset();
        }

        private void GameLoopIsRunningChanged(bool enabled)
        {
            //reset so that we don't get wierd times when game loop is started and stopped
            Reset();
        }

        private void Reset()
        {
            text.Text = "";
            frameCounter = 0;
            timeSinceLastUpdate = 0;
        }

        private void GameLoopUpdate(TimeSpan elapsedTime)
        {
            frameCounter++;
            timeSinceLastUpdate += elapsedTime.TotalSeconds;
            if (frameCounter >= 100)
            {
                text.Text = "FPS: " + (int)(frameCounter / timeSinceLastUpdate);
                frameCounter = 0;
                timeSinceLastUpdate = 0;
            }
        }
    }
}
