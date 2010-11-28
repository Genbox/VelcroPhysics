using System;
using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Controllers
{
    /// <summary>
    /// Put a limit on the linear (translation - the movespeed) and angular (rotation) velocity
    /// of bodies added to this controller.
    /// </summary>
    public class VelocityLimitController : Controller
    {
        private List<Body> _bodies = new List<Body>();
        private float _maxLinearSqared;
        private float _maxAngularSqared;
        private float _maxLinearVelocity;
        private float _maxAngularVelocity;
        public bool LimitLinearVelocity = true;
        public bool LimitAngularVelocity = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="VelocityLimitController"/> class.
        /// Sets the max linear velocity to Settings.MaxTranslation
        /// Sets the max angular velocity to Settings.MaxRotation
        /// </summary>
        public VelocityLimitController()
        {
            _maxLinearVelocity = Settings.MaxTranslation;
            _maxAngularVelocity = Settings.MaxRotation;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VelocityLimitController"/> class.
        /// Pass in 0 or float.MaxValue to disable the limit.
        /// maxAngularVelocity = 0 will disable the angular velocity limit.
        /// </summary>
        /// <param name="maxLinearVelocity">The max linear velocity.</param>
        /// <param name="maxAngularVelocity">The max angular velocity.</param>
        public VelocityLimitController(float maxLinearVelocity, float maxAngularVelocity)
        {
            if (maxLinearVelocity == 0 || maxLinearVelocity == float.MaxValue)
                LimitLinearVelocity = false;

            if (maxAngularVelocity == 0 || maxAngularVelocity == float.MaxValue)
                LimitAngularVelocity = false;

            _maxLinearVelocity = maxLinearVelocity;
            _maxAngularVelocity = maxAngularVelocity;
        }

        /// <summary>
        /// Gets or sets the max angular velocity.
        /// </summary>
        /// <value>The max angular velocity.</value>
        public float MaxAngularVelocity
        {
            get { return _maxAngularVelocity; }
            set
            {
                _maxAngularVelocity = value;
                _maxAngularSqared = _maxAngularVelocity * _maxAngularVelocity;
            }
        }

        /// <summary>
        /// Gets or sets the max linear velocity.
        /// </summary>
        /// <value>The max linear velocity.</value>
        public float MaxLinearVelocity
        {
            get { return _maxLinearVelocity; }
            set
            {
                _maxLinearVelocity = value;
                _maxLinearSqared = _maxLinearVelocity * _maxLinearVelocity;
            }
        }

        public override void Update(float dt)
        {
            foreach (Body body in _bodies)
            {
                if (body.IsControllerIgnored(IgnoreController.VelocityLimitController))
                    continue;

                if (!body.Active || body.IsStatic)
                    continue;

                if (LimitLinearVelocity)
                {
                    //Translation
                    Vector2 translation = dt * body.LinearVelocityInternal;
                    float result;
                    Vector2.Dot(ref translation, ref translation, out result);

                    if (result > _maxLinearSqared)
                    {
                        float ratio = _maxLinearVelocity / translation.Length();
                        body.LinearVelocityInternal *= ratio;
                    }
                }

                if (LimitAngularVelocity)
                {
                    //Rotation
                    float rotation = dt * body.AngularVelocityInternal;
                    if (rotation * rotation > _maxAngularSqared)
                    {
                        float ratio = _maxAngularVelocity / Math.Abs(rotation);
                        body.AngularVelocityInternal *= ratio;
                    }
                }
            }
        }

        public void AddBody(Body body)
        {
            _bodies.Add(body);
        }
    }
}