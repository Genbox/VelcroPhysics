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

#define DEBUG

using System;
using System.Collections.Generic;
using System.Text;

using Box2DX.Common;

namespace Box2DX.Collision
{
    public class EdgeChainDef : ShapeDef
    {
        public int VertexCount;

        public Vec2[] Vertices;

        public bool IsALoop;

        public EdgeChainDef()
        {
            Type = ShapeType.EdgeShape;
            VertexCount = 0;
            IsALoop = true;
        }

        public EdgeChainDef(Vec2[] vertices)
        {
            Type = ShapeType.EdgeShape;
            VertexCount = vertices.Length;
            Vertices = vertices;
            IsALoop = true;
        }
    }

    public class EdgeShape : Shape, Collision.IGenericShape
    {
        private Vec2 _v1;
        private Vec2 _v2;

        private Vec2 _coreV1;
        private Vec2 _coreV2;

        private float _length;

        private Vec2 _normal;

        private Vec2 _direction;

        ///<summary>
        ///Unit vector halfway between _direction and _prevEdge._direction
        ///</summary>
        private Vec2 _cornerDir1;

        ///<summary>
        ///Unit vector halfway between _direction and _nextEdge._direction
        ///</summary>
        private Vec2 _cornerDir2;

        private bool _cornerConvex1;
        private bool _cornerConvex2;

        public EdgeShape _nextEdge;
        public EdgeShape _prevEdge;

        public float GetLength()
        {
	        return _length;
        }
        
        public Vec2 GetVertex1()
        {
	        return _v1;
        }

        public Vec2 GetVertex2()
        {
	        return _v2;
        }

        public Vec2 GetCoreVertex1()
        {
	        return _coreV1;
        }

        public Vec2 GetCoreVertex2()
        {
	        return _coreV2;
        }

        public Vec2 GetNormalVector()
        {
	        return _normal;
        }

        public Vec2 GetDirectionVector()
        {
	        return _direction;
        }

        public Vec2 GetCorner1Vector()
        {
	        return _cornerDir1;
        }

        public Vec2 GetCorner2Vector()
        {
	        return _cornerDir2;
        }

        public EdgeShape GetNextEdge()
        {
	        return _nextEdge;
        }

        public EdgeShape GetPrevEdge()
        {
	        return _prevEdge;
        }

        public Vec2 GetFirstVertex(XForm xf)
        {
	        return Common.Math.Mul(xf, _coreV1);
        }

        public bool Corner1IsConvex()
        {
	        return _cornerConvex1;
        }

        public bool Corner2IsConvex()
        {
	        return _cornerConvex2;
        }

        internal EdgeShape(Vec2 v1, Vec2 v2, ShapeDef def) : base(def)
        {
	        Box2DXDebug.Assert(def.Type == ShapeType.EdgeShape);

	        _type = ShapeType.EdgeShape;
        	
	        _prevEdge = null;
	        _nextEdge = null;
        	
	        _v1 = v1;
	        _v2 = v2;
        	
	        _direction = _v2 - _v1;
	        _length = _direction.Normalize();
	        _normal.Set(_direction.Y, -_direction.X);
        	
	        _coreV1 = -Settings.ToiSlop * (_normal - _direction) + _v1;
	        _coreV2 = -Settings.ToiSlop * (_normal + _direction) + _v2;
        	
	        _cornerDir1 = _normal;
	        _cornerDir2 = -1.0f * _normal;
        }

        internal override void UpdateSweepRadius(Vec2 center)
        {
	        // Update the sweep radius (maximum radius) as measured from
	        // a local center point.
	        Vec2 d = _coreV1 - center;
	        float d1 = Vec2.Dot(d,d);
	        d = _coreV2 - center;
	        float d2 = Vec2.Dot(d,d);
	        _sweepRadius = Common.Math.Sqrt(d1 > d2 ? d1 : d2);
        }
        /*
        bool b2EdgeShape::TestPoint(const XForm& transform, const Vec2& p) const
        {
	        B2_NOT_USED(transform);
	        B2_NOT_USED(p);
	        return false;
        }
        */
        public override SegmentCollide TestSegment(XForm transform,
								        out float lambda,
								        out Vec2 normal,
								        Segment segment,
								        float maxLambda)
        {
	        Vec2 r = segment.P2 - segment.P1;
	        Vec2 v1 = Common.Math.Mul(transform, _v1);
	        Vec2 d = Common.Math.Mul(transform, _v2) - v1;
	        Vec2 n = Vec2.Cross(d, 1.0f);

            lambda = 0f;
            normal = Vec2.Zero;

	        float k_slop = 100.0f * Common.Settings.FLT_EPSILON;
	        float denom = -Vec2.Dot(r, n);

	        // Cull back facing collision and ignore parallel segments.
	        if (denom > k_slop)
	        {
		        // Does the segment intersect the infinite line associated with this segment?
		        Vec2 b = segment.P1 - v1;
		        float a = Vec2.Dot(b, n);

		        if (0.0f <= a && a <= maxLambda * denom)
		        {
			        float mu2 = -r.X * b.Y + r.Y * b.X;

			        // Does the segment intersect this segment?
			        if (-k_slop * denom <= mu2 && mu2 <= denom * (1.0f + k_slop))
			        {
				        a /= denom;
				        n.Normalize();
				        lambda = a;
				        normal = n;
				        return SegmentCollide.HitCollide;
			        }
		        }
	        }

	        return SegmentCollide.MissCollide;
        }

