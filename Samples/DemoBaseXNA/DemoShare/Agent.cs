using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.DemoBaseXNA.DemoShare
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

            DemoMaterial matBody = new DemoMaterial(MaterialType.Blank);
            DemoMaterial matHands = new DemoMaterial(MaterialType.Squares)
            {
                Color = Color.Gray,
                Scale = 8f,
                Depth = .1f
            };

            //Center
            FixtureFactory.CreateCircle(1, 1, _agentBody, matBody);

            //Left arm
            FixtureFactory.CreateRectangle(3, 0.8f, 1, new Vector2(-2, 0), _agentBody, matBody);
            FixtureFactory.CreateCircle(1, 1, _agentBody, new Vector2(-4, 0), matHands);

            //Right arm
            FixtureFactory.CreateRectangle(3, 0.8f, 1, new Vector2(2, 0), _agentBody, matBody);
            FixtureFactory.CreateCircle(1, 1, _agentBody, new Vector2(4, 0), matHands);

            //Top arm
            FixtureFactory.CreateRectangle(0.8f, 3, 1, new Vector2(0, 2), _agentBody, matBody);
            FixtureFactory.CreateCircle(1, 1, _agentBody, new Vector2(0, 4), matHands);

            //Bottom arm
            FixtureFactory.CreateRectangle(0.8f, 3, 1, new Vector2(0, -2), _agentBody, matBody);
            FixtureFactory.CreateCircle(1, 1, _agentBody, new Vector2(0, -4), matHands);
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