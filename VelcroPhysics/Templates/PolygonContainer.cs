using System.Collections.Generic;
using VelcroPhysics.Shared;
using VelcroPhysics.Tools.Triangulation.TriangulationBase;

namespace VelcroPhysics.Templates
{
    public class PolygonContainer : Dictionary<string, Polygon>
    {
        private bool _decomposed;

        public bool IsDecomposed
        {
            get { return _decomposed; }
        }

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
                        Remove(key);
                        for (int i = 0; i < partition.Count; i++)
                        {
                            this[key + "_" + i] = new Polygon(partition[i], true);
                        }
                    }
                    _decomposed = true;
                }
            }
        }
    }
}