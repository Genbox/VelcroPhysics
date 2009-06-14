/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2007 Erin Catto http://www.gphysics.com

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.
*/

// Point-to-point constraint
// C = p2 - p1
// Cdot = v2 - v1
//      = v2 + cross(w2, r2) - v1 - cross(w1, r1)
// J = [-I -r1_skew I r2_skew ]
// Identity used:
// w k % (rx i + ry j) = w * (-ry i + rx j)

// Motor constraint
// Cdot = w2 - w1
// J = [0 0 -1 0 0 1]
// K = invI1 + invI2

using FarseerPhysics.Math;
using Microsoft.Xna.Framework;
// If this is an XNA project then we use math from the XNA framework.
#if XNA

#endif

namespace FarseerPhysics.Dynamics
{
    using SystemMath = Math;

    /// <summary>
    /// Revolute joint definition. This requires defining an
    /// anchor point where the bodies are joined. The definition
    /// uses local anchor points so that the initial configuration
    /// can violate the constraint slightly. You also need to
    /// specify the initial relative angle for joint limits. This
    /// helps when saving and loading a game.
    /// The local anchor points are measured from the body's origin
    /// rather than the center of mass because:
    /// 1. you might not know where the center of mass will be.
    /// 2. if you add/remove shapes from a body and recompute the mass,
    ///    the joints will be broken.
    /// </summary>
    public class RevoluteJointDef : JointDef
    {
        /// <summary>
        /// A flag to enable joint limits.
        /// </summary>
        public bool EnableLimit;

        /// <summary>
        /// A flag to enable the joint motor.
        /// </summary>
        public bool EnableMotor;

        /// <summary>
        /// The local anchor point relative to body1's origin.
        /// </summary>
        public Vector2 LocalAnchor1;

        /// <summary>
        /// The local anchor point relative to body2's origin.
        /// </summary>
        public Vector2 LocalAnchor2;

        /// <summary>
        /// The lower angle for the joint limit (radians).
        /// </summary>
        public float LowerAngle;

        /// <summary>
        /// The maximum motor torque used to achieve the desired motor speed.
        /// Usually in N-m.
        /// </summary>
        public float MaxMotorTorque;

        /// <summary>
        /// The desired motor speed. Usually in radians per second.
        /// </summary>
        public float MotorSpeed;

        /// <summary>
        /// The body2 angle minus body1 angle in the reference state (radians).
        /// </summary>
        public float ReferenceAngle;

        /// <summary>
        /// The upper angle for the joint limit (radians).
        /// </summary>
        public float UpperAngle;

        public RevoluteJointDef()
        {
            Type = JointType.RevoluteJoint;
            LocalAnchor1 = Vector2.Zero;
            LocalAnchor2 = Vector2.Zero;
            ReferenceAngle = 0.0f;
            LowerAngle = 0.0f;
            UpperAngle = 0.0f;
            MaxMotorTorque = 0.0f;
            MotorSpeed = 0.0f;
            EnableLimit = false;
            EnableMotor = false;
        }

        /// <summary>
        /// Initialize the bodies, anchors, and reference angle using the world
        /// anchor.
        /// </summary>
        public void Initialize(Body body1, Body body2, Vector2 anchor)
        {
            Body1 = body1;
            Body2 = body2;
            LocalAnchor1 = body1.GetLocalPoint(anchor);
            LocalAnchor2 = body2.GetLocalPoint(anchor);
            ReferenceAngle = body2.GetAngle() - body1.GetAngle();
        }
    }

    /// <summary>
    /// A revolute joint constrains to bodies to share a common point while they
    /// are free to rotate about the point. The relative rotation about the shared
    /// point is the joint angle. You can limit the relative rotation with
    /// a joint limit that specifies a lower and upper angle. You can use a motor
    /// to drive the relative rotation about the shared point. A maximum motor torque
    /// is provided so that infinite forces are not generated.
    /// </summary>
    public class RevoluteJoint : Joint
    {
        public bool _enableLimit;
        public bool _enableMotor;
        public Vector3 _impulse;
        public LimitState _limitState;
        public Vector2 _localAnchor1; // relative
        public Vector2 _localAnchor2;
        public float _lowerAngle;
        public Mat33 _mass; //effective mass for p2p constraint.
        public float _maxMotorTorque;
        public float _motorImpulse;
        public float _motorMass; // effective mass for motor/limit angular constraint.
        public float _motorSpeed;

