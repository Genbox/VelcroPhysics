using System.Collections.Generic;
using VelcroPhysics.Collision.Shapes;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Templates;

namespace VelcroPhysics.ContentPipelines.SVGImport.Objects
{
    public class BodyTemplateExt : BodyTemplate
    {
        public List<FixtureTemplateExt> Fixtures;
        public string Name { get; set; }

        public BodyTemplateExt()
        {
            Fixtures = new List<FixtureTemplateExt>();
        }

        public Body Create(World world)
        {
            Body body = new Body(world);
            body.BodyType = Type;

            foreach (FixtureTemplateExt fixtureTemplate in Fixtures)
            {
                Fixture fixture = body.CreateFixture(fixtureTemplate.Shape, fixtureTemplate.Name);
                fixture.Restitution = fixtureTemplate.Restitution;
                fixture.Friction = fixtureTemplate.Friction;
            }

            return body;
        }

        public BreakableBody CreateBreakable(World world)
        {
            List<Shape> shapes = new List<Shape>(Fixtures.Count);
            foreach (FixtureTemplateExt f in Fixtures)
            {
                shapes.Add(f.Shape);
            }

            BreakableBody body = new BreakableBody(world, shapes);
            world.AddBreakableBody(body);

            return body;
        }
    }
}