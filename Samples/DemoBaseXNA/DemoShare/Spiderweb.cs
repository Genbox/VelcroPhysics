using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.DemoBaseXNA.DemoShare
{
    public class Spiderweb
    {
        DebugMaterial _defaultMaterial = new DebugMaterial(MaterialType.Blank)
        {
            Color = Color.LightGray,
        };

        public Spiderweb(World world, Vector2 position, Vector2 boxSize)
        {
            Vertices box = PolygonTools.CreateRectangle(boxSize.X, boxSize.Y);

            const int rings = 5;
            const int sides = 12;
            List<List<Fixture>> ringFixtures = new List<List<Fixture>>(rings);

            for (int i = 1; i < rings; i++)
            {
                Vertices vertices = PolygonTools.CreateCircle(i + i * 3, sides);
                List<Fixture> fixtures = new List<Fixture>(sides);

                //Create the first box
                Fixture prev = FixtureFactory.CreatePolygon(world, box, 1, vertices[0], _defaultMaterial);
                prev.Body.FixedRotation = true;
                prev.Body.Position += position;
                prev.Body.BodyType = BodyType.Dynamic;

                fixtures.Add(prev);

                //Connect the first box to the next
                for (int j = 1; j < vertices.Count; j++)
                {
                    Fixture fix = FixtureFactory.CreatePolygon(world, box, 1, vertices[j], _defaultMaterial);
                    fix.Body.FixedRotation = true;
                    fix.Body.BodyType = BodyType.Dynamic;
                    fix.Body.Position += position;

                    DistanceJoint dj = JointFactory.CreateDistanceJoint(world, prev.Body, fix.Body, Vector2.Zero, Vector2.Zero);
                    dj.Frequency = 4.0f;
                    dj.DampingRatio = 0.5f;

                    prev = fix;
                    fixtures.Add(fix);
                }

                //Connect the first and the last box
                DistanceJoint djEnd = JointFactory.CreateDistanceJoint(world, fixtures[0].Body, fixtures[fixtures.Count - 1].Body, Vector2.Zero, Vector2.Zero);
                djEnd.Frequency = 4.0f;
                djEnd.DampingRatio = 0.5f;

                ringFixtures.Add(fixtures);
            }

            //Create an outer ring
            Vertices lastRing = PolygonTools.CreateCircle(rings + rings * 3, sides);
            lastRing.Translate(ref position);

            List<Fixture> lastRingFixtures = ringFixtures[ringFixtures.Count - 1];

            //Fix each of the fixtures of the outer ring
            for (int j = 0; j < lastRingFixtures.Count; j++)
            {
                FixedDistanceJoint fdj = JointFactory.CreateFixedDistanceJoint(world, lastRingFixtures[j].Body, Vector2.Zero, lastRing[j]);
                fdj.Frequency = 4.0f;
                fdj.DampingRatio = 0.5f;
            }

            //Interconnect the rings
            for (int i = 1; i < ringFixtures.Count; i++)
            {
                List<Fixture> prev = ringFixtures[i - 1];
                List<Fixture> current = ringFixtures[i];

                for (int j = 0; j < prev.Count; j++)
                {
                    Fixture prevFixture = prev[j];
                    Fixture currentFixture = current[j];

                    DistanceJoint dj = JointFactory.CreateDistanceJoint(world, prevFixture.Body, currentFixture.Body, Vector2.Zero, Vector2.Zero);
                    dj.Frequency = 4.0f;
                    dj.DampingRatio = 0.5f;
                }
            }
        }
    }
}
