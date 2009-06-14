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

using FarseerPhysics.Math;
using Microsoft.Xna.Framework;
// If this is an XNA project then we use math from the XNA framework.
#if XNA

#else
#endif

namespace FarseerPhysics.Collision
{
    public partial class Collision
    {
        public static void CollideCircles(ref Manifold manifold,
                                          CircleShape circle1, XForm xf1, CircleShape circle2, XForm xf2)
        {
            manifold.PointCount = 0;

            Vector2 p1 = CommonMath.Mul(xf1, circle1.GetLocalPosition());
            Vector2 p2 = CommonMath.Mul(xf2, circle2.GetLocalPosition());

            Vector2 d = p2 - p1;
            float distSqr = Vector2.Dot(d, d);
            float r1 = circle1.GetRadius();
            float r2 = circle2.GetRadius();
            float radiusSum = r1 + r2;
            if (distSqr > radiusSum*radiusSum)
            {
                return;
            }

            float separation;
            if (distSqr < Settings.FLT_EPSILON)
            {
                separation = -radiusSum;
                manifold.Normal = new Vector2(0.0f, 1.0f);
            }
            else
            {
                float dist = CommonMath.Sqrt(distSqr);
                separation = dist - radiusSum;
                float a = 1.0f/dist;
                manifold.Normal.X = a*d.X;
                manifold.Normal.Y = a*d.Y;
            }

            manifold.PointCount = 1;
            manifold.Points[0].ID.Key = 0;
            manifold.Points[0].Separation = separation;

            p1 += r1*manifold.Normal;
            p2 -= r2*manifold.Normal;

            Vector2 p = 0.5f*(p1 + p2);

            manifold.Points[0].LocalPoint1 = CommonMath.MulT(xf1, p);
            manifold.Points[0].LocalPoint2 = CommonMath.MulT(xf2, p);
        }

        public static void CollidePolygonAndCircle(ref Manifold manifold,
                                                   PolygonShape polygon, XForm xf1, CircleShape circle, XForm xf2)
        {
            manifold.PointCount = 0;

            // Compute circle position in the frame of the polygon.
            Vector2 c = CommonMath.Mul(xf2, circle.GetLocalPosition());
            Vector2 cLocal = CommonMath.MulT(xf1, c);

            // Find the min separating edge.
            int normalIndex = 0;
            float separation = -Settings.FLT_MAX;
            float radius = circle.GetRadius();
            int vertexCount = polygon.VertexCount;
            Vector2[] vertices = polygon.GetVertices();
            Vector2[] normals = polygon.Normals;

            for (int i = 0; i < vertexCount; ++i)
            {
                float s = Vector2.Dot(normals[i], cLocal - vertices[i]);
                if (s > radius)
                {
                    // Early out.
                    return;
                }

                if (s > separation)
                {
                    separation = s;
                    normalIndex = i;
                }
            }

            // If the center is inside the polygon ...
            if (separation < Settings.FLT_EPSILON)
            {
                manifold.PointCount = 1;
                manifold.Normal = CommonMath.Mul(xf1.R, normals[normalIndex]);
                manifold.Points[0].ID.Features.IncidentEdge = (byte) normalIndex;
                manifold.Points[0].ID.Features.IncidentVertex = NullFeature;
                manifold.Points[0].ID.Features.ReferenceEdge = 0;
                manifold.Points[0].ID.Features.Flip = 0;
                Vector2 position = c - radius*manifold.Normal;
                manifold.Points[0].LocalPoint1 = CommonMath.MulT(xf1, position);
                manifold.Points[0].LocalPoint2 = CommonMath.MulT(xf2, position);
                manifold.Points[0].Separation = separation - radius;
                return;
            }

            // Project the circle center onto the edge segment.
            int vertIndex1 = normalIndex;
            int vertIndex2 = vertIndex1 + 1 < vertexCount ? vertIndex1 + 1 : 0;
            Vector2 e = vertices[vertIndex2] - vertices[vertIndex1];

            float length = CommonMath.Normalize(ref e);
            //Box2DXDebug.Assert(length > Settings.FLT_EPSILON);

            // Project the center onto the edge.
            float u = Vector2.Dot(cLocal - vertices[vertIndex1], e);
            Vector2 p;
            if (u <= 0.0f)
            {
                p = vertices[vertIndex1];
                manifold.Points[0].ID.Features.IncidentEdge = NullFeature;
                manifold.Points[0].ID.Features.IncidentVertex = (byte) vertIndex1;
            }
            else if (u >= length)
            {
                p = vertices[vertIndex2];
                manifold.Points[0].ID.Features.IncidentEdge = NullFeature;
                manifold.Points[0].ID.Features.IncidentVertex = (byte) vertIndex2;
            }
            else
            {
                p = vertices[vertIndex1] + u*e;
                manifold.Points[0].ID.Features.IncidentEdge = (byte) normalIndex;
                manifold.Points[0].ID.Features.IncidentVertex = NullFeature;
            }

            Vector2 d = cLocal - p;
            float dist = CommonMath.Normalize(ref d);
            if (dist > radius)
            {
                return;
            }

            manifold.PointCount = 1;
            manifold.Normal = CommonMath.Mul(xf1.R, d);
            Vector2 position_ = c - radius*manifold.Normal;
            manifold.Points[0].LocalPoint1 = CommonMath.MulT(xf1, position_);
            manifold.Points[0].LocalPoint2 = CommonMath.MulT(xf2, position_);
            manifold.Points[0].Separation = dist - radius;
            manifold.Points[0].ID.Features.ReferenceEdge = 0;
            manifold.Points[0].ID.Features.Flip = 0;
        }
    }
}