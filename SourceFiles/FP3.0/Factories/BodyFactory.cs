﻿using FarseerPhysics.Dynamics;

#if XNA
using Microsoft.Xna.Framework;
#else
using FarseerPhysics.Math;
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