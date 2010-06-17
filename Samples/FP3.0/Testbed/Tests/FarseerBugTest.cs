using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

// when the dynamic circle hits the static poly shape, observe the poly shape move
// this is due to me changing it between dynamic / static during its creation

namespace FarseerPhysics.TestBed.Tests
{
    public class FarseerBugTest : Test
    {
        Body m_dynamicBody;

        public FarseerBugTest()
        {
            {
                m_dynamicBody = BodyFactory.CreateBody(World);
                m_dynamicBody.Position = new Vector2(10.0f, 20.0f);
                m_dynamicBody.BodyType = BodyType.Dynamic;
                m_dynamicBody.FixedRotation = true;

                CircleShape shape = new CircleShape(0.5f, 20);
                Fixture fixture = m_dynamicBody.CreateFixture(shape);
                fixture.Friction = 0.3f;
            }
            {

                Vertices rect1 = PolygonTools.CreateRectangle(5, 1, new Vector2(0, 0), 0);
                Vertices rect2 = PolygonTools.CreateRectangle(5, 1, new Vector2(0, -2), 0);

                List<Vertices> list = new List<Vertices>();
                list.Add(rect1);
                list.Add(rect2);

                List<Fixture> fixtures = FixtureFactory.CreateCompundPolygon(World, list, 20);
                fixtures[0].Body.BodyType = BodyType.Dynamic;
                fixtures[0].Body.Position = new Vector2(10.0f, 0.0f);
                fixtures[0].Body.BodyType = BodyType.Static;
            }
        }

        internal static Test Create()
        {
            return new FarseerBugTest();
        }
    }
}
