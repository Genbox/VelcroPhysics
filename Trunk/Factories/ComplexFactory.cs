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
        private static ComplexFactory _instance;

        public float Min { get; set; }
        public float Max { get; set; }
        public float SpringConstant { get; set; }
        public float DampingConstant { get; set; }

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
        /// <param name="physicsSimulator"><see cref="PhysicsSimulator"/> to add the chain to.</param>
        /// <param name="start">Starting point of the chain.</param>
        /// <param name="end">Ending point of the chain.</param>
        /// <param name="links">Number of links desired in the chain.</param>
        /// <param name="height">Height of each link.</param>
        /// <param name="mass">Mass of each link.</param>
        /// <param name="collisionGroup">Collision group for the chain.</param>
        /// <param name="type">The joint/spring type.</param>
        /// <returns>Path</returns>
        public Path CreateChain(PhysicsSimulator physicsSimulator, Vector2 start, Vector2 end, int links, float height,
                                float mass, int collisionGroup, LinkType type)
        {
            Path p = CreateChain(start, end, (Vector2.Distance(start, end) / links), height, mass, collisionGroup, type);

            p.AddToPhysicsSimulator(physicsSimulator);

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
        /// <param name="collisionGroup">Collision group for the chain.</param>   
        ///      /// <param name="type">The joint/spring type.</param>

        /// <returns>Path</returns>
        public Path CreateChain(Vector2 start, Vector2 end, int links, float height, float mass, int collisionGroup, LinkType type)
        {
            return CreateChain(start, end, (Vector2.Distance(start, end) / links), height, mass, collisionGroup, type);
        }

        /// <summary>
        /// Creates a chain from start to end points containing the specified number of links.
        /// </summary>
        /// <param name="physicsSimulator"><see cref="PhysicsSimulator"/> to add the chain too.</param>
        /// <param name="start">Starting point of the chain.</param>
        /// <param name="end">Ending point of the chain.</param>
        /// <param name="links">Number of links desired in the chain.</param>
        /// <param name="mass">Mass of each link.</param>
        /// <param name="collisionGroup">Collision group for the chain.</param>
        /// <param name="type">The joint/spring type.</param>
        /// <returns>Path</returns>
        public Path CreateChain(PhysicsSimulator physicsSimulator, Vector2 start, Vector2 end, int links, float mass,
                                int collisionGroup, LinkType type)
        {
            Path path = CreateChain(start, end, (Vector2.Distance(start, end) / links),
                                    (Vector2.Distance(start, end) / links) * (1.0f / 3.0f), mass, collisionGroup, type);

            path.AddToPhysicsSimulator(physicsSimulator);

            return path;
        }

        /// <summary>
        /// Creates a chain.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="links">The links.</param>
        /// <param name="mass">The mass.</param>
        /// <param name="collisionGroup">Collision group for the chain.</param>
        /// <param name="type">The joint/spring type.</param>
        /// <returns></returns>
        public Path CreateChain(Vector2 start, Vector2 end, int links, float mass, int collisionGroup, LinkType type)
        {
            return CreateChain(start, end, (Vector2.Distance(start, end) / links),
                               (Vector2.Distance(start, end) / links) * (1.0f / 3.0f), mass, collisionGroup, type);
        }

        /// <summary>
        /// Creates a chain.
        /// </summary>
        /// <param name="physicsSimulator"><see cref="PhysicsSimulator"/> to add the chain to.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="mass">The mass.</param>
        /// <param name="collisionGroup">Collision group for the chain.</param>
        /// <param name="type">The joint/spring type.</param>
        /// <returns></returns>
        public Path CreateChain(PhysicsSimulator physicsSimulator, Vector2 start, Vector2 end, float width, float height,
                                float mass, int collisionGroup, LinkType type)
        {
            Path path = CreateChain(start, end, width, height, mass, false, false, collisionGroup, type);

            path.AddToPhysicsSimulator(physicsSimulator);

            return path;
        }

        /// <summary>
        /// Creates a chain.
        /// </summary>
        /// <param name="physicsSimulator"><see cref="PhysicsSimulator"/> to add the chain to.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="mass">The mass.</param>
        /// <param name="pinStart">if set to <c>true</c> [pin start].</param>
        /// <param name="pinEnd">if set to <c>true</c> [pin end].</param>
        /// <param name="collisionGroup">Collision group for the chain.</param>
        /// <param name="type">The joint/spring type.</param>
        /// <returns></returns>
        public Path CreateChain(PhysicsSimulator physicsSimulator, Vector2 start, Vector2 end, float width, float height,
                                float mass, bool pinStart, bool pinEnd, int collisionGroup, LinkType type)
        {
            Path path = CreateChain(start, end, width, height, mass, pinStart, pinEnd, collisionGroup, type);

            path.AddToPhysicsSimulator(physicsSimulator);

            return path;
        }

        /// <summary>
        /// Creates a chain.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="mass">The mass.</param>
        /// <param name="collisionGroup">Collision group for the chain.</param>
        /// <param name="type">The joint/spring type.</param>
        /// <returns></returns>
        public Path CreateChain(Vector2 start, Vector2 end, float width, float height, float mass, int collisionGroup, LinkType type)
        {
            return CreateChain(start, end, width, height, mass, false, false, collisionGroup, type);
        }

        /// <summary>
        /// Creates a chain.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="mass">The mass.</param>
        /// <param name="pinStart">if set to <c>true</c> [pin start].</param>
        /// <param name="pinEnd">if set to <c>true</c> [pin end].</param>
        /// <param name="collisionGroup">Collision group for the chain.</param>
        /// <param name="type">The joint/spring type.</param>
        /// <returns></returns>
        public Path CreateChain(Vector2 start, Vector2 end, float width, float height, float mass, bool pinStart,
                                bool pinEnd, int collisionGroup, LinkType type)
        {
            Path path = new Path(width, height, mass, false); // create the path
            path.Add(start); // add starting point
            path.Add(Path.FindMidpoint(start, end));
            // add midpoint of line (must have this because my code needs at least 3 control points)
            path.Add(end); // add end point

            path.Update(); // call update to create all the bodies

            Geom geom;
            for (int i = 0; i < path.Bodies.Count; i++)
            {
                geom = GeomFactory.Instance.CreateRectangleGeom(path.Bodies[i], width, height);
                geom.collisionGroup = collisionGroup;
                path.Add(geom); // add a geom to the chain
            }
            path.LinkBodies(type, Min, Max, SpringConstant, DampingConstant); // link bodies together

            if (pinStart)
                path.Add(JointFactory.Instance.CreateFixedRevoluteJoint(path.Bodies[0], start));
            if (pinEnd)
                path.Add(JointFactory.Instance.CreateFixedRevoluteJoint(path.Bodies[path.Bodies.Count - 1],
                                                                        path.ControlPoints[2]));

            foreach (Joint j in path.Joints)      // chains need a little give ;)
            {
                j.BiasFactor = 0.01f;
                j.Softness = 0.05f;
            }

            return (path);
        }

        /// <summary>
        /// Creates a rope.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="mass">The mass.</param>
        /// <param name="pinStart">if set to <c>true</c> [pin start].</param>
        /// <param name="pinEnd">if set to <c>true</c> [pin end].</param>
        /// <param name="collisionGroup">Collision group for the chain.</param>
        /// <param name="type">The joint/spring type.</param>
        /// <returns></returns>
        public Path CreateRope(Vector2 start, Vector2 end, float width, float height, float mass, bool pinStart,
                               bool pinEnd, int collisionGroup, LinkType type)
        {
            Path path = new Path(width, height, mass, false); // create the path
            path.Add(start); // add starting point
            path.Add(Path.FindMidpoint(start, end));
            // add midpoint of line (must have this because my code needs at least 3 control points)
            path.Add(end); // add end point

            path.Update(); // call update to create all the bodies

            Geom geom;
            for (int i = 0; i < path.Bodies.Count; i++)
            {
                geom = GeomFactory.Instance.CreateRectangleGeom(path.Bodies[i], width, height);
                geom.collisionGroup = collisionGroup;
                path.Add(geom); // add a geom to the chain
            }
            path.LinkBodies(type, Min, Max, SpringConstant, DampingConstant); // link bodies together

            if (pinStart)
                path.Add(JointFactory.Instance.CreateFixedRevoluteJoint(path.Bodies[0], start));
            if (pinEnd)
                path.Add(JointFactory.Instance.CreateFixedRevoluteJoint(path.Bodies[path.Bodies.Count - 1],
                                                                        path.ControlPoints[2]));

            foreach (Joint j in path.Joints)      // ropes need a little give ;)
            {
                j.BiasFactor = 0.01f;
                j.Softness = 0.05f;
            }

            return (path);
        }

        /// <summary>
        /// Creates a track.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="mass">The mass.</param>
        /// <param name="endless">if set to <c>true</c> [endless].</param>
        /// <param name="collisionGroup">Collision group for the chain.</param>
        /// <param name="type">The joint/spring type.</param>
        /// <returns></returns>
        public Path CreateTrack(Vertices points, float width, float height, float mass, bool endless, int collisionGroup, LinkType type)
        {
            Path path = new Path(width, height, mass, endless); // create the path

            foreach (Vector2 v in points)
                path.Add(v); // add all the points to the path

            path.Update(); // update the path

            Geom geom;
            for (int i = 0; i < path.Bodies.Count; i++)
            {
                geom = GeomFactory.Instance.CreateRectangleGeom(path.Bodies[i], width, height);
                geom.collisionGroup = collisionGroup;
                path.Add(geom); // add a geom to the chain
            }
            path.LinkBodies(type, Min, Max, SpringConstant, DampingConstant); // link bodies together

            return path;
        }

        /// <summary>
        /// Creates a track.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="points">The points.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="mass">The mass.</param>
        /// <param name="endless">if set to <c>true</c> [endless].</param>
        /// <param name="collisionGroup">Collision group for the chain.</param>
        /// <param name="type">The joint/spring type.</param>
        /// <returns></returns>
        public Path CreateTrack(PhysicsSimulator physicsSimulator, Vertices points, float width, float height,
                                float mass, bool endless, int collisionGroup, LinkType type)
        {
            Path path = CreateTrack(points, width, height, mass, endless, collisionGroup, type);

            path.AddToPhysicsSimulator(physicsSimulator);

            return path;
        }
    }
}
