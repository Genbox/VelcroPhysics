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
        public static void AttachEdge(Vector2 start, Vector2 end, Body body)
        {
            AttachEdge(start, end, body, null);
        }

        public static void AttachEdge(Vector2 start, Vector2 end, Body body, object userData)
        {
            EdgeShape edgeShape = new EdgeShape(start, end);
            body.CreateFixture(edgeShape, userData);
        }

        public static void AttachLoopShape(Vertices vertices, Body body)
        {
            AttachLoopShape(vertices, body, null);
        }

        public static void AttachLoopShape(Vertices vertices, Body body, object userData)
        {
            LoopShape shape = new LoopShape(vertices);
            body.CreateFixture(shape, userData);
        }

        public static void AttachRectangle(float width, float height, float density, Vector2 offset, Body body,
                                              object userData)
        {
            Vertices rectangleVertices = PolygonTools.CreateRectangle(width / 2, height / 2);
            rectangleVertices.Translate(ref offset);
            PolygonShape rectangleShape = new PolygonShape(rectangleVertices, density);
            body.CreateFixture(rectangleShape, userData);
        }

        public static void AttachRectangle(float width, float height, float density, Vector2 offset, Body body)
        {
            AttachRectangle(width, height, density, offset, body, null);
        }

        public static void AttachCircle(float radius, float density, Body body)
        {
            AttachCircle(radius, density, body, null);
        }

        public static void AttachCircle(float radius, float density, Body body, object userData)
        {
            if (radius <= 0)
                throw new ArgumentOutOfRangeException("radius", "Radius must be more than 0 meters");

            CircleShape circleShape = new CircleShape(radius, density);
            body.CreateFixture(circleShape, userData);
        }

        public static void AttachCircle(float radius, float density, Body body, Vector2 offset)
        {
            AttachCircle(radius, density, body, offset, null);
        }

        public static void AttachCircle(float radius, float density, Body body, Vector2 offset, object userData)
        {
            if (radius <= 0)
                throw new ArgumentOutOfRangeException("radius", "Radius must be more than 0 meters");

            CircleShape circleShape = new CircleShape(radius, density);
            circleShape.Position = offset;
            body.CreateFixture(circleShape, userData);
        }

        public static void AttachPolygon(Vertices vertices, float density, Body body)
        {
            AttachPolygon(vertices, density, body, null);
        }

        public static void AttachPolygon(Vertices vertices, float density, Body body, object userData)
        {
            if (vertices.Count <= 1)
                throw new ArgumentOutOfRangeException("vertices", "Too few points to be a polygon");

            PolygonShape polygon = new PolygonShape(vertices, density);
            body.CreateFixture(polygon, userData);
        }

        public static void AttachEllipse(float xRadius, float yRadius, int edges, float density, Body body)
        {
            AttachEllipse(xRadius, yRadius, edges, density, body, null);
        }

        public static void AttachEllipse(float xRadius, float yRadius, int edges, float density, Body body,
                                            object userData)
        {
            if (xRadius <= 0)
                throw new ArgumentOutOfRangeException("xRadius", "X-radius must be more than 0");

            if (yRadius <= 0)
                throw new ArgumentOutOfRangeException("yRadius", "Y-radius must be more than 0");

            Vertices ellipseVertices = PolygonTools.CreateEllipse(xRadius, yRadius, edges);
            PolygonShape polygonShape = new PolygonShape(ellipseVertices, density);
            body.CreateFixture(polygonShape, userData);
        }
        public static void AttachCompoundPolygon(List<Vertices> list, float density, Body body)
        {
            AttachCompoundPolygon(list, density, body, null);
        }

        public static void AttachCompoundPolygon(List<Vertices> list, float density, Body body, object userData)
        {
            //Then we create several fixtures using the body
            foreach (Vertices vertices in list)
            {
                if (vertices.Count == 2)
                {
                    EdgeShape shape = new EdgeShape(vertices[0], vertices[1]);
                    body.CreateFixture(shape, userData);
                }
                else
                {
                    PolygonShape shape = new PolygonShape(vertices, density);
                    body.CreateFixture(shape, userData);
                }
            }
        }

    }
}