using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Collisions;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    public class RectangleRigidBody : PolygonRigidBody {
        
        public RectangleRigidBody() : base() {
        }

        public RectangleRigidBody(float width, float height, float mass) {
            RectangleRigidBodyConstructor(width, height, mass, _collisionPrecision,_collisionPrecisionType);
        }

        public RectangleRigidBody(float width, float height, float mass, float collisionPrecision) {
            RectangleRigidBodyConstructor(width, height, mass, collisionPrecision, _collisionPrecisionType);
        }

        public RectangleRigidBody(float width, float height, float mass, float collisionPrecisionFactor, CollisionPrecisionType collisionPrecisionType) {
            _collisionPrecisionType = collisionPrecisionType;
            RectangleRigidBodyConstructor(width, height, mass, collisionPrecisionFactor, _collisionPrecisionType);
        }

        private void RectangleRigidBodyConstructor(float width, float height, float mass, float collisionPrecisionFactor, CollisionPrecisionType collisionPrecisionType) {
            _collisionPrecision = collisionPrecisionFactor;
            float momentOfInertia = mass * (width * width + height * height) / 12;
            Vertices vertices = Vertices.CreateRectangle(width, height);
            base.PolygonRigidBodyConstructor(mass, momentOfInertia, vertices, _collisionPrecision, _collisionPrecisionType);
        }

        public void SetGeometry(float width, float height) {
            Vertices vertices = Vertices.CreateRectangle(width, height);
            base.SetGeometry(vertices);
        }
    }
}