        public float _referenceAngle;
        public float _upperAngle;

        public RevoluteJoint(RevoluteJointDef def)
            : base(def)
        {
            _localAnchor1 = def.LocalAnchor1;
            _localAnchor2 = def.LocalAnchor2;
            _referenceAngle = def.ReferenceAngle;

            _impulse = new Vector3();
            _motorImpulse = 0.0f;

            _lowerAngle = def.LowerAngle;
            _upperAngle = def.UpperAngle;
            _maxMotorTorque = def.MaxMotorTorque;
            _motorSpeed = def.MotorSpeed;
            _enableLimit = def.EnableLimit;
            _enableMotor = def.EnableMotor;
            _limitState = LimitState.InactiveLimit;
        }

        public override Vector2 Anchor1
        {
            get { return _body1.GetWorldPoint(_localAnchor1); }
        }

        public override Vector2 Anchor2
        {
            get { return _body2.GetWorldPoint(_localAnchor2); }
        }

        /// <summary>
        /// Get the current joint angle in radians.
        /// </summary>
        public float JointAngle
        {
            get
            {
                Body b1 = _body1;
                Body b2 = _body2;
                return b2._sweep.A - b1._sweep.A - _referenceAngle;
            }
        }


        /// <summary>
        /// Get the current joint angle speed in radians per second.
        /// </summary>
        public float JointSpeed
        {
            get
            {
                Body b1 = _body1;
                Body b2 = _body2;
                return b2._angularVelocity - b1._angularVelocity;
            }
        }

        /// <summary>
        /// Is the joint limit enabled?
        /// </summary>
        public bool IsLimitEnabled
        {
            get { return _enableLimit; }
        }

        /// <summary>
        /// Get the lower joint limit in radians.
        /// </summary>
        public float LowerLimit
        {
            get { return _lowerAngle; }
        }

        /// <summary>
        /// Get the upper joint limit in radians.
        /// </summary>
        public float UpperLimit
        {
            get { return _upperAngle; }
        }

        /// <summary>
        /// Is the joint motor enabled?
        /// </summary>
        public bool IsMotorEnabled
        {
            get { return _enableMotor; }
        }

        /// <summary>
        /// Get\Set the motor speed in radians per second.
        /// </summary>
        public float MotorSpeed
        {
            get { return _motorSpeed; }
            set
            {
                _body1.WakeUp();
                _body2.WakeUp();
                _motorSpeed = value;
            }
        }

        /// <summary>
        /// Get the current motor torque, usually in N-m.
        /// </summary>
        public float MotorTorque
        {
            get { return _motorImpulse; }
        }

        public override Vector2 GetReactionForce(float inv_dt)
        {
            Vector2 P = new Vector2(_impulse.X, _impulse.Y);
            return inv_dt*P;
        }

        public override float GetReactionTorque(float inv_dt)
        {
            return inv_dt*_impulse.Z;
        }

        /// <summary>
        /// Enable/disable the joint limit.
        /// </summary>
        public void EnableLimit(bool flag)
        {
            _body1.WakeUp();
            _body2.WakeUp();
            _enableLimit = flag;
        }

        /// <summary>
        /// Set the joint limits in radians.
        /// </summary>
        public void SetLimits(float lower, float upper)
        {
            //Box2DXDebug.Assert(lower <= upper);
            _body1.WakeUp();
            _body2.WakeUp();
            _lowerAngle = lower;
            _upperAngle = upper;
        }

        /// <summary>
        /// Enable/disable the joint motor.
        /// </summary>
        public void EnableMotor(bool flag)
        {
            _body1.WakeUp();
            _body2.WakeUp();
            _enableMotor = flag;
        }

        /// <summary>
        /// Set the maximum motor torque, usually in N-m.
        /// </summary>
        public void SetMaxMotorTorque(float torque)
        {
            _body1.WakeUp();
            _body2.WakeUp();
            _maxMotorTorque = torque;
        }

