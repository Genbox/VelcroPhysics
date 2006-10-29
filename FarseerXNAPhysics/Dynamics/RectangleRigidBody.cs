using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Collisions;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    public class RectangleRigidBody : RigidBody {
        private float collisionPrecisionFactor = .25f;
        private RectangleRigidBody() {
        }

        public RectangleRigidBody(float width, float height, float mass) {
            InitializeBody(width, height, mass);
            InitializeGeometry(width, height);

            float collisionPrecision = Math.Min(geometry.AABB.Width, geometry.AABB.Height) * collisionPrecisionFactor;

            InitializeGrid(collisionPrecision, 0, true);
        }

        private void InitializeBody(float width, float height, float mass) {
            Mass = mass;
            MomentOfInertia = Mass * (width * width + height * height) / 12;
        }

        private void InitializeGeometry(float width, float height) {
            Geometry = new RectangleGeometry(width, height);
        }

        private void InitializeGrid(float collisionPrecision, float padding, bool prime) {
            Grid = new Grid(geometry, collisionPrecision, 0, true); 
        }
    }
}
