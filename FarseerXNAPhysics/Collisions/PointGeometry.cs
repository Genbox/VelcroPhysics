using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerXNAPhysics.Collisions {
    public class PointGeometry : Geometry {
        public PointGeometry()
            : base() {
            Initialize();
        }

        private void Initialize() {
            Vertices vertices = new Vertices();
            vertices.Add(Vector2.Zero);
            SetVertices(vertices);
        }
    }
}
