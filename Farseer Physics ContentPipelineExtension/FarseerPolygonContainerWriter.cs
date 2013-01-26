using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using FarseerPhysics.Content;

namespace FarseerPhysics.ContentPipeline
{
  [ContentTypeWriter]
  public class FarseerPolygonContainerWriter : ContentTypeWriter<PolygonContainer>
  {
    protected override void Write(ContentWriter output, PolygonContainer container)
    {
      output.Write(container.Count);
      foreach (KeyValuePair<string,Polygon> p in container)
      {
        output.Write(p.Key);
        output.Write(p.Value.closed);
        output.Write(p.Value.vertices.Count);
        foreach (Vector2 vec in p.Value.vertices)
        {
          output.Write(vec);
        }
      }
    }

    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      return typeof(FarseerPolygonContainerReader).AssemblyQualifiedName;
    }
  }
}
