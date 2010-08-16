using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.DemoBaseXNA.DemoShare
{
    public class Agent
    {
        private Body _agentBody;

        public Agent(World world, Vector2 position)
        {
            _agentBody = BodyFactory.CreateBody(world, position);
            _agentBody.BodyType = BodyType.Dynamic;

            //Center
            FixtureFactory.CreateCircle(world, 1, 1, Vector2.Zero, _agentBody);

            //Left arm
            FixtureFactory.CreateRectangle(world, 3, 0.8f, 1, new Vector2(-2, 0), _agentBody);
            FixtureFactory.CreateCircle(world, 1, 1, new Vector2(-4, 0), _agentBody);

            //Right arm
            FixtureFactory.CreateRectangle(world, 3, 0.8f, 1, new Vector2(2, 0), _agentBody);
            FixtureFactory.CreateCircle(world, 1, 1, new Vector2(4, 0), _agentBody);

            //Top arm
            FixtureFactory.CreateRectangle(world, 0.8f, 3, 1, new Vector2(0, 2), _agentBody);
            FixtureFactory.CreateCircle(world, 1, 1, new Vector2(0, 4), _agentBody);

            //Bottom arm
            FixtureFactory.CreateRectangle(world, 0.8f, 3, 1, new Vector2(0, -2), _agentBody);
            FixtureFactory.CreateCircle(world, 1, 1, new Vector2(0, -4), _agentBody);
        }

        public Body Body
        {
            get { return _agentBody; }
        }
    }
}