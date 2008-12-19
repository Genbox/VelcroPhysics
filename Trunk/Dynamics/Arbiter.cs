/// <summary>
/// Some of the code and ideas in this class were taken from a paper entitled
/// "Fast and Simple Physics using Sequential Impulses" by Erin Catto
/// </summary>

using System;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Mathematics;

#if (XNA)
using Microsoft.Xna.Framework;
#endif

namespace FarseerGames.FarseerPhysics.Dynamics
{
    /// <summary>
    /// Used for collision detection.
    /// Constructed when 2 geoms collide. Applies impulses between the 2 geoms.
    /// </summary>
    public class Arbiter : IEquatable<Arbiter>
    {
        //To prevent garbage build up the contact comparer is pre-initialized and held in memory
        private static Comparison<Contact> _contactComparer = CompareSeperation;
        private ContactList _contactList;
        private float _float1;
        private float _float2;
        private float _frictionCoefficientCombined;

        private ContactList _mergedContactList;
        private ContactList _newContactList;
        private PhysicsSimulator _physicsSimulator;

        //used for ref/out methods
        private Vector2 _vec1 = Vector2.Zero;
        private Vector2 _vec2 = Vector2.Zero;
        public Geom GeometryA;
        public Geom GeometryB;

        public Arbiter()
        {
        }

        internal Arbiter(Geom geometry1, Geom geometry2, PhysicsSimulator physicsSimulator)
        {
            ConstructArbiter(geometry1, geometry2, physicsSimulator);
        }

        public Geom GeomA
        {
            get { return GeometryA; }
            set { GeometryA = value; }
        }

        public Geom GeomB
        {
            get { return GeometryB; }
            set { GeometryB = value; }
        }

        /// <summary>
        /// Fully exposed for convenience. Should be treated as. Do not add or remove directly from this list.
        /// </summary>
        public ContactList ContactList
        {
            get { return _contactList; }
        }

        internal int ContactCount
        {
            get { return _contactList.Count; }
        }

        #region PreStepImpulse variables

        private float _kNormal;
        private float _kTangent;
        private float _min;
        private Vector2 _r1;
        private Vector2 _r2;
        private float _restitution;
        private float _rn1;
        private float _rn2;
        private float _rt1;
        private float _rt2;

        #endregion

        #region IEquatable<Arbiter> Members

        public bool Equals(Arbiter other)
        {
            return (GeometryA == other.GeometryA) && (GeometryB == other.GeometryB);
        }

        #endregion

        internal void ConstructArbiter(Geom geometry1, Geom geometry2, PhysicsSimulator physicsSimulator)
        {
            _physicsSimulator = physicsSimulator;

            if (geometry1 < geometry2)
            {
                GeometryA = geometry1;
                GeometryB = geometry2;
            }
            else
            {
                GeometryA = geometry2;
                GeometryB = geometry1;
            }

            switch (physicsSimulator.frictionType)
            {
                case FrictionType.Average:
                    _frictionCoefficientCombined = (GeometryA.frictionCoefficient + GeometryB.frictionCoefficient)/2f;
                    break;
                case FrictionType.Minimum:
                    _frictionCoefficientCombined = Math.Min(GeometryA.frictionCoefficient, GeometryB.frictionCoefficient);
                    break;
                default:
                    _frictionCoefficientCombined = (GeometryA.frictionCoefficient + GeometryB.frictionCoefficient)/2f;
                    break;
            }

            InitializeContactLists(physicsSimulator.maxContactsToDetect);
        }

