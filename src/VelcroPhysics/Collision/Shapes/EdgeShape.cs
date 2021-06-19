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

using Genbox.VelcroPhysics.Collision.RayCast;
using Genbox.VelcroPhysics.Shared;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Collision.Shapes
{
    /// <summary>
    /// A line segment (edge) shape. These can be connected in chains or loops to other edge shapes. Edges created
    /// independently are two-sided and do no provide smooth movement across junctions.
    /// </summary>
    public class EdgeShape : Shape
    {
        internal Vector2 _vertex0;
        internal Vector2 _vertex1;
        internal Vector2 _vertex2;
        internal Vector2 _vertex3;
        internal bool _oneSided;

        /// <summary>Create a new EdgeShape with the specified start and end. This edge supports two-sided collision.</summary>
        /// <param name="start">The start of the edge.</param>
        /// <param name="end">The end of the edge.</param>
        public EdgeShape(Vector2 start, Vector2 end) : base(ShapeType.Edge, Settings.PolygonRadius)
        {
            SetTwoSided(start, end);
        }

        /// <summary>Create a new EdgeShape with ghost vertices for smooth collision. This edge only supports one-sided collision.</summary>
        public EdgeShape(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3) : base(ShapeType.Edge, Settings.PolygonRadius)
        {
            SetOneSided(v0, v1, v2, v3);
        }

        public EdgeShape() : base(ShapeType.Edge, Settings.PolygonRadius) { }

        public override int ChildCount => 1;

        /// <summary>Is true if the edge is connected to an adjacent vertex before vertex 1.</summary>
        public bool OneSided => _oneSided;

        /// <summary>Optional adjacent vertices. These are used for smooth collision.</summary>
        public Vector2 Vertex0
        {
            get => _vertex0;
            set => _vertex0 = value;
        }

        /// <summary>Optional adjacent vertices. These are used for smooth collision.</summary>
        public Vector2 Vertex3
        {
            get => _vertex3;
            set => _vertex3 = value;
        }

        /// <summary>These are the edge vertices</summary>
        public Vector2 Vertex1
        {
            get => _vertex1;
            set
            {
                _vertex1 = value;
                ComputeProperties();
            }
        }

        /// <summary>These are the edge vertices</summary>
        public Vector2 Vertex2
        {
            get => _vertex2;
            set
            {
                _vertex2 = value;
                ComputeProperties();
            }
        }

        public void SetOneSided(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            _vertex0 = v0;
            _vertex1 = v1;
            _vertex2 = v2;
            _vertex3 = v3;
            _oneSided = true;

            ComputeProperties();
        }

        public void SetTwoSided(Vector2 start, Vector2 end)
        {
            _vertex1 = start;
            _vertex2 = end;
            _oneSided = false;

            ComputeProperties();
        }

        public override bool TestPoint(ref Transform transform, ref Vector2 point)
        {
            return false;
        }

        public override bool RayCast(ref RayCastInput input, ref Transform transform, int childIndex, out RayCastOutput output)
        {
            return RayCastHelper.RayCastEdge(ref _vertex1, ref _vertex2, _oneSided, ref input, ref transform, out output);
        }

        public override void ComputeAABB(ref Transform transform, int childIndex, out AABB aabb)
        {
            AABBHelper.ComputeEdgeAABB(ref _vertex1, ref _vertex2, ref transform, out aabb);
        }

        protected sealed override void ComputeProperties()
        {
            _massData._centroid = 0.5f * (_vertex1 + _vertex2);
        }

        public override Shape Clone()
        {
            EdgeShape clone = new EdgeShape();
            clone._shapeType = _shapeType;
            clone._radius = _radius;
            clone._density = _density;
            clone._oneSided = _oneSided;
            clone._vertex0 = _vertex0;
            clone._vertex1 = _vertex1;
            clone._vertex2 = _vertex2;
            clone._vertex3 = _vertex3;
            clone._massData = _massData;
            return clone;
        }
    }
}