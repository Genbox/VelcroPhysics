using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace FarseerSilverlightManual
{
    public class GameLoop
    {
        #region Delegates

        public delegate void UpdateDelegate(TimeSpan elapsedTime);

        #endregion

        private TimeSpan _elapsedTime;
        private DateTime _lastUpdateTime = DateTime.MinValue;
        private Storyboard _storyboard;

        public event UpdateDelegate Update;

        public void Attach(Canvas canvas)
        {
            _storyboard = new Storyboard();
            _storyboard.SetValue(FrameworkElement.NameProperty, "gameloop");
            canvas.Resources.Add("gameloop", _storyboard);
            _lastUpdateTime = DateTime.Now;
            _storyboard.Completed += storyboard_Completed;
            _storyboard.Begin();
        }

        public void Detach(Canvas canvas)
        {
            _storyboard.Stop();
            canvas.Resources.Remove("gameloop");
        }

        private void storyboard_Completed(object sender, EventArgs e)
        {
            _elapsedTime = DateTime.Now - _lastUpdateTime;
            _lastUpdateTime = DateTime.Now;
            if (Update != null) Update(_elapsedTime);
            _storyboard.Begin();
        }
    }
}