        public override void ComputeAABB(out AABB aabb, XForm transform)
        {
	        Vec2 v1 = Common.Math.Mul(transform, _v1);
	        Vec2 v2 = Common.Math.Mul(transform, _v2);
	        aabb.LowerBound = Common.Math.Min(v1, v2);
	        aabb.UpperBound = Common.Math.Max(v1, v2);
        }

        public override void ComputeSweptAABB(out AABB aabb, XForm transform1, XForm transform2)
        {
	        Vec2 v1 = Common.Math.Mul(transform1, _v1);
	        Vec2 v2 = Common.Math.Mul(transform1, _v2);
	        Vec2 v3 = Common.Math.Mul(transform2, _v1);
	        Vec2 v4 = Common.Math.Mul(transform2, _v2);
	        aabb.LowerBound = Common.Math.Min(Common.Math.Min(Common.Math.Min(v1, v2), v3), v4);
	        aabb.UpperBound = Common.Math.Max(Common.Math.Max(Common.Math.Max(v1, v2), v3), v4);
        }

        public override void ComputeMass(out MassData massData)
        {
	        massData.Mass = 0;
	        massData.Center = _v1;

	        // inertia about the local origin
	        massData.I = 0;
        }

        public Vec2 Support(XForm xf, Vec2 d)
        {
	        Vec2 v1 = Common.Math.Mul(xf, _coreV1);
	        Vec2 v2 = Common.Math.Mul(xf, _coreV2);
	        return Vec2.Dot(v1, d) > Vec2.Dot(v2, d) ? v1 : v2;
        }

        public void SetPrevEdge(EdgeShape edge, Vec2 core, Vec2 cornerDir, bool convex)
        {
	        _prevEdge = edge;
	        _coreV1 = core;
	        _cornerDir1 = cornerDir;
	        _cornerConvex1 = convex;
        }

        public void SetNextEdge(EdgeShape edge, Vec2 core, Vec2 cornerDir, bool convex)
        {
	        _nextEdge = edge;
	        _coreV2 = core;
	        _cornerDir2 = cornerDir;
	        _cornerConvex2 = convex;
        }

        public override float ComputeSubmergedArea(	Vec2 normal,
												        float offset,
												        XForm xf, 
												        out Vec2 c)
        {
	        //Note that v0 is independant of any details of the specific edge
	        //We are relying on v0 being consistent between multiple edges of the same body
	        Vec2 v0 = offset * normal;
	        //Vec2 v0 = xf.position + (offset - Vec2.Dot(normal, xf.position)) * normal;

	        Vec2 v1 = Common.Math.Mul(xf, _v1);
	        Vec2 v2 = Common.Math.Mul(xf, _v2);

	        float d1 = Vec2.Dot(normal, v1) - offset;
	        float d2 = Vec2.Dot(normal, v2) - offset;

	        if(d1>0)
	        {
		        if(d2>0)
		        {
                    c = new Vec2();
			        return 0;
		        }
		        else
		        {
			        v1 = -d2 / (d1 - d2) * v1 + d1 / (d1 - d2) * v2;
		        }
	        }
	        else
	        {
		        if(d2>0)
		        {
			        v2 = -d2 / (d1 - d2) * v1 + d1 / (d1 - d2) * v2;
		        }
		        else
		        {
			        //Nothing
		        }
	        }

	        // v0,v1,v2 represents a fully submerged triangle
	        float k_inv3 = 1.0f / 3.0f;

	        // Area weighted centroid
	        c = k_inv3 * (v0 + v1 + v2);

	        Vec2 e1 = v1 - v0;
	        Vec2 e2 = v2 - v0;

	        return 0.5f * Vec2.Cross(e1, e2);
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <returns>false</returns>
        public override bool TestPoint(XForm xf, Vec2 p)
        {
            return false;
        }
    }
}