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
        #region Motor Joint

        public static MotorJoint CreateMotorJoint(World world, Body bodyA, Body bodyB)
        {
            MotorJoint joint = new MotorJoint(bodyA, bodyB);
            world.AddJoint(joint);
            return joint;
        }

        #endregion

        #region Revolute Joint

        public static RevoluteJoint CreateRevoluteJoint(World world, Body bodyA, Vector2 anchorA, Body bodyB, Vector2 anchorB)
        {
            RevoluteJoint joint = new RevoluteJoint(bodyA, bodyB, anchorA, anchorB);
            world.AddJoint(joint);
            return joint;
        }

        /// <summary>
        /// Creates a revolute joint and adds it to the world
        /// </summary>
        /// <param name="world"></param>
        /// <param name="bodyA"></param>
        /// <param name="bodyB"></param>
        /// <param name="anchorB"></param>
        /// <returns></returns>
        public static RevoluteJoint CreateRevoluteJoint(World world, Body bodyA, Body bodyB, Vector2 anchorB)
        {
            Vector2 localanchorA = bodyA.GetLocalPoint(bodyB.GetWorldPoint(anchorB));
            RevoluteJoint joint = new RevoluteJoint(bodyA, bodyB, localanchorA, anchorB);
            world.AddJoint(joint);
            return joint;
        }

        ///// <summary>
        ///// Creates the fixed revolute joint.
        ///// </summary>
        ///// <param name="world">The world.</param>
        ///// <param name="body">The body.</param>
        ///// <param name="bodyAnchor">The body anchor.</param>
        ///// <param name="worldAnchor">The world anchor.</param>
        ///// <returns></returns>
        //public static FixedRevoluteJoint CreateFixedRevoluteJoint(World world, Body body, Vector2 bodyAnchor,
        //                                                          Vector2 worldAnchor)
        //{
        //    FixedRevoluteJoint fixedRevoluteJoint = new FixedRevoluteJoint(body, bodyAnchor, worldAnchor);
        //    world.AddJoint(fixedRevoluteJoint);
        //    return fixedRevoluteJoint;
        //}

        #endregion


        #region Rope Joint

        /// <summary>
        /// Creates a rope joint and adds it to the world
        /// </summary>
        public static RopeJoint CreateRopeJoint(World world, Body bodyA, Body bodyB, Vector2 anchorA, Vector2 anchorB)
        {
            RopeJoint ropeJoint = new RopeJoint(bodyA, bodyB, anchorA, anchorB);
            world.AddJoint(ropeJoint);
            return ropeJoint;
        }

        #endregion

        #region Weld Joint

        public static WeldJoint CreateWeldJoint(World world, Body bodyA, Body bodyB, Vector2 anchorA, Vector2 anchorB)
        {
            WeldJoint weldJoint = new WeldJoint(bodyA, bodyB, anchorA, anchorB);
            world.AddJoint(weldJoint);
            return weldJoint;
        }

        #endregion

        #region Prismatic Joint

        /// <summary>
        /// Creates a prismatic joint and adds it to the world
        /// </summary>
        /// <param name="world"></param>
        /// <param name="bodyA"></param>
        /// <param name="bodyB"></param>
        /// <param name="anchor"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static PrismaticJoint CreatePrismaticJoint(World world, Body bodyA, Body bodyB, Vector2 anchor, Vector2 axis)
        {
            PrismaticJoint joint = new PrismaticJoint(bodyA, bodyB, anchor, axis);
            world.AddJoint(joint);
            return joint;
        }

        //public static FixedPrismaticJoint CreateFixedPrismaticJoint(World world, Body body, Vector2 worldAnchor,
        //                                                            Vector2 axis)
        //{
        //    FixedPrismaticJoint joint = new FixedPrismaticJoint(body, worldAnchor, axis);
        //    world.AddJoint(joint);
        //    return joint;
        //}

        #endregion

        #region Wheel Joint

        /// <summary>
        /// Creates a Wheel Joint and adds it to the world
        /// </summary>
        public static WheelJoint CreateWheelJoint(World world, Body bodyA, Body bodyB, Vector2 anchorB, Vector2 axis)
        {
            WheelJoint joint = new WheelJoint(bodyA, bodyB, anchorB, axis);
            world.AddJoint(joint);
            return joint;
        }

        public static WheelJoint CreateWheelJoint(World world, Body bodyA, Body bodyB, Vector2 axis)
        {
            return CreateWheelJoint(world, bodyA, bodyB, Vector2.Zero, axis);
        }

        #endregion

        #region Angle Joint

        /// <summary>
        /// Creates an angle joint.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="bodyA">The first body.</param>
        /// <param name="bodyB">The second body.</param>
        /// <returns></returns>
        public static AngleJoint CreateAngleJoint(World world, Body bodyA, Body bodyB)
        {
            AngleJoint angleJoint = new AngleJoint(bodyA, bodyB);
            world.AddJoint(angleJoint);

            return angleJoint;
        }

        #endregion

        #region Distance Joint

        public static DistanceJoint CreateDistanceJoint(World world, Body bodyA, Body bodyB, Vector2 anchorA, Vector2 anchorB)
        {
            DistanceJoint distanceJoint = new DistanceJoint(bodyA, bodyB, anchorA, anchorB);
            world.AddJoint(distanceJoint);
            return distanceJoint;
        }

        public static DistanceJoint CreateDistanceJoint(World world, Body bodyA, Body bodyB)
        {
            return CreateDistanceJoint(world, bodyA, bodyB, Vector2.Zero, Vector2.Zero);
        }

        //public static FixedDistanceJoint CreateFixedDistanceJoint(World world, Body body, Vector2 localAnchor,
        //                                                          Vector2 worldAnchor)
        //{
        //    FixedDistanceJoint distanceJoint = new FixedDistanceJoint(body, localAnchor, worldAnchor);
        //    world.AddJoint(distanceJoint);
        //    return distanceJoint;
        //}

        #endregion

        #region Friction Joint

        public static FrictionJoint CreateFrictionJoint(World world, Body bodyA, Body bodyB, Vector2 anchor)
        {
            FrictionJoint frictionJoint = new FrictionJoint(bodyA, bodyB, anchor);
            world.AddJoint(frictionJoint);
            return frictionJoint;
        }

        public static FrictionJoint CreateFrictionJoint(World world, Body bodyA, Body bodyB)
        {
            return CreateFrictionJoint(world, bodyA, bodyB, Vector2.Zero);
        }

        //public static FixedFrictionJoint CreateFixedFrictionJoint(World world, Body body, Vector2 bodyAnchor)
        //{
        //    FixedFrictionJoint frictionJoint = new FixedFrictionJoint(body, bodyAnchor);
        //    world.AddJoint(frictionJoint);
        //    return frictionJoint;
        //}

        #endregion

        #region Gear Joint

        public static GearJoint CreateGearJoint(World world, Body bodyA, Body bodyB, Joint jointA, Joint jointB, float ratio)
        {
            GearJoint gearJoint = new GearJoint(bodyA, bodyB, jointA, jointB, ratio);
            world.AddJoint(gearJoint);
            return gearJoint;
        }

        #endregion

        #region Pulley Joint

        public static PulleyJoint CreatePulleyJoint(World world, Body bodyA, Body bodyB, Vector2 groundAnchorA, Vector2 groundAnchorB, Vector2 anchorA, Vector2 anchorB, float ratio)
        {
            PulleyJoint pulleyJoint = new PulleyJoint(bodyA, bodyB, anchorA, anchorB, groundAnchorA, groundAnchorB, ratio);
            world.AddJoint(pulleyJoint);
            return pulleyJoint;
        }

        #endregion

        public static FixedMouseJoint CreateFixedMouseJoint(World world, Body body, Vector2 target)
        {
            FixedMouseJoint joint = new FixedMouseJoint(body, target);
            world.AddJoint(joint);
            return joint;
        }
    }
}