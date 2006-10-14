using System;
using System.Collections.Generic;
using System.Text;

using FarseerGames.FarseerXNAPhysics.Collisions;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    public class PointRigidBody : RigidBody {
        private PointRigidBody() {
        }

        public PointRigidBody(float mass) {
            InitializeBody(mass);
            InitializeGeometry();
           InitializeGrid();
        }

        private void InitializeBody(float mass) {
            Mass = mass;
            MomentOfInertia = 1;
        }

        private void InitializeGeometry() {
            Geometry = new PointGeometry();
        }

        private void InitializeGrid() {
            Grid = null;
        }
    }
}
