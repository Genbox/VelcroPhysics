using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Mathematics;

namespace FarseerGames.FarseerXNAPhysics.Collisions {
    public class RectangleGeometry : Geometry  {
        private float _maxEdgeLength;
        private float _width = 1;
        private float _height = 1;

        public  RectangleGeometry(float width, float height) : base() {
            _width = width;
            _height = height;
            Initialize();
        }

        public void Initialize() {
            Vertices vertices = Vertices.CreateRectangle(_width, _height);
            SetVertices(vertices);
            //TODO decided how to better set maxEdgeLength
            _maxEdgeLength = Math.Min(_width, _height) * .25f;
            SubDivideEdges(_maxEdgeLength);
        }        
    }
}
