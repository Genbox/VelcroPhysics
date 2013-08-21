using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;

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
