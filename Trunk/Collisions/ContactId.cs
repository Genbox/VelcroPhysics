using System;

namespace FarseerGames.FarseerPhysics.Collisions
{
    /// <summary>
    /// TODO: Write documentation
    /// </summary>
    public struct ContactId : IEquatable<ContactId>
    {
        private int _geometryAIndex;
        private int _geometryAVertex;
        private int _geometryBIndex;

        public ContactId(int geometryAIndex, int geometryAVertex, int geometryBIndex)
        {
            _geometryAIndex = geometryAIndex;
            _geometryAVertex = geometryAVertex;
            _geometryBIndex = geometryBIndex;
        }

        public int Geom1Index
        {
            get { return _geometryAIndex; }
            set { _geometryAIndex = value; }
        }

        public int Geom1Vertex
        {
            get { return _geometryAVertex; }
            set { _geometryAVertex = value; }
        }

        public int Geom2Index
        {
            get { return _geometryBIndex; }
            set { _geometryBIndex = value; }
        }

        #region IEquatable<ContactId> Members

        public bool Equals(ContactId other)
        {
            return (_geometryAIndex == other._geometryAIndex) && (_geometryAVertex == other._geometryAVertex) &&
                   (_geometryBIndex == other._geometryBIndex);
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (!(obj is ContactId))
                return false;

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