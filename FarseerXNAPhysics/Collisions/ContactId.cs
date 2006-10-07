using System;
using System.Collections.Generic;
using System.Text;

namespace FarseerGames.FarseerXNAPhysics.Collisions {
    public struct ContactId : IEquatable<ContactId> {
        int _bodyAIndex;
        int _bodyAVertex;
        int _bodyBIndex;

        public int Body1Index {
            get { return _bodyAIndex; }
            set { _bodyAIndex = value; }
        }

        public int Body1Vertex {
            get { return _bodyAVertex; }
            set { _bodyAVertex = value; }
        }

        public int Body2Index {
            get { return _bodyBIndex; }
            set { _bodyBIndex = value; }
        }

        public ContactId(int bodyAIndex, int bodyAVertex, int bodyBIndex) {
            _bodyAIndex = bodyAIndex;
            _bodyAVertex = bodyAVertex;
            _bodyBIndex = bodyBIndex;
        }

        public bool Equals(ContactId other) {
            return (_bodyAIndex == other._bodyAIndex) && (_bodyAVertex == other._bodyAVertex) && (_bodyBIndex == other._bodyBIndex);
        }

        public override bool Equals(object obj) {
            if (!(obj is ContactId)) { throw new ArgumentException("The object being compared must be of type 'Arbiter'"); }
            return Equals((ContactId)obj);
        }

        public static bool operator ==(ContactId contactId1, ContactId contactId2) {
            return contactId1.Equals(contactId2);
        }

        public static bool operator !=(ContactId contactId1, ContactId contactId2) {
            return !contactId1.Equals(contactId2);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
