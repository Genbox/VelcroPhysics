using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.DemoBaseSilverlight.DemoShare
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
        private CollisionCategory _collidesWith;
        private CollisionCategory _collisionCategories;
        private List<List<Fixture>> _decomposedFixtures;
        private List<Fixture> _fixtures;
        private ObjectType _type;

        public Objects(World world, Vector2 startPosition, Vector2 endPosition, int count, float radius, ObjectType type)
        {
            _fixtures = new List<Fixture>(count);
            _decomposedFixtures = new List<List<Fixture>>(count);
            _type = type;
            CollidesWith = CollisionCategory.All;
            CollisionCategories = CollisionCategory.All;

            for (int i = 0; i < count; i++)
            {
                Fixture fixture;

                switch (type)
                {
                    case ObjectType.Circle:
                        fixture = FixtureFactory.CreateCircle(world, radius, 1);
                        _fixtures.Add(fixture);
                        break;
                    case ObjectType.Rectangle:
                        fixture = FixtureFactory.CreateRectangle(world, radius, radius, 1);
                        _fixtures.Add(fixture);
                        break;
                    case ObjectType.Star:
                        List<Fixture> star = FixtureFactory.CreateGear(world, radius, 10, 0f, 1f, 1);
                        _decomposedFixtures.Add(star);
                        break;
                    case ObjectType.Gear:
                        List<Fixture> gear = FixtureFactory.CreateGear(world, radius, 10, 100f, 1f, 1);
                        _decomposedFixtures.Add(gear);
                        break;
                    default:
                        fixture = FixtureFactory.CreateCircle(world, radius, 1);
                        _fixtures.Add(fixture);
                        break;
                }
            }

            if (type == ObjectType.Circle || type == ObjectType.Rectangle)
            {
                for (int i = 0; i < _fixtures.Count; i++)
                {
                    Fixture fixture = _fixtures[i];
                    fixture.Body.BodyType = BodyType.Dynamic;
                    fixture.Body.Position = Vector2.Lerp(startPosition, endPosition, i / (float) (count - 1));
                    fixture.Restitution = .7f;
                    fixture.Friction = .2f;
                    fixture.CollisionCategories = CollisionCategories;
                    fixture.CollidesWith = CollidesWith;
                }
            }
            else
            {
                for (int i = 0; i < _decomposedFixtures.Count; i++)
                {
                    List<Fixture> fixtures = _decomposedFixtures[i];
                    foreach (Fixture fixture in fixtures)
                    {
                        fixture.Body.BodyType = BodyType.Dynamic;
                        fixture.Body.Position = Vector2.Lerp(startPosition, endPosition, i / (float) (count - 1));
                        fixture.Restitution = .7f;
                        fixture.Friction = .2f;
                        fixture.CollisionCategories = CollisionCategories;
                        fixture.CollidesWith = CollidesWith;
                    }
                }
            }
        }

        public CollisionCategory CollisionCategories
        {
            get { return _collisionCategories; }
            set
            {
                _collisionCategories = value;

                if (_type == ObjectType.Circle || _type == ObjectType.Rectangle)
                {
                    foreach (Fixture fixture in _fixtures)
                    {
                        fixture.CollisionCategories = _collisionCategories;
                    }
                }
                else
                {
                    foreach (List<Fixture> fixtures in _decomposedFixtures)
                    {
                        foreach (Fixture fixture in fixtures)
                        {
                            fixture.CollisionCategories = _collisionCategories;
                        }
                    }
                }
            }
        }

        public CollisionCategory CollidesWith
        {
            get { return _collidesWith; }
            set
            {
                _collidesWith = value;

                if (_type == ObjectType.Circle || _type == ObjectType.Rectangle)
                {
                    foreach (Fixture fixture in _fixtures)
                    {
                        fixture.CollidesWith = _collidesWith;
                    }
                }
                else
                {
                    foreach (List<Fixture> fixtures in _decomposedFixtures)
                    {
                        foreach (Fixture fixture in fixtures)
                        {
                            fixture.CollidesWith = _collidesWith;
                        }
                    }
                }
            }
        }
    }
}