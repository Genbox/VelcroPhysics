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
        internal float bounceVelocity;
        internal ContactId contactId;
        internal float massNormal;
        internal float massTangent;
        public Vector2 Normal;
        internal float normalImpulse;
        internal float normalImpulseBias;
        internal float normalVelocityBias;
        public Vector2 Position;
        internal Vector2 r1;
        internal Vector2 r2;
        public float Separation;
        internal float tangentImpulse;

        public Contact(Vector2 position, Vector2 normal, float separation, ContactId contactId)
        {
            this.contactId = contactId;
            Position = position;
            Normal = normal;
            Separation = separation;
            normalImpulse = 0;
            tangentImpulse = 0;
            massNormal = 0;
            massTangent = 0;
            normalVelocityBias = 0;
            normalImpulseBias = 0;
            r1 = Vector2.Zero;
            r2 = Vector2.Zero;
            bounceVelocity = 0;
        }

        #region IEquatable<Contact> Members

        public bool Equals(Contact other)
        {
            return (contactId == other.contactId);
        }

        #endregion

        /// <summary>
        /// Sets the mass normal.
        /// </summary>
        /// <param name="value">The mass normal.</param>
        public void SetMassNormal(float value)
        {
            massNormal = value;
        }

        /// <summary>
        /// Sets the mass tangent.
        /// </summary>
        /// <param name="value">The mass tangent.</param>
        public void SetMassTangent(float value)
        {
            massTangent = value;
        }

        /// <summary>
        /// Sets the bias.
        /// </summary>
        /// <param name="bias">The bias.</param>
        public void SetBias(float bias)
        {
            normalVelocityBias = bias;
        }

        /// <summary>
        /// Sets the normal impulse.
        /// </summary>
        /// <param name="value">The normal impulse.</param>
        public void SetNormalImpulse(float value)
        {
            normalImpulse = value;
        }

        /// <summary>
        /// Sets the tangent impulse.
        /// </summary>
        /// <param name="value">The tangent impulse.</param>
        public void SetTangentImpulse(float value)
        {
            tangentImpulse = value;
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