using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Collisions;
using FarseerGames.FarseerXNAPhysics.Mathematics;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    public class PolygonRigidBody : RigidBody {
        public PolygonRigidBody() : base() {

        }

        public PolygonRigidBody(Vertices vertices)
            : base() {
            SetVertices(vertices);
        }

        public void SetVertices(Vertices vertices) {
            Geometry = new PolygonGeometry(vertices);
            //TODO the grid parameters more customizable.
            float collisionPrecision = Math.Min(_geometry.AABB.Width, _geometry.AABB.Height) * .25f; //setup a default precision if necassary
            _grid = new Grid(_geometry, collisionPrecision, 0, true);
        }
    }
}
