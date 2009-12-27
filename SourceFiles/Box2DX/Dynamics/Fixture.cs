/*
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com
*
* This software is provided 'as-is', without any express or implied
* warranty.  In no event will the authors be held liable for any damages
* arising from the use of this software.
* Permission is granted to anyone to use this software for any purpose,
* including commercial applications, and to alter it and redistribute it
* freely, subject to the following restrictions:
* 1. The origin of this software must not be misrepresented; you must not
* claim that you wrote the original software. If you use this software
* in a product, an acknowledgment in the product documentation would be
* appreciated but is not required.
* 2. Altered source versions must be plainly marked as such, and must not be
* misrepresented as being the original software.
* 3. This notice may not be removed or altered from any source distribution.
*/

using Box2DX.Common;
using Box2DX.Collision;

namespace Box2DX.Dynamics
{
    /// This holds contact filtering data.
    public struct Filter
    {
        /// The collision category bits. Normally you would just set one bit.
        public ushort CategoryBits;

        /// The collision mask bits. This states the categories that this
        /// shape would accept for collision.
        public ushort MaskBits;

        /// Collision groups allow a certain group of objects to never collide (negative)
        /// or always collide (positive). Zero means no collision group. Non-zero group
        /// filtering always wins against the mask bits.
        public short GroupIndex;
    }

    /// A fixture definition is used to create a fixture. This class defines an
    /// abstract fixture definition. You can reuse fixture definitions safely.
    public class FixtureDef
    {
        /// The shape, this must be set. The shape will be cloned, so you
        /// can create the shape on the stack.
        public Shape Shape;

        /// Use this to store application specific fixture data.
        public object UserData;

        /// The friction coefficient, usually in the range [0,1].
        public float Friction;

        /// The restitution (elasticity) usually in the range [0,1].
        public float Restitution;

        /// The density, usually in kg/m^2.
        public float Density;

        /// A sensor shape collects contact information but never generates a collision
        /// response.
        public bool IsSensor;

        /// Contact filtering data.
        public Filter Filter;

        /// The constructor sets the default fixture definition values.
        public FixtureDef()
        {
            Shape = null;
            UserData = null;
            Friction = 0.2f;
            Restitution = 0.0f;
            Density = 0.0f;
            Filter.CategoryBits = 0x0001;
            Filter.MaskBits = 0xFFFF;
            Filter.GroupIndex = 0;
            IsSensor = false;
        }
    }

    /// A fixture is used to attach a shape to a body for collision detection. A fixture
    /// inherits its transform from its parent. Fixtures hold additional non-geometric data
    /// such as friction, collision filters, etc.
    /// Fixtures are created via b2Body::CreateFixture.
    /// @warning you cannot reuse fixtures.
    public class Fixture
    {
        public AABB _aabb;

        public Fixture _next;
        public Body _body;

        public Shape _shape;

        public float _friction;
        public float _restitution;

        public int _proxyId;
        public Filter _filter;

        public bool _isSensor;

        public object _userData;

        public float _density;

        public Fixture()
        {
            _userData = null;
            _body = null;
            _next = null;
            _proxyId = BroadPhase.NullProxy;
            _shape = null;
            _density = 0.0f;
        }

        public void Destroy()
        {
            // The proxy must be destroyed before calling this.
            Box2DXDebug.Assert(_proxyId == BroadPhase.NullProxy);

            // Free the child shape.
            switch (_shape.Type)
            {
                case ShapeType.CircleShape:
                    {
                        //CircleShape s = (CircleShape)Shape;
                        //s->~b2CircleShape();
                        //allocator->Free(s, sizeof(b2CircleShape));
                    }
                    break;

                case ShapeType.PolygonShape:
                    {
                        //b2PolygonShape* s = (b2PolygonShape*)m_shape;
                        //s->~b2PolygonShape();
                        //allocator->Free(s, sizeof(b2PolygonShape));
                    }
                    break;
                default:
                    Box2DXDebug.Assert(false);
                    break;
            }

            _shape = null;
        }

        public void CreateProxy(BroadPhase broadPhase, Transform xf)
        {
            Box2DXDebug.Assert(_proxyId == BroadPhase.NullProxy);

            // Create proxy in the broad-phase.
            _shape.ComputeAABB(out _aabb, ref xf);
            _proxyId = broadPhase.CreateProxy(_aabb, this);
        }

        public void DestroyProxy(BroadPhase broadPhase)
        {
            if (_proxyId == BroadPhase.NullProxy)
            {
                return;
            }

            // Destroy proxy in the broad-phase.
            broadPhase.DestroyProxy(_proxyId);
            _proxyId = BroadPhase.NullProxy;
        }

        /// Get the type of the child shape. You can use this to down cast to the concrete shape.
        /// @return the shape type.
        public new ShapeType GetType()
        {
            return _shape.GetType();
        }

