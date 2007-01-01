using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Mathematics;
using FarseerGames.FarseerXNAPhysics.Dynamics;
using FarseerGames.FarseerXNAPhysics.Collisions;

namespace FarseerGames.FarseerXNAPhysics {
    public class PhysicsSimulator {
        RigidBodyList _rigidBodyList;
        BodyList _bodyList; //no colliding bodies.
        ArbiterList _arbiterList;
        SpringList _springList;
        JointList _jointList;

        private Vector2 _gravity = Vector2.Zero;
        private int _iterations = 10;
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
            _springList = new SpringList();
            _jointList = new JointList();
            _gravity = gravity;
        }

        public Vector2 Gravity {
            get { return _gravity; }
            set { _gravity = value; }
        }

        public int Iterations {
            get { return _iterations; }
            set { _iterations = value; }
        }

        public float AllowedPenetration {
            get { return _allowedPenetration; }
            set { _allowedPenetration = value; }
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
            if (!_rigidBodyList.Contains(rigidBody)) {
                _rigidBodyList.Add(rigidBody);
            }
        }

        public void Remove(RigidBody rigidBody) {
            _rigidBodyList.Remove(rigidBody);
        }

        public void Add(Body body) {
            if (!_bodyList.Contains(body)) {
                _bodyList.Add(body);
            }
        }

        public void Remove(Body body) {
            _bodyList.Remove(body);
        }

        public void AddSpring(Spring spring) {
            if (!_springList.Contains(spring)) {
                _springList.Add(spring);
            }
        }

        public void RemoveSpring(Spring spring) {
            _springList.Remove(spring);
        }

        public void AddJoint(Joint joint) {
            if (!_jointList.Contains(joint)) {
                _jointList.Add(joint);
            }
        }

        public void RemoveJoint(Joint joint) {
            _jointList.Remove(joint);
        }

        public void Reset() {
            _rigidBodyList.Clear();
            _bodyList.Clear();
            _jointList.Clear();
            _springList.Clear();
        }

        public void Update(float dt) {
            if (dt == 0) { return; }
            //remove all arbiters that contain 1 or more disposed rigid bodies.
            _rigidBodyList.RemoveAll(RigidBodyList.IsDisposed);
            _springList.RemoveAll(SpringList.IsDisposed);
            _jointList.RemoveAll(JointList.IsDisposed);

            _arbiterList.RemoveAll(ArbiterList.ContainsDisposedBody);

            DoBroadPhaseCollision();
            DoNarrowPhaseCollision();
            ApplyForces(dt);
            ApplyImpulses(dt);
            UpdatePositions(dt);
        }

        public void DoBroadPhaseCollision() {
            RigidBody rigidBodyA;
            RigidBody rigidBodyB;

            for (int i = 0; i < _rigidBodyList.Count - 1; i++) {
                for (int j = i + 1; j < _rigidBodyList.Count; j++) {
                    rigidBodyA = _rigidBodyList[i];
                    rigidBodyB = _rigidBodyList[j];
                    //possible early exits
                    if ((rigidBodyA.CollisionGroup == rigidBodyB.CollisionGroup) && rigidBodyA.CollisionGroup != 0 && rigidBodyB.CollisionGroup != 0) {
                        continue;
                    }

                    if (!rigidBodyA.CollisionEnabled || !rigidBodyB.CollisionEnabled) {
                        continue;
                    }

                    if (rigidBodyA.IsStatic && rigidBodyB.IsStatic) { //don't collide two static bodies
                        continue;
                    }

                    if (AABB.Intersect(rigidBodyA.Geometry.AABB, rigidBodyB.Geometry.AABB)) {
                        Arbiter arbiter = new Arbiter(rigidBodyA, rigidBodyB, _allowedPenetration, _biasFactor, _maxContactsPerRigidBodyPair);
                        if (!_arbiterList.Contains(arbiter)) {
                            _arbiterList.Add(arbiter);
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

        public CollisionPoint CollidePoint(Vector2 point) {
            foreach(RigidBody rigidBody in _rigidBodyList){
                if(rigidBody.Collide(point)){
                    return new CollisionPoint(true, rigidBody);
                }
            }
            return new CollisionPoint(false, null);
        }

        public void ApplyForces(float dt) {
            for (int i = 0; i < _springList.Count; i++) {
                _springList[i].Update(dt);
            }

            for (int i = 0; i < _rigidBodyList.Count; i++) {
                _rigidBodyList[i].ApplyForce(_gravity * _rigidBodyList[i].Mass);
                _rigidBodyList[i].IntegrateVelocity(dt);
                _rigidBodyList[i].ClearForce();
                _rigidBodyList[i].ClearTorque();
            }
        }

        public void ApplyImpulses(float dt) {

            float inverseDt = 1f / dt;

            for (int i = 0; i < _jointList.Count; i++) {
                _jointList[i].PreStep(inverseDt);
            }

            for (int i = 0; i < _arbiterList.Count; i++) {
                _arbiterList[i].PreStepImpulse(inverseDt);
            }

            for (int h = 0; h < _iterations; h++) {

                for (int i = 0; i < _jointList.Count; i++) {
                    _jointList[i].Update();
                }

                for (int i = 0; i < _arbiterList.Count; i++) {
                    _arbiterList[i].ApplyImpulse();
                }
                

            }

            //for (int h = 0; h < _iterations; h++) {
            //    for (int i = 0; i < _arbiterList.Count; i++) {
            //        _arbiterList[i].ApplyImpulse();
            //    }
            //}

            //for (int i = 0; i < _jointList.Count; i++) {
            //    _jointList[i].PreStep(inverseDt);
            //}

            //for (int h = 0; h < _iterations; h++) {
            //    for (int i = 0; i < _jointList.Count; i++) {
            //        _jointList[i].Update();
            //    }
            //}

            //for (int i = 0; i < _arbiterList.Count; i++) {
            //    _arbiterList[i].PreStepImpulse(inverseDt);
            //}

            //for (int h = 0; h < _iterations; h++) {
            //    for (int i = 0; i < _arbiterList.Count; i++) {
            //        _arbiterList[i].ApplyImpulse();
            //    }
            //}

        }

        public void UpdatePositions(float dt) {
            for (int i = 0; i < _rigidBodyList.Count; i++) {
                _rigidBodyList[i].IntegratePosition(dt);
            }
        }
    }

    public class CollisionPoint {
        public bool IsCollison;
        public RigidBody RigidBody;

        public CollisionPoint(bool isCollision, RigidBody rigidBody) {
            IsCollison = isCollision;
            RigidBody = rigidBody;
        }
    }
}
