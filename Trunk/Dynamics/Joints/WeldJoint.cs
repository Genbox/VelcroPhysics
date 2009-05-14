using System.Collections.Generic;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Collisions;

#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics
{
    /// <summary>
    /// 
    /// </summary>
    public class WeldJoint
    {
        public event JointDelegate JointUpdated;

        private AngleJoint _angleJoint;
        private RevoluteJoint _revJoint;

        public WeldJoint()
        {
            
        }

        public WeldJoint(Body body1, Body body2, Vector2 anchor)
        {
            _angleJoint = JointFactory.Instance.CreateAngleJoint(body1, body2);
            _revJoint = JointFactory.Instance.CreateRevoluteJoint(body1, body2, anchor);
        }

        public void Add(PhysicsSimulator ps)
        {
            ps.Add(_angleJoint);
            ps.Add(_revJoint);
        }
    }
}