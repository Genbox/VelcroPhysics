/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
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
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision.Shapes
{
    /// <summary>
    /// A chain shape is a free form sequence of line segments.
    /// The chain has two-sided collision, so you can use inside and outside collision.
    /// Therefore, you may use any winding order.
    /// Connectivity information is used to create smooth collisions.
    /// WARNING: The chain will not collide properly if there are self-intersections.
    /// </summary>
    public class ChainShape : Shape
    {
        /// <summary>
        /// The vertices. These are not owned/freed by the chain Shape.
        /// </summary>
        public Vertices Vertices;
        private Vector2 _prevVertex, _nextVertex;
        private bool _hasPrevVertex, _hasNextVertex;

        public ChainShape()
            : base(0)
        {
            ShapeType = ShapeType.Chain;
            _radius = Settings.PolygonRadius;
        }

        public ChainShape(Vertices vertices)
            : base(0)
        {
            ShapeType = ShapeType.Chain;
            _radius = Settings.PolygonRadius;

            Debug.Assert(vertices != null && vertices.Count >= 2);
            Debug.Assert(vertices[0] != vertices[vertices.Count - 1]); // FPE. See http://www.box2d.org/forum/viewtopic.php?f=4&t=7973&p=35363

            for (int i = 1; i < vertices.Count; ++i)
            {
                Vector2 v1 = vertices[i - 1];
                Vector2 v2 = vertices[i];
                // If the code crashes here, it means your vertices are too close together.
                Debug.Assert(Vector2.DistanceSquared(v1, v2) > Settings.LinearSlop * Settings.LinearSlop);
            }

            //Only copy the items if we don't need to conserve memory.
            Vertices = Settings.ConserveMemory ? vertices : new Vertices(vertices);

            _hasPrevVertex = false;
            _hasNextVertex = false;
        }

        /// Create a loop. This automatically adjusts connectivity.
        public void CreateLoop(Vertices vertices)
        {
            Debug.Assert(vertices != null && vertices.Count >= 3);

            for (int i = 1; i < vertices.Count; ++i)
            {
                Vector2 v1 = vertices[i - 1];
                Vector2 v2 = vertices[i];
                // If the code crashes here, it means your vertices are too close together.
                Debug.Assert(Vector2.DistanceSquared(v1, v2) > Settings.LinearSlop * Settings.LinearSlop);
            }

            //TODO: Conserve mem
            Vertices = new Vertices(vertices);
            Vertices.Add(vertices[0]);

            Vertices[Vertices.Count - 1] = Vertices[0]; //TODO: Check this line

            _prevVertex = Vertices[Vertices.Count - 2];
            _nextVertex = Vertices[1];
            _hasPrevVertex = true;
            _hasNextVertex = true;
        }

        public override int ChildCount
        {
            // edge count = vertex count - 1
            get { return Vertices.Count - 1; }
        }

        public override Shape Clone()
        {
            ChainShape loop = new ChainShape();
            loop._density = _density;
            loop._radius = _radius;
            loop.Vertices = Vertices;
            loop.MassData = MassData;
            return loop;
        }

        /// <summary>
        /// Establish connectivity to a vertex that precedes the first vertex.
        /// Don't call this for loops.
        /// </summary>
        public Vector2 PrevVertex
        {
            get { return _prevVertex; }
            set
            {
                _prevVertex = value;
                _hasPrevVertex = true;
            }
        }

        /// <summary>
        /// Establish connectivity to a vertex that follows the last vertex.
        /// Don't call this for loops.
        /// </summary>
        public Vector2 NextVertex
        {
            get { return _nextVertex; }
            set
            {
                _nextVertex = value;
                _hasNextVertex = true;
            }
        }

        /// <summary>
        /// Get a child edge.
        /// </summary>
        /// <param name="edge">The edge.</param>
        /// <param name="index">The index.</param>
        public void GetChildEdge(ref EdgeShape edge, int index)
        {
            Debug.Assert(0 <= index && index < Vertices.Count - 1);
            edge.ShapeType = ShapeType.Edge;
            edge._radius = _radius;

            edge.Vertex1 = Vertices[index + 0];
            edge.Vertex2 = Vertices[index + 1];

            if (index > 0)
            {
                edge.Vertex0 = Vertices[index - 1];
                edge.HasVertex0 = true;
            }
            else
            {
                edge.Vertex0 = _prevVertex;
                edge.HasVertex0 = _hasPrevVertex;
            }

            if (index < Vertices.Count - 2)
            {
                edge.Vertex3 = Vertices[index + 2];
                edge.HasVertex3 = true;
            }
            else
            {
                edge.Vertex3 = _nextVertex;
                edge.HasVertex3 = _hasNextVertex;
            }
        }

        /// <summary>
        /// Test a point for containment in this shape. This only works for convex shapes.
        /// </summary>
        /// <param name="transform">The shape world transform.</param>
        /// <param name="point">a point in world coordinates.</param>
        /// <returns>True if the point is inside the shape</returns>
        public override bool TestPoint(ref Transform transform, ref Vector2 point)
        {
            return false;
        }

        /// <summary>
        /// Cast a ray against a child shape.
        /// </summary>
        /// <param name="output">The ray-cast results.</param>
        /// <param name="input">The ray-cast input parameters.</param>
        /// <param name="transform">The transform to be applied to the shape.</param>
        /// <param name="childIndex">The child shape index.</param>
        /// <returns>True if the ray-cast hits the shape</returns>
        public override bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex)
        {
            Debug.Assert(childIndex < Vertices.Count);

            EdgeShape edgeShape = new EdgeShape();

            int i1 = childIndex;
            int i2 = childIndex + 1;
            if (i2 == Vertices.Count)
            {
                i2 = 0;
            }

            edgeShape.Vertex1 = Vertices[i1];
            edgeShape.Vertex2 = Vertices[i2];

            return edgeShape.RayCast(out output, ref input, ref transform, 0);
        }

        /// <summary>
        /// Given a transform, compute the associated axis aligned bounding box for a child shape.
        /// </summary>
        /// <param name="aabb">The aabb results.</param>
        /// <param name="transform">The world transform of the shape.</param>
        /// <param name="childIndex">The child shape index.</param>
        public override void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex)
        {
            Debug.Assert(childIndex < Vertices.Count);

            int i1 = childIndex;
            int i2 = childIndex + 1;
            if (i2 == Vertices.Count)
            {
                i2 = 0;
            }

            Vector2 v1 = MathUtils.Mul(ref transform, Vertices[i1]);
            Vector2 v2 = MathUtils.Mul(ref transform, Vertices[i2]);

            aabb.LowerBound = Vector2.Min(v1, v2);
            aabb.UpperBound = Vector2.Max(v1, v2);
        }

        /// <summary>
        /// Chains have zero mass.
        /// </summary>
        protected override void ComputeProperties()
        {
            //Does nothing. Chain shapes don't have properties.
        }

        public override float ComputeSubmergedArea(ref Vector2 normal, float offset, ref Transform xf, out Vector2 sc)
        {
            sc = Vector2.Zero;
            return 0;
        }
    }
}