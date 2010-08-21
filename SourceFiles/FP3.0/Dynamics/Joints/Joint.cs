/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2010 Ian Qvist
* 
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com 
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

using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints
{
    public enum JointType
    {
        Revolute,
        Prismatic,
        Distance,
        Pulley,
        Gear,
        Line,
        Weld,
        Friction,
        FixedMouse,
        FixedRevolute,
        FixedDistance,
        FixedLine,
        FixedPrismatic,
        MaxDistance,
        Angle,
        FixedAngle
    }

    public enum LimitState
    {
        Inactive,
        AtLower,
        AtUpper,
        Equal,
    }

    internal struct Jacobian
    {
        public float AngularA;
        public float AngularB;
        public Vector2 LinearA;
        public Vector2 LinearB;

        public void SetZero()
        {
            LinearA = Vector2.Zero;
            AngularA = 0.0f;
            LinearB = Vector2.Zero;
            AngularB = 0.0f;
        }

        public void Set(Vector2 x1, float a1, Vector2 x2, float a2)
        {
            LinearA = x1;
            AngularA = a1;
            LinearB = x2;
            AngularB = a2;
        }

        public float Compute(Vector2 x1, float a1, Vector2 x2, float a2)
        {
            return Vector2.Dot(LinearA, x1) + AngularA * a1 + Vector2.Dot(LinearB, x2) + AngularB * a2;
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
        internal JointEdge EdgeA;
        internal JointEdge EdgeB;
        protected float InvIA;
        protected float InvIB;
        protected float InvMassA;
        protected float InvMassB;
        internal bool IslandFlag;
        protected Vector2 LocalCenterA, LocalCenterB;

        protected Joint(Body body, Body bodyB)
        {
            Debug.Assert(body != bodyB);

            BodyA = body;
            EdgeA = new JointEdge();
            BodyB = bodyB;
            EdgeB = new JointEdge();

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

            EdgeA = new JointEdge();
        }

        /// <summary>
        /// Gets or sets the type of the joint.
        /// </summary>
        /// <value>The type of the joint.</value>
        public JointType JointType { get; set; }

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
        /// Get the anchor point on body1 in world coordinates.
        /// </summary>
        /// <value></value>
        public abstract Vector2 WorldAnchorA { get; }

        /// <summary>
        /// Get the anchor point on body2 in world coordinates.
        /// </summary>
        /// <value></value>
        public abstract Vector2 WorldAnchorB { get; }

        /// <summary>
        /// Set the user data pointer.
        /// </summary>
        /// <value>The data.</value>
        public object UserData { get; set; }

        /// <summary>
        /// Short-cut function to determine if either body is inactive.
        /// </summary>
        /// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
        public bool Active
        {
            get { return BodyA.Active && BodyB.Active; }
        }

        /// <summary>
        /// Set this flag to true if the attached bodies should collide.
        /// </summary>
        public bool CollideConnected { get; set; }

        /// <summary>
        /// Get the reaction force on body2 at the joint anchor in Newtons.
        /// </summary>
        /// <param name="inv_dt">The inv_dt.</param>
        /// <returns></returns>
        public abstract Vector2 GetReactionForce(float inv_dt);

        /// <summary>
        /// Get the reaction torque on body2 in N*m.
        /// </summary>
        /// <param name="inv_dt">The inv_dt.</param>
        /// <returns></returns>
        public abstract float GetReactionTorque(float inv_dt);

        protected void WakeBodies()
        {
            BodyA.Awake = true;
            if (BodyB != null)
            {
                BodyB.Awake = true;
            }
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
                   JointType == JointType.FixedAngle;
        }

        internal abstract void InitVelocityConstraints(ref TimeStep step);
        internal abstract void SolveVelocityConstraints(ref TimeStep step);

        /// <summary>
        /// Solves the position constraints.
        /// </summary>
        /// <returns>returns true if the position errors are within tolerance.</returns>
        internal abstract bool SolvePositionConstraints();
    }
}