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

using System;
using System.Collections.Generic;
using System.Text;
using Box2DX.Common;
using Box2DX.Dynamics;
using Box2DX.Collision;

namespace Box2DX.Dynamics
{
    /// This holds contact filtering data.
    public class FilterData
    {
        /// The collision category bits. Normally you would just set one bit.
        public UInt16 categoryBits;

        /// The collision mask bits. This states the categories that this
        /// shape would accept for collision.
        public UInt16 maskBits;

        /// Collision groups allow a certain group of objects to never collide (negative)
        /// or always collide (positive). Zero means no collision group. Non-zero group
        /// filtering always wins against the mask bits.
        public UInt16 groupIndex;
    }

    /// A fixture definition is used to create a fixture. This class defines an
    /// abstract fixture definition. You can reuse fixture definitions safely.
    public class FixtureDef
    {
        /// Holds the shape type for down-casting.
        public ShapeType type;

        /// Use this to store application specific fixture data.
        public object userData;

        /// The friction coefficient, usually in the range [0,1].
        public float friction;

        /// The restitution (elasticity) usually in the range [0,1].
        public float restitution;

        /// The density, usually in kg/m^2.
        public float density;

        /// A sensor shape collects contact information but never generates a collision
        /// response.
        public bool isSensor;

        /// Contact filtering data.
        public FilterData filter;

        /// The constructor sets the default fixture definition values.
        public FixtureDef()
        {
            type = ShapeType.UnknownShape;
            userData = null;
            friction = 0.2f;
            restitution = 0.0f;
            density = 0.0f;
            filter.categoryBits = 0x0001;
            filter.maskBits = 0xFFFF;
            filter.groupIndex = 0;
            isSensor = false;
        }
    }

    /// This structure is used to build a fixture with a circle shape.
    public class CircleDef : FixtureDef
    {
        
        public Vec2 localPosition;
        public float radius;

        public CircleDef()
        {
            type = ShapeType.CircleShape;
            localPosition.SetZero();
            radius = 1.0f;
        }
    }

    /// Convex polygon. The vertices must be ordered so that the outside of
    /// the polygon is on the right side of the edges (looking along the edge
    /// from start to end).
    public class PolygonDef : FixtureDef
    {
        /// The polygon vertices in local coordinates.
        public Vec2[] vertices = new Vec2[Settings.MaxPolygonVertices];

        /// The number of polygon vertices.
        public int vertexCount;
        
        public PolygonDef()
	    {
		    type = ShapeType.PolygonShape;
		    vertexCount = 0;
	    }

	    /// Build vertices to represent an axis-aligned box.
	    /// @param hx the half-width.
	    /// @param hy the half-height.
	    public void SetAsBox(float hx, float hy)
        {
            vertexCount = 4;
            vertices[0].Set(-hx, -hy);
            vertices[1].Set(hx, -hy);
            vertices[2].Set(hx, hy);
            vertices[3].Set(-hx, hy);
        }

	    /// Build vertices to represent an oriented box.
	    /// @param hx the half-width.
	    /// @param hy the half-height.
	    /// @param center the center of the box in local coordinates.
	    /// @param angle the rotation of the box in local coordinates.
	    public void SetAsBox(float hx, float hy, Vec2 center, float angle)
        {
            vertexCount = 4;
            vertices[0].Set(-hx, -hy);
            vertices[1].Set(hx, -hy);
            vertices[2].Set(hx, hy);
            vertices[3].Set(-hx, hy);

            XForm xf;
            xf.Position = center;
            xf.R.Set(angle);

            vertices[0] = Common.Math.Mul(xf, vertices[0]);
            vertices[1] = Common.Math.Mul(xf, vertices[1]);
            vertices[2] = Common.Math.Mul(xf, vertices[2]);
            vertices[3] = Common.Math.Mul(xf, vertices[3]);
        }
    }

    /// This structure is used to build a chain of edges.
    class EdgeDef : FixtureDef
    {
	    /// The start vertex.
	    public Vec2 vertex1;

	    /// The end vertex.
	    public Vec2 vertex2;

        public EdgeDef()
        {
            type = ShapeType.EdgeShape;
        }
    }

