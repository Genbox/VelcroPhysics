using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Collisions;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    public class RigidBody : Body, ICollideable<RigidBody>, IEquatable<RigidBody>{
        protected Geometry _geometry;
        protected Grid _grid;
        private int _id;
        private int _collisionGroup = 0;
        private bool _collisionEnabled = true;
        private bool _collisonResponseEnabled = true;

        public event EventHandler<CollisionEventArgs> Collision;

              
        public RigidBody() {
           _id = RigidBody.GetNextId();
        }

        public RigidBody(Geometry geometry, Grid grid) {
            _id = RigidBody.GetNextId();
            this._grid = grid;
            this._geometry = geometry;
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

        public int CollisionGroup {
            get { return _collisionGroup; }
            set { _collisionGroup = value; }
        }

        public bool CollisionEnabled {
            get { return _collisionEnabled; }
            set { _collisionEnabled = value; }
        }

        public bool CollisionResponseEnabled {
            get { return _collisonResponseEnabled; }
            set { _collisonResponseEnabled = value; }
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
                if (_grid == null) { break; }//grid can be null for "one-way" collision (points)
                vertexIndex += 1;
                localVertex = _geometry.ConvertToLocalCoordinates(vertex);
                feature = _grid.Evaluate(localVertex);
                //hack
                if (float.IsNaN(feature.Normal.X)) {
                    continue;
                }

                if (feature.Distance < 0f) {
                    feature.Normal = _geometry.ConvertToWorldOrientation(feature.Normal);
                    Contact contact = new Contact(vertex, feature.Normal, feature.Distance, new ContactId(2, vertexIndex, 1));
                    contactList.Add(contact);
                }
            }

            if (contactList.Count == contactList.Capacity) { return; }
            foreach (Vector2 vertex in _geometry.WorldVertices) {
                if (rigidBody._grid == null) { return; } //grid can be null for "one-way" collision(points)
                vertexIndex += 1;
                localVertex = rigidBody._geometry.ConvertToLocalCoordinates(vertex);
                feature = rigidBody._grid.Evaluate(localVertex);
                //hack
                if (float.IsNaN(feature.Normal.X)) {
                    continue;
                }
                if (feature.Distance < 0f) {
                    feature.Normal = rigidBody._geometry.ConvertToWorldOrientation(feature.Normal);
                    feature.Normal = -feature.Normal; //normals must point in same direction.
                    Contact contact = new Contact(vertex, feature.Normal, feature.Distance, new ContactId(1, vertexIndex, 2));
                    contactList.Add(contact);
                }                
            }

            if (this.Collision != null && contactList.Count > 0) {
                this.Collision(rigidBody, new CollisionEventArgs(this, rigidBody, contactList));
            }
            if (rigidBody.Collision != null && contactList.Count > 0) {
                rigidBody.Collision(this, new CollisionEventArgs(rigidBody, this, contactList));
            }
        }

        public bool Collide(Vector2 point) {
            Feature feature;
            point = _geometry.ConvertToLocalCoordinates(point);
            feature = _grid.Evaluate(point);
            if (feature.Distance < 0) {
                return true;
            }
            else {
                return false;
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

    public class CollisionEventArgs : EventArgs {
        private ContactList contacts;
        private RigidBody _body1;
        private RigidBody _body2;

        public int Count {
            get { return contacts.Count; }
        }

        public ContactList Contacts {
            get { return contacts; }
        }

        public RigidBody Body1 {
            get { return _body1; }
        }

        public RigidBody Body2 {
            get { return _body2; }
        }

        public CollisionEventArgs(RigidBody body1, RigidBody body2, ContactList contactList) {
            this._body1 = body1;
            this._body2 = body2;
            this.contacts = contactList;
        }
    }
}
