using System;
using System.Collections.Generic;
using System.Text;

using FarseerGames.FarseerPhysics.Mathematics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Collisions;

namespace FarseerGames.FarseerPhysics.Collisions {
    public class BruteForceCollider : IBroadPhaseCollider   {
        #region IBroadPhaseCollider Members
        PhysicsSimulator physicsSimulator;

        public BruteForceCollider(PhysicsSimulator physicsSimulator) {
            this.physicsSimulator = physicsSimulator;
        }

        public void ProcessRemovedGeoms() {
            //not required by brute force collider
        }

        public void ProcessDisposedGeoms() {
            //not required by brute force collider
        }

        public void Add(Geom geom) {
            //not 
        }

        public void Update() {
            DoCollision();
        }

        Geom geometryA;
        Geom geometryB;
        Arbiter arbiter;
        private void DoCollision() {
            for (int i = 0; i < physicsSimulator.geomList.Count - 1; i++) {
                for (int j = i + 1; j < physicsSimulator.geomList.Count; j++) {
                    geometryA = physicsSimulator.geomList[i];
                    geometryB = physicsSimulator.geomList[j];
                    //possible early exits
                    if (!geometryA.body.enabled || !geometryB.body.enabled) {
                        continue;
                    }

                    if ((geometryA.collisionGroup == geometryB.collisionGroup) && geometryA.collisionGroup != 0 && geometryB.collisionGroup != 0) {
                        continue;
                    }

                    if (!geometryA.collisionEnabled || !geometryB.collisionEnabled) {
                        continue;
                    }

                    if (geometryA.body.isStatic && geometryB.body.isStatic) { //don't collide two static bodies
                        continue;
                    }

                    if (geometryA.body == geometryB.body) { //don't collide two geometries connected to the same body
                        continue;
                    }

                    if (((geometryA.collisionCategories & geometryB.collidesWith) == Enums.CollisionCategories.None) & ((geometryB.collisionCategories & geometryA.collidesWith) == Enums.CollisionCategories.None)) {
                        continue;
                    }

                    bool intersection = true;
                    #region INLINE: if (AABB.Intersect(geometryA.aabb, geometryB.aabb)) ....

                    if (geometryA.aabb.min.X > geometryB.aabb.max.X || geometryB.aabb.min.X > geometryA.aabb.max.X) {
                        intersection = false;
                    }
                    else if (geometryA.aabb.min.Y > geometryB.aabb.Max.Y || geometryB.aabb.min.Y > geometryA.aabb.Max.Y) {
                        intersection = false;
                    }
                    #endregion


                    if (intersection) {
                        arbiter = physicsSimulator.arbiterPool.Fetch();
                        arbiter.ConstructArbiter(geometryA, geometryB, physicsSimulator);

                        if (!physicsSimulator.arbiterList.Contains(arbiter)) {
                            physicsSimulator.arbiterList.Add(arbiter);
                        }
                        else {
                            physicsSimulator.arbiterPool.Release(arbiter);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
