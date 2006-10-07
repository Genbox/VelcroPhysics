using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Collisions;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    public class RigidBody : Body, ICollideable<RigidBody>, IEquatable<RigidBody>{
        protected Geometry _geometry;
        protected Grid _grid;
        private int _id;

        public RigidBody() {
           _id = RigidBody.GetNextId();
        }

        public RigidBody(Geometry geometry, Grid grid) {
            _id = RigidBody.GetNextId();
            _grid = grid;
            _geometry = geometry;
        }

        public sealed override Vector2 Position {
            get {
                return base.Position;
            }
            set {
                base.Position = value;
                if (_geometry != null) {
                    _geometry.Update(value, Orientation);
                }
            }
        }

        public sealed override float Orientation {
            get {
                return base.Orientation;
            }
            set {
                base.Orientation = value;
                if (_geometry != null) {
                    _geometry.Update(Position, value);
                }
            }
        }

        public Geometry Geometry {
            get { return _geometry; }
            set { 
                _geometry = value;
                _geometry.Update(Position, Orientation);
            }
        }

        public Grid Grid {
            get { return _grid; }
            set { _grid = value; }
        }

        public override void IntegratePosition(float dt) {
            base.IntegratePosition(dt);
            _geometry.Update(Position, Orientation);
        }

        public void Collide(RigidBody rigidBody, ContactList contactList) {
            Feature feature; ;
            Vector2 localVertex;
            int vertexIndex = -1;
            foreach (Vector2 vertex in rigidBody._geometry.WorldVertices) {
                if (contactList.Count == contactList.Capacity) { return; }
                vertexIndex += 1;
                localVertex = _geometry.ConvertToLocalCoordinates(vertex);
                feature = _grid.Evaluate(localVertex);
                if (feature.Distance < 0f) {
                    feature.Normal = _geometry.ConvertToWorldOrientation(feature.Normal);
                    Contact contact = new Contact(vertex, feature.Normal, feature.Distance, new ContactId(2, vertexIndex, 1));
                    contactList.Add(contact);
                }
            }
            foreach (Vector2 vertex in _geometry.WorldVertices) {
                vertexIndex += 1;
                localVertex = rigidBody._geometry.ConvertToLocalCoordinates(vertex);
                feature = rigidBody._grid.Evaluate(localVertex);
                if (feature.Distance < 0f) {
                    feature.Normal = rigidBody._geometry.ConvertToWorldOrientation(feature.Normal);
                    feature.Normal = -feature.Normal; //normals must point in same direction.
                    Contact contact = new Contact(vertex, feature.Normal, feature.Distance, new ContactId(1, vertexIndex, 2));
                    contactList.Add(contact);
                }
            }
        }

        public bool Equals(RigidBody other) {
            return _id == other._id;
        }

        public override int GetHashCode() {
           return base.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (!(obj is RigidBody)) { throw new ArgumentException("The object being compared must be of type 'RigidBody'"); }
            return Equals((RigidBody)obj);
        }

        public static bool operator ==(RigidBody rigidBody1, RigidBody rigidBody2) {
            return rigidBody1.Equals(rigidBody2);
        }

        public static bool operator !=(RigidBody rigidBody1, RigidBody rigidBody2) {
            return !rigidBody1.Equals(rigidBody2);
        }

        public static bool operator <(RigidBody rigidBody1, RigidBody rigidBody2) {
            return rigidBody1.Id < rigidBody2.Id;
        }

        public static bool operator >(RigidBody rigidBody1, RigidBody rigidBody2) {
            return rigidBody1.Id > rigidBody2.Id;
        }

        internal int Id {
            get { return _id; }
        }

        private static int _newId = -1;
        
        public static int GetNextId() {
            _newId += 1;
            return _newId;
        }
    }
}
