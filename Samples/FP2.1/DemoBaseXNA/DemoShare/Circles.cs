using System;
using DemoBaseXNA.DrawingSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DemoShare
{
    public class Circles
    {
        private Color _borderColor = Color.Black;
        private Body[] _circleBody;
        private CircleBrush _circleBrush;
        private Geom[] _circleGeom;

        private Color _color = Color.White;

        private int _count = 2;
        private Vector2 _endPosition;
        private int _radius = 100;
        private Vector2 _startPosition;

        /// <exception cref="ArgumentException">Count must be 2 or greater</exception>
        public Circles(Vector2 startPosition, Vector2 endPosition, int count, int radius, Color color, Color borderColor)
        {
            CollidesWith = CollisionCategory.All;
            CollisionCategories = CollisionCategory.All;
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

        public CollisionCategory CollisionCategories { get; set; }

        public CollisionCategory CollidesWith { get; set; }

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
                _circleBody[i].Position = Vector2.Lerp(_startPosition, _endPosition, i/(float) (_count - 1));
            }

            _circleGeom[0] = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, _circleBody[0], _radius, 10);
            _circleGeom[0].RestitutionCoefficient = .7f;
            _circleGeom[0].FrictionCoefficient = .2f;
            _circleGeom[0].CollisionCategories = CollisionCategories;
            _circleGeom[0].CollidesWith = CollidesWith;
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