        internal override void InitVelocityConstraints(TimeStep step)
        {
            Body b1 = _body1;
            Body b2 = _body2;

            if (_enableMotor || _enableLimit)
            {
                // You cannot create a rotation limit between bodies that
                // both have fixed rotation.
                //Box2DXDebug.Assert(b1._invI > 0.0f || b2._invI > 0.0f);
            }

            // Compute the effective mass matrix.
            Vector2 r1 = CommonMath.Mul(b1.GetXForm().R, _localAnchor1 - b1.GetLocalCenter());
            Vector2 r2 = CommonMath.Mul(b2.GetXForm().R, _localAnchor2 - b2.GetLocalCenter());

            // J = [-I -r1_skew I r2_skew]
            //     [ 0       -1 0       1]
            // r_skew = [-ry; rx]

            // Matlab
            // K = [ m1+r1y^2*i1+m2+r2y^2*i2,  -r1y*i1*r1x-r2y*i2*r2x,          -r1y*i1-r2y*i2]
            //     [  -r1y*i1*r1x-r2y*i2*r2x, m1+r1x^2*i1+m2+r2x^2*i2,           r1x*i1+r2x*i2]
            //     [          -r1y*i1-r2y*i2,           r1x*i1+r2x*i2,                   i1+i2]

            float m1 = b1._invMass, m2 = b2._invMass;
            float i1 = b1._invI, i2 = b2._invI;

            _mass.Col1.X = m1 + m2 + r1.Y*r1.Y*i1 + r2.Y*r2.Y*i2;
            _mass.Col2.X = -r1.Y*r1.X*i1 - r2.Y*r2.X*i2;
            _mass.Col3.X = -r1.Y*i1 - r2.Y*i2;
            _mass.Col1.Y = _mass.Col2.X;
            _mass.Col2.Y = m1 + m2 + r1.X*r1.X*i1 + r2.X*r2.X*i2;
            _mass.Col3.Y = r1.X*i1 + r2.X*i2;
            _mass.Col1.Z = _mass.Col3.X;
            _mass.Col2.Z = _mass.Col3.Y;
            _mass.Col3.Z = i1 + i2;

            _motorMass = 1.0f/(i1 + i2);

            if (_enableMotor == false)
            {
                _motorImpulse = 0.0f;
            }

            if (_enableLimit)
            {
                float jointAngle = b2._sweep.A - b1._sweep.A - _referenceAngle;
                if (CommonMath.Abs(_upperAngle - _lowerAngle) < 2.0f*Settings.AngularSlop)
                {
                    _limitState = LimitState.EqualLimits;
                }
                else if (jointAngle <= _lowerAngle)
                {
                    if (_limitState != LimitState.AtLowerLimit)
                    {
                        _impulse.Z = 0.0f;
                    }
                    _limitState = LimitState.AtLowerLimit;
                }
                else if (jointAngle >= _upperAngle)
                {
                    if (_limitState != LimitState.AtUpperLimit)
                    {
                        _impulse.Z = 0.0f;
                    }
                    _limitState = LimitState.AtUpperLimit;
                }
                else
                {
                    _limitState = LimitState.InactiveLimit;
                    _impulse.Z = 0.0f;
                }
            }
            else
            {
                _limitState = LimitState.InactiveLimit;
            }

            if (step.WarmStarting)
            {
                // Scale impulses to support a variable time step.
                _impulse *= step.DtRatio;
                _motorImpulse *= step.DtRatio;

                Vector2 P = new Vector2(_impulse.X, _impulse.Y);

                b1._linearVelocity -= m1*P;
                b1._angularVelocity -= i1*(CommonMath.Cross(ref r1, ref P) + _motorImpulse + _impulse.Z);

                b2._linearVelocity += m2*P;
                b2._angularVelocity += i2*(CommonMath.Cross(ref r2, ref P) + _motorImpulse + _impulse.Z);
            }
            else
            {
                _impulse = Vector3.Zero;
                _motorImpulse = 0.0f;
            }
        }

