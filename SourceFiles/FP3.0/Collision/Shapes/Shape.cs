/*
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
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

using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision.Shapes
{
    /// <summary>
    /// Type of shape
    /// </summary>
    public enum ShapeType
    {
        /// <summary>
        /// The shape type is unknown or not set.
        /// </summary>
        Unknown = -1,
        /// <summary>
        /// The shape type is a circle
        /// </summary>
        Circle = 0,
        /// <summary>
        /// The shape type is a polygon
        /// </summary>
        Polygon = 1,
        TypeCount = 2,
    }

    /// <summary>
    /// Base class for shapes.
    /// </summary>
    public abstract class Shape
    {
        /// <summary>
        /// Area of the shape
        /// </summary>
        public float Area;

        /// <summary>
        /// The position of the shape's centroid relative to the shape's origin.
        /// </summary>
        public Vector2 Center;

        /// <summary>
        /// The rotational inertia of the shape about the local origin.
        /// </summary>
        public float Inertia;

        /// <summary>
        /// The mass of the shape, usually in kilograms.
        /// </summary>
        public float Mass;

        private float _density;

        protected Shape(float radius, float density)
        {
            _density = density;
            Radius = radius;
            ShapeType = ShapeType.Unknown;
        }

        protected Shape(float radius)
        {
            Radius = radius;
        }

        protected Shape()
        {
            ShapeType = ShapeType.Unknown;
        }

        /// <summary>
        /// Get the type of this shape. You can use this to down cast to the concrete shape.
        /// </summary>
        /// <value>The type of the shape.</value>
        public ShapeType ShapeType { get; protected set; }

        /// <summary>
        /// Gets or sets the radius of the shape. Even polygons have a radius.
        /// </summary>
        /// <value>The radius.</value>
        public float Radius { get; set; }

        /// <summary>
        /// The density in kilograms per meter squared.
        /// </summary>
        /// <value>The density.</value>
        public float Density
        {
            get { return _density; }
            set
            {
                _density = value;
                ComputeProperties();
            }
        }

        /// <summary>
        /// Clone the concrete shape.
        /// </summary>
        /// <returns></returns>
        public abstract Shape Clone();

        /// <summary>
        /// Test a point for containment in this shape. This only works for convex shapes.
        /// </summary>
        /// <param name="transform">the shape world transform.</param>
        /// <param name="point">a point in world coordinates.</param>
        /// <returns></returns>
        public abstract bool TestPoint(ref Transform transform, Vector2 point);

        /// <summary>
        /// Cast a ray against this shape.
        /// </summary>
        /// <param name="output">the ray-cast results.</param>
        /// <param name="input">the ray-cast input parameters.</param>
        /// <param name="transform">the transform to be applied to the shape.</param>
        /// <returns>True if the raycast hit something</returns>
        public abstract bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform);

        /// <summary>
        /// Given a transform, compute the associated axis aligned bounding box for this shape.
        /// </summary>
        /// <param name="aabb">returns the axis aligned box.</param>
        /// <param name="transform">the world transform of the shape.</param>
        public abstract void ComputeAABB(out AABB aabb, ref Transform transform);

        /// <summary>
        /// Gets the vertices of the shape. If the shape is not already represented by vertices
        /// an approximation will be made.
        /// </summary>
        /// <returns></returns>
        public abstract Vertices GetVertices();

        /// <summary>
        /// Computes the properties of the shape
        /// The following properties are computed:
        /// - Area
        /// - Mass
        /// - Center of shape
        /// - Interia
        /// </summary>
        protected abstract void ComputeProperties();
    }
}