/// <summary>
/// Some of the code and ideas in this class were taken from a paper entitled
/// "Fast and Simple Physics using Sequential Impulses" by Erin Catto
/// </summary>

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;


using FarseerGames.FarseerPhysics.Mathematics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Collisions;
using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerPhysics.Dynamics {

    public class Arbiter : IEquatable<Arbiter> {
        private PhysicsSimulator physicsSimulator;
        private float frictionCoefficientCombined;
        
        public Geom geometryA;
        public Geom geometryB;
        internal ContactList contactList;
        private ContactList newContactList;
        private ContactList mergedContactList;

        //used for ref/out methods
        private Vector2 vec1 = Vector2.Zero;
        private Vector2 vec2 = Vector2.Zero;
        private Vector2 vec3 = Vector2.Zero;
        private Vector2 vec4 = Vector2.Zero;
        private Vector2 vec5 = Vector2.Zero;
        private float float1 = 0;
        private float float2 = 0;

        private Comparison<Contact> contactComparer;

        public Arbiter() { }

        internal Arbiter(Geom geometry1, Geom geometry2, PhysicsSimulator physicsSimulator) {
            ConstructArbiter(geometry1, geometry2, physicsSimulator);
        }

        internal void ConstructArbiter(Geom geometry1, Geom geometry2, PhysicsSimulator physicsSimulator)
        {
            this.physicsSimulator = physicsSimulator;

            if (geometry1 < geometry2) {
                this.geometryA = geometry1;
                this.geometryB = geometry2;
            }
            else {
                this.geometryA = geometry2;
                this.geometryB = geometry1;
            }

            switch (physicsSimulator.frictionType)
	        {
                case FrictionType.Average:
                    this.frictionCoefficientCombined = (geometryA.frictionCoefficient + geometryB.frictionCoefficient) / 2f;
                    break;
                case FrictionType.Minimum:
                    this.frictionCoefficientCombined = Math.Min(geometryA.FrictionCoefficient , geometryB.FrictionCoefficient);
                    break;
		        default:
                    this.frictionCoefficientCombined = (geometryA.frictionCoefficient + geometryB.frictionCoefficient) / 2f;
                    break;
	        } 
            
            InitializeContactLists(physicsSimulator.maxContactsToDetect);

            //to prevent garbage build up the contact comparer is pre-initialized and held in memory
            contactComparer = new Comparison<Contact>(CompareSeperation);
        }

        public Geom GeomA {
            get { return geometryA; }
            set { geometryA = value; }
        }

        public Geom GeomB {
            get { return geometryB; }
            set { geometryB = value; }
        }

        #region PreStepImpulse variables
        Vector2 r1;
        Vector2 r2;
        float rn1;
        float rn2;
        float rt1;
        float rt2;
        float kTangent;
        float min;
        float restitution;
        float kNormal;
        #endregion
        internal void PreStepImpulse(float inverseDt) {
            Contact contact;
            if (!geometryA.CollisionResponseEnabled || !geometryB.CollisionResponseEnabled) { return; }
            for (int i = 0; i < contactList.Count; i++) {
                contact = contactList[i];

                //calculate contact offset from body position
                Vector2.Subtract(ref contact.Position, ref geometryA.body.position, out r1);
                Vector2.Subtract(ref contact.Position, ref geometryB.body.position, out r2);

                //project normal onto offset vectors
                Vector2.Dot(ref r1, ref contact.Normal, out rn1);
                Vector2.Dot(ref r2, ref contact.Normal, out rn2);

                //calculate mass normal
                float invMassSum = geometryA.body.inverseMass + geometryB.body.inverseMass;
                Vector2.Dot(ref r1, ref r1, out float1);
                Vector2.Dot(ref r2, ref r2, out float2);
                kNormal = invMassSum + geometryA.body.inverseMomentOfInertia * (float1 - rn1 * rn1) + geometryB.body.inverseMomentOfInertia * (float2 - rn2 * rn2);
                contact.MassNormal = 1f / kNormal;

                //calculate mass tangent
                float1 = 1;
                Calculator.Cross(ref contact.Normal, ref float1, out tangent);
                Vector2.Dot(ref r1, ref tangent, out rt1);
                Vector2.Dot(ref r2, ref tangent, out rt2);
                kTangent = geometryA.Body.InverseMass + geometryB.Body.InverseMass;

                Vector2.Dot(ref r1, ref r1, out float1);
                Vector2.Dot(ref r2, ref r2, out float2);
                kTangent += geometryA.body.inverseMomentOfInertia * (float1 - rt1 * rt1) + geometryB.Body.InverseMomentOfInertia * (float2 - rt2 * rt2);
                contact.MassTangent = 1f / kTangent;

                //calc velocity bias
                min = Math.Min(0, physicsSimulator.allowedPenetration + contact.Seperation);
                contact.NormalVelocityBias = -physicsSimulator.biasFactor * inverseDt * min;

                //Compute the restitution, we average the restitution of the two bodies
                //restitution = (2.0f + _geometryA.RestitutionCoefficient + _geometryB.RestitutionCoefficient) * 0.5f;
                restitution = (geometryA.restitutionCoefficient + geometryB.restitutionCoefficient) * .5f;

                //calc bounce velocity
                geometryA.body.GetVelocityAtWorldOffset(ref r1, out vec1);
                geometryB.body.GetVelocityAtWorldOffset(ref r2, out vec2);
                Vector2.Subtract(ref vec2, ref vec1, out dv);

                //calc velocity difference along contact normal
                Vector2.Dot(ref dv, ref contact.Normal, out vn);
                contact.BounceVelocity = vn * restitution;

                //apply accumulated impulse
                Vector2.Multiply(ref contact.Normal, contact.NormalImpulse, out vec1);
                Vector2.Multiply(ref tangent, contact.TangentImpulse, out vec2);
                Vector2.Add(ref vec1, ref vec2, out impulse);

                geometryB.body.ApplyImpulseAtWorldOffset(ref impulse, ref r2);

                Vector2.Multiply(ref impulse, -1, out impulse);
                geometryA.body.ApplyImpulseAtWorldOffset(ref impulse, ref r1);

                contact.NormalImpulseBias = 0;
                contactList[i] = contact;
            }
        }

        #region variables for ApplyImpulse
        Contact contact;
        Vector2 tangent = Vector2.Zero;
        Vector2 dv = Vector2.Zero;

        float vn = 0;
        float normalVelocityBias = 0;
        float normalImpulseBias = 0;
        float normalImpulseBiasOriginal = 0;

        float normalImpulse = 0;
        float oldNormalImpulse = 0;
        float maxTangentImpulse = 0;

        float vt = 0;
        float tangentImpulse = 0;
        float oldTangentImpulse = 0;

        Vector2 impulse = Vector2.Zero;
        Vector2 impulseBias = Vector2.Zero;
        #endregion
        internal void ApplyImpulse() {
            if (!geometryA.CollisionResponseEnabled || !geometryB.CollisionResponseEnabled) { return; }
            for (int i = 0; i < contactList.Count; i++) {

                contact = contactList[i];
                                
                #region INLINE: Vector2.Subtract(ref contact.Position, ref geometryA.body.position, out contact.R1);
                contact.R1.X = contact.Position.X - geometryA.body.position.X;
                contact.R1.Y = contact.Position.Y - geometryA.body.position.Y;
                #endregion
                
                #region INLINE: Vector2.Subtract(ref contact.Position, ref geometryB.body.position, out contact.R2);
                contact.R2.X = contact.Position.X - geometryB.body.position.X;
                contact.R2.Y = contact.Position.Y - geometryB.body.position.Y;
                #endregion

                //calc velocity difference
                geometryA.body.GetVelocityAtWorldOffset(ref contact.R1, out vec1);
                geometryB.body.GetVelocityAtWorldOffset(ref contact.R2, out vec2);
                
                #region INLINE: Vector2.Subtract(ref vec2, ref vec1, out dv);
                                dv.X = vec2.X - vec1.X;
                                dv.Y = vec2.Y - vec1.Y;
                #endregion

                //calc velocity difference along contact normal
                #region INLINE: Vector2.Dot(ref dv, ref contact.Normal, out vn);
                vn = (dv.X * contact.Normal.X) + (dv.Y * contact.Normal.Y);
                #endregion

                                ////float normalImpulse = contact.MassNormal * (-vn + contact.NormalVelocityBias); //comment for preserve momentum
                normalImpulse = contact.MassNormal * -(vn + contact.BounceVelocity); //uncomment for preserve momentum

                //clamp accumulated impulse
                oldNormalImpulse = contact.NormalImpulse;
                contact.NormalImpulse = Math.Max(oldNormalImpulse + normalImpulse, 0);
                normalImpulse = contact.NormalImpulse - oldNormalImpulse;

                //apply contact impulse
                #region INLINE: Vector2.Multiply(ref contact.Normal, normalImpulse, out impulse);
                impulse.X = contact.Normal.X * normalImpulse;
                impulse.Y = contact.Normal.Y * normalImpulse;
                #endregion

                geometryB.body.ApplyImpulseAtWorldOffset(ref impulse, ref contact.R2);
                
                #region INLINE: Vector2.Multiply(ref impulse, -1, out impulse);
                impulse.X = impulse.X * -1;
                impulse.Y = impulse.Y * -1;
                #endregion

                geometryA.body.ApplyImpulseAtWorldOffset(ref impulse, ref contact.R1);

                //calc velocity bias difference (bias preserves momentum)
                geometryA.body.GetVelocityBiasAtWorldOffset(ref contact.R1, out vec1);
                geometryB.body.GetVelocityBiasAtWorldOffset(ref contact.R2, out vec2);
               
                #region INLINE: Vector2.Subtract(ref vec2, ref vec1, out dv);
                dv.X = vec2.X - vec1.X;
                dv.Y = vec2.Y - vec1.Y;
                #endregion

                //calc velocity bias along contact normal
                #region INLINE: Vector2.Dot(ref dv, ref contact.Normal, out normalVelocityBias);
                normalVelocityBias = (dv.X * contact.Normal.X) + (dv.Y * contact.Normal.Y);
                #endregion

                normalImpulseBias = contact.MassNormal * (-normalVelocityBias + contact.NormalVelocityBias);
                normalImpulseBiasOriginal = contact.NormalImpulseBias;
                contact.NormalImpulseBias = Math.Max(normalImpulseBiasOriginal + normalImpulseBias, 0);
                normalImpulseBias = contact.NormalImpulseBias - normalImpulseBiasOriginal;

                #region INLINE: Vector2.Multiply(ref contact.Normal, normalImpulseBias, out impulseBias);
                impulseBias.X = contact.Normal.X * normalImpulseBias;
                impulseBias.Y = contact.Normal.Y * normalImpulseBias;
                #endregion

                //apply bias impulse
                geometryB.body.ApplyBiasImpulseAtWorldOffset(ref impulseBias, ref contact.R2);

                #region INLINE: Vector2.Multiply(ref impulseBias, -1, out impulseBias);
                impulseBias.X = impulseBias.X * -1;
                impulseBias.Y = impulseBias.Y * -1;
                #endregion

                geometryA.body.ApplyBiasImpulseAtWorldOffset(ref impulseBias, ref contact.R1);

                //calc relative velocity at contact.
                geometryA.body.GetVelocityAtWorldOffset(ref contact.R1, out vec1);
                geometryB.body.GetVelocityAtWorldOffset(ref contact.R2, out vec2);

                #region INLINE: Vector2.Subtract(ref vec2, ref vec1, out dv);
                dv.X = vec2.X - vec1.X;
                dv.Y = vec2.Y - vec1.Y;
                #endregion

                //compute friction impulse
                maxTangentImpulse = frictionCoefficientCombined * contact.NormalImpulse;

                //Vector2 tangent = Calculator.Cross(contact.Normal, 1);
                float1 = 1;

                #region INLINE: Calculator.Cross(ref contact.Normal, ref float1, out tangent);
                tangent.X = float1 * contact.Normal.Y;
                tangent.Y = -float1 * contact.Normal.X;
                #endregion

                //float vt = Vector2.Dot(dv, tangent);
                #region INLINE: Vector2.Dot(ref dv, ref tangent, out vt);
                vt = (dv.X * tangent.X) + (dv.Y * tangent.Y);
                #endregion

                tangentImpulse = contact.MassTangent * (-vt);

                ////clamp friction
                oldTangentImpulse = contact.TangentImpulse;
                contact.TangentImpulse = Calculator.Clamp(oldTangentImpulse + tangentImpulse, -maxTangentImpulse, maxTangentImpulse);
                tangentImpulse = contact.TangentImpulse - oldTangentImpulse;

                //apply friction impulse
                #region INLINE:Vector2.Multiply(ref tangent, tangentImpulse, out impulse);
                impulse.X = tangent.X * tangentImpulse;
                impulse.Y = tangent.Y * tangentImpulse;
                #endregion

                //apply impulse
                geometryB.body.ApplyImpulseAtWorldOffset(ref impulse, ref contact.R2);
                #region INLINE: Vector2.Multiply(ref impulse, -1, out impulse);
                impulse.X = impulse.X * -1;
                impulse.Y = impulse.Y * -1;
                #endregion
                geometryA.body.ApplyImpulseAtWorldOffset(ref impulse, ref contact.R1);

                contactList[i] = contact;
            }
        }

        /// <summary>
        /// Fully exposed for convenience. Should be treated as readonly. Do not add or remove directly from this list.
        /// </summary>
        public ContactList ContactList {
            get { return contactList; }
        }

        internal int ContactCount {
            get { return contactList.Count; }
        }

        internal bool ContainsDisposedGeom() {
            return geometryA.IsDisposed || geometryB.IsDisposed;
        }

        internal void Reset() {
            geometryA = null;
            geometryB = null;
            contactList.Clear();
            newContactList.Clear();
            mergedContactList.Clear();
        }

        internal void Collide() {
            newContactList.Clear();
            //_geometryA.Collide(_geometryB, _newContactList);

            #region Added by Daniel Pramel 08/17/08
            /*
             * we don't want to check for a collision, if both bodies are disabled...
             */
            
             if (GeomA.body.enabled == false && GeomB.body.enabled == false) {
                mergedContactList.Clear();
                contactList.Clear();
                return;
            }
            #endregion


            Collide(GeomA, GeomB, newContactList);
            mergedContactList.Clear();

            for (int i = 0; i < newContactList.Count; i++) {
                //int index = _contactList.IndexOf(_newContactList[i]);
                int index = contactList.IndexOfSafe(newContactList[i]);
                if (index > -1) {
                    //continuation of collision
                    Contact contact = newContactList[i];
                    contact.NormalImpulse = contactList[index].NormalImpulse;
                    contact.TangentImpulse = contactList[index].TangentImpulse;
                    mergedContactList.Add(contact);
                }
                else {
                    //first time collision
                    mergedContactList.Add(newContactList[i]);
                }
            }
            contactList.Clear();
            for (int i = 0; i < mergedContactList.Count; i++) {
                contactList.Add(mergedContactList[i]);
            }
        }

        #region Collide variables
        Feature feature;
        Vector2 localVertex;
        Vector2 vertRef;
        Matrix matrixInverseTemp;
        Matrix matrixTemp;
        #endregion
        private void Collide(Geom geometry1, Geom geometry2, ContactList contactList) {
            int vertexIndex = -1;
            //matrixInverseTemp = this.MatrixInverse;
            matrixTemp = geometry1.Matrix;
            for (int i = 0; i < geometry2.worldVertices.Count; i++) {
                if (contactList.Count == physicsSimulator.maxContactsToDetect)
                {
                    break;
                }
                if (geometry1.grid == null) { break; }//grid can be null for "one-way" collision (points)

                vertexIndex += 1;
                vertRef = geometry2.WorldVertices[i];
                geometry1.TransformToLocalCoordinates(ref vertRef, out localVertex);
                if (!geometry1.Intersect(ref localVertex, out feature)) { continue; }

                if (feature.Distance < 0f) {
                    geometry1.TransformNormalToWorld(ref feature.Normal, out feature.Normal);
                    Contact contact = new Contact(geometry2.WorldVertices[i], feature.Normal, feature.Distance, new ContactId(2, vertexIndex, 1));
                    contactList.Add(contact);
                }
            }

            matrixInverseTemp = geometry2.MatrixInverse;
            matrixTemp = geometry2.Matrix;

            for (int i = 0; i < geometry1.WorldVertices.Count; i++) {
                if (contactList.Count == physicsSimulator.maxContactsToDetect)
                {
                    break;
                }
                if (geometry2.grid == null) { break; }//grid can be null for "one-way" collision (points)

                vertexIndex += 1;

                vertRef = geometry1.WorldVertices[i];
                geometry2.TransformToLocalCoordinates(ref vertRef, out localVertex);
                if (!geometry2.Intersect(ref localVertex, out feature)) { continue; }

                if (feature.Distance < 0f) {
                    geometry2.TransformNormalToWorld(ref feature.Normal, out feature.Normal);
                    feature.Normal = -feature.Normal;
                    Contact contact = new Contact(geometry1.WorldVertices[i], feature.Normal, feature.Distance, new ContactId(2, vertexIndex, 1));
                    contactList.Add(contact);
                }
            }

            //sort contact list by seperation (amount of penetration)
            contactList.Sort(contactComparer);

            //resolve deepest contacts first
            int contactCount = contactList.Count;
            if (contactList.Count > physicsSimulator.maxContactsToResolve)
            {
                contactList.RemoveRange(physicsSimulator.maxContactsToResolve, contactCount - physicsSimulator.maxContactsToResolve);
            }

            //allow user to cancel collision if desired
            if (geometry1.Collision != null) {
                if (contactList.Count > 0) {
                    if (!geometry1.Collision(geometry1, geometry2, contactList)) {
                        contactList.Clear();
                    }
                }
            }

            //allow user to cancel collision if desired
            if (geometry2.Collision != null) {
                if (contactList.Count > 0) {
                    if (!geometry2.Collision(geometry2, geometry1, contactList)) {
                        contactList.Clear();
                    }
                }
            }
        }
        
        private static int  CompareSeperation(Contact c1, Contact c2) {
            if (c1.Seperation < c2.Seperation) {
                return -1;
            }
            else if (c1.Seperation == c2.Seperation) {
                return 0;
            }
            else {
                return 1;
            }
        }

        private void InitializeContactLists(int maxContactsToDetect) {
            if (contactList == null) { contactList = new ContactList(maxContactsToDetect); }
            if (newContactList == null) { newContactList = new ContactList(maxContactsToDetect); }
            if (mergedContactList == null) { mergedContactList = new ContactList(maxContactsToDetect); }
        }

        public bool Equals(Arbiter other) {
            return (geometryA == other.geometryA) && (geometryB == other.geometryB);
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












