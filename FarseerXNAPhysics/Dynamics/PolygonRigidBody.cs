using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Collisions;
using FarseerGames.FarseerXNAPhysics.Mathematics;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    public class PolygonRigidBody : RigidBody {
        protected float _collisionPrecisionFactor = .1f;

        public PolygonRigidBody() : base() {

        }

        public PolygonRigidBody(float mass, Vertices vertices) {
            this.PolygonRigidBodyConstructor(mass, null,vertices, _collisionPrecisionFactor);
        }

        public PolygonRigidBody(float mass, Vertices vertices, float collisionPrecisionFactor)
            : base() {
            PolygonRigidBodyConstructor(mass, null, vertices, collisionPrecisionFactor);
        }

        public PolygonRigidBody(float mass, float momentOfInertia, Vertices vertices, float collisionPrecisionFactor)
            : base() {
            PolygonRigidBodyConstructor(mass, momentOfInertia, vertices, collisionPrecisionFactor);
        }

        protected void PolygonRigidBodyConstructor(float mass, float? momentOfInertia, Vertices vertices, float collisionPrecisionFactor) {
            _collisionPrecisionFactor = collisionPrecisionFactor;

            InitializeGeometry(vertices);

            //setup a default precision if necassary
            float gridSize = Math.Min(geometry.AABB.Width, geometry.AABB.Height) * _collisionPrecisionFactor;
            InitializeGrid(gridSize);
            
            //if moment of inertia is not provided, estimate it by computing the moment of inertia for the AABB
            if (momentOfInertia == null) {
                momentOfInertia = mass * (Geometry.AABB.Width * Geometry.AABB.Width + Geometry.AABB.Height * Geometry.AABB.Height) / 12;
            }

            Mass = mass;
            MomentOfInertia = (float)momentOfInertia;
        }

        public void InitializeGeometry(Vertices vertices) {
            Geometry = new PolygonGeometry(vertices);            
        }

        public void InitializeGrid(float gridSize) {
            if (geometry == null) { throw new NullReferenceException("Geomerty must be set prior to calling 'InitializeGrid'"); }
            Grid = new Grid(geometry, gridSize, 0, true);
        }
    }
}
