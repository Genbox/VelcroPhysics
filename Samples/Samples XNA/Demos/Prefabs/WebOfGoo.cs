using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.Samples.MediaSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.Samples.Demos.Prefabs
{
    public class WebOfGoo
    {
        private const float Breakpoint = 100f;

        private List<List<Body>> _ringBodys;
        private List<DistanceJoint> _ringJoints;

        private Sprite _goo;
        private Sprite _link;

        public WebOfGoo(World world, Vector2 position, float radius, int rings, int sides)
        {
            _ringBodys = new List<List<Body>>(rings);
            _ringJoints = new List<DistanceJoint>();

            for (int i = 1; i < rings; i++)
            {
                Vertices vertices = PolygonTools.CreateCircle(i * 2.9f, sides);
                vertices.Translate(ref position);
                List<Body> bodies = new List<Body>(sides);

                //Create the first goo
                Body previous = BodyFactory.CreateCircle(world, radius, 0.2f, vertices[0]);
                previous.BodyType = BodyType.Dynamic;

                bodies.Add(previous);

                //Connect the first goo to the next
                for (int j = 1; j < vertices.Count; j++)
                {
                    Body current = BodyFactory.CreateCircle(world, radius, 0.2f, vertices[j]);
                    current.BodyType = BodyType.Dynamic;

                    DistanceJoint joint = new DistanceJoint(previous, current, Vector2.Zero, Vector2.Zero);
                    joint.Frequency = 4.0f;
                    joint.DampingRatio = 0.5f;
                    joint.Breakpoint = Breakpoint;
                    world.AddJoint(joint);
                    _ringJoints.Add(joint);

                    previous = current;
                    bodies.Add(current);
                }

                //Connect the first and the last goo
                DistanceJoint jointClose = new DistanceJoint(bodies[0], bodies[bodies.Count - 1], Vector2.Zero, Vector2.Zero);
                jointClose.Frequency = 4.0f;
                jointClose.DampingRatio = 0.5f;
                jointClose.Breakpoint = Breakpoint;
                world.AddJoint(jointClose);
                _ringJoints.Add(jointClose);

                _ringBodys.Add(bodies);
            }

            //Create an outer ring
            Vertices frame = PolygonTools.CreateCircle(rings * 2.9f - 0.9f, sides);
            frame.Translate(ref position);

            Body anchor = new Body(world, position);
            anchor.BodyType = BodyType.Static;

            //Attach the outer ring to the anchor
            for (int i = 0; i < _ringBodys[rings - 2].Count; i++)
            {
                DistanceJoint joint = new DistanceJoint(anchor, _ringBodys[rings - 2][i], frame[i], _ringBodys[rings - 2][i].Position, true);
                joint.Frequency = 8.0f;
                joint.DampingRatio = 0.5f;
                joint.Breakpoint = Breakpoint;
                world.AddJoint(joint);
                _ringJoints.Add(joint);
            }

            //Interconnect the rings
            for (int i = 1; i < _ringBodys.Count; i++)
            {
                for (int j = 0; j < sides; j++)
                {
                    DistanceJoint joint = new DistanceJoint(_ringBodys[i - 1][j], _ringBodys[i][j], Vector2.Zero, Vector2.Zero);
                    joint.Frequency = 4.0f;
                    joint.DampingRatio = 0.5f;
                    joint.Breakpoint = Breakpoint;
                    world.AddJoint(joint);
                    _ringJoints.Add(joint);
                }
            }

            _link = new Sprite(ContentWrapper.GetTexture("Link"));
            _goo = new Sprite(ContentWrapper.GetTexture("Goo"));
        }

        public void Draw(SpriteBatch batch)
        {
            foreach (DistanceJoint joint in _ringJoints)
            {
                if (joint.Enabled)
                {
                    Vector2 pos = ConvertUnits.ToDisplayUnits((joint.WorldAnchorA + joint.WorldAnchorB) / 2f);
                    Vector2 AtoB = joint.WorldAnchorB - joint.WorldAnchorA;
                    float distance = ConvertUnits.ToDisplayUnits(AtoB.Length()) + 8f;
                    Vector2 scale = new Vector2(distance / _link.Image.Width, 1f);
                    float angle = (float)MathUtils.VectorAngle(Vector2.UnitX, AtoB);
                    batch.Draw(_link.Image, pos, null, Color.White, angle, _link.Origin, scale, SpriteEffects.None, 0f);
                }
            }

            foreach (List<Body> bodyList in _ringBodys)
            {
                foreach (Body body in bodyList)
                {
                    batch.Draw(_goo.Image, ConvertUnits.ToDisplayUnits(body.Position), null, Color.White, 0f, _goo.Origin, 1f, SpriteEffects.None, 0f);
                }
            }
        }
    }
}