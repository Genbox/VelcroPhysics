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
using Box2DX.Collision;

namespace Box2DX.Dynamics
{
	/// <summary>
	/// Joints and shapes are destroyed when their associated
	/// body is destroyed. Implement this listener so that you
	/// may nullify references to these joints and shapes.
	/// </summary>
	public abstract class DestructionListener
	{
		/// <summary>
		/// Called when any joint is about to be destroyed due
		/// to the destruction of one of its attached bodies.
		/// </summary>
		public abstract void SayGoodbye(Joint joint);

		/// <summary>
		/// Called when any shape is about to be destroyed due
		/// to the destruction of its parent body.
		/// </summary>
		public abstract void SayGoodbye(Shape shape);
	}

	/// <summary>
	/// This is called when a body's shape passes outside of the world boundary.
	/// </summary>
	public abstract class BoundaryListener
	{
		/// <summary>
		/// This is called for each body that leaves the world boundary.
		/// @warning you can't modify the world inside this callback.
		/// </summary>
		public abstract void Violation(Body body);
	}

	/// <summary>
	/// Implement this class to provide collision filtering. In other words, you can implement
	/// this class if you want finer control over contact creation.
	/// </summary>
	public class ContactFilter
	{
		/// <summary>
		/// Return true if contact calculations should be performed between these two shapes.
		/// If you implement your own collision filter you may want to build from this implementation.
		/// @warning for performance reasons this is only called when the AABBs begin to overlap.
		/// </summary>
		public virtual bool ShouldCollide(Shape shape1, Shape shape2)
		{
			FilterData filter1 = shape1.FilterData;
			FilterData filter2 = shape2.FilterData;

			if (filter1.GroupIndex == filter2.GroupIndex && filter1.GroupIndex != 0)
			{
				return filter1.GroupIndex > 0;
			}

			bool collide = (filter1.MaskBits & filter2.CategoryBits) != 0 && (filter1.CategoryBits & filter2.MaskBits) != 0;
			return collide;
		}

		/// <summary>
		/// Return true if the given shape should be considered for ray intersection.
		/// </summary>
		public bool RayCollide(object userData, Shape shape)
		{
			//By default, cast userData as a shape, and then collide if the shapes would collide
			if (userData == null)
				return true;
			return ShouldCollide((Shape)userData, shape);
		}
	}

	public class WorldCallback
	{
		/// <summary>
		/// The default contact filter.
		/// </summary>
		public static ContactFilter DefaultFilter = new ContactFilter();
	}

	/// <summary>
	/// Implement this class to get collision results. You can use these results for
	/// things like sounds and game logic. You can also get contact results by
	/// traversing the contact lists after the time step. However, you might miss
	/// some contacts because continuous physics leads to sub-stepping.
	/// Additionally you may receive multiple callbacks for the same contact in a
	/// single time step.
	/// You should strive to make your callbacks efficient because there may be
	/// many callbacks per time step.
	/// @warning The contact separation is the last computed value.
	/// @warning You cannot create/destroy Box2D entities inside these callbacks.
	/// </summary>
	public abstract class ContactListener
	{
		/// <summary>
		/// Called when a contact point is added. This includes the geometry
		/// and the forces.
		/// </summary>
		public virtual void Add(ContactPoint point) { return; }

		/// <summary>
		/// Called when a contact point persists. This includes the geometry
		/// and the forces.
		/// </summary>
		public virtual void Persist(ContactPoint point) { return; }

		/// <summary>
		/// Called when a contact point is removed. This includes the last
		/// computed geometry and forces.
		/// </summary>
		public virtual void Remove(ContactPoint point) { return; }

		/// <summary>
		/// Called after a contact point is solved.
		/// </summary>
		public virtual void Result(ContactResult point) { return; }
	}

	/// <summary>
	/// Color for debug drawing. Each value has the range [0,1].
	/// </summary>
	public struct Color
	{
		public float R, G, B;

		public Color(float r, float g, float b)
		{
			R = r; G = g; B = b;
		}
	}

	/// <summary>
	/// Implement and register this class with a b2World to provide debug drawing of physics
	/// entities in your game.
	/// </summary>
	public abstract class DebugDraw
	{
		[Flags]
		public enum DrawFlags
		{
			Shape = 0x0001, // draw shapes
			Joint = 0x0002, // draw joint connections
			CoreShape = 0x0004, // draw core (TOI) shapes
			Aabb = 0x0008, // draw axis aligned bounding boxes
			Obb = 0x0010, // draw oriented bounding boxes
			Pair = 0x0020, // draw broad-phase pairs
			CenterOfMass = 0x0040, // draw center of mass frame
			Controller = 0x0080 // draw center of mass frame
		}

		protected DrawFlags _drawFlags;

		public DebugDraw()
		{
			_drawFlags = 0;
		}

		public DrawFlags Flags { get { return _drawFlags; } set { _drawFlags = value; } }

		/// <summary>
		/// Append flags to the current flags.
		/// </summary>
		public void AppendFlags(DrawFlags flags)
		{
			_drawFlags |= flags;
		}

		/// <summary>
		/// Clear flags from the current flags.
		/// </summary>
		public void ClearFlags(DrawFlags flags)
		{
			_drawFlags &= ~flags;
		}

		/// <summary>
		/// Draw a closed polygon provided in CCW order.
		/// </summary>
		public abstract void DrawPolygon(Vec2[] vertices, int vertexCount, Color color);

		/// <summary>
		/// Draw a solid closed polygon provided in CCW order.
		/// </summary>
		public abstract void DrawSolidPolygon(Vec2[] vertices, int vertexCount, Color color);

		/// <summary>
		/// Draw a circle.
		/// </summary>
		public abstract void DrawCircle(Vec2 center, float radius, Color color);

		/// <summary>
		/// Draw a solid circle.
		/// </summary>
		public abstract void DrawSolidCircle(Vec2 center, float radius, Vec2 axis, Color color);

		/// <summary>
		/// Draw a line segment.
		/// </summary>
		public abstract void DrawSegment(Vec2 p1, Vec2 p2, Color color);

		/// <summary>
		/// Draw a transform. Choose your own length scale.
		/// </summary>
		/// <param name="xf">A transform.</param>
		public abstract void DrawXForm(XForm xf);
	}
}
