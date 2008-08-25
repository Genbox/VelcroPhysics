/// <summary>
/// Some of the code and ideas in this class were taken from a paper entitled
/// "Fast and Simple Physics using Sequential Impulses" by Erin Catto
/// </summary>

using System;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Dynamics
{
    public class Arbiter : IEquatable<Arbiter>
    {
        private Comparison<Contact> _contactComparer;
        private float _float1;
        private float _float2;
        private float _frictionCoefficientCombined;

        private ContactList _mergedContactList;
        private ContactList _newContactList;
        private PhysicsSimulator _physicsSimulator;

        //used for ref/out methods
        private Vector2 _vec1 = Vector2.Zero;
        private Vector2 _vec2 = Vector2.Zero;

        public Arbiter()
        {
        }

        internal Arbiter(Geom geometry1, Geom geometry2, PhysicsSimulator physicsSimulator)
        {
            ConstructArbiter(geometry1, geometry2, physicsSimulator);
        }

        public Geom GeomA { get; set; }
        public Geom GeomB { get; set; }

        /// <summary>
        /// Fully exposed for convenience. Should be treated as readonly. Do not add or remove directly from this list.
        /// </summary>
        public ContactList ContactList { get; internal set; }

        internal int ContactCount
        {
            get { return ContactList.Count; }
        }

        #region PreStepImpulse variables

        private float kNormal;
        private float kTangent;
        private float min;
        private Vector2 r1;
        private Vector2 r2;
        private float restitution;
        private float rn1;
        private float rn2;
        private float rt1;
        private float rt2;

        #endregion

        #region IEquatable<Arbiter> Members

        public bool Equals(Arbiter other)
        {
            return (GeomA == other.GeomA) && (GeomB == other.GeomB);
        }

        #endregion

        internal void ConstructArbiter(Geom geometry1, Geom geometry2, PhysicsSimulator physicsSimulator)
        {
            _physicsSimulator = physicsSimulator;

            if (geometry1 < geometry2)
            {
                GeomA = geometry1;
                GeomB = geometry2;
            }
            else
            {
                GeomA = geometry2;
                GeomB = geometry1;
            }

            switch (physicsSimulator.FrictionType)
            {
                case FrictionType.Average:
                    _frictionCoefficientCombined = (GeomA.FrictionCoefficient + GeomB.FrictionCoefficient)/2f;
                    break;
                case FrictionType.Minimum:
                    _frictionCoefficientCombined = Math.Min(GeomA.FrictionCoefficient, GeomB.FrictionCoefficient);
                    break;
                default:
                    _frictionCoefficientCombined = (GeomA.FrictionCoefficient + GeomB.FrictionCoefficient)/2f;
                    break;
            }

            InitializeContactLists(physicsSimulator.MaxContactsToDetect);

            //to prevent garbage build up the contact comparer is pre-initialized and held in memory
            _contactComparer = CompareSeperation;
        }

        internal void PreStepImpulse(float inverseDt)
        {
            Contact contact;
            if (!GeomA.CollisionResponseEnabled || !GeomB.CollisionResponseEnabled)
            {
                return;
            }
            for (int i = 0; i < ContactList.Count; i++)
            {
                contact = ContactList[i];

                //calculate contact offset from body position
                Vector2.Subtract(ref contact.Position, ref GeomA.Body.position, out r1);
                Vector2.Subtract(ref contact.Position, ref GeomB.Body.position, out r2);

                //project normal onto offset vectors
                Vector2.Dot(ref r1, ref contact.Normal, out rn1);
                Vector2.Dot(ref r2, ref contact.Normal, out rn2);

                //calculate mass normal
                float invMassSum = GeomA.Body.InverseMass + GeomB.Body.InverseMass;
                Vector2.Dot(ref r1, ref r1, out _float1);
                Vector2.Dot(ref r2, ref r2, out _float2);
                kNormal = invMassSum + GeomA.Body.InverseMomentOfInertia*(_float1 - rn1*rn1) +
                          GeomB.Body.InverseMomentOfInertia*(_float2 - rn2*rn2);
                contact.MassNormal = 1f/kNormal;

                //calculate mass tangent
                _float1 = 1;
                Calculator.Cross(ref contact.Normal, ref _float1, out _tangent);
                Vector2.Dot(ref r1, ref _tangent, out rt1);
                Vector2.Dot(ref r2, ref _tangent, out rt2);
                kTangent = GeomA.Body.InverseMass + GeomB.Body.InverseMass;

                Vector2.Dot(ref r1, ref r1, out _float1);
                Vector2.Dot(ref r2, ref r2, out _float2);
                kTangent += GeomA.Body.InverseMomentOfInertia*(_float1 - rt1*rt1) +
                            GeomB.Body.InverseMomentOfInertia*(_float2 - rt2*rt2);
                contact.MassTangent = 1f/kTangent;

                //calc velocity bias
                min = Math.Min(0, _physicsSimulator.AllowedPenetration + contact.Seperation);
                contact.NormalVelocityBias = -_physicsSimulator.BiasFactor*inverseDt*min;

                //Compute the restitution, we average the restitution of the two bodies
                //restitution = (2.0f + _geometryA.RestitutionCoefficient + _geometryB.RestitutionCoefficient) * 0.5f;
                restitution = (GeomA.RestitutionCoefficient + GeomB.RestitutionCoefficient)*.5f;

                //calc bounce velocity
                GeomA.Body.GetVelocityAtWorldOffset(ref r1, out _vec1);
                GeomB.Body.GetVelocityAtWorldOffset(ref r2, out _vec2);
                Vector2.Subtract(ref _vec2, ref _vec1, out _dv);

                //calc velocity difference along contact normal
                Vector2.Dot(ref _dv, ref contact.Normal, out _vn);
                contact.BounceVelocity = _vn*restitution;

                //apply accumulated impulse
                Vector2.Multiply(ref contact.Normal, contact.NormalImpulse, out _vec1);
                Vector2.Multiply(ref _tangent, contact.TangentImpulse, out _vec2);
                Vector2.Add(ref _vec1, ref _vec2, out _impulse);

                GeomB.Body.ApplyImpulseAtWorldOffset(ref _impulse, ref r2);

                Vector2.Multiply(ref _impulse, -1, out _impulse);
                GeomA.Body.ApplyImpulseAtWorldOffset(ref _impulse, ref r1);

                contact.NormalImpulseBias = 0;
                ContactList[i] = contact;
            }
        }

        internal void ApplyImpulse()
        {
            if (!GeomA.CollisionResponseEnabled || !GeomB.CollisionResponseEnabled)
            {
                return;
            }
            for (int i = 0; i < ContactList.Count; i++)
            {
                _contact = ContactList[i];

                #region INLINE: Vector2.Subtract(ref contact.Position, ref geometryA.body.position, out contact.R1);

                _contact.R1.X = _contact.Position.X - GeomA.Body.position.X;
                _contact.R1.Y = _contact.Position.Y - GeomA.Body.position.Y;

                #endregion

                #region INLINE: Vector2.Subtract(ref contact.Position, ref geometryB.body.position, out contact.R2);

                _contact.R2.X = _contact.Position.X - GeomB.Body.position.X;
                _contact.R2.Y = _contact.Position.Y - GeomB.Body.position.Y;

                #endregion

                //calc velocity difference
                GeomA.Body.GetVelocityAtWorldOffset(ref _contact.R1, out _vec1);
                GeomB.Body.GetVelocityAtWorldOffset(ref _contact.R2, out _vec2);

                #region INLINE: Vector2.Subtract(ref vec2, ref vec1, out dv);

                _dv.X = _vec2.X - _vec1.X;
                _dv.Y = _vec2.Y - _vec1.Y;

                #endregion

                //calc velocity difference along contact normal

                #region INLINE: Vector2.Dot(ref dv, ref contact.Normal, out vn);

                _vn = (_dv.X*_contact.Normal.X) + (_dv.Y*_contact.Normal.Y);

                #endregion

                ////float normalImpulse = contact.MassNormal * (-vn + contact.NormalVelocityBias); //comment for preserve momentum
                _normalImpulse = _contact.MassNormal*-(_vn + _contact.BounceVelocity); //uncomment for preserve momentum

                //clamp accumulated impulse
                _oldNormalImpulse = _contact.NormalImpulse;
                _contact.NormalImpulse = Math.Max(_oldNormalImpulse + _normalImpulse, 0);
                _normalImpulse = _contact.NormalImpulse - _oldNormalImpulse;

                //apply contact impulse

                #region INLINE: Vector2.Multiply(ref contact.Normal, normalImpulse, out impulse);

                _impulse.X = _contact.Normal.X*_normalImpulse;
                _impulse.Y = _contact.Normal.Y*_normalImpulse;

                #endregion

                GeomB.Body.ApplyImpulseAtWorldOffset(ref _impulse, ref _contact.R2);

                #region INLINE: Vector2.Multiply(ref impulse, -1, out impulse);

                _impulse.X = _impulse.X*-1;
                _impulse.Y = _impulse.Y*-1;

                #endregion

                GeomA.Body.ApplyImpulseAtWorldOffset(ref _impulse, ref _contact.R1);

                //calc velocity bias difference (bias preserves momentum)
                GeomA.Body.GetVelocityBiasAtWorldOffset(ref _contact.R1, out _vec1);
                GeomB.Body.GetVelocityBiasAtWorldOffset(ref _contact.R2, out _vec2);

                #region INLINE: Vector2.Subtract(ref vec2, ref vec1, out dv);

                _dv.X = _vec2.X - _vec1.X;
                _dv.Y = _vec2.Y - _vec1.Y;

                #endregion

                //calc velocity bias along contact normal

                #region INLINE: Vector2.Dot(ref dv, ref contact.Normal, out normalVelocityBias);

                _normalVelocityBias = (_dv.X*_contact.Normal.X) + (_dv.Y*_contact.Normal.Y);

                #endregion

                _normalImpulseBias = _contact.MassNormal*(-_normalVelocityBias + _contact.NormalVelocityBias);
                _normalImpulseBiasOriginal = _contact.NormalImpulseBias;
                _contact.NormalImpulseBias = Math.Max(_normalImpulseBiasOriginal + _normalImpulseBias, 0);
                _normalImpulseBias = _contact.NormalImpulseBias - _normalImpulseBiasOriginal;

                #region INLINE: Vector2.Multiply(ref contact.Normal, normalImpulseBias, out impulseBias);

                _impulseBias.X = _contact.Normal.X*_normalImpulseBias;
                _impulseBias.Y = _contact.Normal.Y*_normalImpulseBias;

                #endregion

                //apply bias impulse
                GeomB.Body.ApplyBiasImpulseAtWorldOffset(ref _impulseBias, ref _contact.R2);

                #region INLINE: Vector2.Multiply(ref impulseBias, -1, out impulseBias);

                _impulseBias.X = _impulseBias.X*-1;
                _impulseBias.Y = _impulseBias.Y*-1;

                #endregion

                GeomA.Body.ApplyBiasImpulseAtWorldOffset(ref _impulseBias, ref _contact.R1);

                //calc relative velocity at contact.
                GeomA.Body.GetVelocityAtWorldOffset(ref _contact.R1, out _vec1);
                GeomB.Body.GetVelocityAtWorldOffset(ref _contact.R2, out _vec2);

                #region INLINE: Vector2.Subtract(ref vec2, ref vec1, out dv);

                _dv.X = _vec2.X - _vec1.X;
                _dv.Y = _vec2.Y - _vec1.Y;

                #endregion

                //compute friction impulse
                _maxTangentImpulse = _frictionCoefficientCombined*_contact.NormalImpulse;

                //Vector2 tangent = Calculator.Cross(contact.Normal, 1);
                _float1 = 1;

                #region INLINE: Calculator.Cross(ref contact.Normal, ref float1, out tangent);

                _tangent.X = _float1*_contact.Normal.Y;
                _tangent.Y = -_float1*_contact.Normal.X;

                #endregion

                //float vt = Vector2.Dot(dv, tangent);

                #region INLINE: Vector2.Dot(ref dv, ref tangent, out vt);

                _vt = (_dv.X*_tangent.X) + (_dv.Y*_tangent.Y);

                #endregion

                _tangentImpulse = _contact.MassTangent*(-_vt);

                ////clamp friction
                _oldTangentImpulse = _contact.TangentImpulse;
                _contact.TangentImpulse = Calculator.Clamp(_oldTangentImpulse + _tangentImpulse, -_maxTangentImpulse,
                                                           _maxTangentImpulse);
                _tangentImpulse = _contact.TangentImpulse - _oldTangentImpulse;

                //apply friction impulse

                #region INLINE:Vector2.Multiply(ref tangent, tangentImpulse, out impulse);

                _impulse.X = _tangent.X*_tangentImpulse;
                _impulse.Y = _tangent.Y*_tangentImpulse;

                #endregion

                //apply impulse
                GeomB.Body.ApplyImpulseAtWorldOffset(ref _impulse, ref _contact.R2);

                #region INLINE: Vector2.Multiply(ref impulse, -1, out impulse);

                _impulse.X = _impulse.X*-1;
                _impulse.Y = _impulse.Y*-1;

                #endregion

                GeomA.Body.ApplyImpulseAtWorldOffset(ref _impulse, ref _contact.R1);

                ContactList[i] = _contact;
            }
        }

        internal bool ContainsDisposedGeom()
        {
            return GeomA.IsDisposed || GeomB.IsDisposed;
        }

        internal void Reset()
        {
            GeomA = null;
            GeomB = null;
            ContactList.Clear();
            _newContactList.Clear();
            _mergedContactList.Clear();
        }

        internal void Collide()
        {
            _newContactList.Clear();
            //_geometryA.Collide(_geometryB, _newContactList);

            #region Added by Daniel Pramel 08/17/08

            /*
             * we don't want to check for a collision, if both bodies are disabled...
             */

            if (GeomA.Body.Enabled == false && GeomB.Body.Enabled == false)
            {
                _mergedContactList.Clear();
                ContactList.Clear();
                return;
            }

            #endregion

            Collide(GeomA, GeomB, _newContactList);
            _mergedContactList.Clear();

            for (int i = 0; i < _newContactList.Count; i++)
            {
                //int index = _contactList.IndexOf(_newContactList[i]);
                int index = ContactList.IndexOfSafe(_newContactList[i]);
                if (index > -1)
                {
                    //continuation of collision
                    Contact contact = _newContactList[i];
                    contact.NormalImpulse = ContactList[index].NormalImpulse;
                    contact.TangentImpulse = ContactList[index].TangentImpulse;
                    _mergedContactList.Add(contact);
                }
                else
                {
                    //first time collision
                    _mergedContactList.Add(_newContactList[i]);
                }
            }
            ContactList.Clear();
            for (int i = 0; i < _mergedContactList.Count; i++)
            {
                ContactList.Add(_mergedContactList[i]);
            }
        }

        private void Collide(Geom geometry1, Geom geometry2, ContactList contactList)
        {
            int vertexIndex = -1;
            //matrixInverseTemp = this.MatrixInverse;
            //matrixTemp = geometry1.Matrix;
            for (int i = 0; i < geometry2.WorldVertices.Count; i++)
            {
                if (contactList.Count == _physicsSimulator.MaxContactsToDetect)
                {
                    break;
                }
                if (geometry1.Grid == null)
                {
                    break;
                } //grid can be null for "one-way" collision (points)

                vertexIndex += 1;
                _vertRef = geometry2.WorldVertices[i];
                geometry1.TransformToLocalCoordinates(ref _vertRef, out _localVertex);
                if (!geometry1.Intersect(ref _localVertex, out _feature))
                {
                    continue;
                }

                if (_feature.Distance < 0f)
                {
                    geometry1.TransformNormalToWorld(ref _feature.Normal, out _feature.Normal);
                    Contact contact = new Contact(geometry2.WorldVertices[i], _feature.Normal, _feature.Distance,
                                                  new ContactId(2, vertexIndex, 1));
                    contactList.Add(contact);
                }
            }

            for (int i = 0; i < geometry1.WorldVertices.Count; i++)
            {
                if (contactList.Count == _physicsSimulator.MaxContactsToDetect)
                {
                    break;
                }
                if (geometry2.Grid == null)
                {
                    break;
                } //grid can be null for "one-way" collision (points)

                vertexIndex += 1;

                _vertRef = geometry1.WorldVertices[i];
                geometry2.TransformToLocalCoordinates(ref _vertRef, out _localVertex);
                if (!geometry2.Intersect(ref _localVertex, out _feature))
                {
                    continue;
                }

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
            if (contactList.Count > _physicsSimulator.MaxContactsToResolve)
            {
                contactList.RemoveRange(_physicsSimulator.MaxContactsToResolve,
                                        contactCount - _physicsSimulator.MaxContactsToResolve);
            }

            //allow user to cancel collision if desired
            if (geometry1.Collision != null)
            {
                if (contactList.Count > 0)
                {
                    if (!geometry1.Collision(geometry1, geometry2, contactList))
                    {
                        contactList.Clear();
                    }
                }
            }

            //allow user to cancel collision if desired
            if (geometry2.Collision != null)
            {
                if (contactList.Count > 0)
                {
                    if (!geometry2.Collision(geometry2, geometry1, contactList))
                    {
                        contactList.Clear();
                    }
                }
            }
        }

        private static int CompareSeperation(Contact c1, Contact c2)
        {
            if (c1.Seperation < c2.Seperation)
            {
                return -1;
            }
            if (c1.Seperation == c2.Seperation)
            {
                return 0;
            }
            return 1;
        }

        private void InitializeContactLists(int maxContactsToDetect)
        {
            if (ContactList == null)
            {
                ContactList = new ContactList(maxContactsToDetect);
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
            {
                throw new ArgumentException("The object being compared must be of type 'Arbiter'");
            }
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

        #region variables for ApplyImpulse

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