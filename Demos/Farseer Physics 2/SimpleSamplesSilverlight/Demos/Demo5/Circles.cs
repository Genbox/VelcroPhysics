using System;
using System.Windows.Media;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.SimpleSamplesSilverlight.Demos.Demo5
{
    public class Circles
    {
        private Body[] _circleBody;
        private Geom[] _circleGeom;
        private CollisionCategory _collidesWith = CollisionCategory.All;
        private CollisionCategory _collisionCategories = CollisionCategory.All;

        private Color _color = Colors.White;

        private int _count = 2;
        private Vector2 _endPosition;
        private int _radius = 100;
        private Vector2 _startPosition;

        /// <exception cref="Exception">count must be 2 or greater</exception>
        public Circles(Vector2 startPosition, Vector2 endPosition, int count, int radius, Color color)
        {
            if (count < 2)
            {
                throw new Exception("count must be 2 or greater");
            }

            _count = count;
            _radius = radius;
            _color = color;
            _startPosition = startPosition;
            _endPosition = endPosition;
        }

        public CollisionCategory CollisionCategories
        {
            get { return _collisionCategories; }
            set { _collisionCategories = value; }
        }

        public CollisionCategory CollidesWith
        {
            get { return _collidesWith; }
            set { _collidesWith = value; }
        }

        public void Load(SimulatorView view, PhysicsSimulator physicsSimulator)
        {
            _circleBody = new Body[_count];
            _circleGeom = new Geom[_count];

            _circleBody[0] = BodyFactory.Instance.CreateCircleBody(physicsSimulator, _radius, .1f);
            _circleBody[0].Position = _startPosition;
            view.AddCircleToCanvas(_circleBody[0], _color, _radius);
            for (int i = 1; i < _count; i++)
            {
                _circleBody[i] = BodyFactory.Instance.CreateBody(physicsSimulator, _circleBody[0]);
                _circleBody[i].Position = Vector2.Lerp(_startPosition, _endPosition, i/(float) (_count - 1));
                view.AddCircleToCanvas(_circleBody[i], _color, _radius);
            }

            _circleGeom[0] = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, _circleBody[0], _radius, 10);
            _circleGeom[0].RestitutionCoefficient = .7f;
            _circleGeom[0].FrictionCoefficient = .2f;
            _circleGeom[0].CollisionCategories = _collisionCategories;
            _circleGeom[0].CollidesWith = _collidesWith;
            for (int j = 1; j < _count; j++)
            {
                _circleGeom[j] = GeomFactory.Instance.CreateGeom(physicsSimulator, _circleBody[j], _circleGeom[0]);
            }
        }
    }
}