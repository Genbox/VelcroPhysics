using System;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Factories
{
    public class JointFactory
    {
        private static JointFactory _instance;

        private JointFactory()
        {
        }

        public static JointFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new JointFactory();
                }
                return _instance;
            }
        }

        //revolute joint
        public RevoluteJoint CreateRevoluteJoint(PhysicsSimulator physicsSimulator, Body body1, Body body2,
                                                 Vector2 initialAnchorPosition)
        {
            RevoluteJoint revoluteJoint = CreateRevoluteJoint(body1, body2, initialAnchorPosition);
            physicsSimulator.Add(revoluteJoint);
            return revoluteJoint;
        }

        public RevoluteJoint CreateRevoluteJoint(Body body1, Body body2, Vector2 initialAnchorPosition)
        {
            RevoluteJoint revoluteJoint = new RevoluteJoint(body1, body2, initialAnchorPosition);
            return revoluteJoint;
        }

        //fixed revolute joint
        public FixedRevoluteJoint CreateFixedRevoluteJoint(PhysicsSimulator physicsSimulator, Body body, Vector2 anchor)
        {
            FixedRevoluteJoint revoluteJoint = CreateFixedRevoluteJoint(body, anchor);
            physicsSimulator.Add(revoluteJoint);
            return revoluteJoint;
        }

        public FixedRevoluteJoint CreateFixedRevoluteJoint(Body body, Vector2 anchor)
        {
            if (body.isStatic)
            {
                throw new Exception("Fixed joints cannot be created on static bodies");
            }
            FixedRevoluteJoint revoluteJoint = new FixedRevoluteJoint(body, anchor);
            return revoluteJoint;
        }

        //pin joint
        public PinJoint CreatePinJoint(PhysicsSimulator physicsSimulator, Body body1, Vector2 anchor1, Body body2,
                                       Vector2 anchor2)
        {
            PinJoint pinJoint = CreatePinJoint(body1, anchor1, body2, anchor2);
            physicsSimulator.Add(pinJoint);
            return pinJoint;
        }

        public PinJoint CreatePinJoint(Body body1, Vector2 anchor1, Body body2, Vector2 anchor2)
        {
            PinJoint pinJoint = new PinJoint(body1, anchor1, body2, anchor2);
            return pinJoint;
        }

        //slider joint
        public SliderJoint CreateSliderJoint(PhysicsSimulator physicsSimulator, Body body1, Vector2 anchor1, Body body2,
                                             Vector2 anchor2, float min, float max)
        {
            SliderJoint sliderJoint = CreateSliderJoint(body1, anchor1, body2, anchor2, min, max);
            physicsSimulator.Add(sliderJoint);
            return sliderJoint;
        }

        public SliderJoint CreateSliderJoint(Body body1, Vector2 anchor1, Body body2, Vector2 anchor2, float min,
                                             float max)
        {
            SliderJoint sliderJoint = new SliderJoint(body1, anchor1, body2, anchor2, min, max);
            return sliderJoint;
        }

        //angle joint
        public AngleJoint CreateAngleJoint(PhysicsSimulator physicsSimulator, Body body1, Body body2)
        {
            AngleJoint angleJoint = CreateAngleJoint(body1, body2);
            physicsSimulator.Add(angleJoint);
            return angleJoint;
        }

        public AngleJoint CreateAngleJoint(Body body1, Body body2)
        {
            AngleJoint angleJoint = new AngleJoint(body1, body2);
            return angleJoint;
        }

        public AngleJoint CreateAngleJoint(PhysicsSimulator physicsSimulator, Body body1, Body body2, float softness,
                                           float biasFactor)
        {
            AngleJoint angleJoint = CreateAngleJoint(body1, body2, softness, biasFactor);
            physicsSimulator.Add(angleJoint);
            return angleJoint;
        }

        public AngleJoint CreateAngleJoint(Body body1, Body body2, float softness, float biasFactor)
        {
            AngleJoint angleJoint = new AngleJoint(body1, body2);
            angleJoint.Softness = softness;
            angleJoint.BiasFactor = biasFactor;
            return angleJoint;
        }

        //fixed angle joint
        public FixedAngleJoint CreateFixedAngleJoint(PhysicsSimulator physicsSimulator, Body body)
        {
            FixedAngleJoint fixedAngleJoint = CreateFixedAngleJoint(body);
            physicsSimulator.Add(fixedAngleJoint);
            return fixedAngleJoint;
        }

        public FixedAngleJoint CreateFixedAngleJoint(Body body)
        {
            FixedAngleJoint fixedAngleJoint = new FixedAngleJoint(body);
            return fixedAngleJoint;
        }

        //angle limit joint
        public AngleLimitJoint CreateAngleLimitJoint(PhysicsSimulator physicsSimulator, Body body1, Body body2,
                                                     float lowerLimit, float upperLimit)
        {
            AngleLimitJoint angleLimitJoint = CreateAngleLimitJoint(body1, body2, lowerLimit, upperLimit);
            physicsSimulator.Add(angleLimitJoint);
            return angleLimitJoint;
        }

        public AngleLimitJoint CreateAngleLimitJoint(Body body1, Body body2, float lowerLimit, float upperLimit)
        {
            AngleLimitJoint angleLimitJoint = new AngleLimitJoint(body1, body2, lowerLimit, upperLimit);
            return angleLimitJoint;
        }

        //fixed angle limit joint
        public FixedAngleLimitJoint CreateFixedAngleLimitJoint(PhysicsSimulator physicsSimulator, Body body,
                                                               float lowerLimit, float upperLimit)
        {
            FixedAngleLimitJoint fixedAngleLimitJoint = CreateFixedAngleLimitJoint(body, lowerLimit, upperLimit);
            physicsSimulator.Add(fixedAngleLimitJoint);
            return fixedAngleLimitJoint;
        }

        public FixedAngleLimitJoint CreateFixedAngleLimitJoint(Body body, float lowerLimit, float upperLimit)
        {
            FixedAngleLimitJoint fixedAngleLimitJoint = new FixedAngleLimitJoint(body, lowerLimit, upperLimit);
            return fixedAngleLimitJoint;
        }
    }
}