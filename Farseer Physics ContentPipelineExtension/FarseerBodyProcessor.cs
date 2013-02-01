using System;
using System.Globalization;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;

namespace FarseerPhysics.ContentPipeline
{
  [ContentProcessor(DisplayName = "Farseer Body Processor")]
  class FarseerBodyProcessor : ContentProcessor<List<RawBodyTemplate>, BodyContainer>
  {
    [DisplayName("Pixel to meter ratio")]
    [Description("The length of one physics simulation unit in pixels.")]
    [DefaultValue(1)]
    public int ScaleFactor
    {
      get { return (int)(1f / _scaleFactor); }
      set { _scaleFactor = 1f / value; }
    }
    private float _scaleFactor = 1f;

    [DisplayName("Cubic bézier iterations")]
    [Description("Amount of subdivisions for decomposing cubic bézier curves into line segments.")]
    [DefaultValue(3)]
    public int BezierIterations
    {
      get { return _bezierIterations; }
      set { _bezierIterations = value; }
    }
    private int _bezierIterations = 3;

    public override BodyContainer Process(List<RawBodyTemplate> input, ContentProcessorContext context)
    {
      if (ScaleFactor < 1)
      {
        throw new Exception("Pixel to meter ratio must be greater than zero.");
      }
      if (BezierIterations < 1)
      {
        throw new Exception("Cubic bézier iterations must be greater than zero.");
      }

      Matrix matScale = Matrix.CreateScale(_scaleFactor, _scaleFactor, 1f);
      SVGPathParser parser = new SVGPathParser(_bezierIterations);
      BodyContainer bodies = new BodyContainer();
      BodyTemplate currentBody = null;

      foreach (RawBodyTemplate rawBody in input)
      {
        if (rawBody.name == "importer_default_path_container")
        {
          continue;
        }
        currentBody = new BodyTemplate();
        currentBody.mass = rawBody.mass;
        currentBody.bodyType = rawBody.bodyType;
        foreach (RawFixtureTemplate rawFixture in rawBody.fixtures)
        {
          List<Polygon> paths = parser.ParseSVGPath(rawFixture.path, rawFixture.transformation * matScale);
          for (int i = 0; i < paths.Count; i++)
          {
            if (paths[i].closed)
            {
              List<Vertices> partition = BayazitDecomposer.ConvexPartition(paths[i].vertices);
              foreach (Vertices v in partition)
              {
                currentBody.fixtures.Add(new FixtureTemplate()
                {
                  shape = new PolygonShape(v, rawFixture.density),
                  restitution = rawFixture.restitution,
                  friction = rawFixture.friction,
                  name = rawFixture.name
                });
              }
            }
            else
            {
              Shape shape;
              if (paths[i].vertices.Count > 2)
              {
                shape = new ChainShape(paths[i].vertices);
              }
              else
              {
                shape = new EdgeShape(paths[i].vertices[0], paths[i].vertices[1]);
              }
              currentBody.fixtures.Add(new FixtureTemplate()
              {
                shape = shape,
                restitution = rawFixture.restitution,
                friction = rawFixture.friction,
                name = rawFixture.name
              });
            }
          }
        }
        if (currentBody.fixtures.Count > 0)
        {
          bodies[rawBody.name] = currentBody;
          currentBody = null;
        }
      }
      return bodies;
    }
  }
}