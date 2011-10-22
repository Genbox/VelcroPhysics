/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2010 Ian Qvist
* 
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.box2d.org 
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
    /// Since there may be many vertices, they are allocated using b2Alloc.
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

        private ChainShape()
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

            if (Settings.ConserveMemory)
                Vertices = vertices;
            else
                // Copy vertices.
                Vertices = new Vertices(vertices);
        }

        public void CreateLoop(Vertices vertices)
        {
            Debug.Assert(vertices.Count >= 3);
            Vertices = new Vertices(vertices);
            Vertices.Add(vertices[0]);
            _prevVertex = Vertices[Vertices.Count - 2];
            _nextVertex = Vertices[1];
            _hasPrevVertex = true;
            _hasNextVertex = true;
        }

        public void CreateChain(Vertices vertices)
        {
            Debug.Assert(vertices == null && vertices.Count == 0);
            Debug.Assert(vertices.Count >= 2);
            Vertices = new Vertices(vertices);
            _hasPrevVertex = false;
            _hasNextVertex = false;
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

        public void SetPrevVertex(Vector2 prevVertex)
        {
            _prevVertex = prevVertex;
            _hasPrevVertex = true;
        }

        public void SetNextVertex(Vector2 nextVertex)
        {
            _nextVertex = nextVertex;
            _hasNextVertex = true;
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
        public override bool RayCast(out RayCastOutput output, ref RayCastInput input,
                                     ref Transform transform, int childIndex)
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

            Vector2 v1 = MathUtils.Multiply(ref transform, Vertices[i1]);
            Vector2 v2 = MathUtils.Multiply(ref transform, Vertices[i2]);

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

        public override float ComputeSubmergedArea(Vector2 normal, float offset, Transform xf, out Vector2 sc)
        {
            sc = Vector2.Zero;
            return 0;
        }
    }
}