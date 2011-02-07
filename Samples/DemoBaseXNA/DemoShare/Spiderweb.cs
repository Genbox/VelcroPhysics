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

        public Spiderweb(World world, Vector2 position, float boxSize, int rings, int sides)
        {
            const float breakpoint = 100;
            Vertices box = PolygonTools.CreateRectangle(boxSize, boxSize);

            List<List<Body>> ringFixtures = new List<List<Body>>(rings);

            for (int i = 1; i < rings; i++)
            {
                Vertices vertices = PolygonTools.CreateCircle(i + i * 3, sides);
                List<Body> fixtures = new List<Body>(sides);

                //Create the first box
                Body prev = BodyFactory.CreatePolygon(world, box, 1, vertices[0], _defaultMaterial);
                prev.FixedRotation = true;
                prev.Position += position;
                prev.BodyType = BodyType.Dynamic;

                fixtures.Add(prev);

                //Connect the first box to the next
                for (int j = 1; j < vertices.Count; j++)
                {
                    Body fix = BodyFactory.CreatePolygon(world, box, 1, vertices[j], _defaultMaterial);
                    fix.FixedRotation = true;
                    fix.BodyType = BodyType.Dynamic;
                    fix.Position += position;

                    DistanceJoint dj = JointFactory.CreateDistanceJoint(world, prev, fix, Vector2.Zero, Vector2.Zero);
                    dj.Frequency = 4.0f;
                    dj.DampingRatio = 0.5f;
                    dj.Breakpoint = breakpoint;

                    prev = fix;
                    fixtures.Add(fix);
                }

                //Connect the first and the last box
                DistanceJoint djEnd = JointFactory.CreateDistanceJoint(world, fixtures[0], fixtures[fixtures.Count - 1], Vector2.Zero, Vector2.Zero);
                djEnd.Frequency = 4.0f;
                djEnd.DampingRatio = 0.5f;
                djEnd.Breakpoint = breakpoint;

                ringFixtures.Add(fixtures);
            }

            //Create an outer ring
            Vertices lastRing = PolygonTools.CreateCircle(rings + rings * 3, sides);
            lastRing.Translate(ref position);

            List<Body> lastRingFixtures = ringFixtures[ringFixtures.Count - 1];

            //Fix each of the fixtures of the outer ring
            for (int j = 0; j < lastRingFixtures.Count; j++)
            {
                FixedDistanceJoint fdj = JointFactory.CreateFixedDistanceJoint(world, lastRingFixtures[j], Vector2.Zero, lastRing[j]);
                fdj.Frequency = 4.0f;
                fdj.DampingRatio = 0.5f;
                fdj.Breakpoint = breakpoint;
            }

            //Interconnect the rings
            for (int i = 1; i < ringFixtures.Count; i++)
            {
                List<Body> prev = ringFixtures[i - 1];
                List<Body> current = ringFixtures[i];

                for (int j = 0; j < prev.Count; j++)
                {
                    Body prevFixture = prev[j];
                    Body currentFixture = current[j];

                    DistanceJoint dj = JointFactory.CreateDistanceJoint(world, prevFixture, currentFixture, Vector2.Zero, Vector2.Zero);
                    dj.Frequency = 4.0f;
                    dj.DampingRatio = 0.5f;
                }
            }
        }
    }
}
