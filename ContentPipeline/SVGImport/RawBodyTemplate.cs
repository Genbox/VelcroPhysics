using System.Collections.Generic;
using Microsoft.Xna.Framework;
using VelcroPhysics.Dynamics;

namespace VelcroPhysics.ContentPipelines.SVGImport
{
    public struct RawFixtureTemplate
    {
        public string Path;
        public string Name;
        public Matrix Transformation;
        public float Density;
        public float Friction;
        public float Restitution;
    }

    public struct RawBodyTemplate
    {
        public List<RawFixtureTemplate> Fixtures;
        public string Name;
        public float Mass;
        public BodyType BodyType;
    }
}