/*
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

using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace FarseerPhysics
{
    public enum JointType
    {
        Revolute,
        Prismatic,
        Distance,
        Pulley,
        Mouse,
        Gear,
        Line,
        Weld,
        Friction,
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
	    public Vector2 LinearA;
	    public float AngularA;
	    public Vector2 LinearB;
	    public float AngularB;

	    public void SetZero()
        {
	        LinearA = Vector2.Zero; AngularA = 0.0f;
	        LinearB = Vector2.Zero; AngularB = 0.0f;
        }

	    public void Set(Vector2 x1, float a1, Vector2 x2, float a2)
        {
	        LinearA = x1; AngularA = a1;
	        LinearB = x2; AngularB = a2;
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
    public class JointEdge
    {
        /// <summary>
        /// Provides quick access to the other body attached.
        /// </summary>
        public Body Other;

        /// <summary>
        /// The joint.
        /// </summary>
        public Joint Joint;

        /// <summary>
        /// The previous joint edge in the body's joint list.
        /// </summary>
        public JointEdge Prev;

        /// <summary>
        /// The next joint edge in the body's joint list.
        /// </summary>
        public JointEdge Next;
    }

    public abstract class Joint
    {
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
        public abstract Vector2 AnchorA { get; }

        /// <summary>
        /// Get the anchor point on body2 in world coordinates.
        /// </summary>
        /// <value></value>
        public abstract Vector2 AnchorB { get; }

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

        /// <summary>
        /// Get the next joint the world joint list.
        /// </summary>
        /// <value></value>
        public Joint Next { get; internal set; }

        /// <summary>
        /// Get the previous joint the world joint list.
        /// </summary>
        /// <value></value>
        public Joint Prev { get; set; }

        /// <summary>
        /// Set the user data pointer.
        /// </summary>
        /// <value>The data.</value>
        public object UserData { get; set; }

        /// <summary>
        /// Short-cut function to determine if either body is inactive.
        /// </summary>
        /// <value>
        ///   &lt;c&gt;true&lt;/c&gt; if this instance is active; otherwise, &lt;c&gt;false&lt;/c&gt;.
        /// </value>
        public bool Active
        {
            get { return BodyA.Active && BodyB.Active; }
        }

	    protected Joint(Body bodyA, Body bodyB)
        {
            Debug.Assert(bodyA != bodyB);

            BodyA = bodyA;
            BodyB = bodyB;

            //Connected bodies should collide by default
            CollideConnected = true;

            _edgeA = new JointEdge();
            _edgeB = new JointEdge();
        }

	    internal abstract void InitVelocityConstraints(ref TimeStep step);
	    internal abstract void SolveVelocityConstraints(ref TimeStep step);
        internal virtual void FinalizeVelocityConstraints() {}

	    // This returns true if the position errors are within tolerance.
        internal abstract bool SolvePositionConstraints();

        internal JointEdge _edgeA;
	    internal JointEdge _edgeB;

        internal bool _islandFlag;

        /// <summary>
        /// Set this flag to true if the attached bodies should collide.
        /// </summary>
        public bool CollideConnected { get; set; }

        // Cache here per time step to reduce cache misses.
        protected Vector2 _localCenterA, _localCenterB;
        protected float _invMassA, _invIA;
	    protected float _invMassB, _invIB;
    }
}
