namespace FarseerGames.FarseerPhysics
{
    public class Scaling
    {
        private float elapsedTime; // holds the total time since the last update
        private bool enabled;

        private float maximumUpdateInterval;
        private float scalingPenalty;

        private float updateInterval;

        public Scaling(float preferredUpdateInterval, float maximumUpdateInterval)
        {
            updateInterval = preferredUpdateInterval;
            this.maximumUpdateInterval = maximumUpdateInterval;
        }

        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// The maximum interval to use
        /// </summary>
        public float MaximumUpdateInterval
        {
            get { return maximumUpdateInterval; }
            set { if (value > updateInterval) maximumUpdateInterval = value; }
        }

        /// <summary>
        /// Returns or sets the interval in seconds in which the simulator is being updated
        /// </summary>
        public float UpdateInterval
        {
            get
            {
                // todo -> add current scaling penalty
                return updateInterval + scalingPenalty;
            }
            set
            {
                updateInterval = value;
                if (updateInterval > maximumUpdateInterval) maximumUpdateInterval = value;
            }
        }

        public float GetUpdateInterval(float dt)
        {
            float interval;

            if (updateInterval > 0 && enabled)
            {
                elapsedTime += dt;
                if (elapsedTime < UpdateInterval)
                {
                    // must use UpdateInterval property
                    return 0;
                }

                interval = elapsedTime;
                elapsedTime = 0;
                return interval;
            }

            return dt;
        }

        public void IncreaseUpdateInterval()
        {
            if (scalingPenalty + updateInterval/4 <= maximumUpdateInterval)
            {
                scalingPenalty += updateInterval/4;
            }
        }

        public void DecreaseUpdateInterval()
        {
            scalingPenalty -= updateInterval/8;
            if (scalingPenalty < 0)
            {
                scalingPenalty = 0;
            }
        }
    }
}