﻿/*
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
namespace Box2D.XNA
{
    /// This holds the mass data computed for a shape.
    public struct MassData
    {
        /// The mass of the shape, usually in kilograms.
        public float mass;

        /// The position of the shape's centroid relative to the shape's origin.
        public Vector2 center;

        /// The rotational inertia of the shape. This may be about the center or local
        /// origin, depending on usage.
        public float i;
    };

    public enum ShapeType
    {   
        Unknown = -1,
        Circle = 0,
        Polygon = 1,
        TypeCount = 2,
    };

    public abstract class Shape
    {
	    public Shape() 
        {
            ShapeType = ShapeType.Unknown; 
        }

	    /// Clone the concrete shape using the provided allocator.
	    public abstract Shape Clone();

	    /// Get the type of this shape. You can use this to down cast to the concrete shape.
	    /// @return the shape type.
        public ShapeType ShapeType { get; protected set; }

	    /// Test a point for containment in this shape. This only works for convex shapes.
	    /// @param xf the shape world transform.
	    /// @param p a point in world coordinates.
	    public abstract bool TestPoint(ref Transform xf, Vector2 p);

	    /// Cast a ray against this shape.
	    /// @param output the ray-cast results.
	    /// @param input the ray-cast input parameters.
	    /// @param transform the transform to be applied to the shape.
        public abstract bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform);


	    /// Given a transform, compute the associated axis aligned bounding box for this shape.
	    /// @param aabb returns the axis aligned box.
	    /// @param xf the world transform of the shape.
	    public abstract void ComputeAABB(out AABB aabb, ref Transform xf);

	    /// Compute the mass properties of this shape using its dimensions and density.
	    /// The inertia tensor is computed about the local origin, not the centroid.
	    /// @param massData returns the mass data for this shape.
	    /// @param density the density in kilograms per meter squared.
	    public abstract void ComputeMass(out MassData massData, float density);

        public abstract int GetSupport(Vector2 d);
        public abstract Vector2 GetSupportVertex(Vector2 d);
        public abstract int GetVertexCount();
        public abstract Vector2 GetVertex(int index);

	    public float _radius;
    }
}
