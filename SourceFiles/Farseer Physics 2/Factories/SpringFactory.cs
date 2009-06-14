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
    public class SpringFactory
    {
        private static SpringFactory _instance;

        private SpringFactory()
        {
        }

        public static SpringFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SpringFactory();
                }
                return _instance;
            }
        }

        #region Linear spring
        public LinearSpring CreateLinearSpring(PhysicsSimulator physicsSimulator, Body body1, Vector2 attachPoint1,
                                               Body body2, Vector2 attachPoint2, float springConstant,
                                               float dampingConstant)
        {
            LinearSpring linearSpring = CreateLinearSpring(body1, attachPoint1, body2, attachPoint2, springConstant,
                                                           dampingConstant);
            physicsSimulator.Add(linearSpring);
            return linearSpring;
        }

        public LinearSpring CreateLinearSpring(Body body1, Vector2 attachPoint1, Body body2, Vector2 attachPoint2,
                                               float springConstant, float dampingConstant)
        {
            LinearSpring linearSpring = new LinearSpring(body1, attachPoint1, body2, attachPoint2, springConstant,
                                                         dampingConstant);
            return linearSpring;
        }
        #endregion

        #region Fixed linear spring
        public FixedLinearSpring CreateFixedLinearSpring(PhysicsSimulator physicsSimulator, Body body,
                                                         Vector2 bodyAttachPoint, Vector2 worldAttachPoint,
                                                         float springConstant, float dampingConstant)
        {
            FixedLinearSpring fixedSpring = CreateFixedLinearSpring(body, bodyAttachPoint, worldAttachPoint,
                                                                    springConstant, dampingConstant);
            physicsSimulator.Add(fixedSpring);
            return fixedSpring;
        }

        public FixedLinearSpring CreateFixedLinearSpring(Body body, Vector2 bodyAttachPoint, Vector2 worldAttachPoint,
                                                         float springConstant, float dampingConstant)
        {
            FixedLinearSpring fixedSpring = new FixedLinearSpring(body, bodyAttachPoint, worldAttachPoint,
                                                                  springConstant, dampingConstant);
            return fixedSpring;
        }
        #endregion

        #region Angle spring
        public AngleSpring CreateAngleSpring(PhysicsSimulator physicsSimulator, Body body1, Body body2,
                                             float springConstant, float dampingConstant)
        {
            AngleSpring angleSpring = CreateAngleSpring(body1, body2, springConstant, dampingConstant);
            physicsSimulator.Add(angleSpring);
            return angleSpring;
        }

        public AngleSpring CreateAngleSpring(Body body1, Body body2, float springConstant, float dampingConstant)
        {
            AngleSpring angleSpring = new AngleSpring(body1, body2, springConstant, dampingConstant);
            return angleSpring;
        }
        #endregion

        #region Fixed angle spring
        public FixedAngleSpring CreateFixedAngleSpring(PhysicsSimulator physicsSimulator, Body body,
                                                       float springConstant, float dampingConstant)
        {
            FixedAngleSpring fixedAngleSpring = CreateFixedAngleSpring(body, springConstant, dampingConstant);
            physicsSimulator.Add(fixedAngleSpring);
            return fixedAngleSpring;
        }

        public FixedAngleSpring CreateFixedAngleSpring(Body body, float springConstant, float dampingConstant)
        {
            FixedAngleSpring fixedAngleSpring = new FixedAngleSpring(body, springConstant, dampingConstant);
            return fixedAngleSpring;
        }
        #endregion
    }
}