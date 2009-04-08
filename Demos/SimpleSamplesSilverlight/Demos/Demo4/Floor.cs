using System.Windows.Media;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.SimpleSamplesSilverlight.Demos.Demo4
{
    public class Floor
    {
        private Body _floorBody;
        private Geom _floorGeom;

        private int _height;
        private Vector2 _position;
        private int _width;

        public Floor(int width, int height, Vector2 position)
        {
            _width = width;
            _height = height;
            _position = position;
        }

        public void Load(SimulatorView view, PhysicsSimulator physicsSimulator)
        {
            //use the body factory to create the physics body
            _floorBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, _width, _height, 1);
            view.AddRectangleToCanvas(_floorBody, Colors.White, new Vector2(_width, _height));
            _floorBody.IsStatic = true;
            _floorGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _floorBody, _width, _height);
            _floorGeom.RestitutionCoefficient = .4f;
            _floorGeom.FrictionCoefficient = .4f;
            _floorBody.Position = _position;
        }
    }
}