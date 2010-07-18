using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class PathTest : Test
    {
        Body movingBody;
        Path path;

        private PathTest()
        {
            //Single body that moves around path
            movingBody = BodyFactory.CreateBody(World);
            movingBody.Position = new Vector2(-25, 25);
            movingBody.BodyType = BodyType.Dynamic;
            movingBody.CreateFixture(new PolygonShape(PolygonTools.CreateRectangle(0.5f, 0.5f)),1);

            //Static shape made up of bodies
            path = new Path();
            path.Add(new Vector2(0, 20));
            path.Add(new Vector2(5, 15));
            path.Add(new Vector2(20, 18));
            path.Add(new Vector2(15, 1));
            path.Add(new Vector2(-5, 14));
            path.Closed = true;

            CircleShape shape = new CircleShape(0.25f);

            PathFactory.EvenlyDistibuteShapesAlongPath(World, path, shape, BodyType.Static, 100);

            //Smaller shape that is movable. Created from small rectangles and circles.
            Vector2 xform = new Vector2(0.5f, 0.5f);
            path.Scale(ref xform);
            xform = new Vector2(5, 5);
            path.Translate(ref xform);

            List<Shape> shapes = new List<Shape>(2);
            shapes.Add(new PolygonShape(PolygonTools.CreateRectangle(0.5f, 0.5f, new Vector2(-0.1f, 0), 0)));
            shapes.Add(new CircleShape(0.5f));

            List<Body> bodies = PathFactory.EvenlyDistibuteShapesAlongPath(World, path, shapes, BodyType.Dynamic, 20);

            //Attach the bodies together with revolute joints
            PathFactory.AttachBodiesWithRevoluteJoint(World, bodies, new Vector2(0, 0.5f), new Vector2(0, -0.5f), true, true);

            xform = new Vector2(-25, 0);
            path.Translate(ref xform);

            Body body = BodyFactory.CreateBody(World);
            body.BodyType = BodyType.Static;

            //Static shape made up of edges
            PathFactory.ConvertPathToEdges(path, body, 25);
            body.Position -= new Vector2(0, 10);

            xform = new Vector2(0, 15);
            path.Translate(ref xform);

            PathFactory.ConvertPathToPolygon(path, body, 1, 50);
        }

        float time;

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            time += 0.01f;
            if (time > 1f)
                time = 0;

            PathFactory.MoveBodyOnPath(path, movingBody, time, 1f, 1f / 60f);

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new PathTest();
        }
    }
}