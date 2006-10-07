using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Mathematics; 

namespace FarseerGames.FarseerXNAPhysics.Collisions
{
    public struct Contact : IEquatable<Contact>
    {
        public ContactId ContactId;
        public Vector2 Position;
        public Vector2 Normal;
        public float Seperation;
        public float NormalImpulse;
        public float TangentImpulse;
        public float MassNormal;
        public float MassTangent;
        public float NormalVelocityBias;
        public float NormalImpulseBias;
        public Vector2 R1;
        public Vector2 R2;
        
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
