using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Mathematics;
using FarseerGames.FarseerXNAPhysics.Dynamics;
using FarseerGames.FarseerXNAPhysics.Collisions;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {

    /// <summary>
    /// Much of the code and ideas in this class were taken from a paper entitled
    /// "Fast and Simple Physics using Sequential Impulses" by Erin Catto
    /// </summary>
    internal class Arbiter : IEquatable<Arbiter> {

        protected RigidBody _rigidBodyA;
        protected RigidBody _rigidBodyB;        
        private ContactList _contactList;
        private ContactList _newContactList;
        private ContactList _mergedContactList;
        private int _maxContacts = 5;
        private float _allowedPenetration = .01f;
        private float _biasFactor = .8f;
        private float _frictionCoefficientCombined;

        private Arbiter() { }

        internal Arbiter(RigidBody rigidBody1, RigidBody rigidBody2, float allowedPenetration, float biasFactor, int maxContacts) {
            _allowedPenetration = allowedPenetration;
            _biasFactor = biasFactor;
            _maxContacts = maxContacts;

            if (rigidBody1 < rigidBody2) {
                _rigidBodyA = rigidBody1;
                _rigidBodyB = rigidBody2;
            }
            else {
                _rigidBodyA = rigidBody2;
                _rigidBodyB = rigidBody1;
            }
            _frictionCoefficientCombined = (float)Math.Sqrt(_rigidBodyA.FrictionCoefficient * _rigidBodyB.FrictionCoefficient);
            InitializeContactLists(_maxContacts);
        }
        
        internal float AllowedPenetration {
            get { return _allowedPenetration; }
            set { _allowedPenetration = value; }
        }

        internal float BiasFactor {
            get { return _biasFactor; }
            set { _biasFactor = value; }
        }

        internal int MaxContacts {
            get { return _maxContacts; }
            set {
                _maxContacts = value;
                InitializeContactLists(_maxContacts);       
            }   
        }

        internal int ContactCount {
            get { return _contactList.Count; }
        }

        internal bool ContainsDisposedRigidBody() {
            return _rigidBodyA.IsDisposed || _rigidBodyB.IsDisposed;
        }

        internal void Collide() {            
            _newContactList.Clear();
            _rigidBodyA.Collide(_rigidBodyB, _newContactList);
            _mergedContactList.Clear();

            for (int i = 0; i < _newContactList.Count; i++) {
                int index = _contactList.IndexOf(_newContactList[i]);
                if (index > -1) {
                    //continuation of collision
                    Contact contact = _newContactList[i];
                    contact.NormalImpulse = _contactList[index].NormalImpulse;
                    contact.TangentImpulse = _contactList[index].TangentImpulse;
                    _mergedContactList.Add(contact);
                }
                else {
                    //first time collision
                    _mergedContactList.Add(_newContactList[i]);
                }                   
            }
            _contactList = new ContactList(_mergedContactList);

            //sort by seperation (depth)
            _contactList.Sort(CompareDepth);
        }

        private int CompareDepth(Contact c1, Contact c2) {
            if(c1.Seperation<c2.Seperation){
                return -1;
            }else if(c1.Seperation == c2.Seperation){
                return 0;
            }else{
                return 1;
            }
        }

        internal void PreStepImpulse(float inverseDt) {
            Contact contact;
            for (int i = 0; i < _contactList.Count; i++) {
 
                contact = _contactList[i];               

                Vector2 r1 = Vector2.Subtract(contact.Position, _rigidBodyA.Position);
                Vector2 r2 = Vector2.Subtract(contact.Position, _rigidBodyB.Position);

                float rn1 = Vector2.Dot(r1, contact.Normal);
                float rn2 = Vector2.Dot(r2, contact.Normal);

                float kNormal = _rigidBodyA.InverseMass + _rigidBodyB.InverseMass;
                kNormal += _rigidBodyA.InverseMomentOfInertia * (Vector2.Dot(r1, r1) - rn1 * rn1) + _rigidBodyB.InverseMomentOfInertia * (Vector2.Dot(r2, r2) - rn2 * rn2);
                contact.MassNormal = 1f / kNormal;

                //tangent
                Vector2 tangent = Calculator.Cross(contact.Normal, 1);
                float rt1 = Vector2.Dot(r1, tangent);
                float rt2 = Vector2.Dot(r2, tangent);
                float kTangent = _rigidBodyA.InverseMass + _rigidBodyB.InverseMass;
                kTangent += _rigidBodyA.InverseMomentOfInertia * (Vector2.Dot(r1, r1) - rt1 * rt1) + _rigidBodyB.InverseMomentOfInertia * (Vector2.Dot(r2, r2) - rt2 * rt2);
                contact.MassTangent = 1f / kTangent;

                float min = Math.Min(0,_allowedPenetration + contact.Seperation);
                contact.NormalVelocityBias = -_biasFactor * inverseDt * min;
                //apply impulses
                Vector2 impulse = Vector2.Multiply(contact.Normal,contact.NormalImpulse) + Vector2.Multiply(tangent,contact.TangentImpulse);

                //calculate restitution
                // Compute the restitution, we average the restitution of the two bodies
                float restitution = (2.0f + _rigidBodyA.RestitutionCoefficient + _rigidBodyB.RestitutionCoefficient) * 0.5f;

                _rigidBodyA.LinearVelocity -= Vector2.Multiply(impulse * restitution, _rigidBodyA.InverseMass);
                _rigidBodyA.AngularVelocity -= _rigidBodyA.InverseMomentOfInertia * Calculator.Cross(r1, impulse);

                _rigidBodyB.LinearVelocity += Vector2.Multiply(impulse * restitution, _rigidBodyB.InverseMass);
                _rigidBodyB.AngularVelocity += _rigidBodyB.InverseMomentOfInertia * Calculator.Cross(r2, impulse);
                contact.NormalImpulseBias = 0;
                _contactList[i] = contact;
            }
        }

        Contact contact;
        internal void ApplyImpulse() {
            for (int i = 0; i < _contactList.Count; i++) {
 
                contact = _contactList[i];
                
                contact.R1 = Vector2.Subtract(contact.Position, _rigidBodyA.Position);
                contact.R2 = Vector2.Subtract(contact.Position, _rigidBodyB.Position);

                //relative vel at contact
                Vector2 dv = _rigidBodyB.LinearVelocity + Calculator.Cross(_rigidBodyB.AngularVelocity, contact.R2) - _rigidBodyA.LinearVelocity - Calculator.Cross(_rigidBodyA.AngularVelocity, contact.R1);

                //compute normal impulse with bias
                float vn = Vector2.Dot(dv, contact.Normal);
                
                //float normalImpulse = contact.MassNormal * (-vn + contact.NormalVelocityBias); //comment for preserve momentum
                float normalImpulse = contact.MassNormal * -vn; //uncomment for preserve momentum

                //clamp accumulated impulse
                float oldNormalImpulse = contact.NormalImpulse;
                contact.NormalImpulse = Math.Max(oldNormalImpulse + normalImpulse, 0);
                normalImpulse = contact.NormalImpulse - oldNormalImpulse;

                //apply contact impulse
                Vector2 impulse = Vector2.Multiply(contact.Normal,normalImpulse);

                _rigidBodyA.LinearVelocity -= _rigidBodyA.InverseMass * impulse;
                _rigidBodyA.AngularVelocity -= _rigidBodyA.InverseMomentOfInertia * Calculator.Cross(contact.R1, impulse);

                _rigidBodyB.LinearVelocity += _rigidBodyB.InverseMass * impulse;
                _rigidBodyB.AngularVelocity += _rigidBodyB.InverseMomentOfInertia * Calculator.Cross(contact.R2, impulse);

                //bias preserves momentum
                 dv = _rigidBodyB.LinearVelocityBias + Calculator.Cross(_rigidBodyB.AngularVelocityBias, contact.R2) - _rigidBodyA.LinearVelocityBias - Calculator.Cross(_rigidBodyA.AngularVelocityBias, contact.R1);
                float normalVelocityBias = Vector2.Dot(dv, contact.Normal);
                float normalImpulseBias = contact.MassNormal * (-normalVelocityBias + contact.NormalVelocityBias);

                float normalImpulseBiasOriginal = contact.NormalImpulseBias;
                contact.NormalImpulseBias = Math.Max(normalImpulseBiasOriginal + normalImpulseBias, 0);
                normalImpulseBias = contact.NormalImpulseBias - normalImpulseBiasOriginal;

                Vector2 impulseBias = Vector2.Multiply(contact.Normal, normalImpulseBias);

                _rigidBodyA.LinearVelocityBias -= _rigidBodyA.InverseMass * impulseBias;
                _rigidBodyA.AngularVelocityBias -= _rigidBodyA.InverseMomentOfInertia * Calculator.Cross(contact.R1, impulseBias);

                _rigidBodyB.LinearVelocityBias += _rigidBodyB.InverseMass * impulseBias;
                _rigidBodyB.AngularVelocityBias += _rigidBodyB.InverseMomentOfInertia * Calculator.Cross(contact.R2, impulseBias);


                //relative vel at contact
                dv = _rigidBodyB.LinearVelocity + Calculator.Cross(_rigidBodyB.AngularVelocity, contact.R2) - _rigidBodyA.LinearVelocity - Calculator.Cross(_rigidBodyA.AngularVelocity, contact.R1);

                //compute friction impulse
                float maxTangentImpulse = _frictionCoefficientCombined * contact.NormalImpulse;

                Vector2 tangent = Calculator.Cross(contact.Normal, 1);
                float vt = Vector2.Dot(dv, tangent);
                float tangentImpulse = contact.MassTangent * (-vt);

                //clamp friction
                float oldTangentImpulse = contact.TangentImpulse;
                contact.TangentImpulse = Calculator.Clamp(oldTangentImpulse + tangentImpulse, -maxTangentImpulse, maxTangentImpulse);
                tangentImpulse = contact.TangentImpulse - oldTangentImpulse;

                //Apply contact impulse
                impulse = Vector2.Multiply(tangent,tangentImpulse);

                _rigidBodyA.LinearVelocity -= _rigidBodyA.InverseMass * impulse;
                _rigidBodyA.AngularVelocity -= _rigidBodyA.InverseMomentOfInertia * Calculator.Cross(contact.R1, impulse);

                _rigidBodyB.LinearVelocity += _rigidBodyB.InverseMass * impulse;
                _rigidBodyB.AngularVelocity += _rigidBodyB.InverseMomentOfInertia * Calculator.Cross(contact.R2, impulse);
                _contactList[i] = contact;
            }
        }

        private void InitializeContactLists(int maxContacts) {
            _contactList = new ContactList(maxContacts);
            _newContactList = new ContactList(maxContacts);
            _mergedContactList = new ContactList(maxContacts);
        }

        public bool Equals(Arbiter other) {
            return (_rigidBodyA == other._rigidBodyA) && (_rigidBodyB == other._rigidBodyB);
        }        

        public override bool Equals(object obj) {
            if (!(obj is Arbiter)) { throw new ArgumentException("The object being compared must be of type 'Arbiter'"); }
            return Equals((Arbiter)obj);
        }

        public override int GetHashCode() {
           return base.GetHashCode();
        }

        public static bool operator ==(Arbiter arbiter1, Arbiter arbiter2) {
            return arbiter1.Equals(arbiter2);
        }

        public static bool operator !=(Arbiter arbiter1, Arbiter arbiter2) {
            return !arbiter1.Equals(arbiter2);
        } 

    }
}
