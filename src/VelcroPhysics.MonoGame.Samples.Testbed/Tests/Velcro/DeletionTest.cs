using Genbox.VelcroPhysics.Collision.ContactSystem;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests.Velcro
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

        private void OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact) { }

        private void OnSeparation(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            fixtureB.Body.RemoveFromWorld();
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            Body body = BodyFactory.CreateCircle(World, 0.4f, 1);
            body.Position = new Vector2(Rand.RandomFloat(-35, 35), 10);
            body.BodyType = BodyType.Dynamic;
            body.Restitution = 1f;

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new DeletionTest();
        }
    }
}