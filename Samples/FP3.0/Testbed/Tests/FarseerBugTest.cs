using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class FarseerBugTest : Test
    {
        Body m_dynamicBody;

        public FarseerBugTest()
        {
            {
                m_dynamicBody = BodyFactory.CreateBody(World);
                m_dynamicBody.Position = new Vector2(10.0f, 10.0f);
                m_dynamicBody.BodyType = BodyType.Dynamic;
                m_dynamicBody.FixedRotation = true;

                CircleShape shape = new CircleShape(0.5f, 20);
                Fixture fixture = m_dynamicBody.CreateFixture(shape);
                fixture.Friction = 0.3f;
            }
            {
                Fixture fixture = FixtureFactory.CreateRectangle(World, 2, 2, 20);

                fixture.Body.BodyType = BodyType.Dynamic;
                fixture.Body.Position = new Vector2(10.0f, 0.0f);
                fixture.Body.BodyType = BodyType.Static;
            }
        }

        internal static Test Create()
        {
            return new FarseerBugTest();
        }
    }
}
