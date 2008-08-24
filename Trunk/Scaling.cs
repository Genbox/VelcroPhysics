using System;
using System.Collections.Generic;
using System.Text;

namespace FarseerGames.FarseerPhysics {
    public class Scaling {

        private bool enabled = false;
        public bool Enabled {
            get { return enabled; }
            set { enabled = value; }
        }

        private float scalingPenalty = 0f;

        private float maximumUpdateInterval;
        /// <summary>
        /// The maximum interval to use
        /// </summary>
        public float MaximumUpdateInterval {
            get { return maximumUpdateInterval; }
            set { 
                if (value > updateInterval) maximumUpdateInterval = value; 
            }
        }

        private float updateInterval = 0f;
        /// <summary>
        /// Returns or sets the interval in seconds in which the simulator is being updated
        /// </summary>
        public float UpdateInterval {
            get { 
                // todo -> add current scaling penalty
                return updateInterval + scalingPenalty; 
            }
            set { 
                updateInterval = value;
                if (updateInterval > maximumUpdateInterval) maximumUpdateInterval = value;
            }
        }

        public Scaling(float preferredUpdateInterval, float maximumUpdateInterval) {
            this.updateInterval = preferredUpdateInterval;
            this.maximumUpdateInterval = maximumUpdateInterval;
        }

        private float elapsedTime = 0;  // holds the total time since the last update

        public float GetUpdateInterval(float dt) {

            float interval;

            if (updateInterval > 0 && enabled) {

                elapsedTime += dt;
                if (elapsedTime < this.UpdateInterval) {    // must use UpdateInterval property
                    return 0;
                }

                interval = elapsedTime;
                elapsedTime = 0;
                return interval;
            }

            return dt;
        }

        public void IncreaseUpdateInterval() {
            if (scalingPenalty + this.updateInterval / 4 <= this.maximumUpdateInterval) {
                scalingPenalty += this.updateInterval / 4;
            }
        }

        public void DecreaseUpdateInterval() {
            scalingPenalty -= this.updateInterval / 8;
            if (scalingPenalty < 0) {
                scalingPenalty = 0;
            }
        }

    }
}
