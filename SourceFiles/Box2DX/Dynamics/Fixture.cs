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

using Box2DX.Common;
using Box2DX.Collision;

namespace Box2DX.Dynamics
{
    /// This holds contact filtering data.
    public class FilterData
    {
        /// The collision category bits. Normally you would just set one bit.
        public UInt16 CategoryBits;

        /// The collision mask bits. This states the categories that this
        /// shape would accept for collision.
        public UInt16 MaskBits;

        /// Collision groups allow a certain group of objects to never collide (negative)
        /// or always collide (positive). Zero means no collision group. Non-zero group
        /// filtering always wins against the mask bits.
        public UInt16 GroupIndex;
    }

    /// A fixture definition is used to create a fixture. This class defines an
    /// abstract fixture definition. You can reuse fixture definitions safely.
    public class FixtureDef
    {
        /// Holds the shape type for down-casting.
        public ShapeType Type;

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
        public FilterData Filter;

        /// The constructor sets the default fixture definition values.
        public FixtureDef()
        {
            Filter = new FilterData();
            
            Type = ShapeType.UnknownShape;
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

    /// This structure is used to build a fixture with a circle shape.
    public class CircleDef : FixtureDef
    {

        public Vec2 LocalPosition;
        public float Radius;

        public CircleDef()
        {
            Type = ShapeType.CircleShape;
            LocalPosition.SetZero();
            Radius = 1.0f;
        }
    }

    /// Convex polygon. The vertices must be ordered so that the outside of
    /// the polygon is on the right side of the edges (looking along the edge
    /// from start to end).
    public class PolygonDef : FixtureDef
    {
        /// The polygon vertices in local coordinates.
        public Vec2[] Vertices = new Vec2[Settings.MaxPolygonVertices];

        /// The number of polygon vertices.
        public int VertexCount;

        public PolygonDef()
        {
            Type = ShapeType.PolygonShape;
            VertexCount = 0;
        }

        /// Build vertices to represent an axis-aligned box.
        /// @param hx the half-width.
        /// @param hy the half-height.
        public void SetAsBox(float hx, float hy)
        {
            VertexCount = 4;
            Vertices[0].Set(-hx, -hy);
            Vertices[1].Set(hx, -hy);
            Vertices[2].Set(hx, hy);
            Vertices[3].Set(-hx, hy);
        }

        /// Build vertices to represent an oriented box.
        /// @param hx the half-width.
        /// @param hy the half-height.
        /// @param center the center of the box in local coordinates.
        /// @param angle the rotation of the box in local coordinates.
        public void SetAsBox(float hx, float hy, Vec2 center, float angle)
        {
            VertexCount = 4;
            Vertices[0].Set(-hx, -hy);
            Vertices[1].Set(hx, -hy);
            Vertices[2].Set(hx, hy);
            Vertices[3].Set(-hx, hy);

            XForm xf = new XForm();
            xf.Position = center;
            xf.R.Set(angle);

            Vertices[0] = Common.Math.Mul(xf, Vertices[0]);
            Vertices[1] = Common.Math.Mul(xf, Vertices[1]);
            Vertices[2] = Common.Math.Mul(xf, Vertices[2]);
            Vertices[3] = Common.Math.Mul(xf, Vertices[3]);
        }
    }

    /// This structure is used to build a chain of edges.
    class EdgeDef : FixtureDef
    {
        /// The start vertex.
        public Vec2 Vertex1;

        /// The end vertex.
        public Vec2 Vertex2;

        public EdgeDef()
        {
            Type = ShapeType.EdgeShape;
        }
    }

    /// A fixture is used to attach a shape to a body for collision detection. A fixture
    /// inherits its transform from its parent. Fixtures hold additional non-geometric data
    /// such as friction, collision filters, etc.
    /// Fixtures are created via b2Body::CreateFixture.
    /// @warning you cannot reuse fixtures.
    public class Fixture
    {
        public ShapeType Type;
        public Fixture Next;
        public Body Body;

        public Shape Shape;

        public float Density;
        public float Friction;
        public float Restitution;

        public UInt16 ProxyId;
        public FilterData Filter;

        public bool IsSensor;

        public object UserData;

        public Fixture()
        {
            UserData = null;
            Body = null;
            Next = null;
            ProxyId = UInt16.MaxValue; //b2_nullProxy;
            Shape = null;
        }

        public void Destroy(BroadPhase broadPhase)
        {
            // Remove proxy from the broad-phase.
            if (ProxyId != UInt16.MaxValue)
            {
                broadPhase.DestroyProxy(ProxyId);
                ProxyId = UInt16.MaxValue;
            }

            // Free the child shape.
            switch (Type)
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

                case ShapeType.EdgeShape:
                    {
                        //b2EdgeShape* s = (b2EdgeShape*)m_shape;
                        //s->~b2EdgeShape();
                        //allocator->Free(s, sizeof(b2EdgeShape));
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
        public ShapeType GetType()
        {
            return Type;
        }

        /// Get the child shape. You can modify the child shape, however you should not change the
        /// number of vertices because this will crash some collision caching mechanisms.
        public Shape GetShape()
        {
            return Shape;
        }

        /// Set the contact filtering data. You must call b2World::Refilter to correct
        /// existing contacts/non-contacts.
        public void SetFilterData(FilterData filter)
        {
            Filter = filter;
        }

        /// Get the contact filtering data.
        public FilterData GetFilterData()
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
            return Next;
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
            return Shape.TestPoint(Body.GetXForm(), p);
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
            return Shape.TestSegment(Body.GetXForm(), out lambda, out normal, segment, maxLambda);
        }

        /// Compute the mass properties of this shape using its dimensions and density.
        /// The inertia tensor is computed about the local origin, not the centroid.
        /// @param massData returns the mass data for this shape.
        public void ComputeMass(out MassData massData)
        {
            Shape.ComputeMass(out massData, Density);
        }

        /// Compute the volume and centroid of this fixture intersected with a half plane
        /// @param normal the surface normal
        /// @param offset the surface offset along normal
        /// @param c returns the centroid
        /// @return the total volume less than offset along normal
        public float ComputeSubmergedArea(Vec2 normal, float offset, out Vec2 c)
        {
            return Shape.ComputeSubmergedArea(normal, offset, Body.GetXForm(), out c);
        }

        /// Get the maximum radius about the parent body's center of mass.
        public float ComputeSweepRadius(Vec2 pivot)
        {
            return Shape.ComputeSweepRadius(pivot);
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

        /// Get the density.
        public float GetDensity()
        {
            return Density;
        }

        /// Set the density.
        /// @warning this does not automatically update the mass of the parent body.
        public void SetDensity(float density)
        {
            Density = density;
        }

        // We need separation create/destroy functions from the constructor/destructor because
        // the destructor cannot access the allocator or broad-phase (no destructor arguments allowed by C++).
        public void Create(BroadPhase broadPhase, Body body, XForm xf, FixtureDef def)
        {
            UserData = def.UserData;
            Friction = def.Friction;
            Restitution = def.Restitution;
            Density = def.Density;

            Body = body;
            Next = null;

            Filter = def.Filter;

            IsSensor = def.IsSensor;

            Type = def.Type;

            // Allocate and initialize the child shape.
            switch (Type)
            {
                case ShapeType.CircleShape:
                    {
                        CircleShape circle = new CircleShape();
                        CircleDef circleDef = (CircleDef)def;
                        circle.LocalPosition = circleDef.LocalPosition;
                        circle.Radius = circleDef.Radius;
                        Shape = circle;
                    }
                    break;

                case ShapeType.PolygonShape:
                    {
                        PolygonShape polygon = new PolygonShape();
                        PolygonDef polygonDef = (PolygonDef)def;
                        polygon.Set(polygonDef.Vertices, polygonDef.VertexCount);
                        Shape = polygon;
                    }
                    break;

                case ShapeType.EdgeShape:
                    {
                        EdgeShape edge = new EdgeShape();
                        EdgeDef edgeDef = (EdgeDef)def;
                        edge.Set(edgeDef.Vertex1, edgeDef.Vertex2);
                        Shape = edge;
                    }
                    break;

                default:
                    Box2DXDebug.Assert(false);
                    break;
            }

            // Create proxy in the broad-phase.
            AABB aabb;
            Shape.ComputeAABB(out aabb, xf);

            bool inRange = broadPhase.InRange(aabb);

            // You are creating a shape outside the world box.
            Box2DXDebug.Assert(inRange);

            if (inRange)
            {
                ProxyId = broadPhase.CreateProxy(aabb, this);
            }
            else
            {
                ProxyId = UInt16.MaxValue; //b2_nullProxy;
            }
        }

        // Do we need a destroy method?
        //public void Destroy(BlockAllocator allocator, BroadPhase broadPhase);

        public bool Synchronize(BroadPhase broadPhase, XForm xf1, XForm xf2)
        {
            if (ProxyId == UInt16.MaxValue) //b2_nullProxy;
            {
                return false;
            }

            // Compute an AABB that covers the swept shape (may miss some rotation effect).
            AABB aabb1, aabb2;
            Shape.ComputeAABB(out aabb1, xf1);
            Shape.ComputeAABB(out aabb2, xf2);

            AABB aabb = new AABB();
            aabb.Combine(aabb1, aabb2);

            if (broadPhase.InRange(aabb))
            {
                broadPhase.MoveProxy(ProxyId, aabb);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void RefilterProxy(BroadPhase broadPhase, XForm xf)
        {
            if (ProxyId == UInt16.MaxValue)
            {
                return;
            }

            broadPhase.DestroyProxy(ProxyId);

            AABB aabb;
            Shape.ComputeAABB(out aabb, xf);

            bool inRange = broadPhase.InRange(aabb);

            if (inRange)
            {
                ProxyId = broadPhase.CreateProxy(aabb, this);
            }
            else
            {
                ProxyId = UInt16.MaxValue;
            }
        }
    }
}
