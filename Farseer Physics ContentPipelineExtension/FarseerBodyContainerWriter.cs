using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using FarseerPhysics.Content;
using FarseerPhysics.Collision.Shapes;

namespace FarseerPhysics.ContentPipeline
{
  [ContentTypeWriter]
  public class FarseerBodyContainerWriter : ContentTypeWriter<BodyContainer>
  {
    protected override void Write(ContentWriter output, BodyContainer container)
    {
      output.Write(container.Count);
      foreach (KeyValuePair<string, BodyTemplate> p in container)
      {
        output.Write(p.Key);
        output.Write(p.Value.mass);
        output.Write((int)p.Value.bodyType);
        output.Write(p.Value.fixtures.Count);
        foreach (FixtureTemplate f in p.Value.fixtures)
        {
          output.Write(f.name);
          output.Write(f.restitution);
          output.Write(f.friction);
          output.Write((int)f.shape.ShapeType);
          switch (f.shape.ShapeType)
          {
            case ShapeType.Circle:
              {
                CircleShape circle = (CircleShape)f.shape;
                output.Write(circle.Density);
                output.Write(circle.Radius);
                output.Write(circle.Position);
              } break;
            case ShapeType.Polygon:
              {
                PolygonShape poly = (PolygonShape)f.shape;
                output.Write(poly.Density);
                output.Write(poly.Vertices.Count);
                foreach (Vector2 v in poly.Vertices)
                {
                  output.Write(v);
                }
                output.Write(poly.MassData.Centroid);
              } break;
            case ShapeType.Edge:
              {
                EdgeShape edge = (EdgeShape)f.shape;
                output.Write(edge.Vertex1);
                output.Write(edge.Vertex2);
                output.Write(edge.HasVertex0);
                if (edge.HasVertex0)
                {
                  output.Write(edge.Vertex0);
                }
                output.Write(edge.HasVertex3);
                if (edge.HasVertex3)
                {
                  output.Write(edge.Vertex3);
                }
              } break;
            case ShapeType.Chain:
              {
                ChainShape chain = (ChainShape)f.shape;
                output.Write(chain.Vertices.Count);
                foreach (Vector2 v in chain.Vertices)
                {
                  output.Write(v);
                }
              } break;
            default:
              throw new Exception("Shape type not supported!");
          }
        }
      }
    }

    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      return typeof(FarseerBodyContainerReader).AssemblyQualifiedName;
    }
  }
}
