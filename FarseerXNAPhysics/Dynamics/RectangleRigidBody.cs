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
            RectangleRigidBodyConstructor(width, height, mass, _collisionPrecisionFactor);
        }

        public RectangleRigidBody(float width, float height, float mass, float collisionPrecisionFactor) {
            RectangleRigidBodyConstructor(width, height, mass, collisionPrecisionFactor);
        }

        private void RectangleRigidBodyConstructor(float width, float height, float mass, float collisionPrecisionFactor) {
            _collisionPrecisionFactor = collisionPrecisionFactor;
            float momentOfInertia = mass * (width * width + height * height) / 12;

            Vertices vertices = Vertices.CreateRectangle(width, height);

            base.PolygonRigidBodyConstructor(mass, momentOfInertia, vertices, _collisionPrecisionFactor);
        }

        public void InitializeGeometry(float width, float height) {
            Vertices vertices = Vertices.CreateRectangle(width, height);
            base.InitializeGeometry(vertices);
        }
    }
}
