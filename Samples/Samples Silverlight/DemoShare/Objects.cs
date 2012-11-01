using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.DemoShare
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

            for (int i = 0; i < count; i++)
            {
                Body body;

                switch (type)
                {
                    case ObjectType.Circle:
                        body = BodyFactory.CreateCircle(world, radius, 1);
                        _bodies.Add(body);
                        break;
                    case ObjectType.Rectangle:
                        body = BodyFactory.CreateRectangle(world, radius, radius, 1);
                        _bodies.Add(body);
                        break;
                    case ObjectType.Star:
                        Body star = BodyFactory.CreateGear(world, radius, 10, 0f, 1f, 1);
                        _bodies.Add(star);
                        break;
                    case ObjectType.Gear:
                        Body gear = BodyFactory.CreateGear(world, radius, 10, 100f, 1f, 1);
                        _bodies.Add(gear);
                        break;
                    default:
                        body = BodyFactory.CreateCircle(world, radius, 1);
                        _bodies.Add(body);
                        break;
                }
            }

            for (int i = 0; i < _bodies.Count; i++)
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