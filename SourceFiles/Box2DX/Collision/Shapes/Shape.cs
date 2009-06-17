/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2007 Erin Catto http://www.gphysics.com

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Collections.Generic;
using System.Text;

using Box2DX.Common;
using Box2DX.Dynamics;

namespace Box2DX.Collision
{
	/// <summary>
	/// This holds the mass data computed for a shape.
	/// </summary>
	public struct MassData
	{
		/// <summary>
		/// The mass of the shape, usually in kilograms.
		/// </summary>
		public float Mass;

		/// <summary>
		/// The position of the shape's centroid relative to the shape's origin.
		/// </summary>
		public Vec2 Center;

		/// <summary>
		/// The rotational inertia of the shape.
		/// </summary>
		public float I;
	}

	/// <summary>
	/// This holds contact filtering data.
	/// </summary>
	public struct FilterData
	{
		/// <summary>
		/// The collision category bits. Normally you would just set one bit.
		/// </summary>
		public ushort CategoryBits;

		/// <summary>
		/// The collision mask bits. This states the categories that this
		/// shape would accept for collision.
		/// </summary>
		public ushort MaskBits;

		/// <summary>
		/// Collision groups allow a certain group of objects to never collide (negative)
		/// or always collide (positive). Zero means no collision group. Non-zero group
		/// filtering always wins against the mask bits.
		/// </summary>
		public short GroupIndex;
	}

	/// <summary>
	/// The various collision shape types supported by Box2D.
	/// </summary>
	public enum ShapeType
	{
		UnknownShape = -1,
		CircleShape,
		PolygonShape,
		ShapeTypeCount,
	}

	/// <summary>
	/// Returns code from TestSegment
	/// </summary>
	public enum SegmentCollide
	{
		StartInsideCollide = -1,
		MissCollide = 0,
		HitCollide = 1
	}

#warning "CAS"
	/// <summary>
	/// A shape definition is used to construct a shape. This class defines an
	/// abstract shape definition. You can reuse shape definitions safely.
	/// </summary>
	public class ShapeDef
	{
		/// <summary>
		/// Holds the shape type for down-casting.
		/// </summary>
		public ShapeType Type;

		/// <summary>
		/// Use this to store application specify shape data.
		/// </summary>
		public object UserData;

		/// <summary>
		/// The shape's friction coefficient, usually in the range [0,1].
		/// </summary>
		public float Friction;

		/// <summary>
		/// The shape's restitution (elasticity) usually in the range [0,1].
		/// </summary>
		public float Restitution;

		/// <summary>
		/// The shape's density, usually in kg/m^2.
		/// </summary>
		public float Density;

		/// <summary>
		/// A sensor shape collects contact information but never generates a collision
		/// response.
		/// </summary>
		public bool IsSensor;

		/// <summary>
		/// Contact filtering data.
		/// </summary>
		public FilterData Filter;

