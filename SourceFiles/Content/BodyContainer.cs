using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;

namespace FarseerPhysics.Content
{
    public class FixtureTemplate
    {
        public Shape shape;
        public float restitution;
        public float friction;
        public string name;
    }

    public class BodyTemplate
    {
        public List<FixtureTemplate> fixtures;
        public float mass;
        public BodyType bodyType;

        public BodyTemplate()
        {
            fixtures = new List<FixtureTemplate>();
        }

        public Body Create(World world)
        {
            Body body = new Body(world);
            body.BodyType = bodyType;
            foreach (FixtureTemplate fixtureTemplate in fixtures)
            {
                Fixture fixture = body.CreateFixture(fixtureTemplate.shape, fixtureTemplate.name);
                fixture.Restitution = fixtureTemplate.restitution;
                fixture.Friction = fixtureTemplate.friction;
            }
            if (mass > 0f)
            {
                body.Mass = mass;
            }
            return body;
        }

        public BreakableBody CreateBreakable(World world)
        {
            List<Shape> shapes = new List<Shape>();
            foreach (FixtureTemplate f in fixtures)
            {
                shapes.Add(f.shape);
            }
            BreakableBody body = new BreakableBody(shapes, world);
            world.AddBreakableBody(body);

            return body;
        }
    }

    public class BodyContainer : Dictionary<string, BodyTemplate> { }
}
