using System;
using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;

namespace FarseerPhysics.ContentPipeline
{
  public struct Polygon
  {
    public Vertices vertices;
    public bool closed;

    public Polygon(Vertices v, bool closed)
    {
      vertices = v;
      this.closed = closed;
    }
  }

  public class PolygonContainer : Dictionary<string, Polygon>
  {
    public bool IsDecomposed
    {
      get { return _decomposed; }
    }
    private bool _decomposed = false;

    public void Decompose()
    {
      Dictionary<string, Polygon> containerCopy = new Dictionary<string, Polygon>(this);
      foreach (string key in containerCopy.Keys)
      {
        if (containerCopy[key].closed)
        {
          List<Vertices> partition = BayazitDecomposer.ConvexPartition(containerCopy[key].vertices);
          if (partition.Count > 1)
          {
            this.Remove(key);
            for (int i = 0; i < partition.Count; i++)
            {
              this[key + "_" + i.ToString()] = new Polygon(partition[i], true);
            }
          }
          _decomposed = true;
        }
      }
    }
  }
}