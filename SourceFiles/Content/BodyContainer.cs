using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;

namespace FarseerPhysics.Content
{
    public class FixtureTemplate
    {
        public Shape Shape;
        public float Restitution;
        public float Friction;
        public string Name;
    }

    public class BodyTemplate
    {
        public List<FixtureTemplate> Fixtures;
        public float Mass;
        public BodyType BodyType;

        public BodyTemplate()
        {
            Fixtures = new List<FixtureTemplate>();
        }

        public Body Create(World world)
        {
            Body body = new Body(world);
            body.BodyType = BodyType;

            foreach (FixtureTemplate fixtureTemplate in Fixtures)
            {
                Fixture fixture = body.CreateFixture(fixtureTemplate.Shape, fixtureTemplate.Name);
                fixture.Restitution = fixtureTemplate.Restitution;
                fixture.Friction = fixtureTemplate.Friction;
            }

            if (Mass > 0f)
                body.Mass = Mass;

            return body;
        }

        public BreakableBody CreateBreakable(World world)
        {
            List<Shape> shapes = new List<Shape>();
            foreach (FixtureTemplate f in Fixtures)
            {
                shapes.Add(f.Shape);
            }

            BreakableBody body = new BreakableBody(world, shapes);
            world.AddBreakableBody(body);

            return body;
        }
    }

    public class BodyContainer : Dictionary<string, BodyTemplate> { }
}
