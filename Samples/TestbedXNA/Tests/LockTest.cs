using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class LockTest : Test
    {
        private Fixture _rectangle;

        private LockTest()
        {
            FixtureFactory.CreateEdge(World, new Vector2(-20, 0), new Vector2(20, 0));

            _rectangle = FixtureFactory.CreateRectangle(World, 2, 2, 1);
            _rectangle.Body.BodyType = BodyType.Dynamic;
            _rectangle.Body.Position = new Vector2(0, 10);
            _rectangle.OnCollision += OnCollision;

            //Properties and methods that were checking for lock before
            //Body.Active
            //Body.LocalCenter
            //Body.Mass
            //Body.Inertia
            //Fixture.DestroyFixture()
            //Body.SetTransformIgnoreContacts()
            //Fixture()
        }

        private bool OnCollision(Fixture fixturea, Fixture fixtureb, Contact manifold)
        {
            _rectangle.Body.CreateFixture(_rectangle.Shape); //Calls the constructor in Fixture
            _rectangle.Body.DestroyFixture(_rectangle);
            //_rectangle.Body.Inertia = 40;
            //_rectangle.Body.LocalCenter = new Vector2(-1,-1);
            //_rectangle.Body.Mass = 10;
            //_rectangle.Body.Active = false;);)
            return true;
        }

        internal static Test Create()
        {
            return new LockTest();
        }
    }
}