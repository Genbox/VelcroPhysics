using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Mathematics;
using FarseerGames.FarseerXNAPhysics.Dynamics;
using FarseerGames.FarseerXNAPhysics.Collisions;

namespace FarseerGames.FarseerXNAPhysics {
    public class PhysicsSimulator : PhysicsSimulatorBase {
        RigidBodyList _rigidBodyList;
        ArbiterList _arbiterList;

        private int _iterations = 5;
        private float _allowedPenetration = .01f;
        private float _biasFactor = .8f;
        private int _maxContactsPerRigidBodyPair = 5;

        public PhysicsSimulator() {
            PhysicsConstructor(Vector2.Zero);
        }

        public PhysicsSimulator(Vector2 gravity) {
            PhysicsConstructor(gravity);
        }

        private void PhysicsConstructor(Vector2 gravity) {
            _rigidBodyList = new RigidBodyList();
            _arbiterList = new ArbiterList();
            _gravity = gravity;
        }

        public int Iterations {
            get { return _iterations; }
            set { _iterations = value; }
        }

        public float AllowedPenetrations {
            get { return _allowedPenetration ; }
            set { _allowedPenetration  = value; }
        }

        public float BiasFactor {
            get { return _biasFactor; }
            set { _biasFactor = value; }
        }

        public int MaxContactsPerRigidBodyPair {
            get { return _maxContactsPerRigidBodyPair; }
            set { _maxContactsPerRigidBodyPair = value; }
        }
	
        public void Add(RigidBody rigidBody) {
            _rigidBodyList.Add(rigidBody);
        }

        public void Remove(RigidBody rigidBody) {
            _rigidBodyList.Remove(rigidBody);
        }

        public override void Update(float dt) {
            DoBroadPhaseCollision();
            DoNarrowPhaseCollision();
            ApplyForces(dt);
            ApplyImpulses(dt);
            UpdatePositions(dt);
        }

        public void DoBroadPhaseCollision() {
            RigidBody rigidBodyA;
            RigidBody rigidBodyB;

            for (int i = 0; i < _rigidBodyList.Count - 1; i++)
            {
                for (int j = i + 1; j < _rigidBodyList.Count; j++)
                {
                    rigidBodyA = _rigidBodyList[i];
                    rigidBodyB = _rigidBodyList[j];
                    if (!rigidBodyA.IsStatic || !rigidBodyB.IsStatic) { //don't collide two static bodies
                        if (AABB.Intersect(rigidBodyA.Geometry.AABB, rigidBodyB.Geometry.AABB)) {
                            Arbiter arbiter = new Arbiter(rigidBodyA, rigidBodyB, _allowedPenetration, _biasFactor, _maxContactsPerRigidBodyPair);
                            if (!_arbiterList.Contains(arbiter)) {
                                _arbiterList.Add(arbiter);
                            }
                        }
                    }
                }
            }
        }

        public void DoNarrowPhaseCollision() {
            for (int i = 0; i < _arbiterList.Count; i++) {
                _arbiterList[i].Collide();
            }
            _arbiterList.RemoveAll(ArbiterList.ContactCountEqualsZero);
        }

        public void ApplyForces(float dt) {
            for (int i = 0; i < _rigidBodyList.Count; i++) {
                _rigidBodyList[i].ApplyForce(_gravity*_rigidBodyList[i].Mass);
                _rigidBodyList[i].IntegrateVelocity(dt);
                _rigidBodyList[i].ClearForce();
                _rigidBodyList[i].ClearTorque();
            }           
        }

        public void ApplyImpulses(float dt) {
            
            float inverseDt = 1f/dt;
            for (int i = 0; i < _arbiterList.Count; i++) {
                _arbiterList[i].PreStepImpulse(inverseDt);
            }

            for (int h = 0; h < _iterations; h++) {
                for (int i = 0; i < _arbiterList.Count; i++) {
                    _arbiterList[i].ApplyImpulse();
                }
            }
        }

        public void UpdatePositions(float dt) {
            for (int i = 0; i < _rigidBodyList.Count; i++) {
                _rigidBodyList[i].IntegratePosition(dt);
            }
        }

        public void SetAllowedPenetration(float allowedPenetration) {

        }

        public void SetBiasFactor(float biasFactor) {

        }

        public void SetMaxContactsPerRigidBodyPair(int maxContacts) {

        }
    }
}
