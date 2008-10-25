using System;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Factories
{
    /// <summary>
    /// An easy to use factory for creating complex structures
    /// </summary>
    public class ComplexFactory
    {
        //TODO: list
        // 1. Create chain object so that people can change the properties of the chain.
        // 2. Remove ball at end of chain

        private static ComplexFactory _instance;

        private ComplexFactory()
        {
        }

        public static ComplexFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ComplexFactory();
                }
                return _instance;
            }
        }

        public Path CreateChain(PhysicsSimulator ps, Vector2 start, Vector2 end, float width, float height, float mass)
        {
            Path p = CreateChain(start, end, width, height, mass, false, false);

            p.AddToPhysicsSimulator(ps);

            return p;
        }

        public Path CreateChain(PhysicsSimulator ps, Vector2 start, Vector2 end, float width, float height, float mass, bool pinStart, bool pinEnd)
        {
            Path p = CreateChain(start, end, width, height, mass, pinStart, pinEnd);

            p.AddToPhysicsSimulator(ps);

            return p;
        }

        public Path CreateChain(Vector2 start, Vector2 end, float width, float height, float mass)
        {
            Path p;

            p = CreateChain(start, end, width, height, mass, false, false);

            return p;
        }

        public Path CreateChain(Vector2 start, Vector2 end, float width, float height, float mass, bool pinStart, bool pinEnd)
        {
            bool flip = true;
            Path p = new Path(width, height, mass, false);  // create the path
            p.Add(start);                                   // add starting point
            p.Add(Path.FindMidpoint(start, end));           // add midpoint of line (must have this because my code needs at least 3 control points)
            p.Add(end);                                     // add end point

            p.Update();                                     // call update to create all the bodies

            Geom g;
            for (int i = 0; i < (p.Bodies.Count - 1); i++)
            {
                if (flip)
                {
                    g = GeomFactory.Instance.CreateRectangleGeom(p.Bodies[i], width, height);
                    flip = !flip;
                }
                else
                {
                    g = GeomFactory.Instance.CreateRectangleGeom(p.Bodies[i], width, height * (1.0f / 3.0f));
                    flip = !flip;
                }
                g.CollisionCategories = CollisionCategory.Cat2;     // currently seting up the collision so chain will not collide with itself
                g.CollidesWith = CollisionCategory.Cat1;
                p.Add(g);                                           // add a geom to the chain
            }
            p.LinkBodies();         // link bodies together with revolute joints

            if (pinStart)
                p.Add(JointFactory.Instance.CreateFixedRevoluteJoint(p.Bodies[0], start));
            if (pinEnd)
                p.Add(JointFactory.Instance.CreateFixedRevoluteJoint(p.Bodies[p.Bodies.Count - 1], p.ControlPoints[2]));

            return (p);
        }
    }
}