        internal override void SolveVelocityConstraints(TimeStep step)
        {
            Body b1 = _body1;
            Body b2 = _body2;

            Vector2 v1 = b1._linearVelocity;
            float w1 = b1._angularVelocity;
            Vector2 v2 = b2._linearVelocity;
            float w2 = b2._angularVelocity;

            float m1 = b1._invMass, m2 = b2._invMass;
            float i1 = b1._invI, i2 = b2._invI;

            //Solve motor constraint.
            if (_enableMotor && _limitState != LimitState.EqualLimits)
            {
                float Cdot = w2 - w1 - _motorSpeed;
                float impulse = _motorMass*(-Cdot);
                float oldImpulse = _motorImpulse;
                float maxImpulse = step.Dt*_maxMotorTorque;
                _motorImpulse = CommonMath.Clamp(_motorImpulse + impulse, -maxImpulse, maxImpulse);
                impulse = _motorImpulse - oldImpulse;

                w1 -= i1*impulse;
                w2 += i2*impulse;
            }

            //Solve limit constraint.
            if (_enableLimit && _limitState != LimitState.InactiveLimit)
            {
                Vector2 r1 = CommonMath.Mul(b1.GetXForm().R, _localAnchor1 - b1.GetLocalCenter());
                Vector2 r2 = CommonMath.Mul(b2.GetXForm().R, _localAnchor2 - b2.GetLocalCenter());

                // Solve point-to-point constraint
                Vector2 Cdot1 = v2 + CommonMath.Cross(w2, r2) - v1 - CommonMath.Cross(w1, r1);
                float Cdot2 = w2 - w1;
                Vector3 Cdot = new Vector3(Cdot1.X, Cdot1.Y, Cdot2);

                Vector3 impulse = _mass.Solve33(-Cdot);

                if (_limitState == LimitState.EqualLimits)
                {
                    _impulse += impulse;
                }
                else if (_limitState == LimitState.AtLowerLimit)
                {
                    float newImpulse = _impulse.Z + impulse.Z;
                    if (newImpulse < 0.0f)
                    {
                        Vector2 reduced = _mass.Solve22(-Cdot1);
                        impulse.X = reduced.X;
                        impulse.Y = reduced.Y;
                        impulse.Z = -_impulse.Z;
                        _impulse.X += reduced.X;
                        _impulse.Y += reduced.Y;
                        _impulse.Z = 0.0f;
                    }
                }
                else if (_limitState == LimitState.AtUpperLimit)
                {
                    float newImpulse = _impulse.Z + impulse.Z;
                    if (newImpulse > 0.0f)
                    {
                        Vector2 reduced = _mass.Solve22(-Cdot1);
                        impulse.X = reduced.X;
                        impulse.Y = reduced.Y;
                        impulse.Z = -_impulse.Z;
                        _impulse.X += reduced.X;
                        _impulse.Y += reduced.Y;
                        _impulse.Z = 0.0f;
                    }
                }

                Vector2 P = new Vector2(impulse.X, impulse.Y);

                v1 -= m1*P;
                w1 -= i1*(CommonMath.Cross(ref r1, ref P) + impulse.Z);

                v2 += m2*P;
                w2 += i2*(CommonMath.Cross(ref r2, ref P) + impulse.Z);
            }
            else
            {
                Vector2 r1 = CommonMath.Mul(b1.GetXForm().R, _localAnchor1 - b1.GetLocalCenter());
                Vector2 r2 = CommonMath.Mul(b2.GetXForm().R, _localAnchor2 - b2.GetLocalCenter());

                // Solve point-to-point constraint
                Vector2 Cdot = v2 + CommonMath.Cross(w2, r2) - v1 - CommonMath.Cross(w1, r1);
                Vector2 impulse = _mass.Solve22(-Cdot);

                _impulse.X += impulse.X;
                _impulse.Y += impulse.Y;

                v1 -= m1*impulse;
                w1 -= i1*CommonMath.Cross(ref r1, ref impulse);

                v2 += m2*impulse;
                w2 += i2*CommonMath.Cross(ref r2, ref impulse);
            }

            b1._linearVelocity = v1;
            b1._angularVelocity = w1;
            b2._linearVelocity = v2;
            b2._angularVelocity = w2;
        }