        internal void PreStepImpulse(float inverseDt)
        {
            if (!GeometryA.collisionResponseEnabled || !GeometryB.collisionResponseEnabled)
                return;

            for (int i = 0; i < _contactList.Count; i++)
            {
                Contact contact = _contactList[i];

                //calculate _contact offset from body position
                Vector2.Subtract(ref contact.Position, ref GeometryA.body.position, out _r1);
                Vector2.Subtract(ref contact.Position, ref GeometryB.body.position, out _r2);

                //project normal onto offset vectors
                Vector2.Dot(ref _r1, ref contact.Normal, out _rn1);
                Vector2.Dot(ref _r2, ref contact.Normal, out _rn2);

                //calculate mass normal
                float invMassSum = GeometryA.body.inverseMass + GeometryB.body.inverseMass;
                Vector2.Dot(ref _r1, ref _r1, out _float1);
                Vector2.Dot(ref _r2, ref _r2, out _float2);
                _kNormal = invMassSum + GeometryA.body.inverseMomentOfInertia*(_float1 - _rn1*_rn1) +
                           GeometryB.body.inverseMomentOfInertia*(_float2 - _rn2*_rn2);
                contact.MassNormal = 1f/_kNormal;

                //calculate mass _tangent
                _float1 = 1;
                Calculator.Cross(ref contact.Normal, ref _float1, out _tangent);
                Vector2.Dot(ref _r1, ref _tangent, out _rt1);
                Vector2.Dot(ref _r2, ref _tangent, out _rt2);
                _kTangent = GeometryA.Body.InverseMass + GeometryB.Body.InverseMass;

                Vector2.Dot(ref _r1, ref _r1, out _float1);
                Vector2.Dot(ref _r2, ref _r2, out _float2);
                _kTangent += GeometryA.body.inverseMomentOfInertia*(_float1 - _rt1*_rt1) +
                             GeometryB.Body.InverseMomentOfInertia*(_float2 - _rt2*_rt2);
                contact.MassTangent = 1f/_kTangent;

                //calc velocity bias
                _min = Math.Min(0, _physicsSimulator.allowedPenetration + contact.Separation);
                contact.NormalVelocityBias = -_physicsSimulator.biasFactor*inverseDt*_min;

                //Compute the _restitution, we average the _restitution of the two bodies
                //_restitution = (2.0f + _geometryA.RestitutionCoefficient + _geometryB.RestitutionCoefficient) * 0.5f;
                _restitution = (GeometryA.restitutionCoefficient + GeometryB.restitutionCoefficient)*.5f;

                //calc bounce velocity
                GeometryA.body.GetVelocityAtWorldOffset(ref _r1, out _vec1);
                GeometryB.body.GetVelocityAtWorldOffset(ref _r2, out _vec2);
                Vector2.Subtract(ref _vec2, ref _vec1, out _dv);

                //calc velocity difference along _contact normal
                Vector2.Dot(ref _dv, ref contact.Normal, out _vn);
                contact.BounceVelocity = _vn*_restitution;

                //apply accumulated _impulse
                Vector2.Multiply(ref contact.Normal, contact.NormalImpulse, out _vec1);
                Vector2.Multiply(ref _tangent, contact.TangentImpulse, out _vec2);
                Vector2.Add(ref _vec1, ref _vec2, out _impulse);

                GeometryB.body.ApplyImpulseAtWorldOffset(ref _impulse, ref _r2);

                Vector2.Multiply(ref _impulse, -1, out _impulse);
                GeometryA.body.ApplyImpulseAtWorldOffset(ref _impulse, ref _r1);

                contact.NormalImpulseBias = 0;
                _contactList[i] = contact;
            }
        }

