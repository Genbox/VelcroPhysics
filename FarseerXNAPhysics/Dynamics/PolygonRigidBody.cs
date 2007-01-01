using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Collisions;
using FarseerGames.FarseerXNAPhysics.Mathematics;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    /// <summary>
    /// Represents a polygon riged body and acts as base class for other polygon primatives: rectangle, circle, point.
    /// <para>
    /// By default, this object will use the "Relative" collison precision type. <see cref="CollisionPrecisionType"/></para>
    /// <para>
    /// To manually construct this object, set all the properties then call SetGeometry and SetGrid in succession to 
    /// initialize the collison capability.</para>
    /// </summary>
    public class PolygonRigidBody : RigidBody {
        protected float _collisionPrecision = .1f;
        protected CollisionPrecisionType _collisionPrecisionType = CollisionPrecisionType.Relative;

        public PolygonRigidBody() : base() {

        }

        public PolygonRigidBody(float mass, Vertices vertices) {
            this.PolygonRigidBodyConstructor(mass, null, vertices, _collisionPrecision,_collisionPrecisionType);
        }

        public PolygonRigidBody(float mass, Vertices vertices, float collisionPrecision)
            : base() {
            PolygonRigidBodyConstructor(mass, null, vertices, collisionPrecision, _collisionPrecisionType);
        }

        public PolygonRigidBody(float mass, float momentOfInertia, Vertices vertices, float collisionPrecision)
            : base() {
            PolygonRigidBodyConstructor(mass, momentOfInertia, vertices, _collisionPrecision, _collisionPrecisionType);
        }

        public PolygonRigidBody(float mass, float momentOfInertia, Vertices vertices, float collisionPrecision, CollisionPrecisionType collisionPrecisionType)
            : base() {
            _collisionPrecisionType = collisionPrecisionType;
            PolygonRigidBodyConstructor(mass, momentOfInertia, vertices, _collisionPrecision, _collisionPrecisionType);            
        }

        protected void PolygonRigidBodyConstructor(float mass, float? momentOfInertia, Vertices vertices, float collisionPrecision, CollisionPrecisionType collisionPrecisionType) {
            _collisionPrecision = collisionPrecision;
            SetGeometry(vertices);
            SetGrid();  
          
            //if moment of inertia is not provided, estimate it by computing the moment of inertia for the AABB
            if (momentOfInertia == null) {
                momentOfInertia = mass * (Geometry.AABB.Width * Geometry.AABB.Width + Geometry.AABB.Height * Geometry.AABB.Height) / 12;
            }

            Mass = mass;
            MomentOfInertia = (float)momentOfInertia;
        }

        public void SetGeometry(Vertices vertices) {
            Geometry = new PolygonGeometry(vertices);           
        }

        public void SetGrid() {
            float gridCellSize;
            if (_geometry == null) { throw new NullReferenceException("Geomerty must be set prior to calling 'InitializeGrid'"); }

            switch (_collisionPrecisionType) {
                case CollisionPrecisionType.Relative:
                    gridCellSize = Math.Min(_geometry.AABB.Width, _geometry.AABB.Height) * _collisionPrecision;
                    break;
                case CollisionPrecisionType.Absolute:
                    gridCellSize = _collisionPrecision;
                    break;
                default:
                    gridCellSize = Math.Min(_geometry.AABB.Width, _geometry.AABB.Height) * _collisionPrecision;
                    break;
            }
            Grid = new Grid(_geometry, gridCellSize, 0, true);
        }
    }
}