    /// A fixture is used to attach a shape to a body for collision detection. A fixture
    /// inherits its transform from its parent. Fixtures hold additional non-geometric data
    /// such as friction, collision filters, etc.
    /// Fixtures are created via b2Body::CreateFixture.
    /// @warning you cannot reuse fixtures.
    public class Fixture
    {
	    ShapeType m_type;
	    Fixture m_next;
	    Body m_body;

	    Shape m_shape;

	    float m_density;
	    float m_friction;
	    float m_restitution;

	    UInt16 m_proxyId;
	    FilterData m_filter;

	    bool m_isSensor;

	    object m_userData;

        public Fixture()
        {
            m_userData = null;
	        m_body = null;
	        m_next = null;
	        m_proxyId = 0; //b2_nullProxy;
	        m_shape = null;
        }
        
        /// Get the type of the child shape. You can use this to down cast to the concrete shape.
	    /// @return the shape type.
	    public ShapeType GetType()
        {
            return m_type;
        }

	    /// Get the child shape. You can modify the child shape, however you should not change the
	    /// number of vertices because this will crash some collision caching mechanisms.
	    public Shape GetShape()
        {
            return m_shape;
        }

	    /// Is this fixture a sensor (non-solid)?
	    /// @return the true if the shape is a sensor.
	    public bool IsSensor()
        {
            return m_isSensor;
        }

	    /// Set if this fixture is a sensor.
	    /// You must call b2World::Refilter to update existing contacts.
	    public void SetSensor(bool sensor)
        {
            m_isSensor = sensor;
        }

	    /// Set the contact filtering data. You must call b2World::Refilter to correct
	    /// existing contacts/non-contacts.
	    public void SetFilterData(FilterData filter)
        {
            m_filter = filter;
        }

	    /// Get the contact filtering data.
	    public FilterData GetFilterData()
        {
            return m_filter;
        }

	    /// Get the parent body of this fixture. This is NULL if the fixture is not attached.
	    /// @return the parent body.
	    public Body GetBody()
        {
            return m_body;
        }

	    /// Get the next fixture in the parent body's fixture list.
	    /// @return the next shape.
	    public Fixture GetNext()
        {
            return m_next;
        }

	    /// Get the user data that was assigned in the fixture definition. Use this to
	    /// store your application specific data.
	    public object GetUserData()
        {
            return m_userData;
        }

	    /// Set the user data. Use this to store your application specific data.
	    public void SetUserData(object data)
        {
            m_userData = data;
        }

	    /// Test a point for containment in this fixture. This only works for convex shapes.
	    /// @param xf the shape world transform.
	    /// @param p a point in world coordinates.
	    public bool TestPoint(Vec2 p)
        {
            return m_shape.TestPoint(m_body.GetXForm(), p);
        }

	    /// Perform a ray cast against this shape.
	    /// @param xf the shape world transform.
	    /// @param lambda returns the hit fraction. You can use this to compute the contact point
	    /// p = (1 - lambda) * segment.p1 + lambda * segment.p2.
	    /// @param normal returns the normal at the contact point. If there is no intersection, the normal
	    /// is not set.
	    /// @param segment defines the begin and end point of the ray cast.
	    /// @param maxLambda a number typically in the range [0,1].
	    public SegmentCollide TestSegment(out float lambda, out Vec2 normal, Segment segment, float maxLambda)
        {
            return m_shape.TestSegment(m_body.GetXForm(), out lambda, out normal, segment, maxLambda);
        }

	    /// Compute the mass properties of this shape using its dimensions and density.
	    /// The inertia tensor is computed about the local origin, not the centroid.
	    /// @param massData returns the mass data for this shape.
	    public void ComputeMass(MassData massData)
        {
            //m_shape->ComputeMass(massData, m_density);
        }

	    /// Compute the volume and centroid of this fixture intersected with a half plane
	    /// @param normal the surface normal
	    /// @param offset the surface offset along normal
	    /// @param c returns the centroid
	    /// @return the total volume less than offset along normal
	    public float ComputeSubmergedArea(Vec2 normal, float offset, Vec2 c)
        {
            return m_shape.ComputeSubmergedArea(normal, offset, m_body.GetXForm(), out c);
        }

	    /// Get the maximum radius about the parent body's center of mass.
	    public float ComputeSweepRadius(Vec2 pivot)
        {
            return m_shape.ComputeSweepRadius(pivot);
        }

