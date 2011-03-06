using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace FarseerPhysics.SamplesFramework
{
    public class Spiderweb
    {
        private World world;
        private Sprite link;
        private Sprite goo;

        private float spriteScale;
        private float radius;


        public Spiderweb(World world, Vector2 position, float radius, int rings, int sides)
        {
            const float breakpoint = 100f;

            this.world = world;

            this.radius = radius;

            List<List<Body>> ringBodys = new List<List<Body>>(rings);

            for (int i = 1; i < rings; ++i)
            {
                Vertices vertices = PolygonTools.CreateCircle(i * 2.9f, sides);
                List<Body> bodies = new List<Body>(sides);

                //Create the first goo
                Body prev = BodyFactory.CreateCircle(world, radius, 0.2f, vertices[0]);
                prev.FixedRotation = true;
                prev.Position += position;
                prev.BodyType = BodyType.Dynamic;

                bodies.Add(prev);

                //Connect the first goo to the next
                for (int j = 1; j < vertices.Count; ++j)
                {
                    Body bod = BodyFactory.CreateCircle(world, radius, 0.2f, vertices[j]);
                    bod.FixedRotation = true;
                    bod.BodyType = BodyType.Dynamic;
                    bod.Position += position;

                    DistanceJoint dj = JointFactory.CreateDistanceJoint(world, prev, bod, Vector2.Zero, Vector2.Zero);
                    dj.Frequency = 4.0f;
                    dj.DampingRatio = 0.5f;
                    dj.Breakpoint = breakpoint;

                    prev = bod;
                    bodies.Add(bod);
                }

                //Connect the first and the last box
                DistanceJoint djEnd = JointFactory.CreateDistanceJoint(world, bodies[0], bodies[bodies.Count - 1], Vector2.Zero, Vector2.Zero);
                djEnd.Frequency = 4.0f;
                djEnd.DampingRatio = 0.5f;
                djEnd.Breakpoint = breakpoint;

                ringBodys.Add(bodies);
            }

            //Create an outer ring
            Vertices lastRing = PolygonTools.CreateCircle(rings * 2.9f, sides);
            lastRing.Translate(ref position);

            List<Body> lastRingFixtures = ringBodys[ringBodys.Count - 1];

            //Fix each of the fixtures of the outer ring
            for (int j = 0; j < lastRingFixtures.Count; ++j)
            {
                FixedDistanceJoint fdj = JointFactory.CreateFixedDistanceJoint(world, lastRingFixtures[j], Vector2.Zero, lastRing[j]);
                fdj.Frequency = 4.0f;
                fdj.DampingRatio = 0.5f;
                fdj.Breakpoint = breakpoint;
            }

            //Interconnect the rings
            for (int i = 1; i < ringBodys.Count; i++)
            {
                List<Body> prev = ringBodys[i - 1];
                List<Body> current = ringBodys[i];

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

        public void LoadContent(ContentManager content)
        {
            link  = new Sprite(content.Load<Texture2D>("Samples/link"));
            goo = new Sprite(content.Load<Texture2D>("Samples/goo"));

            spriteScale = 2f * ConvertUnits.ToDisplayUnits(radius) / goo.texture.Width;
        }

        public void Draw(SpriteBatch batch)
        {
            foreach (Joint j in world.JointList)
            {
                if (j.Enabled && j.JointType != JointType.FixedMouse)
                {
                    Vector2 pos = ConvertUnits.ToDisplayUnits((j.WorldAnchorA + j.WorldAnchorB) / 2f);
                    Vector2 AtoB = j.WorldAnchorB - j.WorldAnchorA;
                    float distance = ConvertUnits.ToDisplayUnits(AtoB.Length()) + 8f * spriteScale;
                    Vector2 scale = new Vector2(distance / link.texture.Width, spriteScale);
                    Vector2 unitx = Vector2.UnitX;
                    float angle = (float)MathUtils.VectorAngle(ref unitx, ref AtoB);
                    batch.Draw(link.texture, pos, null, Color.White, angle, link.origin, scale, SpriteEffects.None, 0f);
                }
            }

            foreach (Body b in world.BodyList)
            {
                if (b.Enabled && b.FixtureList[0].ShapeType == ShapeType.Circle)
                {
                    batch.Draw(goo.texture, ConvertUnits.ToDisplayUnits(b.Position), null,
                               Color.White, 0f, goo.origin, spriteScale, SpriteEffects.None, 0f);
                }
            }
        }
    }
}
