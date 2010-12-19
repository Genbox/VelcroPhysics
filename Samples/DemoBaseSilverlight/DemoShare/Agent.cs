using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.DemoBaseSilverlight.DemoShare
{
    public class Agent
    {
        private Body _agentBody;
        private Category _collidesWith;

        private Category _collisionCategories;

        public Agent(World world, Vector2 position)
        {
            _collidesWith = Category.All;
            _collisionCategories = Category.All;

            _agentBody = BodyFactory.CreateBody(world, position);
            _agentBody.BodyType = BodyType.Dynamic;

            //Center
            FixtureFactory.CreateCircle(1, 1, _agentBody);

            //Left arm
            FixtureFactory.CreateRectangle(3, 0.8f, 1, new Vector2(-2, 0), _agentBody);
            FixtureFactory.CreateCircle(1, 1, _agentBody, new Vector2(-4, 0));

            //Right arm
            FixtureFactory.CreateRectangle(3, 0.8f, 1, new Vector2(2, 0), _agentBody);
            FixtureFactory.CreateCircle(1, 1, _agentBody, new Vector2(4, 0));

            //Top arm
            FixtureFactory.CreateRectangle(0.8f, 3, 1, new Vector2(0, 2), _agentBody);
            FixtureFactory.CreateCircle(1, 1, _agentBody, new Vector2(0, 4));

            //Bottom arm
            FixtureFactory.CreateRectangle(0.8f, 3, 1, new Vector2(0, -2), _agentBody);
            FixtureFactory.CreateCircle(1, 1, _agentBody, new Vector2(0, -4));
        }

        public Category CollisionCategories
        {
            get { return _collisionCategories; }
            set
            {
                _collisionCategories = value;

                foreach (Fixture fixture in _agentBody.FixtureList)
                {
                    fixture.CollisionFilter.CollisionCategories = _collisionCategories;
                }
            }
        }

        public Category CollidesWith
        {
            get { return _collidesWith; }
            set
            {
                _collidesWith = value;

                foreach (Fixture fixture in _agentBody.FixtureList)
                {
                    fixture.CollisionFilter.CollidesWith = _collidesWith;
                }
            }
        }

        public Body Body
        {
            get { return _agentBody; }
        }
    }
}