/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
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
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints
{
    public enum JointType
    {
        Unknown,
        Revolute,
        Prismatic,
        Distance,
        Pulley,
        //Mouse, <- We have fixed mouse
        Gear,
        Wheel,
        Weld,
        Friction,
        Rope,
        Motor,

        //FPE note: From here on and down, it is only FPE joints
        Slider,
        Angle,
        FixedMouse,
        FixedRevolute,
        FixedDistance,
        FixedLine,
        FixedPrismatic,
        FixedAngle,
        FixedFriction,
    }

    public enum LimitState
    {
        Inactive,
        AtLower,
        AtUpper,
        Equal,
    }

    internal struct Jacobian //TODO
    {
        public float AngularA;
        public float AngularB;
        public Vector2 Linear;

        public void SetZero()
        {
            AngularA = 0.0f;
            Linear = Vector2.Zero;
            AngularB = 0.0f;
        }
    }

    /// <summary>
    /// A joint edge is used to connect bodies and joints together
    /// in a joint graph where each body is a node and each joint
    /// is an edge. A joint edge belongs to a doubly linked list
    /// maintained in each attached body. Each joint has two joint
    /// nodes, one for each attached body.
    /// </summary>
    public sealed class JointEdge
    {
        /// <summary>
        /// The joint.
        /// </summary>
        public Joint Joint;

        /// <summary>
        /// The next joint edge in the body's joint list.
        /// </summary>
        public JointEdge Next;

        /// <summary>
        /// Provides quick access to the other body attached.
        /// </summary>
        public Body Other;

        /// <summary>
        /// The previous joint edge in the body's joint list.
        /// </summary>
        public JointEdge Prev;
    }

    public abstract class Joint
    {
        /// <summary>
        /// The Breakpoint simply indicates the maximum Value the JointError can be before it breaks.
        /// The default value is float.MaxValue
        /// </summary>
        public float Breakpoint = float.MaxValue;
        public bool Enabled = true;

        internal JointEdge EdgeA = new JointEdge();
        internal JointEdge EdgeB = new JointEdge();
        internal bool IslandFlag;

        protected Joint()
        {
        }

        protected Joint(Body body, Body bodyB)
        {
            Debug.Assert(body != bodyB);

            BodyA = body;
            BodyB = bodyB;

            //Connected bodies should not collide by default
            CollideConnected = false;
        }

        /// <summary>
        /// Constructor for fixed joint
        /// </summary>
        protected Joint(Body body)
        {
            BodyA = body;

            //Connected bodies should not collide by default
            CollideConnected = false;
        }

        /// <summary>
        /// Gets or sets the type of the joint.
        /// </summary>
        /// <value>The type of the joint.</value>
        public JointType JointType { get; protected set; }

        /// <summary>
        /// Get the first body attached to this joint.
        /// </summary>
        /// <value></value>
        public Body BodyA { get; set; }

        /// <summary>
        /// Get the second body attached to this joint.
        /// </summary>
        /// <value></value>
        public Body BodyB { get; set; }

        /// <summary>
        /// Get the anchor point on bodyA in world coordinates.
        /// </summary>
        /// <value></value>
        public abstract Vector2 WorldAnchorA { get; }

        /// <summary>
        /// Get the anchor point on bodyB in world coordinates.
        /// </summary>
        /// <value></value>
        public abstract Vector2 WorldAnchorB { get; set; }

        /// <summary>
        /// Set the user data pointer.
        /// </summary>
        /// <value>The data.</value>
        public object UserData { get; set; }

        /// <summary>
        /// Set this flag to true if the attached bodies should collide.
        /// </summary>
        public bool CollideConnected { get; set; }

        /// <summary>
        /// Fires when the joint is broken.
        /// </summary>
        public event Action<Joint, float> Broke;

        /// <summary>
        /// Get the reaction force on bodyB at the joint anchor in Newtons.
        /// </summary>
        /// <param name="invDt">The inv_dt.</param>
        /// <returns></returns>
        public abstract Vector2 GetReactionForce(float invDt);

        /// <summary>
        /// Get the reaction torque on bodyB in N*m.
        /// </summary>
        /// <param name="invDt">The inv_dt.</param>
        /// <returns></returns>
        public abstract float GetReactionTorque(float invDt);

        protected void WakeBodies()
        {
            //if (BodyA != null)
                //BodyA.Awake = true;

            if (BodyB != null)
                BodyB.Awake = true;
        }

        /// <summary>
        /// Return true if the joint is a fixed type.
        /// </summary>
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

        internal void Validate(float invDT)
        {
            if (!Enabled)
                return;

            float jointError = GetReactionForce(invDT).Length();
            if (Math.Abs(jointError) <= Breakpoint)
                return;

            Enabled = false;

            if (Broke != null)
                Broke(this, jointError);
        }

        internal abstract void SolveVelocityConstraints(ref SolverData data);

        /// <summary>
        /// Solves the position constraints.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>returns true if the position errors are within tolerance.</returns>
        internal abstract bool SolvePositionConstraints(ref SolverData data);
    }
}