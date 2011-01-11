using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class CloneTest : Test
    {
        private CloneTest()
        {
            //Ground
            FixtureFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            Fixture box = FixtureFactory.CreateRectangle(World, 5, 5, 5);
            box.Restitution = 0.8f;
            box.Friction = 0.9f;
            box.Body.BodyType = BodyType.Dynamic;
            box.Body.Position = new Vector2(10, 10);
            box.Body.SleepingAllowed = false;
            box.Body.LinearDamping = 1;
            box.Body.AngularDamping = 0.5f;
            box.Body.AngularVelocity = 0.5f;
            box.Body.LinearVelocity = new Vector2(0,10);

            Fixture boxClone1 = box.Clone();
            //Swiching the body type to static will reset all forces. This will affect the next clone.
            boxClone1.Body.BodyType = BodyType.Static;
            boxClone1.Body.Position += new Vector2(-10, 0);

            Fixture boxClone2 = boxClone1.Clone();
            boxClone2.Body.BodyType = BodyType.Dynamic;
            boxClone2.Body.Position += new Vector2(-10, 0);
        }

        public static CloneTest Create()
        {
            return new CloneTest();
        }
    }
}