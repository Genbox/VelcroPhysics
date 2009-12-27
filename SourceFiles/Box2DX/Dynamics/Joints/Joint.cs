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

using Box2DX.Common;

namespace Box2DX.Dynamics
{
    public enum JointType
    {
        UnknownJoint,
        RevoluteJoint,
        PrismaticJoint,
        DistanceJoint,
        PulleyJoint,
        MouseJoint,
        GearJoint,
        LineJoint,
        FixedJoint,
        WeldJoint,
        FrictionJoint
    }

    public enum LimitState
    {
        InactiveLimit,
        AtLowerLimit,
        AtUpperLimit,
        EqualLimits
    }

    public struct Jacobian
    {
        public Vec2 LinearA;
        public float AngularA;
        public Vec2 LinearB;
        public float AngularB;

        public void SetZero()
        {
            LinearA.SetZero(); AngularA = 0.0f;
            LinearB.SetZero(); AngularB = 0.0f;
        }

        public void Set(Vec2 x1, float a1, Vec2 x2, float a2)
        {
            LinearA = x1; AngularA = a1;
            LinearB = x2; AngularB = a2;
        }

        public float Compute(Vec2 x1, float a1, Vec2 x2, float a2)
        {
            return Vec2.Dot(LinearA, x1) + AngularA * a1 + Vec2.Dot(LinearB, x2) + AngularB * a2;
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


    /// <summary>
    /// Joint definitions are used to construct joints.
    /// </summary>
    public class JointDef
    {
        public JointDef()
        {
            Type = JointType.UnknownJoint;
            UserData = null;
            BodyA = null;
            BodyB = null;
            CollideConnected = false;
        }

        /// <summary>
        /// The joint type is set automatically for concrete joint types.
        /// </summary>
        public JointType Type;

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

    /// <summary>
    /// The base joint class. Joints are used to constraint two bodies together in
    /// various fashions. Some joints also feature limits and motors.
    /// </summary>
    public abstract class Joint
    {
        protected JointType _type;
        internal Joint _prev;
        internal Joint _next;
        internal JointEdge _edgeA = new JointEdge();
        internal JointEdge _edgeB = new JointEdge();
        internal Body _bodyA;
        internal Body _bodyB;

        internal bool _islandFlag;
        internal bool _collideConnected;

        protected object _userData;

        // Cache here per time step to reduce cache misses.
        protected Vec2 _localCenter1, _localCenter2;
        protected float _invMass1, _invI1;
        protected float _invMass2, _invI2;

        /// <summary>
        /// Get the type of the concrete joint.
        /// </summary>
        public new JointType GetType()
        {
            return _type;
        }

        /// <summary>
        /// Get the first body attached to this joint.
        /// </summary>
        /// <returns></returns>
        public Body GetBodyA()
        {
            return _bodyA;
        }

        /// <summary>
        /// Get the second body attached to this joint.
        /// </summary>
        /// <returns></returns>
        public Body GetBodyB()
        {
            return _bodyB;
        }

        /// <summary>
        /// Get the anchor point on body1 in world coordinates.
        /// </summary>
        /// <returns></returns>
        public abstract Vec2 GetAnchorA();

        /// <summary>
        /// Get the anchor point on body2 in world coordinates.
        /// </summary>
        /// <returns></returns>
        public abstract Vec2 GetAnchorB();

        /// <summary>
        /// Get the reaction force on body2 at the joint anchor in Newtons.
        /// </summary>		
        public abstract Vec2 GetReactionForce(float inv_dt);

        /// <summary>
        /// Get the reaction torque on body2 in N*m.
        /// </summary>		
        public abstract float GetReactionTorque(float inv_dt);

        /// <summary>
        /// Get the next joint the world joint list.
        /// </summary>
        /// <returns></returns>
        public Joint GetNext()
        {
            return _next;
        }

        /// <summary>
        /// Get/Set the user data pointer.
        /// </summary>
        /// <returns></returns>
        public object UserData
        {
            get { return _userData; }
            set { _userData = value; }
        }

        protected Joint(JointDef def)
        {
            _type = def.Type;
            _prev = null;
            _next = null;
            _bodyA = def.BodyA;
            _bodyB = def.BodyB;
            _collideConnected = def.CollideConnected;
            _islandFlag = false;
            _userData = def.UserData;

            _edgeA.Joint = null;
            _edgeA.Other = null;
            _edgeA.Prev = null;
            _edgeA.Next = null;

            _edgeB.Joint = null;
            _edgeB.Other = null;
            _edgeB.Prev = null;
            _edgeB.Next = null;
        }

        internal static Joint Create(JointDef def)
        {
            Joint joint = null;

            switch (def.Type)
            {
                case JointType.DistanceJoint:
                    {
                        joint = new DistanceJoint((DistanceJointDef)def);
                    }
                    break;
                case JointType.MouseJoint:
                    {
                        joint = new MouseJoint((MouseJointDef)def);
                    }
                    break;
                case JointType.PrismaticJoint:
                    {
                        joint = new PrismaticJoint((PrismaticJointDef)def);
                    }
                    break;
                case JointType.RevoluteJoint:
                    {
                        joint = new RevoluteJoint((RevoluteJointDef)def);
                    }
                    break;
                case JointType.PulleyJoint:
                    {
                        joint = new PulleyJoint((PulleyJointDef)def);
                    }
                    break;
                case JointType.GearJoint:
                    {
                        joint = new GearJoint((GearJointDef)def);
                    }
                    break;
                case JointType.LineJoint:
                    {
                        joint = new LineJoint((LineJointDef)def);
                    }
                    break;

                case JointType.WeldJoint:
                    {
                        joint = new WeldJoint((WeldJointDef)def);
                    }
                    break;

                case JointType.FrictionJoint:
                    {
                        joint = new FrictionJoint((FrictionJointDef)def);
                    }
                    break;
                default:
                    Box2DXDebug.Assert(false);
                    break;
            }

            return joint;
        }

        internal static void Destroy(Joint joint)
        {
            joint = null;
        }

        internal abstract void InitVelocityConstraints(TimeStep step);
        internal abstract void SolveVelocityConstraints(TimeStep step);
        internal virtual void FinalizeVelocityConstraints() { }

        // This returns true if the position errors are within tolerance.
        internal abstract bool SolvePositionConstraints(float baumgarte);

        internal void ComputeTransform(ref Transform xf, Vec2 center, Vec2 localCenter, float angle)
        {
            xf.R.Set(angle);
            xf.Position = center - Math.Mul(xf.R, localCenter);
        }

        public bool IsActive()
        {
            return _bodyA.IsActive() && _bodyB.IsActive();
        }
    }
}
