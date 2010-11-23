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
        public static Fixture CreateEdge(World world, Vector2 start, Vector2 end)
        {
            Body body = BodyFactory.CreateBody(world);
            return CreateEdge(start, end, body);
        }

        public static Fixture CreateEdge(Vector2 start, Vector2 end, Body body)
        {
            EdgeShape edgeShape = new EdgeShape(start, end);
            return body.CreateFixture(edgeShape);
        }

        public static Fixture CreateLoopShape(World world, Vertices vertices, float density)
        {
            return CreateLoopShape(world, vertices, density, Vector2.Zero);
        }

        public static Fixture CreateLoopShape(World world, Vertices vertices, float density, Vector2 position)
        {
            Body body = BodyFactory.CreateBody(world, position);
            return CreateLoopShape(vertices, density, body);
        }

        public static Fixture CreateLoopShape(Vertices vertices, float density, Body body)
        {
            LoopShape shape = new LoopShape(vertices, density);
            return body.CreateFixture(shape);
        }

        public static Fixture CreateRectangle(World world, float width, float height, float density)
        {
            return CreateRectangle(world, width, height, density, Vector2.Zero);
        }

        public static Fixture CreateRectangle(World world, float width, float height, float density, Vector2 position)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width", "Width must be more than 0 meters");

            if (height <= 0)
                throw new ArgumentOutOfRangeException("height", "Height must be more than 0 meters");

            Body newBody = BodyFactory.CreateBody(world, position);
            Vertices rectangleVertices = PolygonTools.CreateRectangle(width / 2, height / 2);
            PolygonShape rectangleShape = new PolygonShape(rectangleVertices, density);
            return newBody.CreateFixture(rectangleShape);
        }

        /// <summary>
        /// Special overload that takes in an existing body.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="density">The density.</param>
        /// <param name="offset">The offset. The new shape is offset by this value</param>
        /// <param name="body">The body.</param>
        /// <returns></returns>
        public static Fixture CreateRectangle(float width, float height, float density, Vector2 offset, Body body)
        {
            Vertices rectangleVertices = PolygonTools.CreateRectangle(width / 2, height / 2);
            rectangleVertices.Translate(ref offset);
            PolygonShape rectangleShape = new PolygonShape(rectangleVertices, density);
            return body.CreateFixture(rectangleShape);
        }

        public static Fixture CreateCircle(World world, float radius, float density)
        {
            return CreateCircle(world, radius, density, Vector2.Zero);
        }

        public static Fixture CreateCircle(World world, float radius, float density, Vector2 position)
        {

            Body body = BodyFactory.CreateBody(world, position);
            return CreateCircle(radius, density, body);
        }

        public static Fixture CreateCircle(float radius, float density, Body body)
        {
            if (radius <= 0)
                throw new ArgumentOutOfRangeException("radius", "Radius must be more than 0 meters");

            CircleShape circleShape = new CircleShape(radius, density);
            return body.CreateFixture(circleShape);
        }

        public static Fixture CreateCircle(float radius, float density, Body body, Vector2 offset)
        {
            if (radius <= 0)
                throw new ArgumentOutOfRangeException("radius", "Radius must be more than 0 meters");

            CircleShape circleShape = new CircleShape(radius, density);
            circleShape.Position = offset;
            return body.CreateFixture(circleShape);
        }

        public static Fixture CreateEllipse(World world, float xRadius, float yRadius, int edges, float density)
        {
            return CreateEllipse(world, xRadius, yRadius, edges, density, Vector2.Zero);
        }

        public static Fixture CreateEllipse(World world, float xRadius, float yRadius, int edges, float density,
                                            Vector2 position)
        {
            Body body = BodyFactory.CreateBody(world, position);
            return CreateEllipse(xRadius, yRadius, edges, density, body);
        }

        public static Fixture CreateEllipse(float xRadius, float yRadius, int edges, float density, Body body)
        {
            if (xRadius <= 0)
                throw new ArgumentOutOfRangeException("xRadius", "X-radius must be more than 0");

            if (yRadius <= 0)
                throw new ArgumentOutOfRangeException("yRadius", "Y-radius must be more than 0");

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
            Body body = BodyFactory.CreateBody(world, position);
            return CreatePolygon(vertices, density, body);
        }

        public static Fixture CreatePolygon(Vertices vertices, float density, Body body)
        {
            if (vertices.Count <= 1)
                throw new ArgumentOutOfRangeException("vertices", "Too few points to be a polygon");

            PolygonShape polygon = new PolygonShape(vertices, density);
            return body.CreateFixture(polygon);
        }



        public static List<Fixture> CreateCompoundPolygon(World world, List<Vertices> list, float density)
        {
            return CreateCompoundPolygon(world, list, density, Vector2.Zero);
        }

        public static List<Fixture> CreateCompoundPolygon(World world, List<Vertices> list, float density, Vector2 position)
        {
            //We create a single body
            Body polygonBody = BodyFactory.CreateBody(world, position);
            return CreateCompoundPolygon(list, density, polygonBody);
        }

        public static List<Fixture> CreateCompoundPolygon(List<Vertices> list, float density, Body body)
        {
            List<Fixture> fixtures = new List<Fixture>(list.Count);

            //Then we create several fixtures using the body
            foreach (Vertices vertices in list)
            {
                PolygonShape shape = new PolygonShape(vertices, density);
                fixtures.Add(body.CreateFixture(shape));
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
            CircleShape topCircle = new CircleShape(endRadius, density);
            topCircle.Position = new Vector2(0, height / 2);
            fixtures.Add(fixtures[0].Body.CreateFixture(topCircle));

            CircleShape bottomCircle = new CircleShape(endRadius, density);
            bottomCircle.Position = new Vector2(0, -(height / 2));
            fixtures.Add(fixtures[0].Body.CreateFixture(bottomCircle));
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
    }
}