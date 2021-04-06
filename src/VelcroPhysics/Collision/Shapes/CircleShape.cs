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

using Microsoft.Xna.Framework;
using VelcroPhysics.Collision.RayCast;
using VelcroPhysics.Shared;

namespace VelcroPhysics.Collision.Shapes
{
    /// <summary>
    /// A circle shape.
    /// </summary>
    public class CircleShape : Shape
    {
        internal Vector2 _position;

        /// <summary>
        /// Create a new circle with the desired radius and density.
        /// </summary>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="density">The density of the circle.</param>
        public CircleShape(float radius, float density) : base(ShapeType.Circle, radius, density)
        {
            ComputeProperties();
        }

        internal CircleShape() : base(ShapeType.Circle) { }

        public override int ChildCount => 1;

        /// <summary>
        /// Get or set the position of the circle
        /// </summary>
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                ComputeInertia();
            }
        }

        public override bool TestPoint(ref Transform transform, ref Vector2 point)
        {
            return TestPointHelper.TestPointCircle(ref _position, _radius, ref point, ref transform);
        }

        public override bool RayCast(ref RayCastInput input, ref Transform transform, int childIndex, out RayCastOutput output)
        {
            return RayCastHelper.RayCastCircle(ref _position, Radius, ref input, ref transform, out output);
        }

        public override void ComputeAABB(ref Transform transform, int childIndex, out AABB aabb)
        {
            AABBHelper.ComputeCircleAABB(ref _position, _radius, ref transform, out aabb);
        }

        protected sealed override void ComputeProperties()
        {
            ComputeMass();
            ComputeInertia();
        }

        private void ComputeMass()
        {
            //Velcro: We calculate area for later consumption
            float area = Settings.Pi * _2radius;
            MassData.Area = area;
            MassData.Mass = Density * area;
        }

        private void ComputeInertia()
        {
            MassData.Centroid = Position;

            // inertia about the local origin
            MassData.Inertia = MassData.Mass * (0.5f * _2radius + Vector2.Dot(Position, Position));
        }

        /// <summary>
        /// Compare the circle to another circle
        /// </summary>
        /// <param name="shape">The other circle</param>
        /// <returns>True if the two circles are the same size and have the same position</returns>
        public bool CompareTo(CircleShape shape)
        {
            return Radius == shape.Radius && Position == shape.Position;
        }

        public override Shape Clone()
        {
            CircleShape clone = new CircleShape();
            clone.ShapeType = ShapeType;
            clone._radius = Radius;
            clone._2radius = _2radius; //Velcro note: We also copy the cache
            clone._density = _density;
            clone._position = _position;
            clone.MassData = MassData;
            return clone;
        }
    }
}