using Microsoft.Xna.Framework;
using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;
using VelcroPhysics.Samples.Testbed.Framework;

namespace VelcroPhysics.Samples.Testbed.Tests
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

        private void OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
        }

        private void OnSeparation(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            //fixtureB.Body.Dispose(); //TODO: issue #4
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