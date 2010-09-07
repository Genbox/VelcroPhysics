using System;
using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Factories
{
    /// <summary>
    /// An easy to use factory for creating bodies
    /// </summary>
    public static class FixtureFactory
    {
        public static Fixture CreateEdge(World world, Vector2 start, Vector2 end, float density)
        {
            Body body = BodyFactory.CreateBody(world);
            Vertices edgeVertices = PolygonTools.CreateEdge(start, end);
            PolygonShape rectangleShape = new PolygonShape(edgeVertices);
            return body.CreateFixture(rectangleShape, density);
        }

        public static Fixture CreateRectangle(World world, float width, float height, float density)
        {
            return CreateRectangle(world, width, height, density, Vector2.Zero);
        }

        /// <summary>
        /// Special overload that takes in an existing body.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="density">The density.</param>
        /// <param name="body">The body.</param>
        /// <param name="offset">The offset. The new shape is offset by this value</param>
        /// <returns></returns>
        public static Fixture CreateRectangle(float width, float height, float density, Body body, Vector2 offset)
        {
            Vertices rectangleVertices = PolygonTools.CreateRectangle(width / 2, height / 2);
            rectangleVertices.Translate(ref offset);
            PolygonShape rectangleShape = new PolygonShape(rectangleVertices);
            return body.CreateFixture(rectangleShape, density);
        }

        public static Fixture CreateRectangle(World world, float width, float height, float density, Vector2 position)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width", "Width must be more than 0 meters");

            if (height <= 0)
                throw new ArgumentOutOfRangeException("height", "Height must be more than 0 meters");

            Body newBody = BodyFactory.CreateBody(world, position);
            Vertices rectangleVertices = PolygonTools.CreateRectangle(width / 2, height / 2);
            PolygonShape rectangleShape = new PolygonShape(rectangleVertices);
            return newBody.CreateFixture(rectangleShape, density);
        }

        public static Fixture CreateCircle(World world, float radius, float density)
        {
            return CreateCircle(world, radius, density, Vector2.Zero);
        }

        public static Fixture CreateCircle(World world, float radius, float density, Vector2 offset, Body body = null)
        {
            if (radius <= 0)
                throw new ArgumentOutOfRangeException("radius", "Radius must be more than 0 meters");

            if (body != null)
            {
                CircleShape circleShape = new CircleShape(radius);
                circleShape.Position = offset;
                return body.CreateFixture(circleShape, density);
            }
            else
            {
                Body newBody = BodyFactory.CreateBody(world, offset);
                CircleShape circleShape = new CircleShape(radius);
                return newBody.CreateFixture(circleShape, density);
            }
        }

        public static Fixture CreateEllipse(World world, float xRadius, float yRadius, int edges, float density)
        {
            return CreateEllipse(world, xRadius, yRadius, edges, density, Vector2.Zero);
        }

        public static Fixture CreateEllipse(World world, float xRadius, float yRadius, int edges, float density,
                                            Vector2 position)
        {
            if (xRadius <= 0)
                throw new ArgumentOutOfRangeException("xRadius", "X-radius must be more than 0");

            if (yRadius <= 0)
                throw new ArgumentOutOfRangeException("yRadius", "Y-radius must be more than 0");

            Body body = BodyFactory.CreateBody(world, position);
            Vertices ellipseVertices = PolygonTools.CreateEllipse(xRadius, yRadius, edges);
            PolygonShape polygonShape = new PolygonShape(ellipseVertices);
            return body.CreateFixture(polygonShape, density);
        }

        public static Fixture CreatePolygon(World world, Vertices vertices, float density)
        {
            return CreatePolygon(world, vertices, density, Vector2.Zero);
        }

        public static Fixture CreatePolygon(World world, Vertices vertices, float density, Vector2 position)
        {
            Body body = BodyFactory.CreateBody(world, position);
            PolygonShape polygonShape = new PolygonShape(vertices);
            return body.CreateFixture(polygonShape, density);
        }

        public static Fixture CreatePolygon(Vertices vertices, float density, Body body, Vector2 offset)
        {
            vertices.Translate(ref offset);
            PolygonShape polygon = new PolygonShape(vertices);
            return body.CreateFixture(polygon, density);
        }

        public static List<Fixture> CreateCompoundPolygon(World world, List<Vertices> list, float density)
        {
            //We create a single body
            Body polygonBody = BodyFactory.CreateBody(world);

            List<Fixture> fixtures = new List<Fixture>(list.Count);

            //Then we create several fixtures using the body
            foreach (Vertices vertices in list)
            {
                PolygonShape shape = new PolygonShape(vertices);
                fixtures.Add(polygonBody.CreateFixture(shape, density));
            }

            return fixtures;
        }

        public static List<Fixture> CreateGear(World world, float radius, int numberOfTeeth, float tipPercentage,
                                               float toothHeight, float density)
        {
            Vertices gearPolygon = PolygonTools.CreateGear(radius, numberOfTeeth, tipPercentage, toothHeight);

            //Gears can in some cases be convex
            if (!gearPolygon.IsConvex())
            {
                //Decompose the gear:
                List<Vertices> list = EarclipDecomposer.ConvexPartition(gearPolygon);

                return CreateCompoundPolygon(world, list, density);
            }

            List<Fixture> fixtures = new List<Fixture>();
            fixtures.Add(CreatePolygon(world, gearPolygon, density));
            return fixtures;
        }

        /// <summary>
        /// Creates a capsule.
        /// Note: Automatically decomposes the capsule if it contains too many vertices (controlled by Settings.MaxPolygonVertices)
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="height">The height.</param>
        /// <param name="topRadius">The top radius.</param>
        /// <param name="topEdges">The top edges.</param>
        /// <param name="bottomRadius">The bottom radius.</param>
        /// <param name="bottomEdges">The bottom edges.</param>
        /// <param name="density">The density.</param>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public static List<Fixture> CreateCapsule(World world, float height, float topRadius, int topEdges,
                                                  float bottomRadius,
                                                  int bottomEdges, float density, Vector2 position)
        {
            Vertices verts = PolygonTools.CreateCapsule(height, topRadius, topEdges, bottomRadius, bottomEdges);

            //There are too many vertices in the capsule. We decompose it.
            if (verts.Count >= Settings.MaxPolygonVertices)
            {
                List<Vertices> vertList = EarclipDecomposer.ConvexPartition(verts);
                List<Fixture> fixtureList = CreateCompoundPolygon(world, vertList, density);
                fixtureList[0].Body.Position = position;

                return fixtureList;
            }

            return new List<Fixture> { CreatePolygon(world, verts, density) };
        }

        public static List<Fixture> CreateCapsule(World world, float height, float endRadius, float density)
        {
            //Create the middle rectangle
            Vertices rectangle = PolygonTools.CreateRectangle(endRadius, height / 2);

            List<Vertices> list = new List<Vertices>();
            list.Add(rectangle);

            List<Fixture> fixtures = CreateCompoundPolygon(world, list, density);

            //Create the two circles
            CircleShape topCircle = new CircleShape(endRadius);
            topCircle.Position = new Vector2(0, height / 2);
            fixtures.Add(fixtures[0].Body.CreateFixture(topCircle, density));

            CircleShape bottomCircle = new CircleShape(endRadius);
            bottomCircle.Position = new Vector2(0, -(height / 2));
            fixtures.Add(fixtures[0].Body.CreateFixture(bottomCircle, density));
            return fixtures;
        }

        /// <summary>
        /// Creates a rounded rectangle.
        /// Note: Automatically decomposes the capsule if it contains too many vertices (controlled by Settings.MaxPolygonVertices)
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="xRadius">The x radius.</param>
        /// <param name="yRadius">The y radius.</param>
        /// <param name="segments">The segments.</param>
        /// <param name="density">The density.</param>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public static List<Fixture> CreateRoundedRectangle(World world, float width, float height, float xRadius,
                                                           float yRadius,
                                                           int segments, float density, Vector2 position)
        {
            Vertices verts = PolygonTools.CreateRoundedRectangle(width, height, xRadius, yRadius, segments);

            //There are too many vertices in the capsule. We decompose it.
            if (verts.Count >= Settings.MaxPolygonVertices)
            {
                List<Vertices> vertList = EarclipDecomposer.ConvexPartition(verts);
                List<Fixture> fixtureList = CreateCompoundPolygon(world, vertList, density);
                fixtureList[0].Body.Position = position;
                return fixtureList;
            }

            return new List<Fixture> { CreatePolygon(world, verts, density) };
        }

        public static List<Fixture> CreateRoundedRectangle(World world, float width, float height, float xRadius,
                                                           float yRadius,
                                                           int segments, float density)
        {
            return CreateRoundedRectangle(world, width, height, xRadius, yRadius, segments, density, Vector2.Zero);
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

            BreakableBody breakableBody = new BreakableBody(triangles, world, density);
            breakableBody.MainBody.Position = position;
            world.AddBreakableBody(breakableBody);

            return breakableBody;
        }

        public static Fixture CreateLoopShape(World world, Vertices vertices, Vector2 position, float density)
        {
            Body body = BodyFactory.CreateBody(world, position);
            LoopShape shape = new LoopShape(vertices);
            return body.CreateFixture(shape, density);
        }

        public static Fixture CreateLoopShape(World world, Vertices vertices, float density)
        {
            return CreateLoopShape(world, vertices, Vector2.Zero, density);
        }
    }
}