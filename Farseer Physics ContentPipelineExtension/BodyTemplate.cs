using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.ContentPipeline
{
  struct FixtureTemplate
  {
    public string path;
    public string name;
    public Matrix transformation;
    public float density;
    public float friction;
    public float restitution;
  }

  struct BodyTemplate
  {
    public List<FixtureTemplate> fixtures;
    public float mass;
  }
}
