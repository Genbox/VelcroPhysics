using System;

namespace FarseerGames.FarseerPhysics.Collisions
{
    /// <summary>
    /// Used in collision detection
    /// Provides an implementation that gives an ID for contacts
    /// </summary>
    public struct ContactId : IEquatable<ContactId>
    {
        public ContactId(int geometryAIndex, int geometryAVertex, int geometryBIndex)
            : this()
        {
            Geom1Index = geometryAIndex;
            Geom1Vertex = geometryAVertex;
            Geom2Index = geometryBIndex;
        }

        /// <summary>
        /// Gets or sets the index of geom1.
        /// </summary>
        /// <Value>The index of the geom1.</Value>
        public int Geom1Index { get; set; }

        /// <summary>
        /// Gets or sets the geom1 vertex.
        /// </summary>
        /// <Value>The geom1 vertex.</Value>
        public int Geom1Vertex { get; set; }

        /// <summary>
        /// Gets or sets the index of geom2.
        /// </summary>
        /// <Value>The index of the geom2.</Value>
        public int Geom2Index { get; set; }

        #region IEquatable<ContactId> Members

        public bool Equals(ContactId other)
        {
            return (Geom1Index == other.Geom1Index) && (Geom1Vertex == other.Geom1Vertex) &&
                   (Geom2Index == other.Geom2Index);
        }

        #endregion

        public bool Equals(ref ContactId other)
        {
            return (Geom1Index == other.Geom1Index) && (Geom1Vertex == other.Geom1Vertex) &&
                   (Geom2Index == other.Geom2Index);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ContactId))
                return false;

            return Equals((ContactId)obj);
        }

        public static bool operator ==(ContactId contactId1, ContactId contactId2)
        {
            return contactId1.Equals(ref contactId2);
        }

        public static bool operator !=(ContactId contactId1, ContactId contactId2)
        {
            return !contactId1.Equals(ref contactId2);
        }

        public override int GetHashCode()
        {
            return Geom1Index + Geom1Vertex + Geom2Index;
        }
    }
}