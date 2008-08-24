using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Controllers
{
    public class InactivityController : Controller
    {
        private readonly PhysicsSimulator _physicsSimulator;
        private Vector2 _difference = Vector2.Zero;

        public InactivityController(PhysicsSimulator physicsSimulator)
        {
            MaxIdleTime = 1000;
            ActivationDistance = 100;
            _physicsSimulator = physicsSimulator;
            _physicsSimulator.Add(this);
            Enabled = false; // disable by default
        }

        /// <summary>
        /// Returns the number of active bodies before updating
        /// </summary>
        public int BodiesEnabled { get; private set; }

        /// <summary>
        /// Returns or sets the distance in which deactivated bodies can be reactivated by an active body
        /// </summary>
        public float ActivationDistance { get; set; }

        /// <summary>
        /// Returns or sets the idle time in ms before a body will be deactivated
        /// </summary>
        public float MaxIdleTime { get; set; }

        public override void Validate()
        {
            // this controller is always active
        }

        public override void Update(float dt)
        {
            float ms = dt*1000;

            BodiesEnabled = 0;

            if (isDisposed)
            {
                return;
            }
            foreach (Body b in _physicsSimulator.BodyList)
            {
                if (b.IsStatic) continue; // do not apply to static bodies
                if (b.Enabled == false) continue; // do not apply to disabled bodies

                BodiesEnabled++;

                if (!b.Moves && b.IsAutoIdle)
                {
                    // body doesn't move -> increment idle time
                    b.IdleTime += ms;
                    if (b.IdleTime >= MaxIdleTime) b.Enabled = false;
                }
                else
                {
                    // body moves -> reset the idle time...
                    b.IdleTime = 0;

                    // ... and check if this body can enable disabled bodies
                    foreach (Body b2 in _physicsSimulator.BodyList)
                    {
                        if (b2.enabled == false && b2.IsAutoIdle)
                        {
                            if (IsInActivationDistance(b, b2))
                            {
                                b2.enabled = true;
                                b2.IdleTime = 0;
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