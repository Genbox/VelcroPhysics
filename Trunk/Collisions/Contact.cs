using System;
using System.Collections.Generic;
using System.Text;

#if (XNA)
using Microsoft.Xna.Framework; 
#endif

using FarseerGames.FarseerPhysics.Mathematics; 

namespace FarseerGames.FarseerPhysics.Collisions
{
    public struct Contact : IEquatable<Contact>
    {
        internal ContactId ContactId;
        public Vector2 Position;
        public Vector2 Normal;
        public float Seperation;
        internal float NormalImpulse;
        internal float TangentImpulse;
        internal float MassNormal;
        internal float MassTangent;
        internal float NormalVelocityBias;
        internal float NormalImpulseBias;
        internal Vector2 R1;
        internal Vector2 R2;
        internal float BounceVelocity;
        
        public Contact(Vector2 position,Vector2 normal, float seperation, ContactId contactId)
        {
            ContactId = contactId;
            Position = position;
            Normal = normal;
            Seperation = seperation;
            NormalImpulse = 0;
            TangentImpulse = 0;
            MassNormal = 0;
            MassTangent = 0;
            NormalVelocityBias = 0;
            NormalImpulseBias = 0;
            R1 = Vector2.Zero;
            R2 = Vector2.Zero;
            BounceVelocity = 0;

        }

        public void SetMassNormal(float massNormal) {
            this.MassNormal = massNormal;
        }

        public void SetMassTangent(float massTangent) {
            MassTangent  = massTangent;
        }

        public void SetBias(float bias) {
            NormalVelocityBias = bias;
        }

        public void SetNormalImpulse(float normalImpulse) {
            NormalImpulse = normalImpulse;
        }

        public void SetTangentImpulse(float tangentImpulse) {
            TangentImpulse = tangentImpulse;
        }
        
        public bool Equals(Contact other) {
            return (ContactId == other.ContactId);
        }

        public override bool Equals(object obj) {
            if (!(obj is Contact)) { throw new ArgumentException("The object being compared must be of type 'Arbiter'"); }
            return Equals((Contact)obj);
        }

        public static bool operator ==(Contact contact1, Contact contact2) {
            return contact1.Equals(contact2);
        }

        public static bool operator !=(Contact contact1, Contact contact2) {
            return !contact1.Equals(contact2);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
