using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace FarseerSilverlightDemos
{
    public class GameLoop
    {
        #region Delegates

        public delegate void UpdateDelegate(TimeSpan ElapsedTime);

        #endregion

        private TimeSpan elapsedTime;
        private DateTime lastUpdateTime = DateTime.MinValue;
        private Storyboard storyboard;
        private Canvas targetCanvas;

        public event UpdateDelegate Update;

        public void Attach(Canvas canvas)
        {
            targetCanvas = canvas;
            storyboard = new Storyboard();
            storyboard.SetValue(FrameworkElement.NameProperty, "gameloop");
            canvas.Resources.Add("gameloop", storyboard);
            lastUpdateTime = DateTime.Now;
            storyboard.Completed += storyboard_Completed;
            storyboard.Begin();
        }

        public void Detach(Canvas canvas)
        {
            storyboard.Stop();
            canvas.Resources.Remove("gameloop");
        }

        private void storyboard_Completed(object sender, EventArgs e)
        {
            elapsedTime = DateTime.Now - lastUpdateTime;
            lastUpdateTime = DateTime.Now;
            if (Update != null) Update(elapsedTime);
            storyboard.Begin();
        }
    }
}