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
        public abstract void SayGoodbye(Fixture fixture);
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
        public virtual bool ShouldCollide(Fixture fixtureA, Fixture fixtureB)
        {
            Filter filter1 = fixtureA.GetFilterData();
            Filter filter2 = fixtureB.GetFilterData();

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
        public bool RayCollide(object userData, Fixture fixture)
        {
            //By default, cast userData as a shape, and then collide if the shapes would collide
            if (userData == null)
            {
                return true;
            }

            return ShouldCollide((Fixture)userData, fixture);
        }
    }

    /// Contact impulses for reporting. Impulses are used instead of forces because
    /// sub-step forces may approach infinity for rigid body collisions. These
    /// match up one-to-one with the contact points in b2Manifold.
    public class ContactImpulse
    {
        public float[] normalImpulses = new float[Settings.MaxManifoldPoints];
        public float[] tangentImpulses = new float[Settings.MaxManifoldPoints];
    }

    /// Callback class for AABB queries.
    /// See b2World::Query
    public abstract class QueryCallback
    {
        /// Called for each fixture found in the query AABB.
        /// @return false to terminate the query.
        public abstract bool ReportFixture(Fixture fixture);
    }

    /// Callback class for ray casts.
    /// See b2World::RayCast
    public abstract class RayCastCallback
    {
        /// Called for each fixture found in the query. You control how the ray proceeds
        /// by returning a float that indicates the fractional length of the ray. By returning
        /// 0, you set the ray length to zero. By returning the current fraction, you proceed
        /// to find the closest point. By returning 1, you continue with the original ray
        /// clipping.
        /// @param fixture the fixture hit by the ray
        /// @param point the point of initial intersection
        /// @param normal the normal vector at the point of intersection
        /// @return 0 to terminate, fraction to clip the ray for
        /// closest hit, 1 to continue
        public abstract float ReportFixture(Fixture fixture, Vec2 point, Vec2 normal, float fraction);
    };


    /// Implement this class to get contact information. You can use these results for
    /// things like sounds and game logic. You can also get contact results by
    /// traversing the contact lists after the time step. However, you might miss
    /// some contacts because continuous physics leads to sub-stepping.
    /// Additionally you may receive multiple callbacks for the same contact in a
    /// single time step.
    /// You should strive to make your callbacks efficient because there may be
    /// many callbacks per time step.
    /// @warning You cannot create/destroy Box2D entities inside these callbacks.
    public class ContactListener
    {
        /// Called when two fixtures begin to touch.
        public virtual void BeginContact(Contact contact)
        {
            //B2_NOT_USED(contact);
        }

        /// Called when two fixtures cease to touch.
        public virtual void EndContact(Contact contact)
        {
            //B2_NOT_USED(contact);
        }

        /// This is called after a contact is updated. This allows you to inspect a
        /// contact before it goes to the solver. If you are careful, you can modify the
        /// contact manifold (e.g. disable contact).
        /// A copy of the old manifold is provided so that you can detect changes.
        /// Note: this is called only for awake bodies.
        /// Note: this is called even when the number of contact points is zero.
        /// Note: this is not called for sensors.
        /// Note: if you set the number of contact points to zero, you will not
        /// get an EndContact callback. However, you may get a BeginContact callback
        /// the next step.
        public virtual void PreSolve(Contact contact, Manifold oldManifold)
        {
            //B2_NOT_USED(contact);
            //B2_NOT_USED(oldManifold);
        }

        /// This lets you inspect a contact after the solver is finished. This is useful
        /// for inspecting impulses.
        /// Note: the contact manifold does not include time of impact impulses, which can be
        /// arbitrarily large if the sub-step is small. Hence the impulse is provided explicitly
        /// in a separate data structure.
        /// Note: this is only called for contacts that are touching, solid, and awake.
        public virtual void PostSolve(Contact contact, ContactImpulse impulse)
        {
            //B2_NOT_USED(contact);
            //B2_NOT_USED(impulse);
        }
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
        public void Set(float r, float g, float b)
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
            Aabb = 0x0004, // draw axis aligned bounding boxes
            Pair = 0x0008, // draw broad-phase pairs
            CenterOfMass = 0x0010, // draw center of mass frame
        };

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
        public abstract void DrawXForm(Transform xf);

        //public abstract void DrawPoint(Vec2 p, float size, Color color);

        //public abstract void DrawString(int x, int y, string text);

        //public abstract void DrawAABB(AABB aabb, Color c);
    }
}
