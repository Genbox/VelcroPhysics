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

using Microsoft.Xna.Framework;

namespace FarseerPhysics
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
        public Vector2 Center;

        /// <summary>
        /// The rotational inertia of the shape. This may be about the center or local
        /// origin, depending on usage.
        /// </summary>
        public float Inertia;
    }

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
        Polygon = 1
    }

    /// <summary>
    /// Base class for shapes.
    /// </summary>
    public abstract class Shape
    {
        protected Shape(float radius, float density)
        {
            Radius = radius;
            Density = density;
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
        /// Clone the concrete shape.
        /// </summary>
        /// <returns></returns>
        public abstract Shape Clone();

        /// <summary>
        /// Get the type of this shape. You can use this to down cast to the concrete shape.
        /// </summary>
        /// <value>The type of the shape.</value>
        public ShapeType ShapeType { get; protected set; }

        /// <summary>
        /// Test a point for containment in this shape. This only works for convex shapes.
        /// </summary>
        /// <param name="xf">the shape world transform.</param>
        /// <param name="point">a point in world coordinates.</param>
        /// <returns></returns>
        public abstract bool TestPoint(ref Transform xf, Vector2 point);

        /// <summary>
        /// Cast a ray against this shape.
        /// </summary>
        /// <param name="output">the ray-cast results.</param>
        /// <param name="input">the ray-cast input parameters.</param>
        /// <param name="transform"the transform to be applied to the shape.</param>
        /// <returns>True if the raycast hit something</returns>
        public abstract bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform);

        /// <summary>
        /// Given a transform, compute the associated axis aligned bounding box for this shape.
        /// </summary>
        /// <param name="aabb">returns the axis aligned box.</param>
        /// <param name="xf">the world transform of the shape.</param>
        public abstract void ComputeAABB(out AABB aabb, ref Transform xf);

        /// <summary>
        /// Compute the mass properties of this shape using its dimensions and density.
        /// The inertia tensor is computed about the local origin, not the centroid.
        /// </summary>
        /// <param name="density"></param>
        protected abstract void ComputeMass();

        /// <summary>
        /// Gets or sets the radius.
        /// </summary>
        /// <value>The radius.</value>
        public float Radius
        {
            get { return _radius; }
            set
            {
                _radius = value;
                Radius2 = _radius * _radius;
            }
        }


        /// <summary>
        /// The density in kilograms per meter squared.
        /// </summary>
        /// <value>The density.</value>
        public float Density
        {
            get;
            set;
        }

        public MassData MassData
        {
            get;
            set;
        }

        private float _radius;
        internal float Radius2;
    }
}
