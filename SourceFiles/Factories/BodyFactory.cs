using System;
using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Factories
{
    public static class BodyFactory
    {
        public static Body CreateBody(World world, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null)
        {
            return new Body(world, position, rotation, bodyType, userData);
        }

        public static Body CreateEdge(World world, Vector2 start, Vector2 end, object userData = null)
        {
            Body body = CreateBody(world);
            body.UserData = userData;

            FixtureFactory.AttachEdge(start, end, body);
            return body;
        }

        public static Body CreateChainShape(World world, Vertices vertices, Vector2 position = new Vector2(), object userData = null)
        {
            Body body = CreateBody(world, position);
            body.UserData = userData;

            FixtureFactory.AttachChainShape(vertices, body);
            return body;
        }

        public static Body CreateLoopShape(World world, Vertices vertices, Vector2 position = new Vector2(), object userData = null)
        {
            Body body = CreateBody(world, position);
            body.UserData = userData;

            FixtureFactory.AttachLoopShape(vertices, body);
            return body;
        }

        public static Body CreateRectangle(World world, float width, float height, float density, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width", "Width must be more than 0 meters");

            if (height <= 0)
                throw new ArgumentOutOfRangeException("height", "Height must be more than 0 meters");

            Body body = CreateBody(world, position, rotation, bodyType, userData);

            Vertices rectangleVertices = PolygonTools.CreateRectangle(width / 2, height / 2);
            FixtureFactory.AttachPolygon(rectangleVertices, density, body);

            return body;
        }

        public static Body CreateCircle(World world, float radius, float density, Vector2 position = new Vector2(), BodyType bodyType = BodyType.Static, object userData = null)
        {
            Body body = CreateBody(world, position, 0, bodyType, userData);
            FixtureFactory.AttachCircle(radius, density, body);
            return body;
        }

        public static Body CreateEllipse(World world, float xRadius, float yRadius, int edges, float density, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null)
        {
            Body body = CreateBody(world, position, rotation, bodyType, userData);
            FixtureFactory.AttachEllipse(xRadius, yRadius, edges, density, body);
            return body;
        }

        public static Body CreatePolygon(World world, Vertices vertices, float density, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null)
        {
            Body body = CreateBody(world, position, rotation, bodyType, userData);
            FixtureFactory.AttachPolygon(vertices, density, body);
            return body;
        }

        public static Body CreateCompoundPolygon(World world, List<Vertices> list, float density, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null)
        {
            //We create a single body
            Body body = CreateBody(world, position, rotation, bodyType, userData);
            FixtureFactory.AttachCompoundPolygon(list, density, body);
            return body;
        }

        public static Body CreateGear(World world, float radius, int numberOfTeeth, float tipPercentage, float toothHeight, float density, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null)
        {
            Vertices gearPolygon = PolygonTools.CreateGear(radius, numberOfTeeth, tipPercentage, toothHeight);

            //Gears can in some cases be convex
            if (!gearPolygon.IsConvex())
            {
                //Decompose the gear:
                List<Vertices> list = Triangulate.ConvexPartition(gearPolygon, TriangulationAlgorithm.Earclip);

                return CreateCompoundPolygon(world, list, density, position, rotation, bodyType, userData);
            }

            return CreatePolygon(world, gearPolygon, density, position, rotation, bodyType, userData);
        }

        public static Body CreateCapsule(World world, float height, float topRadius, int topEdges, float bottomRadius, int bottomEdges, float density, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null)
        {
            Vertices verts = PolygonTools.CreateCapsule(height, topRadius, topEdges, bottomRadius, bottomEdges);

            //There are too many vertices in the capsule. We decompose it.
            if (verts.Count >= Settings.MaxPolygonVertices)
            {
                List<Vertices> vertList = Triangulate.ConvexPartition(verts, TriangulationAlgorithm.Earclip);
                return CreateCompoundPolygon(world, vertList, density, position, rotation, bodyType, userData);
            }

            return CreatePolygon(world, verts, density, position, rotation, bodyType, userData);
        }

        public static Body CreateCapsule(World world, float height, float endRadius, float density, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null)
        {
            //Create the middle rectangle
            Vertices rectangle = PolygonTools.CreateRectangle(endRadius, height / 2);

            List<Vertices> list = new List<Vertices>();
            list.Add(rectangle);

            Body body = CreateCompoundPolygon(world, list, density, position, rotation, bodyType, userData);
            FixtureFactory.AttachCircle(endRadius, density, body, new Vector2(0, height / 2));
            FixtureFactory.AttachCircle(endRadius, density, body, new Vector2(0, -(height / 2)));

            //Create the two circles
            //CircleShape topCircle = new CircleShape(endRadius, density);
            //topCircle.Position = new Vector2(0, height / 2);
            //body.CreateFixture(topCircle);

            //CircleShape bottomCircle = new CircleShape(endRadius, density);
            //bottomCircle.Position = new Vector2(0, -(height / 2));
            //body.CreateFixture(bottomCircle);
            return body;
        }

        public static Body CreateRoundedRectangle(World world, float width, float height, float xRadius, float yRadius, int segments, float density, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null)
        {
            Vertices verts = PolygonTools.CreateRoundedRectangle(width, height, xRadius, yRadius, segments);

            //There are too many vertices in the capsule. We decompose it.
            if (verts.Count >= Settings.MaxPolygonVertices)
            {
                List<Vertices> vertList = Triangulate.ConvexPartition(verts, TriangulationAlgorithm.Earclip);
                return CreateCompoundPolygon(world, vertList, density, position, rotation, bodyType, userData);
            }

            return CreatePolygon(world, verts, density, position, rotation, bodyType, userData);
        }

        public static Body CreateLineArc(World world, float radians, int sides, float radius, bool closed = false, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null)
        {
            Body body = CreateBody(world, position, rotation, bodyType, userData);
            FixtureFactory.AttachLineArc(radians, sides, radius, closed, body);
            return body;
        }

        public static Body CreateSolidArc(World world, float density, float radians, int sides, float radius, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null)
        {
            Body body = CreateBody(world, position, rotation, bodyType, userData);
            FixtureFactory.AttachSolidArc(density, radians, sides, radius, body);

            return body;
        }

        public static BreakableBody CreateBreakableBody(World world, Vertices vertices, float density, Vector2 position = new Vector2(), float rotation = 0)
        {
            //TODO: Implement a Voronoi diagram algorithm to split up the vertices
            List<Vertices> triangles = Triangulate.ConvexPartition(vertices, TriangulationAlgorithm.Earclip);

            BreakableBody breakableBody = new BreakableBody(world, triangles, density, position, rotation);
            breakableBody.MainBody.Position = position;
            world.AddBreakableBody(breakableBody);
            return breakableBody;
        }

        public static BreakableBody CreateBreakableBody(World world, IEnumerable<Shape> shapes, Vector2 position = new Vector2(), float rotation = 0)
        {
            BreakableBody breakableBody = new BreakableBody(world, shapes, position, rotation);
            breakableBody.MainBody.Position = position;
            world.AddBreakableBody(breakableBody);
            return breakableBody;
        }
    }
}