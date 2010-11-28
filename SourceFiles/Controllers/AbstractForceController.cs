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
        /// Maximum Force to be applied. As opposed to Maximum Speed this is independent of the velocity of
        /// the affected body
        /// </summary>
        public float MaximumForce { get; set; }

        /// <summary>
        /// Timing Modes
        /// Switched: Standard on/off mode using the baseclass enabled property
        /// Triggered: When the Trigger() method is called the force is active for a specified Impulse Length
        /// Curve: Still to be defined. The basic idea is having a Trigger combined with a curve for the strength
        /// </summary>
        public enum TimingModes
        {
            Switched,
            Triggered,
            Curve
        }

        /// <summary>
        /// Timing Mode of the force instance
        /// </summary>
        public TimingModes TimingMode { get; set; }

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
        public Curve StrengthCurve;

        /// <summary>
        /// Variation of the force applied to each body affected
        /// !! Must be used in inheriting classes properly !!
        /// </summary>
        public float Variation { get; set; }

        /// <summary>
        /// Provided for reuse to provide Variation functionality in inheriting classes
        /// </summary>
        protected Random Randomize;

        /// <summary>
        /// Modes for Decay. Actual Decay must be implemented in inheriting classes
        /// </summary>
        public enum DecayModes
        {
            None,
            Step,
            Linear,
            InverseSquare,
            Curve
        }

        /// <summary>
        /// See DecayModes
        /// </summary>
        public DecayModes DecayMode {get; set;}

        /// <summary>
        /// Start of the distance based Decay. To set a non decaying area
        /// </summary>
        public float DecayStart { get; set; }

        /// <summary>
        /// Maximum distance a force should be applied
        /// </summary>
        public float DecayEnd { get; set; }

        /// <summary>
        /// Curve to be used for Decay in Curve mode
        /// </summary>
        public Curve DecayCurve;

        /// <summary>
        /// Calculate the Decay for a given body. Meant to ease force development and stick to
        /// the DRY principle and provide unified and predictable decay math.
        /// </summary>
        /// <param name="Body"></param>
        /// <returns></returns>
        protected float GetDecayMultiplier(Body Body)
        {
            float Distance = (Body.Position - Position).Length();
            switch (DecayMode)
            {
                case DecayModes.None:
                    {
                        return 1.0f;
                    }
                case DecayModes.Step:
                    {
                        if (Distance < DecayEnd)
                            return 1.0f; 
                        else
                            return 0.0f;
                    }
                case DecayModes.Linear:
                    {
                        if (Distance < DecayStart)
                            return 1.0f;
                        if (Distance > DecayEnd)
                            return 0.0f;
                        return (DecayEnd - DecayStart / Distance - DecayStart);
                    }
                case DecayModes.InverseSquare:
                    {
                        if (Distance < DecayStart)
                            return 1.0f;
                        else
                            return 1.0f / ((Distance - DecayStart)*(Distance - DecayStart));
                    }
                case DecayModes.Curve:
                    {
                        if (Distance < DecayStart)
                            return 1.0f;
                        else
                            return DecayCurve.Evaluate(Distance - DecayStart);
                    }
                default:
                    return 1.0f;
            }
            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AbstractForceController()
        {

            Enabled = true;

            Strength = 1.0f;
            Position = new Vector2(0, 0);
            MaximumSpeed = 100.0f;
            TimingMode = TimingModes.Switched;
            ImpulseTime = 0.0f;
            ImpulseLength = 1.0f;
            Triggered = false;
            StrengthCurve = new Curve();
            Variation = 0.0f;
            Randomize = new Random(1234);
            DecayMode = DecayModes.None;
            DecayCurve = new Curve();
            DecayStart = 0.0f;
            DecayEnd = 0.0f;

            StrengthCurve.Keys.Add(new CurveKey(0, 5));
            StrengthCurve.Keys.Add(new CurveKey(0.1f, 5));
            StrengthCurve.Keys.Add(new CurveKey(0.2f, -4));
            StrengthCurve.Keys.Add(new CurveKey(1f, 0));

        }

        /// <summary>
        /// Overloaded Contstructor with supplying Timing Mode
        /// </summary>
        /// <param name="Mode"></param>
        public AbstractForceController(TimingModes Mode)
        {
            this.TimingMode = Mode;
            switch (Mode)
            {
                case TimingModes.Switched:
                    this.Enabled = true;
                    break;
                case TimingModes.Triggered:
                    this.Enabled = false;
                    break;
                case TimingModes.Curve:
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
            switch (TimingMode)
            {
                case TimingModes.Switched:
                    {
                        if (this.Enabled)
                        {
                            ApplyForce(dt, Strength);
                        }
                        break;
                    }
                case TimingModes.Triggered:
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
                case TimingModes.Curve:
                    {
                        if (Triggered)
                        {
                            if (ImpulseTime < ImpulseLength)
                            {
                                ApplyForce(dt, Strength * StrengthCurve.Evaluate(ImpulseTime));
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
