using System;
using System.Windows.Media;

namespace SimpleSamplesWPF
{
    public class GameLoop : IDisposable
    {
        private bool isRunning;
        public bool IsRunning
        {
            get { return isRunning; }
            set {
                if (value) Start();
                else Stop();
            }
        }

        private DateTime lastUpdateTime = DateTime.MinValue;

        public delegate void UpdateDelegate(TimeSpan elapsedTime);
        public event UpdateDelegate Update;

        public delegate void IsRunningChangedDelegate(bool isEnabled);
        public event IsRunningChangedDelegate IsRunningChanged;

        private void BeforeRender(object sender, EventArgs e)
        {
            if (Update != null)
            {
                TimeSpan elapsedTime = DateTime.Now - lastUpdateTime;
                lastUpdateTime = DateTime.Now;
                Update(elapsedTime);
            }
        }

        public void Start()
        {
            lastUpdateTime = DateTime.Now;
            if(isRunning) return;
            CompositionTarget.Rendering += BeforeRender; //wpf raises this event before each render pass
            isRunning = true;

            if (IsRunningChanged != null)
                IsRunningChanged(true);
        }

        public void Stop()
        {
            if(!isRunning) return;
            CompositionTarget.Rendering -= BeforeRender; //unhook the event
            isRunning = false;

            if (IsRunningChanged != null)
                IsRunningChanged(false);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
