/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using System;
using System.Diagnostics;
using Genbox.VelcroPhysics.Definitions.Joints;
using Genbox.VelcroPhysics.Dynamics.Joints.Misc;
using Genbox.VelcroPhysics.Dynamics.Solver;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Dynamics.Joints
{
    public abstract class Joint
    {
        private float _breakpoint;
        private JointType _jointType;
        private bool _collideConnected;
        private object _userData;

        internal bool _enabled;
        internal bool _islandFlag;

        internal JointEdge _edgeA = new JointEdge();
        internal JointEdge _edgeB = new JointEdge();

        /// <summary>Indicate if this join is enabled or not. Disabling a joint means it is still in the simulation, but inactive.</summary>
        protected Body _bodyA;
        protected Body _bodyB;

        protected Joint(JointType jointType)
        {
            _jointType = jointType;
            _breakpoint = float.MaxValue;

            //Connected bodies should not collide by default
            _collideConnected = false;
            _enabled = true;
        }

        protected Joint(Body bodyA, Body bodyB, JointType jointType) : this(jointType)
        {
            //Can't connect a joint to the same body twice.
            Debug.Assert(bodyA != bodyB);

            _bodyA = bodyA;
            _bodyB = bodyB;
        }

        /// <summary>Constructor for fixed joint</summary>
        protected Joint(Body body, JointType jointType) : this(jointType)
        {
            _bodyA = body;
        }

        protected Joint(JointDef def) : this(def.Type)
        {
            Debug.Assert(def.BodyA != def.BodyB);

            _jointType = def.Type;
            _bodyA = def.BodyA;
            _bodyB = def.BodyB;
            _collideConnected = def.CollideConnected;
            _islandFlag = false;
            _userData = def.UserData;
        }

        /// <summary>Gets or sets the type of the joint.</summary>
        /// <value>The type of the joint.</value>
        public JointType JointType => _jointType;

        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        /// <summary>Get the first body attached to this joint.</summary>
        public Body BodyA => _bodyA;

        /// <summary>Get the second body attached to this joint.</summary>
        public Body BodyB => _bodyB;

        /// <summary>
        /// Get the anchor point on bodyA in world coordinates. On some joints, this value indicate the anchor point
        /// within the world.
        /// </summary>
        public abstract Vector2 WorldAnchorA { get; set; }

        /// <summary>
        /// Get the anchor point on bodyB in world coordinates. On some joints, this value indicate the anchor point
        /// within the world.
        /// </summary>
        public abstract Vector2 WorldAnchorB { get; set; }

        /// <summary>Set the user data pointer.</summary>
        /// <value>The data.</value>
        public object UserData
        {
            get => _userData;
            set => _userData = value;
        }

        /// <summary>Set this flag to true if the attached bodies should collide.</summary>
        public bool CollideConnected
        {
            get => _collideConnected;
            set => _collideConnected = value;
        }

        /// <summary>
        /// The Breakpoint simply indicates the maximum Value the JointError can be before it breaks. The default value is
        /// float.MaxValue, which means it never breaks.
        /// </summary>
        public float Breakpoint
        {
            get => _breakpoint;
            set => _breakpoint = value;
        }

        /// <summary>Fires when the joint is broken.</summary>
        public event Action<Joint, float> Broke;

        /// <summary>Get the reaction force on body at the joint anchor in Newtons.</summary>
        /// <param name="invDt">The inverse delta time.</param>
        public abstract Vector2 GetReactionForce(float invDt);

        /// <summary>Get the reaction torque on the body at the joint anchor in N*m.</summary>
        /// <param name="invDt">The inverse delta time.</param>
        public abstract float GetReactionTorque(float invDt);

        /// <summary>
        /// Shift the origin for any points stored in world coordinates.
        /// </summary>
        public virtual void ShiftOrigin(ref Vector2 newOrigin) { }

        protected void WakeBodies()
        {
            if (BodyA != null)
                BodyA.Awake = true;

            if (BodyB != null)
                BodyB.Awake = true;
        }

        /// <summary>Return true if the joint is a fixed type.</summary>
        public bool IsFixedType()
        {
            return JointType == JointType.FixedRevolute ||
                   JointType == JointType.FixedDistance ||
                   JointType == JointType.FixedPrismatic ||
                   JointType == JointType.FixedLine ||
                   JointType == JointType.FixedMouse ||
                   JointType == JointType.FixedAngle ||
                   JointType == JointType.FixedFriction;
        }

        internal abstract void InitVelocityConstraints(ref SolverData data);

        internal void Validate(float invDt)
        {
            if (!_enabled)
                return;

            float jointErrorSquared = GetReactionForce(invDt).LengthSquared();

            if (Math.Abs(jointErrorSquared) <= _breakpoint * _breakpoint)
                return;

            _enabled = false;

            Broke?.Invoke(this, (float)Math.Sqrt(jointErrorSquared));
        }

        internal abstract void SolveVelocityConstraints(ref SolverData data);

        /// <summary>Solves the position constraints.</summary>
        /// <param name="data"></param>
        /// <returns>returns true if the position errors are within tolerance.</returns>
        internal abstract bool SolvePositionConstraints(ref SolverData data);

        public static Joint Create(JointDef def)
        {
            switch (def.Type)
            {
                case JointType.Distance:
                    return new DistanceJoint((DistanceJointDef)def);
                case JointType.FixedMouse:
                    return new FixedMouseJoint((FixedMouseJointDef)def);
                case JointType.Prismatic:
                    return new PrismaticJoint((PrismaticJointDef)def);
                case JointType.Revolute:
                    return new RevoluteJoint((RevoluteJointDef)def);
                case JointType.Pulley:
                    return new PulleyJoint((PulleyJointDef)def);
                case JointType.Gear:
                    return new GearJoint((GearJointDef)def);
                case JointType.Wheel:
                    return new WheelJoint((WheelJointDef)def);
                case JointType.Weld:
                    return new WeldJoint((WeldJointDef)def);
                case JointType.Friction:
                    return new FrictionJoint((FrictionJointDef)def);
                case JointType.Motor:
                    return new MotorJoint((MotorJointDef)def);
                default:
                    Debug.Assert(false);
                    break;
            }

            return null;
        }
    }
}