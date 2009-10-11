using System.Collections.Generic;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Controllers;
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
        public float SpringRestLengthFactor { get; set; }

        private ComplexFactory()
        {
            SpringRestLengthFactor = 1f;
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
        /// <param name="type">The joint/spring type.</param>
        /// <returns>Path</returns>
        public Path CreateChain(PhysicsSimulator physicsSimulator, Vector2 start, Vector2 end, int links, float height,
                                float mass, LinkType type)
        {
            Path p = CreateChain(start, end, (Vector2.Distance(start, end) / links), height, mass, type);

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
        /// <param name="type">The joint/spring type.</param>
        /// <returns>Path</returns>
        public Path CreateChain(Vector2 start, Vector2 end, int links, float height, float mass, LinkType type)
        {
            return CreateChain(start, end, (Vector2.Distance(start, end) / links), height, mass, type);
        }

        /// <summary>
        /// Creates a chain from start to end points containing the specified number of links.
        /// </summary>
        /// <param name="physicsSimulator"><see cref="PhysicsSimulator"/> to add the chain too.</param>
        /// <param name="start">Starting point of the chain.</param>
        /// <param name="end">Ending point of the chain.</param>
        /// <param name="links">Number of links desired in the chain.</param>
        /// <param name="mass">Mass of each link.</param>
        /// <param name="type">The joint/spring type.</param>
        /// <returns>Path</returns>
        public Path CreateChain(PhysicsSimulator physicsSimulator, Vector2 start, Vector2 end, int links, float mass,
                                LinkType type)
        {
            Path path = CreateChain(start, end, (Vector2.Distance(start, end) / links),
                                    (Vector2.Distance(start, end) / links) * (1.0f / 3.0f), mass, type);

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
        /// <param name="type">The joint/spring type.</param>
        /// <returns></returns>
        public Path CreateChain(Vector2 start, Vector2 end, int links, float mass, LinkType type)
        {
            return CreateChain(start, end, (Vector2.Distance(start, end) / links),
                               (Vector2.Distance(start, end) / links) * (1.0f / 3.0f), mass, type);
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
        /// <param name="type">The joint/spring type.</param>
        /// <returns></returns>
        public Path CreateChain(PhysicsSimulator physicsSimulator, Vector2 start, Vector2 end, float width, float height,
                                float mass, LinkType type)
        {
            Path path = CreateChain(start, end, width, height, mass, false, false, type);

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
        /// <param name="type">The joint/spring type.</param>
        /// <returns></returns>
        public Path CreateChain(PhysicsSimulator physicsSimulator, Vector2 start, Vector2 end, float width, float height,
                                float mass, bool pinStart, bool pinEnd, LinkType type)
        {
            Path path = CreateChain(start, end, width, height, mass, pinStart, pinEnd, type);

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
        /// <param name="linkWidth">The distance between links.</param> 
        /// <param name="mass">The mass.</param>
        /// <param name="pinStart">if set to <c>true</c> [pin start].</param>
        /// <param name="pinEnd">if set to <c>true</c> [pin end].</param>
        /// <param name="type">The joint/spring type.</param>
        /// <returns></returns>
        public Path CreateChain(PhysicsSimulator physicsSimulator, Vector2 start, Vector2 end, float width, float height, float linkWidth,
                                float mass, bool pinStart, bool pinEnd, LinkType type)
        {
            Path path = CreateChain(start, end, width, height, linkWidth, mass, pinStart, pinEnd, type);

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
        /// <param name="type">The joint/spring type.</param>
        /// <returns></returns>
        public Path CreateChain(Vector2 start, Vector2 end, float width, float height, float mass, LinkType type)
        {
            return CreateChain(start, end, width, height, mass, false, false, type);
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
        /// <param name="type">The joint/spring type.</param>
        /// <returns></returns>
        public Path CreateChain(Vector2 start, Vector2 end, float width, float height, float mass, bool pinStart,
                                bool pinEnd, LinkType type)
        {
            return CreateChain(start, end, width, height, width, mass, pinStart, pinEnd, type);
        }

        /// <summary>
        /// Creates a chain.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="linkWidth">The distance between links.</param> 
        /// <param name="mass">The mass.</param>
        /// <param name="pinStart">if set to <c>true</c> [pin start].</param>
        /// <param name="pinEnd">if set to <c>true</c> [pin end].</param>
        /// <param name="type">The joint/spring type.</param>
        /// <returns></returns>
        public Path CreateChain(Vector2 start, Vector2 end, float width, float height, float linkWidth, float mass, bool pinStart,
                                bool pinEnd, LinkType type)
        {
            Path path = new Path(width, height, linkWidth, mass, false);
            path.Add(start);
            
            // add midpoint of line (must have this because my code needs at least 3 control points)
            path.Add(Vertices.FindMidpoint(start, end));
            
            path.Add(end);

            // call update to create all the bodies
            path.Update();
            
            // link bodies together
            path.LinkBodies(type, Min, Max, SpringConstant, DampingConstant, SpringRestLengthFactor);

            if (pinStart)
                path.Add(JointFactory.Instance.CreateFixedRevoluteJoint(path.Bodies[0], start));
            if (pinEnd)
                path.Add(JointFactory.Instance.CreateFixedRevoluteJoint(path.Bodies[path.Bodies.Count - 1],
                                                                        path.ControlPoints[2]));

            // chains need a little give
            foreach (Joint j in path.Joints)
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
            Path path = new Path(width, height, mass, endless);

            // add all the points to the path
            foreach (Vector2 v in points)
                path.Add(v);

            // create the bodies
            path.Update();

            for (int i = 0; i < path.Bodies.Count; i++)
            {
                Geom geom = GeomFactory.Instance.CreateRectangleGeom(path.Bodies[i], width, height);
                geom.CollisionGroup = collisionGroup;
                path.Add(geom);
            }

            // link bodies together
            path.LinkBodies(type, Min, Max, SpringConstant, DampingConstant);

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

        /// <summary>
        /// Creates a gravity controller and adds it to the physics simulator.
        /// </summary>
        /// <param name="simulator">the physicsSimulator used by this controller.</param>
        /// <param name="bodies">The bodies you want to generate gravity.</param>
        /// <param name="type">the type of gravity this uses.</param>
        /// <param name="strength">the maximum strength of gravity (the gravity strength when two bodies are on the same spot)</param>
        /// <param name="radius">the maximum distance that can be between 2 bodies before it will stop trying to apply gravity between them.</param>
        public GravityController CreateGravityController(PhysicsSimulator simulator, List<Body> bodies, GravityType type, float strength, float radius)
        {
            GravityController gravityController = new GravityController(simulator, bodies, strength, radius);
            gravityController.GravityType = type;
            simulator.Add(gravityController);
            return gravityController;
        }

        /// <summary>
        /// Creates a gravity controller and adds it to the physics simulator.
        /// </summary>
        /// <param name="simulator">the physicsSimulator used by this controller.</param>
        /// <param name="bodies">The bodies you want to generate gravity.</param>
        /// <param name="strength">the maximum strength of gravity (the gravity strength when two bodies are on the same spot)</param>
        /// <param name="radius">the maximum distance that can be between 2 bodies before it will stop trying to apply gravity between them.</param>
        /// <returns></returns>
        public GravityController CreateGravityController(PhysicsSimulator simulator, List<Body> bodies, float strength, float radius)
        {
            GravityController gravityController = new GravityController(simulator, bodies, strength, radius);
            simulator.Add(gravityController);
            return gravityController;
        }

        /// <summary>
        /// Creates a gravity controller and adds it to the physics simulator.
        /// </summary>
        /// <param name="simulator">the physicsSimulator used by this controller.</param>
        /// <param name="points">The points you want to generate gravity.</param>
        /// <param name="type">the type of gravity this uses.</param>
        /// <param name="strength">the maximum strength of gravity (the gravity strength when two bodies are on the same spot)</param>
        /// <param name="radius">the maximum distance that can be between 2 bodies before it will stop trying to apply gravity between them.</param>
        public GravityController CreateGravityController(PhysicsSimulator simulator, List<Vector2> points, GravityType type, float strength, float radius)
        {
            GravityController gravityController = new GravityController(simulator, points, strength, radius);
            gravityController.GravityType = type;
            simulator.Add(gravityController);
            return gravityController;
        }

        /// <summary>
        /// Creates a gravity controller and adds it to the physics simulator.
        /// </summary>
        /// <param name="simulator">the physicsSimulator used by this controller.</param>
        /// <param name="points">The points you want to generate gravity.</param>
        /// <param name="strength">the maximum strength of gravity (the gravity strength when two bodies are on the same spot)</param>
        /// <param name="radius">the maximum distance that can be between 2 bodies before it will stop trying to apply gravity between them.</param>
        /// <returns></returns>
        public GravityController CreateGravityController(PhysicsSimulator simulator, List<Vector2> points, float strength, float radius)
        {
            GravityController gravityController = new GravityController(simulator, points, strength, radius);
            simulator.Add(gravityController);
            return gravityController;
        }
    }
}
