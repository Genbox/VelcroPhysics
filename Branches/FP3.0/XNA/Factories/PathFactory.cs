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
        /// Duplicates the given Body along the given path for approximatly the given copies.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="path"></param>
        /// <param name="body"></param>
        /// <param name="copies"></param>
        public static void EvenlyDistibuteShapesAlongPath(World world, Path path, Body body, int copies)
        {
            List<Vector3> centers = path.SubdivideEvenly(copies);

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
            }
        }

    }
}