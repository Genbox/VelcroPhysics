using System.Collections.Generic;
using VelcroPhysics.Collision.Shapes;
using VelcroPhysics.Dynamics;

namespace VelcroPhysics.ContentPipelines.SVGImport.Objects
{
    public class BodyTemplate2
    {
        public BodyType BodyType;
        public List<FixtureTemplate2> Fixtures;
        public float Mass;

        public BodyTemplate2()
        {
            Fixtures = new List<FixtureTemplate2>();
        }

        public Body Create(World world)
        {
            Body body = new Body(world);
            body.BodyType = BodyType;

            foreach (FixtureTemplate2 fixtureTemplate in Fixtures)
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
            foreach (FixtureTemplate2 f in Fixtures)
            {
                shapes.Add(f.Shape);
            }

            BreakableBody body = new BreakableBody(world, shapes);
            world.AddBreakableBody(body);

            return body;
        }
    }
}