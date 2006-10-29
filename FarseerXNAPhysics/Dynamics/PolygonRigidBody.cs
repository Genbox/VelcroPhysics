using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Collisions;
using FarseerGames.FarseerXNAPhysics.Mathematics;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    public class PolygonRigidBody : RigidBody {
        private float collionPrecisionFactor = .25f;

        public PolygonRigidBody() : base() {

        }

        public PolygonRigidBody(float mass, Vertices vertices)
            : base() {

            InitializeGeometry(vertices);

            //setup a default precision if necassary
            float collisionPrecision = Math.Min(geometry.AABB.Width, geometry.AABB.Height) * collionPrecisionFactor; 
            InitializeGrid(collisionPrecision, 0, true);
            
            //by default, estimate moment of inertia to be MOI for original bounding box.
            float momentOfInertia = mass * (Geometry.AABB.Width * Geometry.AABB.Width + Geometry.AABB.Height * Geometry.AABB.Height) / 12;
            InitializeBody(mass, momentOfInertia);
        }

        private void InitializeBody(float mass, float momentOfInertia) {
            Mass = mass;
            MomentOfInertia = momentOfInertia;
        }

        private void InitializeGeometry(Vertices vertices) {
            Geometry = new PolygonGeometry(vertices);
        }

        private void InitializeGrid(float collisionPrecision, float padding, bool prime) {            
            Grid = new Grid(geometry, collisionPrecision, 0, true);
        }
    }
}