		/// <summary>
		/// The constructor sets the default shape definition values.
		/// </summary>
		public ShapeDef()
		{
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

	/// <summary>
	/// A shape is used for collision detection. Shapes are created in World.
	/// You can use shape for collision detection before they are attached to the world.
	/// Warning: you cannot reuse shapes.
	/// </summary>
	public abstract class Shape : IDisposable
	{
		#region Fields and Properties

		protected ShapeType _type;
		/// <summary>
		/// Get the type of this shape. You can use this to down cast to the concrete shape.
		/// </summary>
		//public ShapeType Type { get { return _type; } }
		public new ShapeType GetType() { return _type; }

		internal Shape _next;
		/// <summary>
		/// Get the next shape in the parent body's shape list.
		/// </summary>
		//public Shape Next { get { return _next; } }
		public Shape GetNext() { return _next; }

		internal Body _body;
		/// <summary>
		/// Get the parent body of this shape. This is NULL if the shape is not attached.
		/// </summary>
		//public Body Body { get { return _body; } }
		public Body GetBody() { return _body; }

		// Sweep radius relative to the parent body's center of mass.
		protected float _sweepRadius;
		/// <summary>
		/// Get the maximum radius about the parent body's center of mass.
		/// </summary>
		public float GetSweepRadius() { return _sweepRadius; }

		protected float _density;
		public float Density { get { return _density; } set { _density = value; } }

		protected float _friction;
		public float Friction { get { return _friction; } set { _friction = value; } }

		protected float _restitution;
		public float Restitution { get { return _restitution; } set { _restitution = value; } }

		protected ushort _proxyId;

		protected bool _isSensor;
		/// <summary>
		/// Is this shape a sensor (non-solid)?
		/// </summary>
		public bool IsSensor { get { return _isSensor; } }

		protected FilterData _filter;
		/// <summary>
		/// Get\Set the contact filtering data. You must call World.Refilter to correct
		/// existing contacts/non-contacts.
		/// </summary>
		public FilterData FilterData
		{
			get { return _filter; }
			set { _filter = value; }
		}

		protected object _userData;
		/// <summary>
		/// Get the user data that was assigned in the shape definition. Use this to
		/// store your application specific data.
		/// </summary>
		public object UserData
		{
			get { return _userData; }
			set { _userData = value; }
		}

		#endregion Fields and Properties

		protected Shape(ShapeDef def)
		{
			_userData = def.UserData;
			_friction = def.Friction;
			_restitution = def.Restitution;
			_density = def.Density;
			_body = null;
			_sweepRadius = 0.0f;
			_next = null;
			_proxyId = PairManager.NullProxy;
			_filter = def.Filter;
			_isSensor = def.IsSensor;
		}

		/// <summary>
		/// Test a point for containment in this shape. This only works for convex shapes.
		/// </summary>
		/// <param name="xf">The shape world transform.</param>
		/// <param name="p">A point in world coordinates.</param>
		/// <returns></returns>
		public abstract bool TestPoint(XForm xf, Vec2 p);

		/// <summary>
		/// Perform a ray cast against this shape.
		/// </summary>
		/// <param name="xf">The shape world transform.</param>
		/// <param name="lambda">Returns the hit fraction. You can use this to compute the contact point
		/// p = (1 - lambda) * segment.P1 + lambda * segment.P2.</param>
		/// <param name="normal"> Returns the normal at the contact point. If there is no intersection, 
		/// the normal is not set.</param>
		/// <param name="segment">Defines the begin and end point of the ray cast.</param>
		/// <param name="maxLambda">A number typically in the range [0,1].</param>
		public abstract SegmentCollide TestSegment(XForm xf, out float lambda, out Vec2 normal, Segment segment, float maxLambda);

		/// <summary>
		/// Given a transform, compute the associated axis aligned bounding box for this shape.
		/// </summary>
		/// <param name="aabb">Returns the axis aligned box.</param>
		/// <param name="xf">The world transform of the shape.</param>
		public abstract void ComputeAABB(out AABB aabb, XForm xf);

		/// <summary>
		/// Given two transforms, compute the associated swept axis aligned bounding box for this shape.
		/// </summary>
		/// <param name="aabb">Returns the axis aligned box.</param>
		/// <param name="xf1">The starting shape world transform.</param>
		/// <param name="xf2">The ending shape world transform.</param>
		public abstract void ComputeSweptAABB(out AABB aabb, XForm xf1, XForm xf2);

		/// <summary>
		/// Compute the mass properties of this shape using its dimensions and density.
		/// The inertia tensor is computed about the local origin, not the centroid.
		/// </summary>
		/// <param name="massData">Returns the mass data for this shape</param>
		public abstract void ComputeMass(out MassData massData);

		/// <summary>
		/// Compute the volume and centroid of this shape intersected with a half plane.
		/// </summary>
		/// <param name="normal">Normal the surface normal.</param>
		/// <param name="offset">Offset the surface offset along normal.</param>
		/// <param name="xf">The shape transform.</param>
		/// <param name="c">Returns the centroid.</param>
		/// <returns>The total volume less than offset along normal.</returns>
		public abstract float ComputeSubmergedArea(Vec2 normal, float offset, XForm xf, out Vec2 c);

		internal abstract void UpdateSweepRadius(Vec2 center);

		internal static Shape Create(ShapeDef def)
		{
			switch (def.Type)
			{
				case ShapeType.CircleShape:
					{
						return new CircleShape(def);
					}

				case ShapeType.PolygonShape:
					{
						return new PolygonShape(def);
					}

				default:
					Box2DXDebug.Assert(false);
					return null;
			}
		}

		internal static void Destroy(Shape s)
		{
			switch (s.GetType())
			{
				case ShapeType.CircleShape:
					if (s is IDisposable)
						(s as IDisposable).Dispose();
					s = null;
					break;

				case ShapeType.PolygonShape:
					if (s is IDisposable)
						(s as IDisposable).Dispose();
					s = null;
					break;

				default:
					Box2DXDebug.Assert(false);
					break;
			}
		}

		internal void CreateProxy(BroadPhase broadPhase, XForm transform)
		{
			Box2DXDebug.Assert(_proxyId == PairManager.NullProxy);

			AABB aabb;
			ComputeAABB(out aabb, transform);

			bool inRange = broadPhase.InRange(aabb);

			// You are creating a shape outside the world box.
			Box2DXDebug.Assert(inRange);

			if (inRange)
			{
				_proxyId = broadPhase.CreateProxy(aabb, this);
			}
			else
			{
				_proxyId = PairManager.NullProxy;
			}
		}

		internal void DestroyProxy(BroadPhase broadPhase)
		{
			if (_proxyId != PairManager.NullProxy)
			{
				broadPhase.DestroyProxy(_proxyId);
				_proxyId = PairManager.NullProxy;
			}
		}

		internal bool Synchronize(BroadPhase broadPhase, XForm transform1, XForm transform2)
		{
			if (_proxyId == PairManager.NullProxy)
			{
				return false;
			}

			// Compute an AABB that covers the swept shape (may miss some rotation effect).
			AABB aabb;
			ComputeSweptAABB(out aabb, transform1, transform2);

			if (broadPhase.InRange(aabb))
			{
				broadPhase.MoveProxy(_proxyId, aabb);
				return true;
			}
			else
			{
				return false;
			}
		}

		internal void RefilterProxy(BroadPhase broadPhase, XForm transform)
		{
			if (_proxyId == PairManager.NullProxy)
			{
				return;
			}

			broadPhase.DestroyProxy(_proxyId);

			AABB aabb;
			ComputeAABB(out aabb, transform);

			bool inRange = broadPhase.InRange(aabb);

			if (inRange)
			{
				_proxyId = broadPhase.CreateProxy(aabb, this);
			}
			else
			{
				_proxyId = PairManager.NullProxy;
			}
		}

		public virtual void Dispose()
		{
			Box2DXDebug.Assert(_proxyId == PairManager.NullProxy);
		}
	}
}