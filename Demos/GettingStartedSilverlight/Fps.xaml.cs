using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace FarseerSilverlightDemos
{
    public partial class Fps : UserControl
    {
        int frameCounter = -1;
        double timeSinceLastUpdate=0;

        public Fps()
        {
            InitializeComponent();
            Page.gameLoop.Update += new GameLoop.UpdateDelegate(gameLoop_Update);
        }

        void gameLoop_Update(TimeSpan ElapsedTime)
        {
            if (frameCounter == -1)
            {
                timeSinceLastUpdate = 0;
            }
            frameCounter++;
            timeSinceLastUpdate += ElapsedTime.TotalSeconds;
            if (frameCounter >= 100)
            {
                text.Text = "FPS: " + (int)(frameCounter / timeSinceLastUpdate);
                frameCounter = 0;
                timeSinceLastUpdate = 0;
            }
        }
    }
}
