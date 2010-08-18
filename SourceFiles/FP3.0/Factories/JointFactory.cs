using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;

using Microsoft.Xna.Framework;

namespace FarseerPhysics.Factories
{
    /// <summary>
    /// An easy to use factory for using joints.
    /// </summary>
    public static class JointFactory
    {
        #region Revolute Joint
        /// <summary>
        /// Creates a revolute joint and adds it to the world
        /// </summary>
        /// <param name="world"></param>
        /// <param name="bodyA"></param>
        /// <param name="bodyB"></param>
        /// <param name="localanchorA"></param>
        /// <returns></returns>
        public static RevoluteJoint CreateRevoluteJoint(Body bodyA, Body bodyB, Vector2 localanchorA)
        {
            Vector2 localanchorB = bodyB.GetLocalPoint(bodyA.GetWorldPoint(localanchorA));
            RevoluteJoint joint = new RevoluteJoint(bodyA, bodyB, localanchorA, localanchorB);
            return joint;
        }

        /// <summary>
        /// Creates a revolute joint and adds it to the world
        /// </summary>
        /// <param name="world"></param>
        /// <param name="bodyA"></param>
        /// <param name="bodyB"></param>
        /// <param name="localanchorA"></param>
        /// <returns></returns>
        public static RevoluteJoint CreateRevoluteJoint(World world, Body bodyA, Body bodyB, Vector2 localanchorA)
        {
            RevoluteJoint joint = CreateRevoluteJoint(bodyA, bodyB, localanchorA);
            world.AddJoint(joint);
            return joint;
        }
        #endregion

        #region Weld Joint
        /// <summary>
        /// Creates a weld joint
        /// </summary>
        /// <param name="world"></param>
        /// <param name="bodyA"></param>
        /// <param name="bodyB"></param>
        /// <param name="localanchorA"></param>
        /// <returns></returns>
        public static WeldJoint CreateWeldJoint(Body bodyA, Body bodyB, Vector2 localanchorA)
        {
            Vector2 localanchorB = bodyB.GetLocalPoint(bodyA.GetWorldPoint(localanchorA));
            WeldJoint joint = new WeldJoint(bodyA, bodyB, localanchorA, localanchorB);
            return joint;
        }

        /// <summary>
        /// Creates a weld joint and adds it to the world
        /// </summary>
        /// <param name="world"></param>
        /// <param name="bodyA"></param>
        /// <param name="bodyB"></param>
        /// <param name="localanchorA"></param>
        /// <returns></returns>
        public static WeldJoint CreateWeldJoint(World world, Body bodyA, Body bodyB, Vector2 localanchorA)
        {
            WeldJoint joint = CreateWeldJoint(bodyA, bodyB, localanchorA);
            world.AddJoint(joint);
            return joint;
        }
        #endregion
        
    }
}