﻿using FarseerPhysics.Dynamics;
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
        /// <param name="bodyA"></param>
        /// <param name="bodyB"></param>
        /// <param name="localanchorB"></param>
        /// <returns></returns>
        public static RevoluteJoint CreateRevoluteJoint(Body bodyA, Body bodyB, Vector2 localanchorB)
        {
            Vector2 localanchorA = bodyA.GetLocalPoint(bodyB.GetWorldPoint(localanchorB));
            RevoluteJoint joint = new RevoluteJoint(bodyA, bodyB, localanchorA, localanchorB);
            return joint;
        }

        /// <summary>
        /// Creates a revolute joint and adds it to the world
        /// </summary>
        /// <param name="world"></param>
        /// <param name="bodyA"></param>
        /// <param name="bodyB"></param>
        /// <param name="localanchorB"></param>
        /// <returns></returns>
        public static RevoluteJoint CreateRevoluteJoint(World world, Body bodyA, Body bodyB, Vector2 localanchorB)
        {
            RevoluteJoint joint = CreateRevoluteJoint(bodyA, bodyB, localanchorB);
            world.AddJoint(joint);
            return joint;
        }

        /// <summary>
        /// Creates the fixed revolute joint.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="body">The body.</param>
        /// <param name="bodyAnchor">The body anchor.</param>
        /// <param name="worldAnchor">The world anchor.</param>
        /// <returns></returns>
        public static FixedRevoluteJoint CreateFixedRevoluteJoint(World world, Body body, Vector2 bodyAnchor, Vector2 worldAnchor)
        {
            FixedRevoluteJoint fixedRevoluteJoint = new FixedRevoluteJoint(body, bodyAnchor, worldAnchor);
            world.AddJoint(fixedRevoluteJoint);
            return fixedRevoluteJoint;
        }

        #endregion

        #region Weld Joint

        /// <summary>
        /// Creates a weld joint
        /// </summary>
        /// <param name="world"></param>
        /// <param name="bodyA"></param>
        /// <param name="bodyB"></param>
        /// <param name="localanchorB"></param>
        /// <returns></returns>
        public static WeldJoint CreateWeldJoint(Body bodyA, Body bodyB, Vector2 localanchorB)
        {
            Vector2 localanchorA = bodyA.GetLocalPoint(bodyB.GetWorldPoint(localanchorB));
            WeldJoint joint = new WeldJoint(bodyA, bodyB, localanchorA, localanchorB);
            return joint;
        }

        /// <summary>
        /// Creates a weld joint and adds it to the world
        /// </summary>
        /// <param name="world"></param>
        /// <param name="bodyA"></param>
        /// <param name="bodyB"></param>
        /// <param name="localanchorB"></param>
        /// <returns></returns>
        public static WeldJoint CreateWeldJoint(World world, Body bodyA, Body bodyB, Vector2 localanchorB)
        {
            WeldJoint joint = CreateWeldJoint(bodyA, bodyB, localanchorB);
            world.AddJoint(joint);
            return joint;
        }

        #endregion

        #region Prismatic Joint

        /// <summary>
        /// Creates a prsimatic joint
        /// </summary>
        /// <param name="bodyA"></param>
        /// <param name="bodyB"></param>
        /// <param name="localanchorB"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static PrismaticJoint CreatePrismaticJoint(Body bodyA, Body bodyB, Vector2 localanchorB, Vector2 axis)
        {
            Vector2 localanchorA = bodyA.GetLocalPoint(bodyB.GetWorldPoint(localanchorB));
            PrismaticJoint joint = new PrismaticJoint(bodyA, bodyB, localanchorA, localanchorB, axis);
            return joint;
        }

        /// <summary>
        /// Creates a prismatic joint and adds it to the world
        /// </summary>
        /// <param name="world"></param>
        /// <param name="bodyA"></param>
        /// <param name="bodyB"></param>
        /// <param name="localanchorB"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static PrismaticJoint CreatePrismaticJoint(World world, Body bodyA, Body bodyB, Vector2 localanchorB,
                                                          Vector2 axis)
        {
            PrismaticJoint joint = CreatePrismaticJoint(bodyA, bodyB, localanchorB, axis);
            world.AddJoint(joint);
            return joint;
        }

        #endregion

        #region Line Joint

        /// <summary>
        /// Creates a line joint
        /// </summary>
        /// <param name="bodyA"></param>
        /// <param name="bodyB"></param>
        /// <param name="localanchorB"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static LineJoint CreateLineJoint(Body bodyA, Body bodyB, Vector2 localanchorB, Vector2 axis)
        {
            Vector2 localanchorA = bodyA.GetLocalPoint(bodyB.GetWorldPoint(localanchorB));
            LineJoint joint = new LineJoint(bodyA, bodyB, localanchorA, localanchorB, axis);
            return joint;
        }

        /// <summary>
        /// Creates a line joint and adds it to the world
        /// </summary>
        /// <param name="world"></param>
        /// <param name="bodyA"></param>
        /// <param name="bodyB"></param>
        /// <param name="localanchorB"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static LineJoint CreateLineJoint(World world, Body bodyA, Body bodyB, Vector2 localanchorB, Vector2 axis)
        {
            LineJoint joint = CreateLineJoint(bodyA, bodyB, localanchorB, axis);
            world.AddJoint(joint);
            return joint;
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

        /// <summary>
        /// Creates a fixed angle joint.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="body">The body.</param>
        /// <returns></returns>
        public static FixedAngleJoint CreateFixedAngleJoint(World world, Body body)
        {
            FixedAngleJoint angleJoint = new FixedAngleJoint(body);
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

        public static FixedDistanceJoint CreateFixedDistanceJoint(World world, Body body, Vector2 localAnchor, Vector2 worldAnchor)
        {
            FixedDistanceJoint distanceJoint = new FixedDistanceJoint(body, localAnchor, worldAnchor);
            world.AddJoint(distanceJoint);
            return distanceJoint;
        }

        #endregion

        #region Friction Joint

        public static FrictionJoint CreateFrictionJoint(World world, Body bodyA, Body bodyB, Vector2 anchorA, Vector2 anchorB)
        {
            FrictionJoint frictionJoint = new FrictionJoint(bodyA, bodyB, anchorA, anchorB);
            world.AddJoint(frictionJoint);
            return frictionJoint;
        }

        #endregion

        #region Gear Joint

        public static GearJoint CreateGearJoint(World world, Joint jointA, Joint jointB, float ratio)
        {
            GearJoint gearJoint = new GearJoint(jointA, jointB, ratio);
            world.AddJoint(gearJoint);
            return gearJoint;
        }

        #endregion

        #region Pully Joint

        public static PulleyJoint CreatePullyJoint(World world, Body bodyA, Body bodyB, Vector2 groundAnchorA, Vector2 groundAnchorB, Vector2 anchorA, Vector2 anchorB, float ratio)
        {
            PulleyJoint pulleyJoint = new PulleyJoint(bodyA, bodyB, groundAnchorA, groundAnchorB, anchorA, anchorB, ratio);
            world.AddJoint(pulleyJoint);
            return pulleyJoint;
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

        #region Slider Joint

        public static SliderJoint CreateSliderJoint(World world, Body bodyA, Body bodyB, Vector2 anchorA, Vector2 anchorB, float minLength, float maxLength)
        {
            SliderJoint sliderJoint = new SliderJoint(bodyA, bodyB, anchorA, anchorB, minLength, maxLength);
            world.AddJoint(sliderJoint);
            return sliderJoint;
        }

        #endregion
    }
}