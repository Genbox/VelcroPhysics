using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

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
