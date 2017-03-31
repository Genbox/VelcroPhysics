using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Testbed.Tests
{
    public class PathTest : Test
    {
        private Body _movingBody;
        private Path _path;

        private float _time;

        private PathTest()
        {
            //Single body that moves around path
            _movingBody = BodyFactory.CreateBody(World);
            _movingBody.Position = new Vector2(-25, 25);
            _movingBody.BodyType = BodyType.Dynamic;
            _movingBody.CreateFixture(new PolygonShape(PolygonTools.CreateRectangle(0.5f, 0.5f), 1));

            //Static shape made up of bodies
            _path = new Path();
            _path.Add(new Vector2(0, 20));
            _path.Add(new Vector2(5, 15));
            _path.Add(new Vector2(20, 18));
            _path.Add(new Vector2(15, 1));
            _path.Add(new Vector2(-5, 14));
            _path.Closed = true;

            CircleShape shape = new CircleShape(0.25f, 1);

            PathManager.EvenlyDistributeShapesAlongPath(World, _path, shape, BodyType.Static, 100);

            //Smaller shape that is movable. Created from small rectangles and circles.
            Vector2 xform = new Vector2(0.5f, 0.5f);
            _path.Scale(ref xform);
            xform = new Vector2(5, 5);
            _path.Translate(ref xform);

            List<Shape> shapes = new List<Shape>(2);
            shapes.Add(new PolygonShape(PolygonTools.CreateRectangle(0.5f, 0.5f, new Vector2(-0.1f, 0), 0), 1));
            shapes.Add(new CircleShape(0.5f, 1));

            List<Body> bodies = PathManager.EvenlyDistributeShapesAlongPath(World, _path, shapes, BodyType.Dynamic, 20);

            //Attach the bodies together with revolute joints
            PathManager.AttachBodiesWithRevoluteJoint(World, bodies, new Vector2(0, 0.5f), new Vector2(0, -0.5f), true, true);

            xform = new Vector2(-25, 0);
            _path.Translate(ref xform);

            Body body = BodyFactory.CreateBody(World);
            body.BodyType = BodyType.Static;

            //Static shape made up of edges
            PathManager.ConvertPathToEdges(_path, body, 25);
            body.Position -= new Vector2(0, 10);

            xform = new Vector2(0, 15);
            _path.Translate(ref xform);

            PathManager.ConvertPathToPolygon(_path, body, 1, 50);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            _time += 0.01f;
            if (_time > 1f)
                _time = 0;

            PathManager.MoveBodyOnPath(_path, _movingBody, _time, 1f, 1f / 60f);

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new PathTest();
        }
    }
}