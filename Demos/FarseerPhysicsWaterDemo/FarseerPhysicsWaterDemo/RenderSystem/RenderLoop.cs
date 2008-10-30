using System;
using System.Windows.Media;

namespace FarseerPhysicsWaterDemo.RenderSystem
{
    public class RenderLoop
    {
        #region properties
        public int StepSize { get; set; }
        #endregion

        #region public methods
        public RenderLoop()
        {
            StepSize = 10;//milliseconds
        }

        public void Initialize()
        {
            _updateTimeSpan = new TimeSpan(0, 0, 0, 0, StepSize);
            _compositionTargetRendering = CompositionTarget_Rendering;
            CompositionTarget.Rendering += _compositionTargetRendering;
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            Render();
        }

        private void Render()
        {
            if (_firstRun)
            {
                _previousTime = DateTime.Now;
                _firstRun = false;
            }

            _currentTime = DateTime.Now;
            _elapsedTime = _currentTime - _previousTime;
            _previousTime = _currentTime;

            accumulator += _elapsedTime.TotalMilliseconds;

            if (Update != null)
            {
                while (accumulator >= _updateTimeSpan.TotalMilliseconds)
                {
                    Update(_updateTimeSpan);
                    accumulator -= _updateTimeSpan.TotalMilliseconds;
                }
            }

            if (Draw != null)
            {
                Draw(_elapsedTime);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //dispose managed resources 
                CompositionTarget.Rendering -= _compositionTargetRendering;
            }
        }
        #endregion

        #region private methods
        #endregion

        #region events
        public delegate void StepEventHandler(TimeSpan elapsedTime);
        public event StepEventHandler Update;
        public event StepEventHandler Draw;
        #endregion

        #region private variables
        private bool _firstRun = true;

        private TimeSpan _updateTimeSpan;
        private TimeSpan _elapsedTime;

        private DateTime _currentTime;
        private DateTime _previousTime;

        double accumulator;

        private EventHandler _compositionTargetRendering;
        #endregion
    }
}