        internal void ApplyImpulse()
        {
            if (!GeometryA.collisionResponseEnabled || !GeometryB.collisionResponseEnabled)
                return;

            for (int i = 0; i < _contactList.Count; i++)
            {
                _contact = _contactList[i];

                #region INLINE: Vector2.Subtract(ref _contact.Position, ref geometryA.body.position, out _contact.R1);

                _contact.R1.X = _contact.Position.X - GeometryA.body.position.X;
                _contact.R1.Y = _contact.Position.Y - GeometryA.body.position.Y;

                #endregion

                #region INLINE: Vector2.Subtract(ref _contact.Position, ref geometryB.body.position, out _contact.R2);

                _contact.R2.X = _contact.Position.X - GeometryB.body.position.X;
                _contact.R2.Y = _contact.Position.Y - GeometryB.body.position.Y;

                #endregion

                //calc velocity difference
                GeometryA.body.GetVelocityAtWorldOffset(ref _contact.R1, out _vec1);
                GeometryB.body.GetVelocityAtWorldOffset(ref _contact.R2, out _vec2);

                #region INLINE: Vector2.Subtract(ref _vec2, ref _vec1, out _dv);

                _dv.X = _vec2.X - _vec1.X;
                _dv.Y = _vec2.Y - _vec1.Y;

                #endregion

                //calc velocity difference along contact normal

                #region INLINE: Vector2.Dot(ref _dv, ref _contact.Normal, out _vn);

                _vn = (_dv.X*_contact.Normal.X) + (_dv.Y*_contact.Normal.Y);

                #endregion

                _normalImpulse = _contact.MassNormal*-(_vn + _contact.BounceVelocity); //uncomment for preserve momentum

                //clamp accumulated impulse
                _oldNormalImpulse = _contact.NormalImpulse;
                _contact.NormalImpulse = Math.Max(_oldNormalImpulse + _normalImpulse, 0);
                _normalImpulse = _contact.NormalImpulse - _oldNormalImpulse;

                //apply contact impulse

                #region INLINE: Vector2.Multiply(ref _contact.Normal, _normalImpulse, out _impulse);

                _impulse.X = _contact.Normal.X*_normalImpulse;
                _impulse.Y = _contact.Normal.Y*_normalImpulse;

                #endregion

                GeometryB.body.ApplyImpulseAtWorldOffset(ref _impulse, ref _contact.R2);

                #region INLINE: Vector2.Multiply(ref _impulse, -1, out _impulse);

                _impulse.X = _impulse.X*-1;
                _impulse.Y = _impulse.Y*-1;

                #endregion

                GeometryA.body.ApplyImpulseAtWorldOffset(ref _impulse, ref _contact.R1);

                //calc velocity bias difference (bias preserves momentum)
                GeometryA.body.GetVelocityBiasAtWorldOffset(ref _contact.R1, out _vec1);
                GeometryB.body.GetVelocityBiasAtWorldOffset(ref _contact.R2, out _vec2);

                #region INLINE: Vector2.Subtract(ref _vec2, ref _vec1, out _dv);

                _dv.X = _vec2.X - _vec1.X;
                _dv.Y = _vec2.Y - _vec1.Y;

                #endregion

                //calc velocity bias along contact normal

                #region INLINE: Vector2.Dot(ref _dv, ref _contact.Normal, out _normalVelocityBias);

                _normalVelocityBias = (_dv.X*_contact.Normal.X) + (_dv.Y*_contact.Normal.Y);

                #endregion

                _normalImpulseBias = _contact.MassNormal*(-_normalVelocityBias + _contact.NormalVelocityBias);
                _normalImpulseBiasOriginal = _contact.NormalImpulseBias;
                _contact.NormalImpulseBias = Math.Max(_normalImpulseBiasOriginal + _normalImpulseBias, 0);
                _normalImpulseBias = _contact.NormalImpulseBias - _normalImpulseBiasOriginal;

                #region INLINE: Vector2.Multiply(ref _contact.Normal, _normalImpulseBias, out _impulseBias);

                _impulseBias.X = _contact.Normal.X*_normalImpulseBias;
                _impulseBias.Y = _contact.Normal.Y*_normalImpulseBias;

                #endregion

                //apply bias impulse
                GeometryB.body.ApplyBiasImpulseAtWorldOffset(ref _impulseBias, ref _contact.R2);

                #region INLINE: Vector2.Multiply(ref _impulseBias, -1, out _impulseBias);

                _impulseBias.X = _impulseBias.X*-1;
                _impulseBias.Y = _impulseBias.Y*-1;

                #endregion

                GeometryA.body.ApplyBiasImpulseAtWorldOffset(ref _impulseBias, ref _contact.R1);

                //calc relative velocity at contact.
                GeometryA.body.GetVelocityAtWorldOffset(ref _contact.R1, out _vec1);
                GeometryB.body.GetVelocityAtWorldOffset(ref _contact.R2, out _vec2);

                #region INLINE: Vector2.Subtract(ref _vec2, ref _vec1, out _dv);

                _dv.X = _vec2.X - _vec1.X;
                _dv.Y = _vec2.Y - _vec1.Y;

                #endregion

                //compute friction impulse
                _maxTangentImpulse = _frictionCoefficientCombined*_contact.NormalImpulse;

                //Vector2 _tangent = Calculator.Cross(_contact.Normal, 1);
                _float1 = 1;

                #region INLINE: Calculator.Cross(ref _contact.Normal, ref _float1, out _tangent);

                _tangent.X = _float1*_contact.Normal.Y;
                _tangent.Y = -_float1*_contact.Normal.X;

                #endregion

                //float _vt = Vector2.Dot(_dv, _tangent);

                #region INLINE: Vector2.Dot(ref _dv, ref _tangent, out _vt);

                _vt = (_dv.X*_tangent.X) + (_dv.Y*_tangent.Y);

                #endregion

                _tangentImpulse = _contact.MassTangent*(-_vt);

                //clamp friction
                _oldTangentImpulse = _contact.TangentImpulse;
                _contact.TangentImpulse = Calculator.Clamp(_oldTangentImpulse + _tangentImpulse, -_maxTangentImpulse,
                                                           _maxTangentImpulse);
                _tangentImpulse = _contact.TangentImpulse - _oldTangentImpulse;

                //apply friction impulse

                #region INLINE:Vector2.Multiply(ref _tangent, _tangentImpulse, out _impulse);

                _impulse.X = _tangent.X*_tangentImpulse;
                _impulse.Y = _tangent.Y*_tangentImpulse;

                #endregion

                //apply impulse
                GeometryB.body.ApplyImpulseAtWorldOffset(ref _impulse, ref _contact.R2);

                #region INLINE: Vector2.Multiply(ref _impulse, -1, out _impulse);

                _impulse.X = _impulse.X*-1;
                _impulse.Y = _impulse.Y*-1;

                #endregion

                GeometryA.body.ApplyImpulseAtWorldOffset(ref _impulse, ref _contact.R1);

                _contactList[i] = _contact;
            }
        }

