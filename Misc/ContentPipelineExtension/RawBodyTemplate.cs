using System.Collections.Generic;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;

namespace FarseerPhysics.ContentPipeline
{
    struct RawFixtureTemplate
    {
        public string Path;
        public string Name;
        public Matrix Transformation;
        public float Density;
        public float Friction;
        public float Restitution;
    }

    struct RawBodyTemplate
    {
        public List<RawFixtureTemplate> Fixtures;
        public string Name;
        public float Mass;
        public BodyType BodyType;
    }
}
