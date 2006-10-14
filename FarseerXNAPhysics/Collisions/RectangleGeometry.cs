using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Mathematics;

namespace FarseerGames.FarseerXNAPhysics.Collisions {
    public class RectangleGeometry : Geometry  {


        public  RectangleGeometry(float width, float height) : base() {
            Initialize(width, height);
        }

        private void Initialize(float width, float height) {
            Vertices vertices = Vertices.CreateRectangle(width, height);
            SetVertices(vertices);
            //TODO decided how to better set maxEdgeLength
            //float maxEdgeLength = Math.Min(_width, _height) * .25f;
            //SubDivideEdges(_maxEdgeLength);
        }        
    }
}
