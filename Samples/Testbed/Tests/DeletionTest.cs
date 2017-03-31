using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Testbed.Tests
{
    public class DeletionTest : Test
    {
        private DeletionTest()
        {
            //Ground body
            Body ground = BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
            ground.OnCollision += OnCollision;
            ground.OnSeparation += OnSeparation;
        }

        private bool OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            return true;
        }

        private void OnSeparation(Fixture fixtureA, Fixture fixtureB)
        {
            fixtureB.Body.Dispose();
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            Body body = BodyFactory.CreateCircle(World, 0.4f, 1);
            body.Position = new Vector2(Rand.RandomFloat(-35, 35), 10);
            body.BodyType = BodyType.Dynamic;
            body.Restitution = 1f;

            base.Update(settings, gameTime);
        }

        public static Test Create()
        {
            return new DeletionTest();
        }
    }
}