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
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Collision.Shapes
{
    /// <summary>
    /// A chain shape is a free form sequence of line segments. The chain has one-sided collision, with the surface
    /// normal pointing to the right of the edge. This provides a counter-clockwise winding like the polygon shape.
    /// Connectivity information is used to create smooth collisions. Warning: the chain will not collide properly if there are
    /// self-intersections.
    /// </summary>
    public class ChainShape : Shape
    {
        internal Vector2 _prevVertex, _nextVertex;
        internal Vertices _vertices;

        /// <summary>Create a new ChainShape from the vertices.</summary>
        /// <param name="vertices">The vertices to use. Must contain 2 or more vertices.</param>
        /// <param name="createLoop">
        /// Set to true to create a closed loop. It connects the first vertex to the last, and
        /// automatically adjusts connectivity to create smooth collisions along the chain.
        /// </param>
        public ChainShape(Vertices vertices, bool createLoop = false) : base(ShapeType.Chain, Settings.PolygonRadius)
        {
            Debug.Assert(vertices != null && vertices.Count >= 3);
            Debug.Assert(vertices[0] != vertices[vertices.Count - 1]); //Velcro. See http://www.box2d.org/forum/viewtopic.php?f=4&t=7973&p=35363

            for (int i = 1; i < vertices.Count; ++i)
            {
                // If the code crashes here, it means your vertices are too close together.
                Vector2 current = vertices[i];
                Vector2 prev = vertices[i - 1];
                Debug.Assert(MathUtils.DistanceSquared(ref prev, ref current) > Settings.LinearSlop * Settings.LinearSlop);
            }

            _vertices = new Vertices(vertices);

            //Velcro: Merged CreateLoop() and CreateChain() to this
            if (createLoop)
            {
                _vertices.Add(vertices[0]);
                _prevVertex = _vertices[_vertices.Count - 2];
                _nextVertex = _vertices[1];
            }

            ComputeProperties();
        }

        private ChainShape() : base(ShapeType.Chain, Settings.PolygonRadius) { }

        /// <summary>The vertices. These are not owned/freed by the chain Shape.</summary>
        public Vertices Vertices => _vertices;

        /// <summary>Edge count = vertex count - 1</summary>
        public override int ChildCount => _vertices.Count - 1;

        /// <summary>Establish connectivity to a vertex that precedes the first vertex. Don't call this for loops.</summary>
        public Vector2 PrevVertex
        {
            get => _prevVertex;
            set => _prevVertex = value;
        }

        /// <summary>Establish connectivity to a vertex that follows the last vertex. Don't call this for loops.</summary>
        public Vector2 NextVertex
        {
            get => _nextVertex;
            set => _nextVertex = value;
        }

        //Velcro: The original code returned an EdgeShape for each call. To reduce garbage we merge the properties onto an existing EdgeShape
        internal void GetChildEdge(EdgeShape edge, int index)
        {
            Debug.Assert(0 <= index && index < _vertices.Count - 1);
            Debug.Assert(edge != null);

            //Velcro: It is already an edge shape
            //edge._shapeType = ShapeType.Edge;
            edge._radius = _radius;

            edge._vertex1 = _vertices[index + 0];
            edge._vertex2 = _vertices[index + 1];
            edge._oneSided = true;

            if (index > 0)
                edge._vertex0 = _vertices[index - 1];
            else
                edge._vertex0 = _prevVertex;

            if (index < _vertices.Count - 2)
                edge._vertex3 = _vertices[index + 2];
            else
                edge._vertex3 = _nextVertex;
        }

        public EdgeShape GetChildEdge(int index)
        {
            EdgeShape edgeShape = new EdgeShape();
            GetChildEdge(edgeShape, index);
            return edgeShape;
        }

        public override bool TestPoint(ref Transform transform, ref Vector2 point)
        {
            return false;
        }

        public override bool RayCast(ref RayCastInput input, ref Transform transform, int childIndex, out RayCastOutput output)
        {
            Debug.Assert(childIndex < _vertices.Count);

            int i1 = childIndex;
            int i2 = childIndex + 1;

            if (i2 == _vertices.Count)
                i2 = 0;

            Vector2 v1 = _vertices[i1];
            Vector2 v2 = _vertices[i2];

            return RayCastHelper.RayCastEdge(ref v1, ref v2, false, ref input, ref transform, out output);
        }

        public override void ComputeAABB(ref Transform transform, int childIndex, out AABB aabb)
        {
            Debug.Assert(childIndex < _vertices.Count);

            int i1 = childIndex;
            int i2 = childIndex + 1;

            if (i2 == _vertices.Count)
                i2 = 0;

            Vector2 v1 = _vertices[i1];
            Vector2 v2 = _vertices[i2];

            AABBHelper.ComputeEdgeAABB(ref v1, ref v2, ref transform, out aabb);
        }

        protected sealed override void ComputeProperties()
        {
            //Does nothing. Chain shapes don't have properties.
        }

        public override Shape Clone()
        {
            ChainShape clone = new ChainShape();
            clone._shapeType = _shapeType;
            clone._density = _density;
            clone._radius = _radius;
            clone._prevVertex = _prevVertex;
            clone._nextVertex = _nextVertex;
            clone._vertices = new Vertices(_vertices);
            return clone;
        }
    }
}