        internal bool ContainsDisposedGeom()
        {
            return GeometryA.IsDisposed || GeometryB.IsDisposed || (GeometryA.body.isStatic && GeometryB.body.isStatic);
        }

        internal void Reset()
        {
            GeometryA = null;
            GeometryB = null;
            _contactList.Clear();
            _newContactList.Clear();
            _mergedContactList.Clear();
        }

        internal void Collide()
        {
            _newContactList.Clear();

            #region Added by Daniel Pramel 08/17/08

            /*
             * we don't want to check for a collision, if both bodies are disabled...
             */
            //NOTE: Arbiters should not be created in the broad phase if both bodies are disabled
            //is this redundant?
            if (GeomA.body.Enabled == false && GeomB.body.Enabled == false)
            {
                _mergedContactList.Clear();
                _contactList.Clear();
                return;
            }

            #endregion

            Collide(GeomA, GeomB, _newContactList);
            _mergedContactList.Clear();

            for (int i = 0; i < _newContactList.Count; i++)
            {
                int index = _contactList.IndexOfSafe(_newContactList[i]);
                if (index > -1)
                {
                    //continuation of collision
                    Contact contact = _newContactList[i];
                    contact.NormalImpulse = _contactList[index].NormalImpulse;
                    contact.TangentImpulse = _contactList[index].TangentImpulse;
                    _mergedContactList.Add(contact);
                }
                else
                {
                    //first time collision
                    _mergedContactList.Add(_newContactList[i]);
                }
            }
            _contactList.Clear();
            for (int i = 0; i < _mergedContactList.Count; i++)
            {
                _contactList.Add(_mergedContactList[i]);
            }
        }

