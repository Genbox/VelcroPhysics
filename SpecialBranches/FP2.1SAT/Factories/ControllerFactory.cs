using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Factories
{
    /// <summary>
    /// An easy to use factory for creating controllers (springs)
    /// </summary>
    public class ControllerFactory
    {
        private static ControllerFactory _instance;

        private ControllerFactory()
        {
        }

        public static ControllerFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ControllerFactory();
                }
                return _instance;
            }
        }

        public LinearSpring CreateLinearSpring(PhysicsSimulator physicsSimulator, Body body1, Vector2 attachPoint1,
                                               Body body2, Vector2 attachPoint2, float springConstant,
                                               float dampningConstant)
        {
            LinearSpring linearSpring = CreateLinearSpring(body1, attachPoint1, body2, attachPoint2, springConstant,
                                                           dampningConstant);
            physicsSimulator.Add(linearSpring);
            return linearSpring;
        }

        public LinearSpring CreateLinearSpring(Body body1, Vector2 attachPoint1, Body body2, Vector2 attachPoint2,
                                               float springConstant, float dampningConstant)
        {
            LinearSpring linearSpring = new LinearSpring(body1, attachPoint1, body2, attachPoint2, springConstant,
                                                         dampningConstant);
            return linearSpring;
        }

        public FixedLinearSpring CreateFixedLinearSpring(PhysicsSimulator physicsSimulator, Body body,
                                                         Vector2 bodyAttachPoint, Vector2 worldAttachPoint,
                                                         float springConstant, float dampningConstant)
        {
            FixedLinearSpring fixedSpring = CreateFixedLinearSpring(body, bodyAttachPoint, worldAttachPoint,
                                                                    springConstant, dampningConstant);
            physicsSimulator.Add(fixedSpring);
            return fixedSpring;
        }

        public FixedLinearSpring CreateFixedLinearSpring(Body body, Vector2 bodyAttachPoint, Vector2 worldAttachPoint,
                                                         float springConstant, float dampningConstant)
        {
            FixedLinearSpring fixedSpring = new FixedLinearSpring(body, bodyAttachPoint, worldAttachPoint,
                                                                  springConstant, dampningConstant);
            return fixedSpring;
        }

        public AngleSpring CreateAngleSpring(PhysicsSimulator physicsSimulator, Body body1, Body body2,
                                             float springConstant, float dampningConstant)
        {
            AngleSpring angleSpring = CreateAngleSpring(body1, body2, springConstant, dampningConstant);
            physicsSimulator.Add(angleSpring);
            return angleSpring;
        }

        public AngleSpring CreateAngleSpring(Body body1, Body body2, float springConstant, float dampningConstant)
        {
            AngleSpring angleSpring = new AngleSpring(body1, body2, springConstant, dampningConstant);
            return angleSpring;
        }

        public FixedAngleSpring CreateFixedAngleSpring(PhysicsSimulator physicsSimulator, Body body,
                                                       float springConstant, float dampningConstant)
        {
            FixedAngleSpring fixedAngleSpring = CreateFixedAngleSpring(body, springConstant, dampningConstant);
            physicsSimulator.Add(fixedAngleSpring);
            return fixedAngleSpring;
        }

        public FixedAngleSpring CreateFixedAngleSpring(Body body, float springConstant, float dampningConstant)
        {
            FixedAngleSpring fixedAngleSpring = new FixedAngleSpring(body, springConstant, dampningConstant);
            return fixedAngleSpring;
        }
    }
}