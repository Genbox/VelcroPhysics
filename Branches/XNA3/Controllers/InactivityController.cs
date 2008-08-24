using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Mathematics;
using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerPhysics.Controllers {
    public class InactivityController : Controller {


        private int _bodiesEnabled = 0;
        /// <summary>
        /// Returns the number of active bodies before updating
        /// </summary>
        public int BodiesEnabled {
            get { return _bodiesEnabled; }
        }

        PhysicsSimulator _physicsSimulator;

        private float _activationDistance = 100;
        /// <summary>
        /// Returns or sets the distance in which deactivated bodies can be reactivated by an active body
        /// </summary>
        public float ActivationDistance {
            get { return _activationDistance; }
            set { _activationDistance = value; }
        }

        private float _maxIdleTime = 1000;
        /// <summary>
        /// Returns or sets the idle time in ms before a body will be deactivated
        /// </summary>
        public float MaxIdleTime {
            get { return _maxIdleTime; }
            set { _maxIdleTime = value; }
        }

        Vector2 _difference = Vector2.Zero;

        public InactivityController(PhysicsSimulator physicsSimulator) {
            _physicsSimulator = physicsSimulator;
            _physicsSimulator.Add(this);
            this.Enabled = false; // disable by default
        }

        public override void Validate() {
            // this controller is always active
        }

        public override void Update(float dt) {
            float ms = dt * 1000;

            _bodiesEnabled = 0;

            if (isDisposed) { return; }
            foreach (Body b in _physicsSimulator.BodyList) {

                if (b.IsStatic == true) continue;   // do not apply to static bodies
                if (b.Enabled == false) continue;   // do not apply to disabled bodies

                _bodiesEnabled++;

                if (!b.Moves && b.IsAutoIdle) {
                    // body doesn't move -> increment idle time
                    b.IdleTime += ms;
                    if (b.IdleTime >= _maxIdleTime) b.Enabled = false;
                } else {
                    // body moves -> reset the idle time...
                    b.IdleTime = 0;

                    // ... and check if this body can enable disabled bodies
                    foreach (Body b2 in _physicsSimulator.bodyList) {
                        if (b2.enabled == false && b2.IsAutoIdle == true) {
                            if (IsInActivationDistance(b, b2)) {
                                b2.enabled = true;
                                b2.IdleTime = 0;
                            }
                        }
                    }

                }


            }

        }

        public bool IsInActivationDistance(Body b1, Body b2) {
            float distance;
            _difference = b1.position - b2.position;
            distance = _difference.Length();
            if (distance < 0) { _difference *= -1; }

            return distance <= _activationDistance;

        }


    }
}