        private void Collide(Geom geometry1, Geom geometry2, ContactList contactList)
        {
            int vertexIndex = -1;

            //Iterate the second geometry vertices
            for (int i = 0; i < geometry2.worldVertices.Count; i++)
            {
                if (contactList.Count == _physicsSimulator.maxContactsToDetect)
                    break;

                //grid can be null for "one-way" collision (points)
                if (geometry1.grid == null)
                    break;

                vertexIndex += 1;
                _vertRef = geometry2.WorldVertices[i];
                geometry1.TransformToLocalCoordinates(ref _vertRef, out _localVertex);

                //The geometry intersects when distance <= 0
                //Continue in the list if the current vector does not intersect
                if (!geometry1.Intersect(ref _localVertex, out _feature))
                    continue;

                //If the second geometry's current vector intersects with the first geometry
                //create a new contact and add it to the contact list.
                if (_feature.Distance < 0f)
                {
                    geometry1.TransformNormalToWorld(ref _feature.Normal, out _feature.Normal);
                    Contact contact = new Contact(geometry2.WorldVertices[i], _feature.Normal, _feature.Distance,
                                                  new ContactId(2, vertexIndex, 1));
                    contactList.Add(contact);
                }
            }

            //Iterate the first geometry vertices
            for (int i = 0; i < geometry1.WorldVertices.Count; i++)
            {
                if (contactList.Count == _physicsSimulator.maxContactsToDetect)
                    break;

                //grid can be null for "one-way" collision (points)
                if (geometry2.grid == null)
                    break;

                vertexIndex += 1;
                _vertRef = geometry1.WorldVertices[i];
                geometry2.TransformToLocalCoordinates(ref _vertRef, out _localVertex);

                if (!geometry2.Intersect(ref _localVertex, out _feature))
                    continue;

                if (_feature.Distance < 0f)
                {
                    geometry2.TransformNormalToWorld(ref _feature.Normal, out _feature.Normal);
                    _feature.Normal = -_feature.Normal;
                    Contact contact = new Contact(geometry1.WorldVertices[i], _feature.Normal, _feature.Distance,
                                                  new ContactId(2, vertexIndex, 1));
                    contactList.Add(contact);
                }
            }

            //sort contact list by seperation (amount of penetration)
            contactList.Sort(_contactComparer);

            //resolve deepest contacts first
            int contactCount = contactList.Count;
            if (contactCount > _physicsSimulator.maxContactsToResolve)
            {
                contactList.RemoveRange(_physicsSimulator.maxContactsToResolve,
                                        contactCount - _physicsSimulator.maxContactsToResolve);
            }

            //allow user to cancel collision if desired
            if (geometry1.OnCollision != null)
            {
                //If the contactlist is populated, this means that there is an collision.
                if (contactList.Count > 0)
                {
                    if (!geometry1.OnCollision(geometry1, geometry2, contactList))
                    {
                        //The user aborted the collision. Clear the contact list as we don't need it anymore.
                        contactList.Clear();
                    }
                }
            }

            //allow user to cancel collision if desired
            if (geometry2.OnCollision != null)
            {
                if (contactList.Count > 0)
                {
                    if (!geometry2.OnCollision(geometry2, geometry1, contactList))
                    {
                        contactList.Clear();
                    }
                }
            }
        }

        private static int CompareSeperation(Contact c1, Contact c2)
        {
            if (c1.Separation < c2.Separation)
            {
                return -1;
            }

            if (c1.Separation == c2.Separation)
            {
                return 0;
            }
            return 1;
        }

        private void InitializeContactLists(int maxContactsToDetect)
        {
            if (_contactList == null)
            {
                _contactList = new ContactList(maxContactsToDetect);
            }
            if (_newContactList == null)
            {
                _newContactList = new ContactList(maxContactsToDetect);
            }
            if (_mergedContactList == null)
            {
                _mergedContactList = new ContactList(maxContactsToDetect);
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Arbiter))
                return false;

            return Equals((Arbiter) obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Arbiter arbiter1, Arbiter arbiter2)
        {
            return arbiter1.Equals(arbiter2);
        }

        public static bool operator !=(Arbiter arbiter1, Arbiter arbiter2)
        {
            return !arbiter1.Equals(arbiter2);
        }

        #region Collide variables

        private Feature _feature;
        private Vector2 _localVertex;
        private Vector2 _vertRef;

        #endregion

        #region Variables for ApplyImpulse

        private Contact _contact;
        private Vector2 _dv = Vector2.Zero;
        private Vector2 _impulse = Vector2.Zero;
        private Vector2 _impulseBias = Vector2.Zero;
        private float _maxTangentImpulse;
        private float _normalImpulse;

        private float _normalImpulseBias;
        private float _normalImpulseBiasOriginal;
        private float _normalVelocityBias;

        private float _oldNormalImpulse;
        private float _oldTangentImpulse;
        private Vector2 _tangent = Vector2.Zero;
        private float _tangentImpulse;
        private float _vn;
        private float _vt;

        #endregion
    }
}