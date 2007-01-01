using System;
using System.Collections.Generic;
using System.Text;

using FarseerGames.FarseerXNAPhysics.Collisions;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    public class CircleRigidBody : PolygonRigidBody {
        public CircleRigidBody() {
        }

        public CircleRigidBody(float radius, int edgeCount, float mass) {
            CircleRigidBodyConstructor(radius, edgeCount, mass, _collisionPrecision, _collisionPrecisionType);
        }

        public CircleRigidBody(float radius, int edgeCount, float mass, float collisionPrecisionFactor) {
            CircleRigidBodyConstructor(radius, edgeCount, mass, collisionPrecisionFactor, _collisionPrecisionType);
        }

        public CircleRigidBody(float radius, int edgeCount, float mass, float collisionPrecision, CollisionPrecisionType collisionPrecisionType) {
            _collisionPrecisionType = collisionPrecisionType;
            CircleRigidBodyConstructor(radius, edgeCount, mass, collisionPrecision,_collisionPrecisionType);
        }

        private void CircleRigidBodyConstructor(float radius, int edgeCount, float mass, float collisionPrecision, CollisionPrecisionType collisionPrecisionType) {
            _collisionPrecision = collisionPrecision;
            float momentOfInertia = .5f * Mass * radius * radius;
            Vertices vertices = Vertices.CreateCircle(radius, edgeCount);
            base.PolygonRigidBodyConstructor(mass, momentOfInertia, vertices, _collisionPrecision, _collisionPrecisionType);
        }

        public void SetGeometry(float radius, int edgeCount) {
            Vertices vertices = Vertices.CreateCircle(radius,edgeCount);
            base.SetGeometry(vertices);
        }
    }
}
