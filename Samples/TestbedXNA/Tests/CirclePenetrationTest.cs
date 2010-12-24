using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class CirclePenetrationTest : Test
    {
        private CirclePenetrationTest()
        {
            World.Gravity = Vector2.Zero;

            List<Vertices> borders = new List<Vertices>(4);

            const float borderWidth = 0.2f;
            const float width = 40f;
            const float height = 25f;

            //Bottom
            borders.Add(PolygonTools.CreateRectangle(width, borderWidth, new Vector2(0, height), 0));

            //Left
            borders.Add(PolygonTools.CreateRectangle(borderWidth, height, new Vector2(-width, 0), 0));

            //Top
            borders.Add(PolygonTools.CreateRectangle(width, borderWidth, new Vector2(0, -height), 0));

            //Right
            borders.Add(PolygonTools.CreateRectangle(borderWidth, height, new Vector2(width, 0), 0));

            List<Fixture> fixtures = FixtureFactory.CreateCompoundPolygon(World, borders, 1, new Vector2(0, 20));

            foreach (Fixture fixture in fixtures)
            {
                fixture.Restitution = 1f;
                fixture.Friction = 0;
            }

            Fixture circle = FixtureFactory.CreateCircle(World, 0.32f, 1);
            circle.Body.BodyType = BodyType.Dynamic;
            circle.Restitution = 1f;
            circle.Friction = 0;

            circle.Body.ApplyLinearImpulse(new Vector2(200, 50));
        }

        internal static Test Create()
        {
            return new CirclePenetrationTest();
        }
    }
}