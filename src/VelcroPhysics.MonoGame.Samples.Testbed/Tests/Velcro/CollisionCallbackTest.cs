using Genbox.VelcroPhysics.Collision.ContactSystem;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests.Velcro
{
    public class CollisionCallbackTest : Test
    {
        private int _collision1Fired;
        private int _collision2Fired;
        private int _separation1Fired;
        private int _separation2Fired;
        private int _contactDisabled;

        private CollisionCallbackTest()
        {
            //Ground
            BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            BodyFactory.CreateRectangle(World, 2, 2, 1, new Vector2(-30, 1), 0, BodyType.Dynamic);
            Body body1 = BodyFactory.CreateRectangle(World, 2, 2, 1, new Vector2(-30, 10), 0, BodyType.Dynamic);
            body1.OnCollision += OnCollision;
            body1.OnCollision += OnCollision2;
            body1.OnSeparation += OnSeparation;
            body1.OnSeparation += OnSeparation2;

            Body body2 = BodyFactory.CreateBody(World, new Vector2(0, 5), 0, BodyType.Dynamic);
            Fixture fixture1 = FixtureFactory.AttachRectangle(2, 2, 1, new Vector2(0, -1), body2);
            FixtureFactory.AttachRectangle(2, 2, 1, new Vector2(0, 1), body2);
            fixture1.OnCollision += OnFixtureCollision;
        }

        private void OnFixtureCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            contact.Enabled = false;
            _contactDisabled++;
        }

        private void OnSeparation(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            _separation1Fired++;
        }

        private void OnSeparation2(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            _separation2Fired++;
        }

        private void OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            _collision1Fired++;
        }

        private void OnCollision2(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            _collision2Fired++;

            fixtureA.Body.ApplyLinearImpulse(new Vector2(0, 100));
            fixtureA.Body.OnCollision -= OnCollision2;
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DebugView.DrawString(4, 60, "Collision1 fired: " + _collision1Fired);
            DebugView.DrawString(4, 60 + TextLine, "Collision2 fired:" + _collision2Fired);
            DebugView.DrawString(4, 60 + TextLine * 2, "Separation1 fired: " + _separation1Fired);
            DebugView.DrawString(4, 60 + TextLine * 3, "Separation2 fired: " + _separation2Fired);
            DebugView.DrawString(4, 60 + TextLine * 4, "Contact disabled: " + _contactDisabled);

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new CollisionCallbackTest();
        }
    }
}