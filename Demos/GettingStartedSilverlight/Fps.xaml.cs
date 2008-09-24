using System;
using System.Windows.Controls;

namespace FarseerSilverlightDemos
{
    public partial class Fps : UserControl
    {
        private int frameCounter = -1;
        private double timeSinceLastUpdate;

        public Fps()
        {
            InitializeComponent();
            Page.gameLoop.Update += gameLoop_Update;
        }

        private void gameLoop_Update(TimeSpan ElapsedTime)
        {
            if (frameCounter == -1)
            {
                timeSinceLastUpdate = 0;
            }
            frameCounter++;
            timeSinceLastUpdate += ElapsedTime.TotalSeconds;
            if (frameCounter >= 100)
            {
                text.Text = "FPS: " + (int) (frameCounter/timeSinceLastUpdate);
                frameCounter = 0;
                timeSinceLastUpdate = 0;
            }
        }
    }
}