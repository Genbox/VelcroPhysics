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
        /// <summary>
        /// Id of the contact
        /// </summary>
        public ContactId ContactId;

        /// <summary>
        /// Position of the contact
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// Normal of the contact
        /// </summary>
        public Vector2 Normal;

        /// <summary>
        /// The amount of penetration
        /// </summary>
        public float Separation;

        internal float bounceVelocity;
        internal float massNormal;
        internal float massTangent;
        internal float normalImpulse;
        internal float normalImpulseBias;
        internal float normalVelocityBias;
        internal Vector2 r1;
        internal Vector2 r2;
        internal float tangentImpulse;

        public Contact(Vector2 position, Vector2 normal, float separation, ContactId contactId)
        {
            ContactId = contactId;
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
            return (ContactId.Equals(ref other.ContactId));
        }

        #endregion

        public bool Equals(ref Contact other)
        {
            return (ContactId.Equals(ref other.ContactId));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Contact))
                return false;

            return Equals((Contact)obj);
        }

        public static bool operator ==(Contact contact1, Contact contact2)
        {
            return contact1.Equals(ref contact2);
        }

        public static bool operator !=(Contact contact1, Contact contact2)
        {
            return !contact1.Equals(ref contact2);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}