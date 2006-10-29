using System;
using System.Collections.Generic;
using System.Text;

using FarseerGames.FarseerXNAPhysics.Collisions;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    public class CircleRigidBody : RigidBody {
        private float collisionPrecisionFactor = .25f;
        private CircleRigidBody() {
        }

        public CircleRigidBody(float radius, int edgeCount, float mass) {
            InitializeBody(radius, mass);
            InitializeGeometry(radius, edgeCount);

            float collisionPrecision = Math.Min(geometry.AABB.Width, geometry.AABB.Height) * collisionPrecisionFactor;

            InitializeGrid(collisionPrecision, 0, true);
        }

        private void InitializeBody(float radius, float mass) {
            Mass = mass;
            MomentOfInertia = .5f * Mass * radius * radius;
        }

        private void InitializeGeometry(float radius, int edgeCount) {
            Geometry = new CircleGeometry(radius, edgeCount);
        }

        private void InitializeGrid(float collisionPrecision, float padding, bool prime) {
            Grid = new Grid(geometry, collisionPrecision, 0, true); 
        }
    }
}
