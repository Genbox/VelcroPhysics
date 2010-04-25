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

        public static void ConvertPathToEdges(World world, Path path, Body body, float density, int subdivisions)
        {
            List<Vector2> verts = path.GetVertices(subdivisions);

            for (int i = 1; i < verts.Count; i++)
            {
                PolygonShape shape = new PolygonShape(PolygonTools.CreateEdge(verts[i], verts[i - 1]), density);
                MassData data = shape.MassData;
                data.Mass = 1;
                data.Inertia = 1;
                shape.MassData = data;
                body.CreateFixture(shape);
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

    }
}