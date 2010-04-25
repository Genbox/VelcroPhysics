using System;
using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class PathTest : Test
    {
        private PathTest()
        {
            Body ground;
            {
                ground = BodyFactory.CreateBody(World);

                Vertices edge = PolygonTools.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                PolygonShape shape = new PolygonShape(edge, 0);
                ground.CreateFixture(shape);
            }

            {
                Path path = new Path();

                path.Add(new Vector2(0, 20));
                path.Add(new Vector2(5, 15));
                path.Add(new Vector2(20, 18));
                path.Add(new Vector2(15, 1));
                path.Add(new Vector2(-5, 14));

                path.Closed = true;

                Body body = new Body(World);
                body.BodyType = BodyType.Static;
                //body.CreateFixture(new PolygonShape(PolygonTools.CreateRectangle(0.5f, 0.5f, new Vector2(-0.5f, 0), 0), 1));
                Fixture f = body.CreateFixture(new CircleShape(0.25f, 1));

                PathFactory.EvenlyDistibuteShapesAlongPath(World, path, body, 140);

                Vector2 scale = new Vector2(0.5f, 0.5f);
                path.Scale(ref scale);
                scale = new Vector2(5, 5);
                path.Translate(ref scale);

                body = new Body(World);
                body.BodyType = BodyType.Dynamic;
                body.CreateFixture(new PolygonShape(PolygonTools.CreateRectangle(0.5f, 0.5f, new Vector2(-0.5f, 0), 0), 1));
                f = body.CreateFixture(new CircleShape(0.5f, 1));

                List<Body> bodies = PathFactory.EvenlyDistibuteShapesAlongPath(World, path, body, 30);

                JointFactory.AttachBodiesWithRevoluteJoint(World, bodies, new Vector2(0, 0.5f), new Vector2(0, -0.5f), true, true);
            }
        }

        internal static Test Create()
        {
            return new PathTest();
        }
    }
}