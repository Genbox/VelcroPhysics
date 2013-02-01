using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;

namespace FarseerPhysics.ContentPipeline
{
  struct RawFixtureTemplate
  {
    public string path;
    public string name;
    public Matrix transformation;
    public float density;
    public float friction;
    public float restitution;
  }

  struct RawBodyTemplate
  {
    public List<RawFixtureTemplate> fixtures;
    public string name;
    public float mass;
    public BodyType bodyType;
  }
}
