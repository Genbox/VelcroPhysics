using System;

#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Collisions
{
    /// <summary>
    /// Used internal in collision detection.
    /// Represents a contact point between 2 geoms.
    /// </summary>
    public struct Contact : IEquatable<Contact>
    {
        internal float BounceVelocity;
        internal ContactId ContactId;
        internal float MassNormal;
        internal float MassTangent;
        public Vector2 Normal;
        internal float NormalImpulse;
        internal float NormalImpulseBias;
        internal float NormalVelocityBias;
        public Vector2 Position;
        internal Vector2 R1;
        internal Vector2 R2;
        public float Separation;
        internal float TangentImpulse;

        public Contact(Vector2 position, Vector2 normal, float separation, ContactId contactId)
        {
            ContactId = contactId;
            Position = position;
            Normal = normal;
            Separation = separation;
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

        #region IEquatable<Contact> Members

        public bool Equals(Contact other)
        {
            return (ContactId == other.ContactId);
        }

        #endregion

        /// <summary>
        /// Sets the mass normal.
        /// </summary>
        /// <param name="massNormal">The mass normal.</param>
        public void SetMassNormal(float massNormal)
        {
            MassNormal = massNormal;
        }

        /// <summary>
        /// Sets the mass tangent.
        /// </summary>
        /// <param name="massTangent">The mass tangent.</param>
        public void SetMassTangent(float massTangent)
        {
            MassTangent = massTangent;
        }

        /// <summary>
        /// Sets the bias.
        /// </summary>
        /// <param name="bias">The bias.</param>
        public void SetBias(float bias)
        {
            NormalVelocityBias = bias;
        }

        /// <summary>
        /// Sets the normal impulse.
        /// </summary>
        /// <param name="normalImpulse">The normal impulse.</param>
        public void SetNormalImpulse(float normalImpulse)
        {
            NormalImpulse = normalImpulse;
        }

        /// <summary>
        /// Sets the tangent impulse.
        /// </summary>
        /// <param name="tangentImpulse">The tangent impulse.</param>
        public void SetTangentImpulse(float tangentImpulse)
        {
            TangentImpulse = tangentImpulse;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Contact))
                return false;

            return Equals((Contact) obj);
        }

        public static bool operator ==(Contact contact1, Contact contact2)
        {
            return contact1.Equals(contact2);
        }

        public static bool operator !=(Contact contact1, Contact contact2)
        {
            return !contact1.Equals(contact2);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}