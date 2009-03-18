namespace FarseerGames.FarseerPhysics
{
    public class Scaling
    {
        private float _elapsedTime; // holds the total time since the last update
        private bool _enabled;

        private float _maximumUpdateInterval;
        private float _scalingPenalty;

        private float _updateInterval;

        public Scaling(float preferredUpdateInterval, float maximumUpdateInterval)
        {
            _updateInterval = preferredUpdateInterval;
            _maximumUpdateInterval = maximumUpdateInterval;
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        /// <summary>
        /// The maximum interval to use
        /// </summary>
        public float MaximumUpdateInterval
        {
            get { return _maximumUpdateInterval; }
            set { if (value > _updateInterval) _maximumUpdateInterval = value; }
        }

        /// <summary>
        /// Returns or sets the interval in seconds in which the simulator is being updated
        /// </summary>
        public float UpdateInterval
        {
            //TODO: Rename this or the GetUpdateInterval(float dt) method
            //it is confusing for users having 2 similar named prop/method
            get
            {
                // TODO: Add current scaling penalty
                return _updateInterval + _scalingPenalty;
            }
            set
            {
                _updateInterval = value;
                if (_updateInterval > _maximumUpdateInterval) _maximumUpdateInterval = value;
            }
        }

        public float GetUpdateInterval(float dt)
        {
            if (_updateInterval > 0 && _enabled)
            {
                _elapsedTime += dt;
                if (_elapsedTime < UpdateInterval)
                {
                    // must use UpdateInterval property
                    return 0;
                }

                float interval = _elapsedTime;
                _elapsedTime = 0;
                return interval;
            }

            return dt;
        }

        public void IncreaseUpdateInterval()
        {
            if (_scalingPenalty + _updateInterval/4 <= _maximumUpdateInterval)
            {
                _scalingPenalty += _updateInterval/4;
            }
        }

        public void DecreaseUpdateInterval()
        {
            _scalingPenalty -= _updateInterval/8;
            if (_scalingPenalty < 0)
            {
                _scalingPenalty = 0;
            }
        }
    }
}