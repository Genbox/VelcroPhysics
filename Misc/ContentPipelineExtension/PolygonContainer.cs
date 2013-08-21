using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;

namespace FarseerPhysics.ContentPipeline
{
    public struct Polygon
    {
        public Vertices Vertices;
        public bool Closed;

        public Polygon(Vertices v, bool closed)
        {
            Vertices = v;
            Closed = closed;
        }
    }

    public class PolygonContainer : Dictionary<string, Polygon>
    {
        public bool IsDecomposed { get; private set; }

        public void Decompose()
        {
            Dictionary<string, Polygon> containerCopy = new Dictionary<string, Polygon>(this);
            foreach (string key in containerCopy.Keys)
            {
                if (containerCopy[key].Closed)
                {
                    List<Vertices> partition = Triangulate.ConvexPartition(containerCopy[key].Vertices, TriangulationAlgorithm.Bayazit);
                    if (partition.Count > 1)
                    {
                        this.Remove(key);
                        for (int i = 0; i < partition.Count; i++)
                        {
                            this[key + "_" + i] = new Polygon(partition[i], true);
                        }
                    }
                    IsDecomposed = true;
                }
            }
        }
    }
}