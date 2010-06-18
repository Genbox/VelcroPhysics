using System;
using System.Collections.Generic;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Factories
{
    /// <summary>
    /// An easy to use factory for using paths.
    /// </summary>
    public static class PathFactory
    {
        /// <summary>
        /// Convert a path into a set of edges.
        /// Note: use only for static edges.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="path"></param>
        /// <param name="body"></param>
        /// <param name="subdivisions"></param>
        public static void ConvertPathToEdges(World world, Path path, Body body, int subdivisions)
        {
            List<Vector2> verts = path.GetVertices(subdivisions);

            for (int i = 1; i < verts.Count; i++)
            {
                body.CreateFixture(new PolygonShape(PolygonTools.CreateEdge(verts[i], verts[i - 1]), 0));
            }

            if (path.Closed)
            {
                body.CreateFixture(new PolygonShape(PolygonTools.CreateEdge(verts[verts.Count - 1], verts[0]), 0));
            }
        }

        /// <summary>
        /// Convert a closed path into a polygon.
        /// Convex decomposition is automatically performed.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="path"></param>
        /// <param name="body"></param>
        /// <param name="density"></param>
        /// <param name="subdivisions"></param>
        public static void ConvertPathToPolygon(World world, Path path, Body body, float density, int subdivisions)
        {
            if (!path.Closed)
                throw new Exception("The path must be closed to convert to a polygon.");
            
            List<Vector2> verts = path.GetVertices(subdivisions);

            List<Vertices> decomposedVerts = EarclipDecomposer.ConvexPartition(new Vertices(verts));
            //List<Vertices> decomposedVerts = BayazitDecomposer.ConvexPartition(new Vertices(verts));

            foreach (var item in decomposedVerts)
            {
                body.CreateFixture(new PolygonShape(item, density));
            }
        }
        
        /// <summary>
        /// Duplicates the given Body along the given path for approximatly the given copies.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="path"></param>
        /// <param name="body"></param>
        /// <param name="copies"></param>
        public static List<Body> EvenlyDistibuteShapesAlongPath(World world, Path path, Body body, int copies)
        {
            List<Vector3> centers = path.SubdivideEvenly(copies);
            List<Body> bodyList = new List<Body>();

            Body b;
            int firstIndex = world.BodyList.Count;

            for (int i = 0; i < centers.Count; i++)
            {
                b = new Body(world);
                // copy the type from original body
                b.BodyType = body.BodyType;
                b.Position = new Vector2(centers[i].X, centers[i].Y);
                b.Rotation = centers[i].Z;

                foreach (var fixture in body.FixtureList)
                {
                    b.CreateFixture(fixture.Shape);
                }

                world.Add(b);

                bodyList.Add(b);
            }

            return bodyList;
        }

        public static void MoveBodyOnPath(Path path, Body body, float time, float strength, float timeStep)
        {
            Vector2 destination = path.GetPosition(time);

            Vector2 positionDelta = body.Position - destination;

            Vector2 velocity = (positionDelta / timeStep) * strength;

            body.LinearVelocity = -velocity;
        }

        public static void AttachBodiesWithRevoluteJoint(World world, List<Body> bodies, Vector2 localAnchorA, Vector2 localAnchorB, bool connectFirstAndLast, bool collideConnected)
        {
            for (int i = 1; i < bodies.Count; i++)
            {
                RevoluteJoint joint = new RevoluteJoint(bodies[i], bodies[i - 1], new Vector2());
                joint.LocalAnchorA = localAnchorA;
                joint.LocalAnchorB = localAnchorB;
                joint.CollideConnected = collideConnected;
                world.Add(joint);
            }

            if (connectFirstAndLast)
            {
                RevoluteJoint lastjoint = new RevoluteJoint(bodies[0], bodies[bodies.Count - 1], new Vector2());
                lastjoint.LocalAnchorA = localAnchorA;
                lastjoint.LocalAnchorB = localAnchorB;
                lastjoint.CollideConnected = collideConnected;
                world.Add(lastjoint);
            }
        }

    }
}