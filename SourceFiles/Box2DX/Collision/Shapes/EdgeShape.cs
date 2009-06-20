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

using Box2DX.Common;

namespace Box2DX.Collision
{
    public class EdgeShape : Shape
    {
        public Vec2 V1;
        public Vec2 V2;

        private float _length;

        public Vec2 Normal;

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

        public EdgeShape()
        {
            Type = ShapeType.EdgeShape;

            Radius = Settings.PolygonRadius;

            _prevEdge = null;
            _nextEdge = null;
        }

        public float GetLength()
        {
            return _length;
        }

        public Vec2 GetVertex1()
        {
            return V1;
        }

        public Vec2 GetVertex2()
        {
            return V2;
        }

        public Vec2 GetNormalVector()
        {
            return Normal;
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

        public bool Corner1IsConvex()
        {
            return _cornerConvex1;
        }

        public bool Corner2IsConvex()
        {
            return _cornerConvex2;
        }

        public int GetSupport(Vec2 d)
        {
            return Vec2.Dot(V1, d) > Vec2.Dot(V2, d) ? 0 : 1;
        }

        public Vec2 GetSupportVertex(Vec2 d)
        {
            return Vec2.Dot(V1, d) > Vec2.Dot(V2, d) ? V1 : V2;
        }

        public void Set(Vec2 v1, Vec2 v2)
        {
            V1 = v1;
            V2 = v2;

            _direction = V2 - V1;
            _length = _direction.Normalize();
            Normal = Vec2.Cross(_direction, 1.0f);

            _cornerDir1 = Normal;
            _cornerDir2 = -1.0f * Normal;
        }

        public override SegmentCollide TestSegment(XForm transform,
                                        out float lambda,
                                        out Vec2 normal,
                                        Segment segment,
                                        float maxLambda)
        {
            Vec2 r = segment.P2 - segment.P1;
            Vec2 v1 = Math.Mul(transform, V1);
            Vec2 d = Math.Mul(transform, V2) - v1;
            Vec2 n = Vec2.Cross(d, 1.0f);

            lambda = 0f;
            normal = Vec2.Zero;

            float k_slop = 100.0f * Settings.FLT_EPSILON;
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
            Vec2 v1 = Math.Mul(transform, V1);
            Vec2 v2 = Math.Mul(transform, V2);
            aabb.LowerBound = Math.Min(v1, v2);
            aabb.UpperBound = Math.Max(v1, v2);
        }

        public override void ComputeMass(out MassData massData, float density)
        {
            //B2_NOT_USED(density);

            massData.Mass = 0;
            massData.Center = V1;
            massData.I = 0;
        }

        public void SetPrevEdge(EdgeShape edge, Vec2 cornerDir, bool convex)
        {
            _prevEdge = edge;
            _cornerDir1 = cornerDir;
            _cornerConvex1 = convex;
        }

        public void SetNextEdge(EdgeShape edge, Vec2 cornerDir, bool convex)
        {
            _nextEdge = edge;
            _cornerDir2 = cornerDir;
            _cornerConvex2 = convex;
        }

        public override float ComputeSubmergedArea(Vec2 normal,
                                                        float offset,
                                                        XForm xf,
                                                        out Vec2 c)
        {
            //Note that v0 is independant of any details of the specific edge
            //We are relying on v0 being consistent between multiple edges of the same body
            Vec2 v0 = offset * normal;
            //Vec2 v0 = xf.position + (offset - Vec2.Dot(normal, xf.position)) * normal;

            Vec2 v1 = Math.Mul(xf, V1);
            Vec2 v2 = Math.Mul(xf, V2);

            float d1 = Vec2.Dot(normal, v1) - offset;
            float d2 = Vec2.Dot(normal, v2) - offset;

            if (d1 > 0)
            {
                if (d2 > 0)
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
                if (d2 > 0)
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

        public override float ComputeSweepRadius(Vec2 pivot)
        {
            float ds1 = Vec2.DistanceSquared(V1, pivot);
            float ds2 = Vec2.DistanceSquared(V2, pivot);
            return (float)System.Math.Sqrt(Math.Max(ds1, ds2));
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