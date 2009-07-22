using FarseerGames.FarseerPhysics.Dynamics;

#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Controllers
{
    /// <summary>
    /// Can be used to add support for resting bodies.
    /// </summary>
    public class InactivityController : Controller
    {
        /// <summary>
        /// Returns or sets the distance in which deactivated bodies can be reactivated by an active body
        /// </summary>
        public float ActivationDistance = 100;
        private int _bodiesEnabled;

        private Vector2 _difference = Vector2.Zero;
    
        /// <summary>
        /// Returns or sets the idle time in ms before a body will be deactivated
        /// </summary>
        public float MaxIdleTime = 1000;
        private PhysicsSimulator _physicsSimulator;

        public InactivityController(PhysicsSimulator physicsSimulator)
        {
            _physicsSimulator = physicsSimulator;
        }

        /// <summary>
        /// Returns the number of active bodies before updating
        /// </summary>
        public int BodiesEnabled
        {
            get { return _bodiesEnabled; }
        }

        public override void Validate()
        {
            // this controller is always active
        }

        public override void Update(float dt, float dtReal)
        {
            if (IsDisposed)
                return;

            float ms = dt*1000;

            _bodiesEnabled = 0;

            foreach (Body body in _physicsSimulator.BodyList)
            {
                if (body.IsStatic) continue; // do not apply to static bodies
                if (body.Enabled == false) continue; // do not apply to disabled bodies

                _bodiesEnabled++;

                if (!body.Moves && body.IsAutoIdle)
                {
                    // body doesn't move -> increment idle time
                    body.IdleTime += ms;
                    if (body.IdleTime >= MaxIdleTime) body.Enabled = false;
                }
                else
                {
                    // body moves -> reset the idle time...
                    body.IdleTime = 0;

                    // ... and check if this body can enable disabled bodies
                    foreach (Body body2 in _physicsSimulator.BodyList)
                    {
                        if (body2.Enabled == false && body2.IsAutoIdle)
                        {
                            if (IsInActivationDistance(body, body2))
                            {
                                body2.Enabled = true;
                                body2.IdleTime = 0;
                            }
                        }
                    }
                }
            }
        }

        public bool IsInActivationDistance(Body b1, Body b2)
        {
            _difference = b1.position - b2.position;
            float distance = _difference.Length();
            if (distance < 0)
            {
                _difference *= -1;
            }

            return distance <= ActivationDistance;
        }
    }
}