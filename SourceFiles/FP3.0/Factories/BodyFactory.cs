using System;
using System.Collections.Generic;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Factories
{
    /// <summary>
    /// An easy to use factory for creating bodies
    /// </summary>
    public static class BodyFactory
    {
        public static Body CreateBody(World world)
        {
            Body body = new Body(world);
            world.Add(body);
            return body;
        }

        public static Body CreateBody(World world, Vector2 position)
        {
            Body body = new Body(world);
            body.Position = position;
            world.Add(body);
            return body;
        }
    }
}