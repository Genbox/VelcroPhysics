using System;
using System.Collections.Generic;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Tools.Triangulation.TriangulationBase;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Factories
{
    /// <summary>An easy to use factory for creating bodies</summary>
    public static class FixtureFactory
    {
        public static Fixture AttachEdge(Vector2 start, Vector2 end, Body body, object? userData = null)
        {
            EdgeShape edgeShape = new EdgeShape(start, end);
            Fixture f = body.AddFixture(edgeShape);
            f.UserData = userData;
            return f;
        }

        public static Fixture AttachChainShape(Vertices vertices, Body body, object? userData = null)
        {
            ChainShape shape = new ChainShape(vertices);
            Fixture f = body.AddFixture(shape);
            f.UserData = userData;
            return f;
        }

        public static Fixture AttachLoopShape(Vertices vertices, Body body, object? userData = null)
        {
            ChainShape shape = new ChainShape(vertices, true);
            Fixture f = body.AddFixture(shape);
            f.UserData = userData;
            return f;
        }

        public static Fixture AttachRectangle(float width, float height, float density, Vector2 offset, Body body, object? userData = null)
        {
            Vertices rectangleVertices = PolygonUtils.CreateRectangle(width / 2, height / 2);
            rectangleVertices.Translate(ref offset);
            PolygonShape rectangleShape = new PolygonShape(rectangleVertices, density);
            Fixture f = body.AddFixture(rectangleShape);
            f.UserData = userData;
            return f;
        }

        public static Fixture AttachCircle(float radius, float density, Body body, object? userData = null)
        {
            if (radius <= 0)
                throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be more than 0 meters");

            CircleShape circleShape = new CircleShape(radius, density);
            Fixture f = body.AddFixture(circleShape);
            f.UserData = userData;
            return f;
        }

        public static Fixture AttachCircle(float radius, float density, Body body, Vector2 offset, object? userData = null)
        {
            if (radius <= 0)
                throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be more than 0 meters");

            CircleShape circleShape = new CircleShape(radius, density);
            circleShape.Position = offset;
            Fixture f = body.AddFixture(circleShape);
            f.UserData = userData;
            return f;
        }

        public static Fixture AttachPolygon(Vertices vertices, float density, Body body, object? userData = null)
        {
            if (vertices.Count <= 1)
                throw new ArgumentOutOfRangeException(nameof(vertices), "Too few points to be a polygon");

            PolygonShape polygon = new PolygonShape(vertices, density);
            Fixture f = body.AddFixture(polygon);
            f.UserData = userData;
            return f;
        }

        public static Fixture AttachEllipse(float xRadius, float yRadius, int edges, float density, Body body, object? userData = null)
        {
            if (xRadius <= 0)
                throw new ArgumentOutOfRangeException(nameof(xRadius), "X-radius must be more than 0");

            if (yRadius <= 0)
                throw new ArgumentOutOfRangeException(nameof(yRadius), "Y-radius must be more than 0");

            Vertices ellipseVertices = PolygonUtils.CreateEllipse(xRadius, yRadius, edges);
            PolygonShape polygonShape = new PolygonShape(ellipseVertices, density);
            Fixture f = body.AddFixture(polygonShape);
            f.UserData = userData;
            return f;
        }

        public static List<Fixture> AttachCompoundPolygon(List<Vertices> list, float density, Body body)
        {
            List<Fixture> res = new List<Fixture>(list.Count);

            //Then we create several fixtures using the body
            foreach (Vertices vertices in list)
            {
                if (vertices.Count == 2)
                {
                    EdgeShape shape = new EdgeShape(vertices[0], vertices[1]);
                    res.Add(body.AddFixture(shape));
                }
                else
                {
                    PolygonShape shape = new PolygonShape(vertices, density);
                    res.Add(body.AddFixture(shape));
                }
            }

            return res;
        }

        public static Fixture AttachLineArc(float radians, int sides, float radius, bool closed, Body body)
        {
            Vertices arc = PolygonUtils.CreateArc(radians, sides, radius);
            arc.Rotate((MathConstants.Pi - radians) / 2);
            return closed ? AttachLoopShape(arc, body) : AttachChainShape(arc, body);
        }

        public static List<Fixture> AttachSolidArc(float density, float radians, int sides, float radius, Body body)
        {
            Vertices arc = PolygonUtils.CreateArc(radians, sides, radius);
            arc.Rotate((MathConstants.Pi - radians) / 2);

            //Close the arc
            arc.Add(arc[0]);

            List<Vertices> triangles = Triangulate.ConvexPartition(arc, TriangulationAlgorithm.Earclip);

            return AttachCompoundPolygon(triangles, density, body);
        }

        public static Fixture CreateFromDef(Body body, FixtureDef fixtureDef)
        {
            return body.AddFixture(fixtureDef);
        }
    }
}