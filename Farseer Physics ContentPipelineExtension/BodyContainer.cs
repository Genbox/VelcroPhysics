using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using FarseerPhysics.Common;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common.Decomposition;

namespace FarseerPhysics.ContentPipeline
{
  public class FixtureTemplate
  {
    public Shape shape;
    public float restitution;
    public float friction;
    public string name;
  }

  public class BodyTemplate
  {
    public List<FixtureTemplate> fixtures;
    public float mass;
    public BodyType bodyType;

    public BodyTemplate()
    {
      fixtures = new List<FixtureTemplate>();
    }
  }

  public class BodyContainer : Dictionary<string, BodyTemplate> { }
}
