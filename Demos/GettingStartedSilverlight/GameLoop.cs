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
    public class GameLoop
    {
        Canvas targetCanvas = null;
        Storyboard storyboard;
        DateTime lastUpdateTime = DateTime.MinValue;
        TimeSpan elapsedTime;
        public delegate void UpdateDelegate(TimeSpan ElapsedTime);
        public event UpdateDelegate Update;
        public void Attach(Canvas canvas)
        {
            targetCanvas = canvas;
            storyboard = new Storyboard();
            storyboard.SetValue(FrameworkElement.NameProperty, "gameloop");
            canvas.Resources.Add("gameloop", storyboard);
            lastUpdateTime = DateTime.Now;
            storyboard.Completed += new EventHandler(storyboard_Completed);
            storyboard.Begin();
        }

        public void Detach(Canvas canvas)
        {
            storyboard.Stop();
            canvas.Resources.Remove("gameloop");
        }

        void storyboard_Completed(object sender, EventArgs e)
        {
            elapsedTime = DateTime.Now - lastUpdateTime;
            lastUpdateTime = DateTime.Now;
            if (Update!=null) Update(elapsedTime);
            storyboard.Begin();
        }
    }
}
