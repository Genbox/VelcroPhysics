using System;
using System.Collections.Generic;
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
            Body body = BodyFactory.CreateBody(world);
            Vertices edgeVertices = PolygonTools.CreateEdge(start, end);
            PolygonShape rectangleShape = new PolygonShape(edgeVertices, 0);
            return body.CreateFixture(rectangleShape);
        }

        public static Fixture CreateRectangle(World world, float width, float height, float density)
        {
            return CreateRectangle(world, width, height, density, Vector2.Zero);
        }

        public static Fixture CreateRectangle(World world, float width, float height, float density, Vector2 position)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width", "Width must be more than 0");

            if (height <= 0)
                throw new ArgumentOutOfRangeException("height", "Height must be more than 0");

            if (density <= 0)
                throw new ArgumentOutOfRangeException("density", "Density must be more than 0");

            Body body = BodyFactory.CreateBody(world, position);
            Vertices rectangleVertices = PolygonTools.CreateRectangle(width / 2, height / 2);
            PolygonShape rectangleShape = new PolygonShape(rectangleVertices, density);
            return body.CreateFixture(rectangleShape);
        }

        public static Fixture CreateCircle(World world, float radius, float density)
        {
            return CreateCircle(world, radius, density, Vector2.Zero);
        }

        public static Fixture CreateCircle(World world, float radius, float density, Vector2 position)
        {
            if (radius <= 0)
                throw new ArgumentOutOfRangeException("radius", "Radius must be more than 0");

            if (density <= 0)
                throw new ArgumentOutOfRangeException("density", "Density must be more than 0");

            Body body = BodyFactory.CreateBody(world, position);
            CircleShape circleShape = new CircleShape(radius, density);
            return body.CreateFixture(circleShape);
        }

        public static Fixture CreateEllipse(World world, float xRadius, float yRadius, int edges, float density)
        {
            return CreateEllipse(world, xRadius, yRadius, edges, density, Vector2.Zero);
        }

        public static Fixture CreateEllipse(World world, float xRadius, float yRadius, int edges, float density, Vector2 position)
        {
            if (xRadius <= 0)
                throw new ArgumentOutOfRangeException("xRadius", "X-radius must be more than 0");

            if (yRadius <= 0)
                throw new ArgumentOutOfRangeException("yRadius", "Y-radius must be more than 0");

            if (density <= 0)
                throw new ArgumentOutOfRangeException("density", "Density must be more than 0");

            Body body = BodyFactory.CreateBody(world, position);
            Vertices ellipseVertices = PolygonTools.CreateEllipse(xRadius, yRadius, edges);
            PolygonShape polygonShape = new PolygonShape(ellipseVertices, density);
            return body.CreateFixture(polygonShape);
        }

        public static Fixture CreatePolygon(World world, Vertices vertices, float density)
        {
            return CreatePolygon(world, vertices, density, Vector2.Zero);
        }

        public static Fixture CreatePolygon(World world, Vertices vertices, float density, Vector2 position)
        {
            if (density <= 0)
                throw new ArgumentOutOfRangeException("density", "Density must be more than 0");

            Body body = BodyFactory.CreateBody(world, position);
            PolygonShape polygonShape = new PolygonShape(vertices, density);
            return body.CreateFixture(polygonShape);
        }

        public static List<Fixture> CreateCompundPolygon(World world, List<Vertices> list, float density)
        {
            //We create a single body
            Body polygonBody = BodyFactory.CreateBody(world);
            polygonBody.BodyType = BodyType.Dynamic;

            List<Fixture> fixtures = new List<Fixture>(list.Count);

            //Then we create several fixtures using the body
            foreach (Vertices vertices in list)
            {
                PolygonShape shape = new PolygonShape(vertices, density);
                fixtures.Add(polygonBody.CreateFixture(shape));
            }

            return fixtures;
        }

        public static List<Fixture> CreateGear(World world, float radius, int numberOfTeeth, float tipPercentage, float toothHeight, float density)
        {
            Vertices gearPolygon = PolygonTools.CreateGear(radius, numberOfTeeth, tipPercentage, toothHeight);

            //Gears can in some cases be convex
            if (!gearPolygon.IsConvex())
            {
                //Decompose the gear:
                List<Vertices> list = EarclipDecomposer.ConvexPartition(gearPolygon);

                return CreateCompundPolygon(world, list, density);
            }

            List<Fixture> fixtures = new List<Fixture>();
            fixtures.Add(CreatePolygon(world, gearPolygon, density));
            return fixtures;
        }

        public static List<Fixture> CreateCapsule(World world, float height, float endRadius, float density)
        {
            //Create the middle rectangle
            Vertices leftEdge = PolygonTools.CreateEdge(new Vector2(-endRadius, height / 2), new Vector2(-endRadius, -(height / 2)));
            Vertices rightEdge = PolygonTools.CreateEdge(new Vector2(endRadius, height / 2), new Vector2(endRadius, -(height / 2)));

            List<Vertices> list = new List<Vertices>();
            list.Add(leftEdge);
            list.Add(rightEdge);

            List<Fixture> fixtures = CreateCompundPolygon(world, list, density);

            //Create the two circles
            CircleShape topCircle = new CircleShape(endRadius, density);
            topCircle.Position = new Vector2(0, height / 2);
            fixtures.Add(fixtures[0].Body.CreateFixture(topCircle));

            CircleShape bottomCircle = new CircleShape(endRadius, density);
            bottomCircle.Position = new Vector2(0, -(height / 2));
            fixtures.Add(fixtures[0].Body.CreateFixture(bottomCircle));
            return fixtures;
        }

        public static BreakableBody CreateBreakableBody(World world, Vertices vertices, float density)
        {
            return CreateBreakableBody(world, vertices, density, Vector2.Zero);
        }

        /// <summary>
        /// Creates a breakable body. You would want to remove collinear points before using this.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="vertices">The vertices.</param>
        /// <param name="density">The density.</param>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public static BreakableBody CreateBreakableBody(World world, Vertices vertices, float density, Vector2 position)
        {
            List<Vertices> triangles = EarclipDecomposer.ConvexPartition(vertices);

            BreakableBody breakableBody = new BreakableBody(world);

            foreach (Vertices triangle in triangles)
            {
                PolygonShape polygonShape = new PolygonShape(triangle, density);
                Fixture f = breakableBody.MainBody.CreateFixture(polygonShape);
                breakableBody.AddPart(f);
            }

            breakableBody.MainBody.Position = position;
            world.Add(breakableBody);

            return breakableBody;
        }
    }
}