using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using FarseerPhysics.Common;

namespace FarseerPhysics.Content
{
  public class FarseerPolygonContainerReader : ContentTypeReader<PolygonContainer>
  {
    protected override PolygonContainer Read(ContentReader input, PolygonContainer existingInstance)
    {
      PolygonContainer paths = existingInstance;

      if (paths == null)
      {
        paths = new PolygonContainer();
      }

      int count = input.ReadInt32();
      for (int i = 0; i < count; i++)
      {
        string name = input.ReadString();
        bool closed = input.ReadBoolean();
        int vertsCount = input.ReadInt32();
        Vertices verts = new Vertices();
        for (int j = 0; j < vertsCount; j++)
        {
          verts.Add(input.ReadVector2());
        }
        paths[name] = new Polygon(verts, closed);
      }

      return paths;
    }
  }
}
