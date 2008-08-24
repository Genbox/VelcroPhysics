using System;

namespace FarseerGames.FarseerPhysics.Collisions
{
    public struct ContactId : IEquatable<ContactId>
    {
        private int geometryAIndex;
        private int geometryAVertex;
        private int geometryBIndex;

        public ContactId(int geometryAIndex, int geometryAVertex, int geometryBIndex)
        {
            this.geometryAIndex = geometryAIndex;
            this.geometryAVertex = geometryAVertex;
            this.geometryBIndex = geometryBIndex;
        }

        public int Geom1Index
        {
            get { return geometryAIndex; }
            set { geometryAIndex = value; }
        }

        public int Geom1Vertex
        {
            get { return geometryAVertex; }
            set { geometryAVertex = value; }
        }

        public int Geom2Index
        {
            get { return geometryBIndex; }
            set { geometryBIndex = value; }
        }

        #region IEquatable<ContactId> Members

        public bool Equals(ContactId other)
        {
            return (geometryAIndex == other.geometryAIndex) && (geometryAVertex == other.geometryAVertex) &&
                   (geometryBIndex == other.geometryBIndex);
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