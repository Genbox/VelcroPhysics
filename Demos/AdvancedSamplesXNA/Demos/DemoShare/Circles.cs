using System;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerPhysicsDemos.Demos.DemoShare
{
    public class Circles
    {
        private Color _borderColor = Color.Black;
        private Color _color = Color.White;

        private int _count = 2;
        private Vector2 _endPosition;
        private int _radius = 100;
        private Vector2 _startPosition;
        private Body[] _circleBody;
        private CircleBrush _circleBrush;
        private Geom[] _circleGeom;

        private CollisionCategory _collidesWith = CollisionCategory.All;
        private CollisionCategory _collisionCategories = CollisionCategory.All;

        public Circles(Vector2 startPosition, Vector2 endPosition, int count, int radius, Color color, Color borderColor)
        {
            if (count < 2)
            {
                throw new ArgumentException("Count must be 2 or greater", "count");
            }
            _count = count;
            _radius = radius;
            _color = color;
            _borderColor = borderColor;
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

        public void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            _circleBrush = new CircleBrush(_radius, _color, _borderColor);
            _circleBrush.Load(graphicsDevice);

            _circleBody = new Body[_count];
            _circleGeom = new Geom[_count];

            _circleBody[0] = BodyFactory.Instance.CreateCircleBody(physicsSimulator, _radius, .5f);
            _circleBody[0].Position = _startPosition;
            for (int i = 1; i < _count; i++)
            {
                _circleBody[i] = BodyFactory.Instance.CreateBody(physicsSimulator, _circleBody[0]);
                _circleBody[i].Position = Vector2.Lerp(_startPosition, _endPosition, i / (float)(_count - 1));
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

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _count; i++)
            {
                _circleBrush.Draw(spriteBatch, _circleGeom[i].Position);
            }
        }
    }
}