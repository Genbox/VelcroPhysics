using System.Windows.Media;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using SimpleSamplesWPF.Demos;

namespace SimpleSamplesWPF.SharedDemoObjects
{
    public class RectanglePlatform
    {
        private int _collisionGroup;
        private int _height;
        private Body _platformBody;
        private Geom _platformGeom;

        private Vector2 _position;
        private int _width;

        public RectanglePlatform(int width, int height, Vector2 position, int collisionGroup)
        {
            _width = width;
            _height = height;
            _position = position;
            _collisionGroup = collisionGroup;
        }

        public void Load(Demo demo, PhysicsSimulator physicsSimulator)
        {
            //use the body factory to create the physics body
            _platformBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, _width, _height, 1);
            demo.AddRectangleToCanvas(_platformBody, Colors.White, new Vector2(_width, _height));
            _platformBody.IsStatic = true;
            _platformBody.Position = _position;

            _platformGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _platformBody, _width, _height);
            _platformGeom.CollisionGroup = 100;
            _platformGeom.CollisionGroup = _collisionGroup;
            _platformGeom.FrictionCoefficient = 1;
        }
    }
}
