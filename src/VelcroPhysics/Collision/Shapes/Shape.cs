/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
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

using System.Diagnostics;
using Genbox.VelcroPhysics.Collision.RayCast;
using Genbox.VelcroPhysics.Shared;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Collision.Shapes
{
    /// <summary>
    /// A shape is used for collision detection. You can create a shape however you like. Shapes used for simulation
    /// in World are created automatically when a Fixture is created. Shapes may encapsulate a one or more child shapes.
    /// </summary>
    public abstract class Shape
    {
        internal float _density;
        internal float _radius;
        internal ShapeType _shapeType;
        internal MassData _massData;

        protected Shape(ShapeType type, float radius = 0, float density = 0)
        {
            Debug.Assert(radius >= 0);
            Debug.Assert(density >= 0);

            _shapeType = type;
            _radius = radius;
            _density = density;
            _massData = new MassData();
        }

        /// <summary>
        /// Contains the properties of the shape such as:
        /// - Area of the shape
        /// - Centroid
        /// - Inertia
        /// - Mass
        /// </summary>
        public void GetMassData(out MassData massData)
        {
            massData = _massData;
        }

        /// <summary>Get the type of this shape.</summary>
        /// <value>The type of the shape.</value>
        public ShapeType ShapeType => _shapeType;

        /// <summary>Get the number of child primitives.</summary>
        public abstract int ChildCount { get; }

        /// <summary>Radius of the Shape Changing the radius causes a recalculation of shape properties.</summary>
        public float Radius
        {
            get => _radius;
            set
            {
                Debug.Assert(value >= 0);

                if (_radius != value)
                {
                    _radius = value;
                    ComputeProperties();
                }
            }
        }

        //Velcro: Moved density to the base shape. Simplifies a lot of code everywhere else
        /// <summary>Gets or sets the density. Changing the density causes a recalculation of shape properties.</summary>
        /// <value>The density.</value>
        public float Density
        {
            get => _density;
            set
            {
                Debug.Assert(value >= 0);

                if (_density != value)
                {
                    _density = value;
                    ComputeProperties();
                }
            }
        }

        /// <summary>Clone the concrete shape</summary>
        /// <returns>A clone of the shape</returns>
        public abstract Shape Clone();

        /// <summary>Test a point for containment in this shape. Note: This only works for convex shapes.</summary>
        /// <param name="transform">The shape world transform.</param>
        /// <param name="point">A point in world coordinates.</param>
        /// <returns>True if the point is inside the shape</returns>
        public abstract bool TestPoint(ref Transform transform, ref Vector2 point);

        /// <summary>Cast a ray against a child shape.</summary>
        /// <param name="input">The ray-cast input parameters.</param>
        /// <param name="transform">The transform to be applied to the shape.</param>
        /// <param name="childIndex">The child shape index.</param>
        /// <param name="output">The ray-cast results.</param>
        /// <returns>True if the ray-cast hits the shape</returns>
        public abstract bool RayCast(ref RayCastInput input, ref Transform transform, int childIndex, out RayCastOutput output);

        /// <summary>Given a transform, compute the associated axis aligned bounding box for a child shape.</summary>
        /// <param name="transform">The world transform of the shape.</param>
        /// <param name="childIndex">The child shape index.</param>
        /// <param name="aabb">The AABB results.</param>
        public abstract void ComputeAABB(ref Transform transform, int childIndex, out AABB aabb);

        /// <summary>
        /// Compute the mass properties of this shape using its dimensions and density. The inertia tensor is computed
        /// about the local origin, not the centroid.
        /// </summary>
        protected abstract void ComputeProperties();
    }
}