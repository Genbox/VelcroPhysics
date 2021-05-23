using System.Collections.Generic;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests.Velcro
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
            borders.Add(PolygonUtils.CreateRectangle(width, borderWidth, new Vector2(0, height), 0));

            //Left
            borders.Add(PolygonUtils.CreateRectangle(borderWidth, height, new Vector2(-width, 0), 0));

            //Top
            borders.Add(PolygonUtils.CreateRectangle(width, borderWidth, new Vector2(0, -height), 0));

            //Right
            borders.Add(PolygonUtils.CreateRectangle(borderWidth, height, new Vector2(width, 0), 0));

            Body body = BodyFactory.CreateCompoundPolygon(World, borders, 1, new Vector2(0, 20));

            foreach (Fixture fixture in body.FixtureList)
            {
                fixture.Restitution = 1f;
                fixture.Friction = 0;
            }

            Body circle = BodyFactory.CreateCircle(World, 0.32f, 1);
            circle.BodyType = BodyType.Dynamic;
            circle.Restitution = 1f;
            circle.Friction = 0;

            circle.ApplyLinearImpulse(new Vector2(200, 50));
        }

        internal static Test Create()
        {
            return new CirclePenetrationTest();
        }
    }
}