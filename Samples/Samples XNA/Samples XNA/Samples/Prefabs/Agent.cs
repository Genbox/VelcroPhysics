using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.SamplesFramework
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
            FixtureFactory.AttachCircle(0.5f, 0.5f, _agentBody);

            //Left arm
            FixtureFactory.AttachRectangle(1.5f, 0.4f, 1f, new Vector2(-1f, 0f), _agentBody);
            FixtureFactory.AttachCircle(0.5f, 0.5f, _agentBody, new Vector2(-2f, 0f));

            //Right arm
            FixtureFactory.AttachRectangle(1.5f, 0.4f, 1f, new Vector2(1f, 0f), _agentBody);
            FixtureFactory.AttachCircle(0.5f, 0.5f, _agentBody, new Vector2(2f, 0f));

            //Top arm
            FixtureFactory.AttachRectangle(0.4f, 1.5f, 1f, new Vector2(0f, 1f), _agentBody);
            FixtureFactory.AttachCircle(0.5f, 0.5f, _agentBody, new Vector2(0f, 2f));

            //Bottom arm
            FixtureFactory.AttachRectangle(0.4f, 1.5f, 1f, new Vector2(0f, -1f), _agentBody);
            FixtureFactory.AttachCircle(0.5f, 0.5f, _agentBody, new Vector2(0f, -2f));
        }

        public Category CollisionCategories
        {
            get { return _collisionCategories; }
            set
            {
                _collisionCategories = value;
                Body.CollisionCategories = value;
            }
        }

        public Category CollidesWith
        {
            get { return _collidesWith; }
            set
            {
                _collidesWith = value;
                Body.CollidesWith = value;
            }
        }

        public Body Body
        {
            get { return _agentBody; }
        }
    }
}