using System;
using System.Collections.Generic;
using System.Text;

using FarseerGames.FarseerXNAPhysics.Collisions;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    public class CircleRigidBody : PolygonRigidBody {
        private float collisionPrecisionFactor = .25f;
        public CircleRigidBody() {
        }

        public CircleRigidBody(float radius, int edgeCount, float mass) {
            CircleRigidBodyConstructor(radius, edgeCount, mass, _collisionPrecisionFactor);
        }

        public CircleRigidBody(float radius, int edgeCount, float mass, float collisionPrecisionFactor) {
            CircleRigidBodyConstructor(radius, edgeCount, mass, collisionPrecisionFactor);
        }

        private void CircleRigidBodyConstructor(float radius, int edgeCount, float mass, float collisionPrecisionFactor) {
            _collisionPrecisionFactor = collisionPrecisionFactor;
            float momentOfInertia = .5f * Mass * radius * radius;

            Vertices vertices = Vertices.CreateCircle(radius, edgeCount);

            base.PolygonRigidBodyConstructor(mass, momentOfInertia, vertices, _collisionPrecisionFactor);
        }

        public void InitializeGeometry(float radius, int edgeCount) {
            Vertices vertices = Vertices.CreateCircle(radius,edgeCount);
            base.InitializeGeometry(vertices);
        }
    }
}
