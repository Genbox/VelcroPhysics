using System.Collections.Generic;
using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.SamplesFramework
{
    public enum ObjectType
    {
        Circle,
        Rectangle,
        Gear,
        Star
    }

    public class Objects
    {
        private Category _collidesWith;
        private Category _collisionCategories;
        private List<Body> _bodies;

        public Objects(World world, Vector2 startPosition, Vector2 endPosition, int count, float radius, ObjectType type)
        {
            _bodies = new List<Body>(count);
            CollidesWith = Category.All;
            CollisionCategories = Category.All;

            for (int i = 0; i < count; ++i)
            {
                switch (type)
                {
                    case ObjectType.Circle:
                        _bodies.Add(BodyFactory.CreateCircle(world, radius, 1f));
                        break;
                    case ObjectType.Rectangle:
                        _bodies.Add(BodyFactory.CreateRectangle(world, radius, radius, 1f));
                        break;
                    case ObjectType.Star:
                        _bodies.Add(BodyFactory.CreateGear(world, radius, 10, 0f, 1f, 1f));
                        break;
                    case ObjectType.Gear:
                        _bodies.Add(BodyFactory.CreateGear(world, radius, 10, 100f, 1f, 1f));
                        break;
                }
            }

            for (int i = 0; i < _bodies.Count; ++i)
            {
                Body body = _bodies[i];
                body.BodyType = BodyType.Dynamic;
                body.Position = Vector2.Lerp(startPosition, endPosition, i / (float)(count - 1));
                body.Restitution = .7f;
                body.Friction = .2f;
                body.CollisionCategories = CollisionCategories;
                body.CollidesWith = CollidesWith;
            }
        }

        public Category CollisionCategories
        {
            get { return _collisionCategories; }
            set
            {
                _collisionCategories = value;

                foreach (Body body in _bodies)
                {
                    body.CollisionCategories = _collisionCategories;
                }
            }
        }

        public Category CollidesWith
        {
            get { return _collidesWith; }
            set
            {
                _collidesWith = value;

                foreach (Body body in _bodies)
                {
                    body.CollidesWith = _collidesWith;
                }
            }
        }
    }
}