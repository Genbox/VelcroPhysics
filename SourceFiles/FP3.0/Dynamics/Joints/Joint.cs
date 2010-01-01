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

    public class JointDef
    {
        /// <summary>
        /// The joint type is set automatically for concrete joint types.
        /// </summary>
        internal JointType Type;

        /// <summary>
        /// Use this to attach application specific data to your joints.
        /// </summary>
        public object UserData;

        /// <summary>
        /// The first attached body.
        /// </summary>
        public Body BodyA;

        /// <summary>
        /// The second attached body.
        /// </summary>
        public Body BodyB;

        /// <summary>
        /// Set this flag to true if the attached bodies should collide.
        /// </summary>
	    public bool CollideConnected;
    }

    public abstract class Joint
    {
        /// <summary>
        /// Gets or sets the type of the joint.
        /// </summary>
        /// <value>The type of the joint.</value>
        public JointType JointType { get; private set; }

        /// <summary>
        /// Get the first body attached to this joint.
        /// </summary>
        /// <returns></returns>
	    public Body GetBodyA()
        {
            return BodyA;
        }

        /// <summary>
        /// Get the second body attached to this joint.
        /// </summary>
        /// <returns></returns>
	    public Body GetBodyB()
        {
            return BodyB;
        }

        /// <summary>
        /// Get the anchor point on body1 in world coordinates.
        /// </summary>
        /// <returns></returns>
	    public abstract Vector2 GetAnchorA();

        /// <summary>
        /// Get the anchor point on body2 in world coordinates.
        /// </summary>
        /// <returns></returns>
	    public abstract Vector2 GetAnchorB();

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
        /// <returns></returns>
	    public Joint GetNext()
        {
	        return Next;
        }

        /// <summary>
        /// Get the user data pointer.
        /// </summary>
        /// <returns></returns>
	    public object GetUserData()
        {
            return _userData;
        }

        /// <summary>
        /// Set the user data pointer.
        /// </summary>
        /// <param name="data">The data.</param>
	    public void SetUserData(object data)
        {
            _userData = data;
        }

        /// <summary>
        /// Short-cut function to determine if either body is inactive.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </returns>
        public bool IsActive()
        {
            return BodyA.Active && BodyB.Active;

        }

	    internal static Joint Create(JointDef def)
        {
	        Joint joint = null;

	        switch (def.Type)
	        {
	        case JointType.Distance:
		        {
			        joint = new DistanceJoint((DistanceJointDef)def);
		        }
		        break;

	        case JointType.Mouse:
		        {
			        joint = new MouseJoint((MouseJointDef)def);
		        }
		        break;

	        case JointType.Prismatic:
		        {
			        joint = new PrismaticJoint((PrismaticJointDef)def);
		        }
		        break;

	        case JointType.Revolute:
		        {
			        joint = new RevoluteJoint((RevoluteJointDef)def);
		        }
		        break;

	        case JointType.Pulley:
		        {
			        joint = new PulleyJoint((PulleyJointDef)def);
		        }
		        break;

	        case JointType.Gear:
		        {
			        joint = new GearJoint((GearJointDef)def);
		        }
		        break;

	        case JointType.Line:
		        {
			        joint = new LineJoint((LineJointDef)def);
		        }
		        break;

            case JointType.Weld:
                {
                    joint = new WeldJoint((WeldJointDef)def);
                }
                break;
            case JointType.Friction:
                {
                    joint = new FrictionJoint((FrictionJointDef)def);
                }
                break;
                
	        default:
		        Debug.Assert(false);
		        break;
	        }

	        return joint;
        }

	    protected Joint(JointDef def)
        {
            Debug.Assert(def.BodyA != def.BodyB);

	        JointType = def.Type;
	        BodyA = def.BodyA;
	        BodyB = def.BodyB;
	        CollideConnected = def.CollideConnected;
	        _userData = def.UserData;

            EdgeA = new JointEdge();
            EdgeB = new JointEdge();
        }

	    internal abstract void InitVelocityConstraints(ref TimeStep step);
	    internal abstract void SolveVelocityConstraints(ref TimeStep step);
        internal virtual void FinalizeVelocityConstraints() {}

	    // This returns true if the position errors are within tolerance.
	    internal abstract bool SolvePositionConstraints(float baumgarte);

        internal Joint Prev;
	    internal Joint Next;
	    internal JointEdge EdgeA;
	    internal JointEdge EdgeB;
	    internal Body BodyA;
	    internal Body BodyB;

	    internal bool IslandFlag;
	    internal bool CollideConnected;

        private object _userData;

	    // Cache here per time step to reduce cache misses.
	    internal Vector2 LocalCenterA, LocalCenterB;
	    internal float InvMassA, InvIA;
	    internal float InvMassB, InvIB;
    }
}
