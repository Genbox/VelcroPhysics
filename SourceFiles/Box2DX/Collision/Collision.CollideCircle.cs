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
    public partial class Collision
    {
        public static void CollideCircles(out Manifold manifold,
            CircleShape circle1, Transform xf1,
            CircleShape circle2, Transform xf2)
        {
            manifold = new Manifold();
            manifold._pointCount = 0;

            Vec2 p1 = Math.Mul(xf1, circle1._p);
            Vec2 p2 = Math.Mul(xf2, circle2._p);

            Vec2 d = p2 - p1;
            float distSqr = Vec2.Dot(d, d);
            float radius = circle1._radius + circle2._radius;
            if (distSqr > radius * radius)
            {
                return;
            }

            manifold._type = Manifold.ManifoldType.Circles;
            manifold._localPoint = circle1._p;
            manifold._localPlaneNormal.SetZero();
            manifold._pointCount = 1;

            manifold._points[0]._localPoint = circle2._p;
            manifold._points[0]._id.Key = 0;
        }

        public static void CollidePolygonAndCircle(
            out Manifold manifold,
            PolygonShape polygon, Transform xf1,
            CircleShape circle, Transform xf2)
        {
            manifold = new Manifold();
            manifold._pointCount = 0;

            // Compute circle position in the frame of the polygon.
            Vec2 c = Math.Mul(xf2, circle._p);
            Vec2 cLocal = Math.MulT(xf1, c);

            // Find the min separating edge.
            int normalIndex = 0;
            float separation = -Settings.floatMax;
            float radius = polygon._radius + circle._radius;
            int vertexCount = polygon._vertexCount;
            Vec2[] vertices = polygon._vertices;
            Vec2[] normals = polygon._normals;

            for (int i = 0; i < vertexCount; ++i)
            {
                float s = Vec2.Dot(normals[i], cLocal - vertices[i]);

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

            // Vertices that subtend the incident face.
            int vertIndex1 = normalIndex;
            int vertIndex2 = vertIndex1 + 1 < vertexCount ? vertIndex1 + 1 : 0;
            Vec2 v1 = vertices[vertIndex1];
            Vec2 v2 = vertices[vertIndex2];

            // If the center is inside the polygon ...
            if (separation < Settings.epsilon)
            {
                manifold._pointCount = 1;
                manifold._type = Manifold.ManifoldType.FaceA;
                manifold._localPlaneNormal = normals[normalIndex];
                manifold._localPoint = 0.5f * (v1 + v2);
                manifold._points[0]._localPoint = circle._p;
                manifold._points[0]._id.Key = 0;
                return;
            }

            // Compute barycentric coordinates
            float u1 = Vec2.Dot(cLocal - v1, v2 - v1);
            float u2 = Vec2.Dot(cLocal - v2, v1 - v2);
            if (u1 <= 0.0f)
            {
                if (Vec2.DistanceSquared(cLocal, v1) > radius * radius)
                {
                    return;
                }

                manifold._pointCount = 1;
                manifold._type = Manifold.ManifoldType.FaceA;
                manifold._localPlaneNormal = cLocal - v1;
                manifold._localPlaneNormal.Normalize();
                manifold._localPoint = v1;
                manifold._points[0]._localPoint = circle._p;
                manifold._points[0]._id.Key = 0;
            }
            else if (u2 <= 0.0f)
            {
                if (Vec2.DistanceSquared(cLocal, v2) > radius * radius)
                {
                    return;
                }

                manifold._pointCount = 1;
                manifold._type = Manifold.ManifoldType.FaceA;
                manifold._localPlaneNormal = cLocal - v2;
                manifold._localPlaneNormal.Normalize();
                manifold._localPoint = v2;
                manifold._points[0]._localPoint = circle._p;
                manifold._points[0]._id.Key = 0;
            }
            else
            {
                Vec2 faceCenter = 0.5f * (v1 + v2);
                separation = Vec2.Dot(cLocal - faceCenter, normals[vertIndex1]);
                if (separation > radius)
                {
                    return;
                }

                manifold._pointCount = 1;
                manifold._type = Manifold.ManifoldType.FaceA;
                manifold._localPlaneNormal = normals[vertIndex1];
                manifold._localPoint = faceCenter;
                manifold._points[0]._localPoint = circle._p;
                manifold._points[0]._id.Key = 0;
            }
        }
    }
}