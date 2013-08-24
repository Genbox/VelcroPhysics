using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;

namespace FarseerPhysics.ContentPipeline
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
    }

    public class BodyContainer : Dictionary<string, BodyTemplate> { }
}
