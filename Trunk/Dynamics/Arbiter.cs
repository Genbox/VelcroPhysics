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

            switch (physicsSimulator.FrictionType)
            {
                case FrictionType.Average:
                    _frictionCoefficientCombined = (GeometryA.FrictionCoefficient + GeometryB.FrictionCoefficient)/2f;
                    break;
                case FrictionType.Minimum:
                    _frictionCoefficientCombined = Math.Min(GeometryA.FrictionCoefficient, GeometryB.FrictionCoefficient);
                    break;
                default:
                    _frictionCoefficientCombined = (GeometryA.FrictionCoefficient + GeometryB.FrictionCoefficient)/2f;
                    break;
            }

            //Initialize the contact lists
            if (_contactList == null)
            {
                _contactList = new ContactList(physicsSimulator.MaxContactsToDetect);
            }
            if (_newContactList == null)
            {
                _newContactList = new ContactList(physicsSimulator.MaxContactsToDetect);
            }
            if (_mergedContactList == null)
            {
                _mergedContactList = new ContactList(physicsSimulator.MaxContactsToDetect);
            }
        }

        internal void PreStepImpulse(float inverseDt)
        {
            for (int i = 0; i < _contactList.Count; i++)
            {
                Contact contact = _contactList[i];

                //calculate contact offset from body position
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
                contact.massNormal = 1f/_kNormal;

                //calculate mass tangent
                _float1 = 1;
                Calculator.Cross(ref contact.Normal, ref _float1, out _tangent);
                Vector2.Dot(ref _r1, ref _tangent, out _rt1);
                Vector2.Dot(ref _r2, ref _tangent, out _rt2);
                _kTangent = GeometryA.body.inverseMass + GeometryB.body.inverseMass;

                Vector2.Dot(ref _r1, ref _r1, out _float1);
                Vector2.Dot(ref _r2, ref _r2, out _float2);
                _kTangent += GeometryA.body.inverseMomentOfInertia*(_float1 - _rt1*_rt1) +
                             GeometryB.body.InverseMomentOfInertia*(_float2 - _rt2*_rt2);
                contact.massTangent = 1f/_kTangent;

                //calc velocity bias
                _min = Math.Min(0, _physicsSimulator.AllowedPenetration + contact.Separation);
                contact.normalVelocityBias = -_physicsSimulator.BiasFactor*inverseDt*_min;

                //Compute the restitution, we average the restitution of the two bodies
                _restitution = (GeometryA.RestitutionCoefficient + GeometryB.RestitutionCoefficient)*.5f;

                //calc bounce velocity
                GeometryA.body.GetVelocityAtWorldOffset(ref _r1, out _vec1);
                GeometryB.body.GetVelocityAtWorldOffset(ref _r2, out _vec2);
                Vector2.Subtract(ref _vec2, ref _vec1, out _dv);

                //calc velocity difference along contact normal
                Vector2.Dot(ref _dv, ref contact.Normal, out _vn);
                contact.bounceVelocity = _vn*_restitution;

                //apply accumulated impulse
                Vector2.Multiply(ref contact.Normal, contact.normalImpulse, out _vec1);
                Vector2.Multiply(ref _tangent, contact.tangentImpulse, out _vec2);
                Vector2.Add(ref _vec1, ref _vec2, out _impulse);

                GeometryB.body.ApplyImpulseAtWorldOffset(ref _impulse, ref _r2);

                Vector2.Multiply(ref _impulse, -1, out _impulse);
                GeometryA.body.ApplyImpulseAtWorldOffset(ref _impulse, ref _r1);

                contact.normalImpulseBias = 0;
                _contactList[i] = contact;
            }
        }

        internal void ApplyImpulse()
        {
            for (int i = 0; i < _contactList.Count; i++)
            {
                _contact = _contactList[i];

                #region INLINE: Vector2.Subtract(ref _contact.Position, ref geometryA.body.position, out _contact.R1);

                _contact.r1.X = _contact.Position.X - GeometryA.body.position.X;
                _contact.r1.Y = _contact.Position.Y - GeometryA.body.position.Y;

                #endregion

                #region INLINE: Vector2.Subtract(ref _contact.Position, ref geometryB.body.position, out _contact.R2);

                _contact.r2.X = _contact.Position.X - GeometryB.body.position.X;
                _contact.r2.Y = _contact.Position.Y - GeometryB.body.position.Y;

                #endregion

                //calc velocity difference
                GeometryA.body.GetVelocityAtWorldOffset(ref _contact.r1, out _vec1);
                GeometryB.body.GetVelocityAtWorldOffset(ref _contact.r2, out _vec2);

                #region INLINE: Vector2.Subtract(ref _vec2, ref _vec1, out _dv);

                _dv.X = _vec2.X - _vec1.X;
                _dv.Y = _vec2.Y - _vec1.Y;

                #endregion

                //calc velocity difference along contact normal
                #region INLINE: Vector2.Dot(ref _dv, ref _contact.Normal, out _vn);

                _vn = (_dv.X*_contact.Normal.X) + (_dv.Y*_contact.Normal.Y);

                #endregion

                _normalImpulse = _contact.massNormal*-(_vn + _contact.bounceVelocity); //uncomment for preserve momentum

                //clamp accumulated impulse
                _oldNormalImpulse = _contact.normalImpulse;
                _contact.normalImpulse = Math.Max(_oldNormalImpulse + _normalImpulse, 0);
                _normalImpulse = _contact.normalImpulse - _oldNormalImpulse;

                //apply contact impulse
                #region INLINE: Vector2.Multiply(ref _contact.Normal, _normalImpulse, out _impulse);

                _impulse.X = _contact.Normal.X*_normalImpulse;
                _impulse.Y = _contact.Normal.Y*_normalImpulse;

                #endregion

                GeometryB.body.ApplyImpulseAtWorldOffset(ref _impulse, ref _contact.r2);

                #region INLINE: Vector2.Multiply(ref _impulse, -1, out _impulse);

                _impulse.X = _impulse.X*-1;
                _impulse.Y = _impulse.Y*-1;

                #endregion

                GeometryA.body.ApplyImpulseAtWorldOffset(ref _impulse, ref _contact.r1);

                //calc velocity bias difference (bias preserves momentum)
                GeometryA.body.GetVelocityBiasAtWorldOffset(ref _contact.r1, out _vec1);
                GeometryB.body.GetVelocityBiasAtWorldOffset(ref _contact.r2, out _vec2);

                #region INLINE: Vector2.Subtract(ref _vec2, ref _vec1, out _dv);

                _dv.X = _vec2.X - _vec1.X;
                _dv.Y = _vec2.Y - _vec1.Y;

                #endregion

                //calc velocity bias along contact normal
                #region INLINE: Vector2.Dot(ref _dv, ref _contact.Normal, out _normalVelocityBias);

                _normalVelocityBias = (_dv.X*_contact.Normal.X) + (_dv.Y*_contact.Normal.Y);

                #endregion

                _normalImpulseBias = _contact.massNormal*(-_normalVelocityBias + _contact.normalVelocityBias);
                _normalImpulseBiasOriginal = _contact.normalImpulseBias;
                _contact.normalImpulseBias = Math.Max(_normalImpulseBiasOriginal + _normalImpulseBias, 0);
                _normalImpulseBias = _contact.normalImpulseBias - _normalImpulseBiasOriginal;

                #region INLINE: Vector2.Multiply(ref _contact.Normal, _normalImpulseBias, out _impulseBias);

                _impulseBias.X = _contact.Normal.X*_normalImpulseBias;
                _impulseBias.Y = _contact.Normal.Y*_normalImpulseBias;

                #endregion

                //apply bias impulse
                GeometryB.body.ApplyBiasImpulseAtWorldOffset(ref _impulseBias, ref _contact.r2);

                #region INLINE: Vector2.Multiply(ref _impulseBias, -1, out _impulseBias);

                _impulseBias.X = _impulseBias.X*-1;
                _impulseBias.Y = _impulseBias.Y*-1;

                #endregion

                GeometryA.body.ApplyBiasImpulseAtWorldOffset(ref _impulseBias, ref _contact.r1);

                //calc relative velocity at contact.
                GeometryA.body.GetVelocityAtWorldOffset(ref _contact.r1, out _vec1);
                GeometryB.body.GetVelocityAtWorldOffset(ref _contact.r2, out _vec2);

                #region INLINE: Vector2.Subtract(ref _vec2, ref _vec1, out _dv);

                _dv.X = _vec2.X - _vec1.X;
                _dv.Y = _vec2.Y - _vec1.Y;

                #endregion

                //compute friction impulse
                _maxTangentImpulse = _frictionCoefficientCombined*_contact.normalImpulse;
                _float1 = 1;

                #region INLINE: Calculator.Cross(ref _contact.Normal, ref _float1, out _tangent);

                _tangent.X = _float1*_contact.Normal.Y;
                _tangent.Y = -_float1*_contact.Normal.X;

                #endregion

                #region INLINE: Vector2.Dot(ref _dv, ref _tangent, out _vt);

                _vt = (_dv.X*_tangent.X) + (_dv.Y*_tangent.Y);

                #endregion

                _tangentImpulse = _contact.massTangent*(-_vt);

                //clamp friction
                _oldTangentImpulse = _contact.tangentImpulse;
                _contact.tangentImpulse = Calculator.Clamp(_oldTangentImpulse + _tangentImpulse, -_maxTangentImpulse,
                                                           _maxTangentImpulse);
                _tangentImpulse = _contact.tangentImpulse - _oldTangentImpulse;

                //apply friction impulse
                #region INLINE:Vector2.Multiply(ref _tangent, _tangentImpulse, out _impulse);

                _impulse.X = _tangent.X*_tangentImpulse;
                _impulse.Y = _tangent.Y*_tangentImpulse;

                #endregion

                //apply impulse
                GeometryB.body.ApplyImpulseAtWorldOffset(ref _impulse, ref _contact.r2);

                #region INLINE: Vector2.Multiply(ref _impulse, -1, out _impulse);

                _impulse.X = _impulse.X*-1;
                _impulse.Y = _impulse.Y*-1;

                #endregion

                GeometryA.body.ApplyImpulseAtWorldOffset(ref _impulse, ref _contact.r1);

                _contactList[i] = _contact;
            }
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

            //Call the narrow phase collider and get back the contacts
            _physicsSimulator.NarrowPhaseCollider.Collide(GeometryA, GeometryB, _newContactList);

            //sort contact list by seperation (amount of penetration)
            _newContactList.Sort(_contactComparer);

            //resolve deepest contacts first
            int contactCount = _newContactList.Count;
            if (contactCount > _physicsSimulator.MaxContactsToResolve)
                _newContactList.RemoveRange(_physicsSimulator.MaxContactsToResolve, contactCount - _physicsSimulator.MaxContactsToResolve);

            //allow user to cancel collision if desired
            if (GeometryA.OnCollision != null)
                if (_newContactList.Count > 0)
                    if (!GeometryA.OnCollision(GeometryA, GeometryB, _newContactList))
                        _newContactList.Clear();

            if (GeometryB.OnCollision != null)
                if (_newContactList.Count > 0)
                    if (!GeometryB.OnCollision(GeometryB, GeometryA, _newContactList))
                        _newContactList.Clear(); _mergedContactList.Clear();

            //Calculate on the new contacts gathered (Warm starting is done here)
            for (int i = 0; i < _newContactList.Count; i++)
            {
                int index = _contactList.IndexOfSafe(_newContactList[i]);
                if (index > -1)
                {
                    //continuation of collision
                    Contact contact = _newContactList[i];
                    contact.normalImpulse = _contactList[index].normalImpulse;
                    contact.tangentImpulse = _contactList[index].tangentImpulse;
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
                _contactList.Add(_mergedContactList[i]);
        }

        private static int CompareSeperation(Contact c1, Contact c2)
        {
            if (c1.Separation < c2.Separation)
                return -1;

            if (c1.Separation == c2.Separation)
                return 0;
            
            return 1;
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