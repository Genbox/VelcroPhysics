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
        public AABB Aabb;

        public Fixture _next;
        public Body Body;

        public Shape Shape;

        public MassData _massData;
        public float Friction;
        public float Restitution;

        public int ProxyId;
        public Filter Filter;

        public bool IsSensor;

        public object UserData;

        public Fixture()
        {
            UserData = null;
            Body = null;
            _next = null;
            ProxyId = BroadPhase.NullProxy;
            Shape = null;
        }

        public void Destroy(BroadPhase broadPhase)
        {
            // Remove proxy from the broad-phase.
            if (ProxyId != BroadPhase.NullProxy)
            {
                broadPhase.DestroyProxy(ProxyId);
                ProxyId = BroadPhase.NullProxy;
            }

            // Free the child shape.
            switch (Shape.Type)
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

            Shape = null;
        }

        /// Get the type of the child shape. You can use this to down cast to the concrete shape.
        /// @return the shape type.
        public new ShapeType GetType()
        {
            return Shape.GetType();
        }

        /// Get the child shape. You can modify the child shape, however you should not change the
        /// number of vertices because this will crash some collision caching mechanisms.
        public Shape GetShape()
        {
            return Shape;
        }

        /// Get the contact filtering data.
        public Filter GetFilterData()
        {
            return Filter;
        }

        /// Get the parent body of this fixture. This is NULL if the fixture is not attached.
        /// @return the parent body.
        public Body GetBody()
        {
            return Body;
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
            return UserData;
        }

        /// Set the user data. Use this to store your application specific data.
        public void SetUserData(object data)
        {
            UserData = data;
        }

        /// Test a point for containment in this fixture. This only works for convex shapes.
        /// @param xf the shape world transform.
        /// @param p a point in world coordinates.
        public bool TestPoint(Vec2 p)
        {
            return Shape.TestPoint(Body.GetTransform(), p);
        }

        /// Cast a ray against this shape.
        /// @param output the ray-cast results.
        /// @param input the ray-cast input parameters.
        public void RayCast(out RayCastOutput output, ref RayCastInput input)
        {
            Shape.RayCast(out output, ref input, Body.GetTransform());
        }

        /// Get the mass data for this fixture. The mass data is based on the density and
        /// the shape. The rotational inertia is about the shape's origin.
        public MassData GetMassData()
        {
            return _massData;
        }

        /// Get the coefficient of friction.
        public float GetFriction()
        {
            return Friction;
        }

        /// Set the coefficient of friction.
        public void SetFriction(float friction)
        {
            Friction = friction;
        }

        /// Get the coefficient of restitution.
        public float GetRestitution()
        {
            return Restitution;
        }

        /// Set the coefficient of restitution.
        public void SetRestitution(float restitution)
        {
            Restitution = restitution;
        }

        // We need separation create/destroy functions from the constructor/destructor because
        // the destructor cannot access the allocator or broad-phase (no destructor arguments allowed by C++).
        public void Create(BroadPhase broadPhase, Body body, Transform xf, FixtureDef def)
        {
            UserData = def.UserData;
            Friction = def.Friction;
            Restitution = def.Restitution;

            Body = body;
            _next = null;

            Filter = def.Filter;

            IsSensor = def.IsSensor;

            Shape = def.Shape.Clone();

            Shape.ComputeMass(out _massData, def.Density);

            // Create proxy in the broad-phase.
            Shape.ComputeAABB(out Aabb, ref xf);

            ProxyId = broadPhase.CreateProxy(Aabb, this);
        }

        public void Synchronize(BroadPhase broadPhase, Transform transform1, Transform transform2)
        {
            if (ProxyId == BroadPhase.NullProxy)
            {
                return;
            }

            // Compute an AABB that covers the swept shape (may miss some rotation effect).
            AABB aabb1, aabb2;
            Shape.ComputeAABB(out aabb1, ref transform1);
            Shape.ComputeAABB(out aabb2, ref transform2);

            Aabb.Combine(aabb1, aabb2);

            Vec2 displacement = transform2.Position - transform1.Position;

            broadPhase.MoveProxy(ProxyId, Aabb, displacement);
        }

        public void SetFilterData(Filter filter)
        {
            Filter = filter;

            if (Body == null)
            {
                return;
            }

            // Flag associated contacts for filtering.
            ContactEdge edge = Body.GetContactList();
            while (edge != null)
            {
                Contact contact = edge.Contact;
                Fixture fixtureA = contact.GetFixtureA();
                Fixture fixtureB = contact.GetFixtureB();
                if (fixtureA == this || fixtureB == this)
                {
                    contact.FlagForFiltering();
                }
            }
        }

        public void SetSensor(bool sensor)
        {
            if (IsSensor == sensor)
            {
                return;
            }

            IsSensor = sensor;

            if (Body == null)
            {
                return;
            }

            // Flag associated contacts for filtering.
            ContactEdge edge = Body.GetContactList();
            while (edge != null)
            {
                Contact contact = edge.Contact;
                Fixture fixtureA = contact.GetFixtureA();
                Fixture fixtureB = contact.GetFixtureB();
                if (fixtureA == this || fixtureB == this)
                {
                    contact.SetAsSensor(IsSensor);
                }
            }
        }
    }
}
