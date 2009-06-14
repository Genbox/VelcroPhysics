using System;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;

// If this is an XNA project then we use math from the XNA framework.
#if XNA
using Microsoft.Xna.Framework;
#endif

namespace FarseerPhysics.Factories
{
    /// <summary>
    /// An easy to use factory for creating bodies
    /// </summary>
    public class BodyFactory
    {
        private static BodyFactory _instance;

        private BodyFactory()
        {
        }

        public static BodyFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BodyFactory();
                }
                return _instance;
            }
        }

        public Body CreateBody(PhysicsSimulator physicsSimulator, Vector2 position, float angle)
        {
            BodyDef bodyDef = new BodyDef();

            bodyDef.Position = position;
            bodyDef.Angle = angle;

            return physicsSimulator.CreateBody(bodyDef);
        }
    }
}