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
    public static class FixtureFactory
    {
        public static Fixture CreateEdge(World world, Vector2 start, Vector2 end)
        {
            Body body = CreateBody(world);
            Vertices edgeVertices = PolygonTools.CreateEdge(start, end);
            PolygonShape rectangleShape = new PolygonShape(edgeVertices, 0);
            return body.CreateFixture(rectangleShape);
        }

        public static Fixture CreateRectangle(World world, float width, float height, float density)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width", "Width must be more than 0");

            if (height <= 0)
                throw new ArgumentOutOfRangeException("height", "Height must be more than 0");

            if (density <= 0)
                throw new ArgumentOutOfRangeException("density", "Density must be more than 0");

            Body body = CreateBody(world);
            Vertices rectangleVertices = PolygonTools.CreateRectangle(height / 2, width / 2);
            PolygonShape rectangleShape = new PolygonShape(rectangleVertices, density);
            return body.CreateFixture(rectangleShape);
        }

        public static Fixture CreateCircle(World world, float radius, float density)
        {
            if (radius <= 0)
                throw new ArgumentOutOfRangeException("radius", "Radius must be more than 0");

            if (density <= 0)
                throw new ArgumentOutOfRangeException("density", "Density must be more than 0");

            Body body = CreateBody(world);
            CircleShape circleShape = new CircleShape(radius, density);
            return body.CreateFixture(circleShape);
        }

        public static Fixture CreateEllipse(World world, float xRadius, float yRadius, int edges, float density)
        {
            if (xRadius <= 0)
                throw new ArgumentOutOfRangeException("xRadius", "X-radius must be more than 0");

            if (yRadius <= 0)
                throw new ArgumentOutOfRangeException("yRadius", "Y-radius must be more than 0");

            if (density <= 0)
                throw new ArgumentOutOfRangeException("density", "Density must be more than 0");

            Body body = CreateBody(world);
            Vertices ellipseVertices = PolygonTools.CreateEllipse(xRadius, yRadius, edges);
            PolygonShape polygonShape = new PolygonShape(ellipseVertices, density);
            return body.CreateFixture(polygonShape);
        }

        public static Fixture CreatePolygon(World world, Vertices vertices, float density)
        {
            if (density <= 0)
                throw new ArgumentOutOfRangeException("density", "Density must be more than 0");

            Body body = CreateBody(world);
            PolygonShape polygonShape = new PolygonShape(vertices, density);
            return body.CreateFixture(polygonShape);
        }

        public static Body CreateBody(World world)
        {
            Body body = new Body(world);
            world.Add(body);
            return body;
        }

        public static BreakableBody CreateBreakableBody(World world, Vertices vertices, float density)
        {
            vertices = BooleanTools.Simplify(vertices);
            List<Vertices> triangles = EarclipDecomposer.ConvexPartition(vertices);

            BreakableBody breakableBody = new BreakableBody(world);

            foreach (Vertices triangle in triangles)
            {
                PolygonShape polygonShape = new PolygonShape(triangle, density);
                Fixture f = breakableBody.MainBody.CreateFixture(polygonShape);
                breakableBody.AddPart(f);
            }

            world.Add(breakableBody);

            return breakableBody;
        }
    }
}