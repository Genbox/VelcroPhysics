using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Collisions;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    public class RectangleRigidBody : RigidBody {
        protected float _width = 1;
        protected float _height = 1;

        public RectangleRigidBody() {
        }


        public RectangleRigidBody(float width, float height, float mass) {
            _width = width;
            _height = height;
            Mass = mass;
            MomentOfInertia = Mass * (_width * _width + _height * _height) / 12;
           
            Geometry = new RectangleGeometry(_width, _height);
            float collisionPrecision = Math.Min(_geometry.AABB.Width, _geometry.AABB.Height) * .25f;
            Grid = new Grid(_geometry, collisionPrecision, 0, true); 
        }
    }
}
