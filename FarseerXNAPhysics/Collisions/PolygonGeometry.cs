using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Mathematics;

namespace FarseerGames.FarseerXNAPhysics.Collisions {
    public class PolygonGeometry : Geometry {
        public PolygonGeometry(Vertices vertices): base(vertices) {
        }

        public PolygonGeometry(Vertices vertices, float maxEdgeLength)
            : base(vertices) {
        }
    }
}