        internal override bool SolvePositionConstraints(float baumgarte)
        {
            // TODO_ERIN block solve with limit.

            Body b1 = _body1;
            Body b2 = _body2;

            float angularError = 0.0f;
            float positionError = 0.0f;

            // Solve angular limit constraint.
            if (_enableLimit && _limitState != LimitState.InactiveLimit)
            {
                float angle = b2._sweep.A - b1._sweep.A - _referenceAngle;
                float limitImpulse = 0.0f;

                if (_limitState == LimitState.EqualLimits)
                {
                    // Prevent large angular corrections
                    float C = CommonMath.Clamp(angle, -Settings.MaxAngularCorrection, Settings.MaxAngularCorrection);
                    limitImpulse = -_motorMass*C;
                    angularError = CommonMath.Abs(C);
                }
                else if (_limitState == LimitState.AtLowerLimit)
                {
                    float C = angle - _lowerAngle;
                    angularError = -C;

                    // Prevent large angular corrections and allow some slop.
                    C = CommonMath.Clamp(C + Settings.AngularSlop, -Settings.MaxAngularCorrection, 0.0f);
                    limitImpulse = -_motorMass*C;
                }
                else if (_limitState == LimitState.AtUpperLimit)
                {
                    float C = angle - _upperAngle;
                    angularError = C;

                    // Prevent large angular corrections and allow some slop.
                    C = CommonMath.Clamp(C - Settings.AngularSlop, 0.0f, Settings.MaxAngularCorrection);
                    limitImpulse = -_motorMass*C;
                }

                b1._sweep.A -= b1._invI*limitImpulse;
                b2._sweep.A += b2._invI*limitImpulse;

                b1.SynchronizeTransform();
                b2.SynchronizeTransform();
            }

            // Solve point-to-point constraint.
            {
                Vector2 r1 = CommonMath.Mul(b1.GetXForm().R, _localAnchor1 - b1.GetLocalCenter());
                Vector2 r2 = CommonMath.Mul(b2.GetXForm().R, _localAnchor2 - b2.GetLocalCenter());

                Vector2 C = b2._sweep.C + r2 - b1._sweep.C - r1;
                positionError = C.Length();

                float invMass1 = b1._invMass, invMass2 = b2._invMass;
                float invI1 = b1._invI, invI2 = b2._invI;

                // Handle large detachment.
                float k_allowedStretch = 10.0f*Settings.LinearSlop;
                if (C.LengthSquared() > k_allowedStretch*k_allowedStretch)
                {
                    // Use a particle solution (no rotation).
                    Vector2 u = C;
                    u.Normalize();
                    float k = invMass1 + invMass2;
                    //Box2DXDebug.Assert(k > Settings.FLT_EPSILON);
                    float m = 1.0f/k;
                    Vector2 impulse = m*(-C);
                    float k_beta = 0.5f;
                    b1._sweep.C -= k_beta*invMass1*impulse;
                    b2._sweep.C += k_beta*invMass2*impulse;

                    C = b2._sweep.C + r2 - b1._sweep.C - r1;
                }

                Mat22 K1 = new Mat22();
                K1.Col1.X = invMass1 + invMass2;
                K1.Col2.X = 0.0f;
                K1.Col1.Y = 0.0f;
                K1.Col2.Y = invMass1 + invMass2;

                Mat22 K2 = new Mat22();
                K2.Col1.X = invI1*r1.Y*r1.Y;
                K2.Col2.X = -invI1*r1.X*r1.Y;
                K2.Col1.Y = -invI1*r1.X*r1.Y;
                K2.Col2.Y = invI1*r1.X*r1.X;

                Mat22 K3 = new Mat22();
                K3.Col1.X = invI2*r2.Y*r2.Y;
                K3.Col2.X = -invI2*r2.X*r2.Y;
                K3.Col1.Y = -invI2*r2.X*r2.Y;
                K3.Col2.Y = invI2*r2.X*r2.X;

                Mat22 K = K1 + K2 + K3;
                Vector2 impulse_ = K.Solve(-C);

                b1._sweep.C -= b1._invMass*impulse_;
                b1._sweep.A -= b1._invI*CommonMath.Cross(ref r1, ref impulse_);

                b2._sweep.C += b2._invMass*impulse_;
                b2._sweep.A += b2._invI*CommonMath.Cross(ref r2, ref impulse_);

                b1.SynchronizeTransform();
                b2.SynchronizeTransform();
            }

            return positionError <= Settings.LinearSlop && angularError <= Settings.AngularSlop;
        }
    }
}