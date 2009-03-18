using System;

namespace FarseerGames.FarseerPhysics.Collisions
{
    /// <summary>
    /// Used in collision detection
    /// Provides an implementation that gives an ID for contacts
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

        /// <summary>
        /// Gets or sets the index of geom1.
        /// </summary>
        /// <Value>The index of the geom1.</Value>
        public int Geom1Index
        {
            get { return _geometryAIndex; }
            set { _geometryAIndex = value; }
        }

        /// <summary>
        /// Gets or sets the geom1 vertex.
        /// </summary>
        /// <Value>The geom1 vertex.</Value>
        public int Geom1Vertex
        {
            get { return _geometryAVertex; }
            set { _geometryAVertex = value; }
        }

        /// <summary>
        /// Gets or sets the index of geom2.
        /// </summary>
        /// <Value>The index of the geom2.</Value>
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
            return _geometryAIndex + _geometryAVertex + _geometryBIndex;
        }
    }
}