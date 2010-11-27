using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;

namespace FarseerPhysics.Controllers
{
    public abstract class AbstractForceController : Controller
    {
        /// <summary>
        /// Global Strength of the force to be applied
        /// </summary>
        public float Strength { get; set; }

        /// <summary>
        /// Position of the Force. Can be ignored (left at (0,0) for forces
        /// that are not position-dependent
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Maximum speed of the bodies. Bodies that are travelling faster are supposed to be ignored
        /// </summary>
        public float MaximumSpeed { get; set; }

        /// <summary>
        /// Timing Modes
        /// Switched: Standard on/off mode using the baseclass enabled property
        /// Triggered: When the Trigger() method is called the force is active for a specified Impulse Length
        /// Curve: Still to be defined. The basic idea is having a Trigger combined with a curve for the strength
        /// </summary>
        public enum TimingMode
        {
            Switched,
            Triggered,
            Curve
        }

        /// <summary>
        /// Timing Mode of the force instance
        /// </summary>
        public TimingMode Mode { get; set; }

        /// <summary>
        /// Time of the current impulse. Incremented in update till ImpulseLength is reached
        /// </summary>
        public float ImpulseTime { get; private set; }

        /// <summary>
        /// Length of a triggered impulse. Used in both Triggered and Curve Mode
        /// </summary>
        public float ImpulseLength { get; set; }

        /// <summary>
        /// Indicating if we are currently during an Impulse (Triggered and Curve Mode)
        /// </summary>
        public bool Triggered { get; private set; }

        /// <summary>
        /// Curve used by Curve Mode as an animated multiplier for the force strength.
        /// Only positions between 0 and 1 are considered as that range is stretched to
        /// have ImpulseLength.
        /// </summary>
        public Curve Curve;

        /// <summary>
        /// Constructor
        /// </summary>
        public AbstractForceController()
        {

            this.Enabled = true;

            this.Strength = 1.0f;
            this.Position = new Vector2(0, 0);
            this.MaximumSpeed = 100.0f;
            this.Mode = TimingMode.Switched;
            this.ImpulseTime = 0.0f;
            this.ImpulseLength = 1.0f;
            this.Triggered = false;
            this.Curve = new Curve();

            Curve.Keys.Add(new CurveKey(0, 5));
            Curve.Keys.Add(new CurveKey(0.1f, 5));
            Curve.Keys.Add(new CurveKey(0.2f, -4));
            Curve.Keys.Add(new CurveKey(1f, 0));

        }

        /// <summary>
        /// Overloaded Contstructor with supplying Timing Mode
        /// </summary>
        /// <param name="Mode"></param>
        public AbstractForceController(TimingMode Mode)
        {
            this.Mode = Mode;
            switch (Mode)
            {
                case TimingMode.Switched:
                    this.Enabled = true;
                    break;
                case TimingMode.Triggered:
                    this.Enabled = false;
                    break;
                case TimingMode.Curve:
                    this.Enabled = false;
                    break;
            }
        }

        /// <summary>
        /// Triggers the trigger modes (Trigger and Curve)
        /// </summary>
        public void Trigger()
        {
            this.Triggered = true;
            this.ImpulseTime = 0;
        }

        /// <summary>
        /// Inherited from Controller
        /// Depending on the TimingMode perform timing logic and call ApplyForce()
        /// </summary>
        /// <param name="dt"></param>
        public override void Update(float dt)
        {
            switch (Mode)
            {
                case TimingMode.Switched:
                    {
                        if (this.Enabled)
                        {
                            ApplyForce(dt, Strength);
                        }
                        break;
                    }
                case TimingMode.Triggered:
                    {
                        if (Triggered)
                        {
                            if (ImpulseTime < ImpulseLength)
                            {
                                ApplyForce(dt, Strength);
                                ImpulseTime += dt;
                            }
                            else
                            {
                                Triggered = false;
                            }
                        }
                        break;
                    }
                case TimingMode.Curve:
                    {
                        if (Triggered)
                        {
                            if (ImpulseTime < ImpulseLength)
                            {
                                ApplyForce(dt, Strength * Curve.Evaluate(ImpulseTime));
                                ImpulseTime += dt;
                            }
                            else
                            {
                                Triggered = false;
                            }
                        }
                        break;
                    }

            }
        }

        /// <summary>
        /// Apply the force supplying strength (wich is modified in Update() according to the TimingMode
        /// </summary>
        /// <param name="dt"></param>
        public abstract void ApplyForce(float dt, float Strength);

    }
}