        /// Get the child shape. You can modify the child shape, however you should not change the
        /// number of vertices because this will crash some collision caching mechanisms.
        public Shape GetShape()
        {
            return _shape;
        }

        /// Get the fixture's AABB. This AABB may be enlarge and/or stale.
        /// If you need a more accurate AABB, compute it using the shape and
        /// the body transform.
        public AABB GetAABB()
        {
            return _aabb;
        }

        /// Get the contact filtering data.
        public Filter GetFilterData()
        {
            return _filter;
        }

        /// Get the parent body of this fixture. This is NULL if the fixture is not attached.
        /// @return the parent body.
        public Body GetBody()
        {
            return _body;
        }

        /// Get the next fixture in the parent body's fixture list.
        /// @return the next shape.
        public Fixture GetNext()
        {
            return _next;
        }

        /// Get the user data that was assigned in the fixture definition. Use this to
        /// store your application specific data.
        public object GetUserData()
        {
            return _userData;
        }

        /// Set the user data. Use this to store your application specific data.
        public void SetUserData(object data)
        {
            _userData = data;
        }

        /// Test a point for containment in this fixture.
        /// @param xf the shape world transform.
        /// @param p a point in world coordinates.
        public bool TestPoint(Vec2 p)
        {
            return _shape.TestPoint(_body.GetTransform(), p);
        }

        /// Cast a ray against this shape.
        /// @param output the ray-cast results.
        /// @param input the ray-cast input parameters.
        public bool RayCast(out RayCastOutput output, ref RayCastInput input)
        {
            return _shape.RayCast(out output, ref input, _body.GetTransform());
        }

        /// Get the mass data for this fixture. The mass data is based on the density and
        /// the shape. The rotational inertia is about the shape's origin.
        public void GetMassData(out MassData massData)
        {
            _shape.ComputeMass(out massData, _density);
        }

        /// Set the density of this fixture. This will _not_ automatically adjust the mass
        /// of the body. You must call b2Body::ResetMassData to update the body's mass.
        public void SetDensity(float density)
        {
            _density = density;
        }

        /// Get the density of this fixture.
        public float GetDensity()
        {
            return _density;
        }

        /// Get the coefficient of friction.
        public float GetFriction()
        {
            return _friction;
        }

        /// Set the coefficient of friction.
        public void SetFriction(float friction)
        {
            _friction = friction;
        }

        /// Get the coefficient of restitution.
        public float GetRestitution()
        {
            return _restitution;
        }

        /// Set the coefficient of restitution.
        public void SetRestitution(float restitution)
        {
            _restitution = restitution;
        }

        // We need separation create/destroy functions from the constructor/destructor because
        // the destructor cannot access the allocator or broad-phase (no destructor arguments allowed by C++).
        public void Create(Body body, FixtureDef def)
        {
            _userData = def.UserData;
            _friction = def.Friction;
            _restitution = def.Restitution;

            _body = body;
            _next = null;

            _filter = def.Filter;

            _isSensor = def.IsSensor;

            _shape = def.Shape.Clone();

            _density = def.Density;
        }

        public void Synchronize(BroadPhase broadPhase, Transform transform1, Transform transform2)
        {
            if (_proxyId == BroadPhase.NullProxy)
            {
                return;
            }

            // Compute an AABB that covers the swept shape (may miss some rotation effect).
            AABB aabb1, aabb2;
            _shape.ComputeAABB(out aabb1, ref transform1);
            _shape.ComputeAABB(out aabb2, ref transform2);

            _aabb.Combine(aabb1, aabb2);

            Vec2 displacement = transform2.Position - transform1.Position;

            broadPhase.MoveProxy(_proxyId, _aabb, displacement);
        }

        /// Set the contact filtering data. This will not update contacts until the next time
        /// step when either parent body is active and awake.
        public void SetFilterData(Filter filter)
        {
            _filter = filter;

            if (_body == null)
            {
                return;
            }

            // Flag associated contacts for filtering.
            ContactEdge edge = _body.GetContactList();
            while (edge != null)
            {
                Contact contact = edge.Contact;
                Fixture fixtureA = contact.GetFixtureA();
                Fixture fixtureB = contact.GetFixtureB();
                if (fixtureA == this || fixtureB == this)
                {
                    contact.FlagForFiltering();
                }

                edge = edge.Next;
            }
        }

        public void SetSensor(bool sensor)
        {
            if (_isSensor == sensor)
            {
                return;
            }

            _isSensor = sensor;

            if (_body == null)
            {
                return;
            }

            ContactEdge edge = _body.GetContactList();
            while (edge != null)
            {
                Contact contact = edge.Contact;
                Fixture fixtureA = contact.GetFixtureA();
                Fixture fixtureB = contact.GetFixtureB();
                if (fixtureA == this || fixtureB == this)
                {
                    contact.SetSensor(fixtureA.IsSensor() || fixtureB.IsSensor());
                }

                edge = edge.Next;
            }
        }

        public bool IsSensor()
        {
            return _isSensor;
        }
    }
}
