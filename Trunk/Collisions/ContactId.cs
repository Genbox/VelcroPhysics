using System;

namespace FarseerGames.FarseerPhysics.Collisions
{
    public struct ContactId : IEquatable<ContactId>
    {
        public ContactId(int geometryAIndex, int geometryAVertex, int geometryBIndex) : this()
        {
            Geom1Index = geometryAIndex;
            Geom1Vertex = geometryAVertex;
            Geom2Index = geometryBIndex;
        }

        public int Geom1Index { get; set; }
        public int Geom1Vertex { get; set; }
        public int Geom2Index { get; set; }

        #region IEquatable<ContactId> Members

        public bool Equals(ContactId other)
        {
            return (Geom1Index == other.Geom1Index) && (Geom1Vertex == other.Geom1Vertex) &&
                   (Geom2Index == other.Geom2Index);
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (!(obj is ContactId))
            {
                throw new ArgumentException("The object being compared must be of type 'ContactId'");
            }
            return Equals((ContactId) obj);
        }

        public static bool operator ==(ContactId contactId1, ContactId contactId2)
        {
            return contactId1.Equals(contactId2);
        }

        public static bool operator !=(ContactId contactId1, ContactId contactId2)
        {
            return !contactId1.Equals(contactId2);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}