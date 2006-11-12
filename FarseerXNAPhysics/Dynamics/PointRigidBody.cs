using System;
using System.Collections.Generic;
using System.Text;

using FarseerGames.FarseerXNAPhysics.Collisions;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    public class PointRigidBody : PolygonRigidBody  {
        public PointRigidBody() {
        }

        public PointRigidBody(float mass) {
            PointRigidBodyConstructor(mass);
        }

        private void PointRigidBodyConstructor(float mass) {
            Mass = mass;
            MomentOfInertia = 1;
            InitializeGeometry();
            Grid = null;
        }

        private void InitializeGeometry() {
            Geometry = new PointGeometry();
        }
    }
} 