	    /// Get the coefficient of friction.
	    public float GetFriction()
        {
            return m_friction;
        }

	    /// Set the coefficient of friction.
	    public void SetFriction(float friction)
        {
            m_friction = friction;
        }

	    /// Get the coefficient of restitution.
	    public float GetRestitution()
        {
            return m_restitution;
        }

	    /// Set the coefficient of restitution.
	    public void SetRestitution(float restitution)
        {
            m_restitution = restitution;
        }

	    /// Get the density.
	    public float GetDensity()
        {
            return m_density;
        }

	    /// Set the density.
	    /// @warning this does not automatically update the mass of the parent body.
	    public void SetDensity(float density)
        {
            m_density = density;
        }

	    // We need separation create/destroy functions from the constructor/destructor because
	    // the destructor cannot access the allocator or broad-phase (no destructor arguments allowed by C++).
        public void Create(BlockAllocator allocator, BroadPhase broadPhase, Body body, XForm xf, FixtureDef def)
        {
            m_userData = def.userData;
	        m_friction = def.friction;
	        m_restitution = def.restitution;
	        m_density = def.density;

	        m_body = body;
	        m_next = null;

	        m_filter = def.filter;

	        m_isSensor = def.isSensor;

	        m_type = def.type;

	        // Allocate and initialize the child shape.
	        switch (m_type)
	        {
	        case ShapeType.CircleShape:
                {
			        CircleShape circle = new CircleShape();
			        CircleDef circleDef = (CircleDef)def;
			        circle.m_p = circleDef.localPosition;
			        circle.m_radius = circleDef.radius;
			        m_shape = circle;
		        }
		        break;

	        case ShapeType.PolygonShape:
		        {
			        PolygonShape polygon = new PolygonShape();
			        PolygonDef polygonDef = (PolygonDef)def;
			        polygon.Set(polygonDef.vertices, polygonDef.vertexCount);
			        m_shape = polygon;
		        }
		        break;

	        case ShapeType.EdgeShape:
		        {
			        EdgeShape edge = new EdgeShape();
			        EdgeDef edgeDef = (EdgeDef)def;
			        edge.Set(edgeDef.vertex1, edgeDef.vertex2);
			        m_shape = edge;
		        }
		        break;

	        default:
                Box2DXDebug.Assert(false);
		        break;
	        }

	        // Create proxy in the broad-phase.
	        AABB aabb;
	        m_shape.ComputeAABB(out aabb, xf);

	        bool inRange = broadPhase.InRange(aabb);

	        // You are creating a shape outside the world box.
            Box2DXDebug.Assert(inRange);

	        if (inRange)
	        {
		        m_proxyId = broadPhase.CreateProxy(aabb, this);
	        }
	        else
	        {
                m_proxyId = UInt16.MaxValue; //b2_nullProxy;
	        }
        }

        // Do we need a destroy method?
	    //public void Destroy(BlockAllocator allocator, BroadPhase broadPhase);

        public bool Synchronize(BroadPhase broadPhase, XForm xf1, XForm xf2)
        {
            if (m_proxyId == UInt16.MaxValue) //b2_nullProxy;
            {
                return false;
            }

            // Compute an AABB that covers the swept shape (may miss some rotation effect).
            AABB aabb1, aabb2;
            m_shape.ComputeAABB(out aabb1, xf1);
            m_shape.ComputeAABB(out aabb2, xf2);

            AABB aabb = new AABB();
            aabb.Combine(aabb1, aabb2);

            if (broadPhase.InRange(aabb))
            {
                broadPhase.MoveProxy(m_proxyId, aabb);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void RefilterProxy(BroadPhase broadPhase, XForm xf)
        {
            if (m_proxyId == UInt16.MaxValue)
            {
                return;
            }

            broadPhase.DestroyProxy(m_proxyId);

            AABB aabb;
            m_shape.ComputeAABB(out aabb, xf);

            bool inRange = broadPhase.InRange(aabb);

            if (inRange)
            {
                m_proxyId = broadPhase.CreateProxy(aabb, this);
            }
            else
            {
                m_proxyId = UInt16.MaxValue;
            }
        }
    }
}