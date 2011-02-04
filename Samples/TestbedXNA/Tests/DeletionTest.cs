using System;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class DeletionTest : Test
    {
        private DeletionTest()
        {
            //Ground body
            Fixture ground = FixtureFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
            ground.OnCollision += OnCollision;
            ground.OnSeparation += OnSeparation;

            Vertices v = new Vertices(3);
            v.Add(new Vector2(17.63f, 36.31f));
            v.Add(new Vector2(17.52f, 36.69f));
            v.Add(new Vector2(17.19f, 36.36f));

            FixtureFactory.CreatePolygon(World, v, 1);
        }

        private bool OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            return false;
        }

        private void OnSeparation(Fixture fixtureA, Fixture fixtureB)
        {
            fixtureB.Dispose();
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            Fixture fix = FixtureFactory.CreateCircle(World, 0.4f, 1);
            fix.Body.Position = new Vector2(Rand.RandomFloat(-35, 35), 10);
            fix.Body.BodyType = BodyType.Dynamic;
            fix.Restitution = 1f;

            base.Update(settings, gameTime);
        }

        public static Test Create()
        {
            return new DeletionTest();
        }
    }
}