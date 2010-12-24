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
            return CreateEdge(world, start, end, null);
        }

        public static Fixture CreateEdge(World world, Vector2 start, Vector2 end, Object userData)
        {
            Body body = BodyFactory.CreateBody(world);
            return CreateEdge(start, end, body, userData);
        }

        public static Fixture CreateEdge(Vector2 start, Vector2 end, Body body)
        {
            return CreateEdge(start, end, body, null);
        }

        public static Fixture CreateEdge(Vector2 start, Vector2 end, Body body, Object userData)
        {
            EdgeShape edgeShape = new EdgeShape(start, end);
            return body.CreateFixture(edgeShape, userData);
        }

        public static Fixture CreateLoopShape(World world, Vertices vertices, float density)
        {
            return CreateLoopShape(world, vertices, density, null);
        }

        public static Fixture CreateLoopShape(World world, Vertices vertices, float density, Object userData)
        {
            return CreateLoopShape(world, vertices, density, Vector2.Zero, userData);
        }

        public static Fixture CreateLoopShape(World world, Vertices vertices, float density, Vector2 position)
        {
            return CreateLoopShape(world, vertices, density, position, null);
        }

        public static Fixture CreateLoopShape(World world, Vertices vertices, float density, Vector2 position,
                                              Object userData)
        {
            Body body = BodyFactory.CreateBody(world, position);
            return CreateLoopShape(vertices, density, body, userData);
        }

        public static Fixture CreateLoopShape(Vertices vertices, float density, Body body)
        {
            return CreateLoopShape(vertices, density, body, null);
        }

        public static Fixture CreateLoopShape(Vertices vertices, float density, Body body, Object userData)
        {
            LoopShape shape = new LoopShape(vertices, density);
            return body.CreateFixture(shape, userData);
        }

        public static Fixture CreateRectangle(World world, float width, float height, float density)
        {
            return CreateRectangle(world, width, height, density, null);
        }

        public static Fixture CreateRectangle(World world, float width, float height, float density, Object userData)
        {
            return CreateRectangle(world, width, height, density, Vector2.Zero, userData);
        }

        public static Fixture CreateRectangle(World world, float width, float height, float density, Vector2 position)
        {
            return CreateRectangle(world, width, height, density, position, null);
        }

        public static Fixture CreateRectangle(World world, float width, float height, float density, Vector2 position,
                                              Object userData)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width", "Width must be more than 0 meters");

            if (height <= 0)
                throw new ArgumentOutOfRangeException("height", "Height must be more than 0 meters");

            Body newBody = BodyFactory.CreateBody(world, position);
            Vertices rectangleVertices = PolygonTools.CreateRectangle(width/2, height/2);
            PolygonShape rectangleShape = new PolygonShape(rectangleVertices, density);
            return newBody.CreateFixture(rectangleShape, userData);
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
        public static Fixture CreateRectangle(float width, float height, float density, Vector2 offset, Body body,
                                              Object userData)
        {
            Vertices rectangleVertices = PolygonTools.CreateRectangle(width/2, height/2);
            rectangleVertices.Translate(ref offset);
            PolygonShape rectangleShape = new PolygonShape(rectangleVertices, density);
            return body.CreateFixture(rectangleShape, userData);
        }

        public static Fixture CreateRectangle(float width, float height, float density, Vector2 offset, Body body)
        {
            return CreateRectangle(width, height, density, offset, body, null);
        }

        public static Fixture CreateCircle(World world, float radius, float density)
        {
            return CreateCircle(world, radius, density, null);
        }

        public static Fixture CreateCircle(World world, float radius, float density, Object userData)
        {
            return CreateCircle(world, radius, density, Vector2.Zero, userData);
        }

        public static Fixture CreateCircle(World world, float radius, float density, Vector2 position)
        {
            return CreateCircle(world, radius, density, position, null);
        }

        public static Fixture CreateCircle(World world, float radius, float density, Vector2 position, Object userData)
        {
            Body body = BodyFactory.CreateBody(world, position);
            return CreateCircle(radius, density, body, userData);
        }

        public static Fixture CreateCircle(float radius, float density, Body body)
        {
            return CreateCircle(radius, density, body, null);
        }

        public static Fixture CreateCircle(float radius, float density, Body body, Object userData)
        {
            if (radius <= 0)
                throw new ArgumentOutOfRangeException("radius", "Radius must be more than 0 meters");

            CircleShape circleShape = new CircleShape(radius, density);
            return body.CreateFixture(circleShape, userData);
        }

        public static Fixture CreateCircle(float radius, float density, Body body, Vector2 offset)
        {
            return CreateCircle(radius, density, body, offset, null);
        }

        public static Fixture CreateCircle(float radius, float density, Body body, Vector2 offset, Object userData)
        {
            if (radius <= 0)
                throw new ArgumentOutOfRangeException("radius", "Radius must be more than 0 meters");

            CircleShape circleShape = new CircleShape(radius, density);
            circleShape.Position = offset;
            return body.CreateFixture(circleShape, userData);
        }

        public static Fixture CreateEllipse(World world, float xRadius, float yRadius, int edges, float density)
        {
            return CreateEllipse(world, xRadius, yRadius, edges, density, null);
        }

        public static Fixture CreateEllipse(World world, float xRadius, float yRadius, int edges, float density,
                                            Object userData)
        {
            return CreateEllipse(world, xRadius, yRadius, edges, density, Vector2.Zero, userData);
        }

        public static Fixture CreateEllipse(World world, float xRadius, float yRadius, int edges, float density,
                                            Vector2 position)
        {
            return CreateEllipse(world, xRadius, yRadius, edges, density, position, null);
        }

        public static Fixture CreateEllipse(World world, float xRadius, float yRadius, int edges, float density,
                                            Vector2 position, Object userData)
        {
            Body body = BodyFactory.CreateBody(world, position);
            return CreateEllipse(xRadius, yRadius, edges, density, body, userData);
        }

        public static Fixture CreateEllipse(float xRadius, float yRadius, int edges, float density, Body body)
        {
            return CreateEllipse(xRadius, yRadius, edges, density, body, null);
        }

        public static Fixture CreateEllipse(float xRadius, float yRadius, int edges, float density, Body body,
                                            Object userData)
        {
            if (xRadius <= 0)
                throw new ArgumentOutOfRangeException("xRadius", "X-radius must be more than 0");

            if (yRadius <= 0)
                throw new ArgumentOutOfRangeException("yRadius", "Y-radius must be more than 0");

            Vertices ellipseVertices = PolygonTools.CreateEllipse(xRadius, yRadius, edges);
            PolygonShape polygonShape = new PolygonShape(ellipseVertices, density);
            return body.CreateFixture(polygonShape, userData);
        }

        public static Fixture CreatePolygon(World world, Vertices vertices, float density)
        {
            return CreatePolygon(world, vertices, density, null);
        }

        public static Fixture CreatePolygon(World world, Vertices vertices, float density, Object userData)
        {
            return CreatePolygon(world, vertices, density, Vector2.Zero, userData);
        }

        public static Fixture CreatePolygon(World world, Vertices vertices, float density, Vector2 position)
        {
            return CreatePolygon(world, vertices, density, position, null);
        }

        public static Fixture CreatePolygon(World world, Vertices vertices, float density, Vector2 position,
                                            Object userData)
        {
            Body body = BodyFactory.CreateBody(world, position);
            return CreatePolygon(vertices, density, body, userData);
        }

        public static Fixture CreatePolygon(Vertices vertices, float density, Body body)
        {
            return CreatePolygon(vertices, density, body, null);
        }

        public static Fixture CreatePolygon(Vertices vertices, float density, Body body, Object userData)
        {
            if (vertices.Count <= 1)
                throw new ArgumentOutOfRangeException("vertices", "Too few points to be a polygon");

            PolygonShape polygon = new PolygonShape(vertices, density);
            return body.CreateFixture(polygon, userData);
        }

        public static List<Fixture> CreateCompoundPolygon(World world, List<Vertices> list, float density)
        {
            return CreateCompoundPolygon(world, list, density, BodyType.Static);
        }

        public static List<Fixture> CreateCompoundPolygon(World world, List<Vertices> list, float density, BodyType type)
        {
            List<Fixture> fixtures = CreateCompoundPolygon(world, list, density, null);
            fixtures[0].Body.BodyType = type;
            return fixtures;
        }

        public static List<Fixture> CreateCompoundPolygon(World world, List<Vertices> list, float density,
                                                          Object userData)
        {
            return CreateCompoundPolygon(world, list, density, Vector2.Zero, userData);
        }

        public static List<Fixture> CreateCompoundPolygon(World world, List<Vertices> list, float density,
                                                          Vector2 position)
        {
            return CreateCompoundPolygon(world, list, density, position, null);
        }

        public static List<Fixture> CreateCompoundPolygon(World world, List<Vertices> list, float density,
                                                          Vector2 position, Object userData)
        {
            //We create a single body
            Body polygonBody = BodyFactory.CreateBody(world, position);
            return CreateCompoundPolygon(list, density, polygonBody, userData);
        }

        public static List<Fixture> CreateCompoundPolygon(List<Vertices> list, float density, Body body)
        {
            return CreateCompoundPolygon(list, density, body, null);
        }

        public static List<Fixture> CreateCompoundPolygon(List<Vertices> list, float density, Body body, Object userData)
        {
            List<Fixture> fixtures = new List<Fixture>(list.Count);

            //Then we create several fixtures using the body
            foreach (Vertices vertices in list)
            {
                PolygonShape shape = new PolygonShape(vertices, density);
                fixtures.Add(body.CreateFixture(shape, userData));
            }

            return fixtures;
        }

        public static List<Fixture> CreateGear(World world, float radius, int numberOfTeeth, float tipPercentage,
                                               float toothHeight, float density)
        {
            return CreateGear(world, radius, numberOfTeeth, tipPercentage, toothHeight, density, null);
        }

        public static List<Fixture> CreateGear(World world, float radius, int numberOfTeeth, float tipPercentage,
                                               float toothHeight, float density, Object userData)
        {
            Vertices gearPolygon = PolygonTools.CreateGear(radius, numberOfTeeth, tipPercentage, toothHeight);

            //Gears can in some cases be convex
            if (!gearPolygon.IsConvex())
            {
                //Decompose the gear:
                List<Vertices> list = EarclipDecomposer.ConvexPartition(gearPolygon);

                return CreateCompoundPolygon(world, list, density, userData);
            }

            List<Fixture> fixtures = new List<Fixture>();
            fixtures.Add(CreatePolygon(world, gearPolygon, density, userData));
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
                                                  int bottomEdges, float density, Vector2 position, Object userData)
        {
            Vertices verts = PolygonTools.CreateCapsule(height, topRadius, topEdges, bottomRadius, bottomEdges);

            //There are too many vertices in the capsule. We decompose it.
            if (verts.Count >= Settings.MaxPolygonVertices)
            {
                List<Vertices> vertList = EarclipDecomposer.ConvexPartition(verts);
                List<Fixture> fixtureList = CreateCompoundPolygon(world, vertList, density, userData);
                fixtureList[0].Body.Position = position;

                return fixtureList;
            }

            return new List<Fixture> {CreatePolygon(world, verts, density, userData)};
        }

        public static List<Fixture> CreateCapsule(World world, float height, float topRadius, int topEdges,
                                                  float bottomRadius,
                                                  int bottomEdges, float density, Vector2 position)
        {
            return CreateCapsule(world, height, topRadius, topEdges, bottomRadius, bottomEdges, density, position, null);
        }

        public static List<Fixture> CreateCapsule(World world, float height, float endRadius, float density)
        {
            return CreateCapsule(world, height, endRadius, density, null);
        }

        public static List<Fixture> CreateCapsule(World world, float height, float endRadius, float density,
                                                  Object userData)
        {
            //Create the middle rectangle
            Vertices rectangle = PolygonTools.CreateRectangle(endRadius, height/2);

            List<Vertices> list = new List<Vertices>();
            list.Add(rectangle);

            List<Fixture> fixtures = CreateCompoundPolygon(world, list, density, userData);

            //Create the two circles
            CircleShape topCircle = new CircleShape(endRadius, density);
            topCircle.Position = new Vector2(0, height/2);
            fixtures.Add(fixtures[0].Body.CreateFixture(topCircle, userData));

            CircleShape bottomCircle = new CircleShape(endRadius, density);
            bottomCircle.Position = new Vector2(0, -(height/2));
            fixtures.Add(fixtures[0].Body.CreateFixture(bottomCircle, userData));
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
                                                           int segments, float density, Vector2 position,
                                                           Object userData)
        {
            Vertices verts = PolygonTools.CreateRoundedRectangle(width, height, xRadius, yRadius, segments);

            //There are too many vertices in the capsule. We decompose it.
            if (verts.Count >= Settings.MaxPolygonVertices)
            {
                List<Vertices> vertList = EarclipDecomposer.ConvexPartition(verts);
                List<Fixture> fixtureList = CreateCompoundPolygon(world, vertList, density, userData);
                fixtureList[0].Body.Position = position;
                return fixtureList;
            }

            return new List<Fixture> {CreatePolygon(world, verts, density)};
        }

        public static List<Fixture> CreateRoundedRectangle(World world, float width, float height, float xRadius,
                                                           float yRadius,
                                                           int segments, float density, Vector2 position)
        {
            return CreateRoundedRectangle(world, width, height, xRadius, yRadius, segments, density, position, null);
        }

        public static List<Fixture> CreateRoundedRectangle(World world, float width, float height, float xRadius,
                                                           float yRadius,
                                                           int segments, float density)
        {
            return CreateRoundedRectangle(world, width, height, xRadius, yRadius, segments, density, null);
        }

        public static List<Fixture> CreateRoundedRectangle(World world, float width, float height, float xRadius,
                                                           float yRadius,
                                                           int segments, float density, Object userData)
        {
            return CreateRoundedRectangle(world, width, height, xRadius, yRadius, segments, density, Vector2.Zero,
                                          userData);
        }

        public static BreakableBody CreateBreakableBody(World world, Vertices vertices, float density)
        {
            return CreateBreakableBody(world, vertices, density, null);
        }

        public static BreakableBody CreateBreakableBody(World world, Vertices vertices, float density, Object userData)
        {
            return CreateBreakableBody(world, vertices, density, Vector2.Zero, userData);
        }

        /// <summary>
        /// Creates a breakable body. You would want to remove collinear points before using this.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="vertices">The vertices.</param>
        /// <param name="density">The density.</param>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public static BreakableBody CreateBreakableBody(World world, Vertices vertices, float density, Vector2 position,
                                                        Object userData)
        {
            List<Vertices> triangles = EarclipDecomposer.ConvexPartition(vertices);

            BreakableBody breakableBody = new BreakableBody(triangles, world, density, userData);
            breakableBody.MainBody.Position = position;
            world.AddBreakableBody(breakableBody);

            return breakableBody;
        }

        public static BreakableBody CreateBreakableBody(World world, Vertices vertices, float density, Vector2 position)
        {
            return CreateBreakableBody(world, vertices, density, position, null);
        }
    }
}