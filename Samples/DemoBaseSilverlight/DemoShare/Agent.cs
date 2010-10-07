using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.DemoBaseSilverlight.DemoShare
{
    public class Agent
    {
        private Body _agentBody;
        private CollisionCategory _collidesWith;

        private CollisionCategory _collisionCategories;

        public Agent(World world, Vector2 position)
        {
            _collidesWith = CollisionCategory.All;
            _collisionCategories = CollisionCategory.All;

            _agentBody = BodyFactory.CreateBody(world, position);
            _agentBody.BodyType = BodyType.Dynamic;

            //Center
            FixtureFactory.CreateCircle(world, 1, 1, Vector2.Zero, _agentBody);

            //Left arm
            FixtureFactory.CreateRectangle(3, 0.8f, 1, _agentBody, new Vector2(-2, 0));
            FixtureFactory.CreateCircle(world, 1, 1, new Vector2(-4, 0), _agentBody);

            //Right arm
            FixtureFactory.CreateRectangle(3, 0.8f, 1, _agentBody, new Vector2(2, 0));
            FixtureFactory.CreateCircle(world, 1, 1, new Vector2(4, 0), _agentBody);

            //Top arm
            FixtureFactory.CreateRectangle(0.8f, 3, 1, _agentBody, new Vector2(0, 2));
            FixtureFactory.CreateCircle(world, 1, 1, new Vector2(0, 4), _agentBody);

            //Bottom arm
            FixtureFactory.CreateRectangle(0.8f, 3, 1, _agentBody, new Vector2(0, -2));
            FixtureFactory.CreateCircle(world, 1, 1, new Vector2(0, -4), _agentBody);
        }

        public CollisionCategory CollisionCategories
        {
            get { return _collisionCategories; }
            set
            {
                _collisionCategories = value;

                foreach (Fixture fixture in _agentBody.FixtureList)
                {
                    fixture.CollisionCategories = _collisionCategories;
                }
            }
        }

        public CollisionCategory CollidesWith
        {
            get { return _collidesWith; }
            set
            {
                _collidesWith = value;

                foreach (Fixture fixture in _agentBody.FixtureList)
                {
                    fixture.CollidesWith = _collidesWith;
                }
            }
        }

        public Body Body
        {
            get { return _agentBody; }
        }
    }
}