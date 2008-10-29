using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
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
        // 1. Done
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

        /// <summary>
        /// Creates a chain from start to end points containing the specified number of links.
        /// </summary>
        /// <param name="ps">PhysicsSimulator to add the chain too.</param>
        /// <param name="start">Starting point of the chain.</param>
        /// <param name="end">Ending point of the chain.</param>
        /// <param name="links">Number of links desired in the chain.</param>
        /// <param name="height">Height of each link.</param>
        /// <param name="mass">Mass of each link.</param>
        /// <param name="group">Collision group for the chain.</param>
        /// <returns>Path</returns>
        public Path CreateChain(PhysicsSimulator ps, Vector2 start, Vector2 end, int links, float height, float mass, int group)
        {
            Path p = CreateChain(start, end, (Vector2.Distance(start, end) / (float)links), height, mass, group);

            p.AddToPhysicsSimulator(ps);

            return p;
        }

        /// <summary>
        /// Creates a chain from start to end points containing the specified number of links.
        /// </summary>
        /// <param name="start">Starting point of the chain.</param>
        /// <param name="end">Ending point of the chain.</param>
        /// <param name="links">Number of links desired in the chain.</param>
        /// <param name="height">Height of each link.</param>
        /// <param name="mass">Mass of each link.</param>
        /// <param name="group">Collision group for the chain.</param>
        /// <returns>Path</returns>
        public Path CreateChain(Vector2 start, Vector2 end, int links, float height, float mass, int group)
        {
            Path p = CreateChain(start, end, (Vector2.Distance(start, end) / (float)links), height, mass, group);

            return p;
        }

        /// <summary>
        /// Creates a chain from start to end points containing the specified number of links.
        /// </summary>
        /// <param name="ps">PhysicsSimulator to add the chain too.</param>
        /// <param name="start">Starting point of the chain.</param>
        /// <param name="end">Ending point of the chain.</param>
        /// <param name="links">Number of links desired in the chain.</param>
        /// <param name="mass">Mass of each link.</param>
        /// <param name="group">Collision group for the chain.</param>
        /// <returns>Path</returns>
        public Path CreateChain(PhysicsSimulator ps, Vector2 start, Vector2 end, int links, float mass, int group)
        {
            Path p = CreateChain(start, end, (Vector2.Distance(start, end) / (float)links), (Vector2.Distance(start, end) / (float)links)*(1.0f/3.0f), mass, group);

            p.AddToPhysicsSimulator(ps);

            return p;
        }

        public Path CreateChain(Vector2 start, Vector2 end, int links, float mass, int group)
        {
            Path p = CreateChain(start, end, (Vector2.Distance(start, end) / (float)links), (Vector2.Distance(start, end) / (float)links)*(1.0f/3.0f), mass, group);

            return p;
        }

        public Path CreateChain(PhysicsSimulator ps, Vector2 start, Vector2 end, float width, float height, float mass, int group)
        {
            Path p = CreateChain(start, end, width, height, mass, false, false, group);

            p.AddToPhysicsSimulator(ps);

            return p;
        }

        public Path CreateChain(PhysicsSimulator ps, Vector2 start, Vector2 end, float width, float height, float mass, bool pinStart, bool pinEnd, int group)
        {
            Path p = CreateChain(start, end, width, height, mass, pinStart, pinEnd, group);

            p.AddToPhysicsSimulator(ps);

            return p;
        }

        public Path CreateChain(Vector2 start, Vector2 end, float width, float height, float mass, int group)
        {
            Path p = CreateChain(start, end, width, height, mass, false, false, group);

            return p;
        }

        public Path CreateChain(Vector2 start, Vector2 end, float width, float height, float mass, bool pinStart, bool pinEnd, int group)
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
                g.collisionGroup = group;
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