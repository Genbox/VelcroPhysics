/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.Google.Com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2008 Erin Catto http://www.Gphysics.Com

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

using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;

namespace TestBed
{
    public class CollisionFiltering : Test
    {
        // This is a test of collision filtering.
        // There is a triangle, a box, and a circle.
        // There are 6 shapes. 3 large and 3 small.
        // The 3 small ones always collide.
        // The 3 large ones never collide.
        // The boxes don't collide with triangles (except if both are small).
        const short k_smallGroup = 1;
        const short k_largeGroup = -1;

        const ushort k_defaultCategory = 0x0001;
        const ushort k_triangleCategory = 0x0002;
        const ushort k_boxCategory = 0x0004;
        const ushort k_circleCategory = 0x0008;

        const ushort k_triangleMask = 0xFFFF;
        const ushort k_boxMask = 0xFFFF ^ k_triangleCategory;
        const ushort k_circleMask = 0xFFFF;

        public CollisionFiltering()
        {
            // Ground body
            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsEdge(new Vec2(-40.0f, 0.0f), new Vec2(40.0f, 0.0f));

                FixtureDef sd = new FixtureDef();
                sd.Shape = shape;
                sd.Friction = 0.3f;

                BodyDef bd = new BodyDef();
                Body ground = _world.CreateBody(bd);
                ground.CreateFixture(sd);
            }

            // Small triangle
            Vec2[] vertices = new Vec2[3];
            vertices[0].Set(-1.0f, 0.0f);
            vertices[1].Set(1.0f, 0.0f);
            vertices[2].Set(0.0f, 2.0f);
            PolygonShape polygon = new PolygonShape();
            polygon.Set(vertices, 3);

            FixtureDef triangleShapeDef = new FixtureDef();
            triangleShapeDef.Shape = polygon;
            triangleShapeDef.Density = 1.0f;

            triangleShapeDef.Filter.GroupIndex = k_smallGroup;
            triangleShapeDef.Filter.CategoryBits = k_triangleCategory;
            triangleShapeDef.Filter.MaskBits = k_triangleMask;

            BodyDef triangleBodyDef = new BodyDef();
            triangleBodyDef.Position.Set(-5.0f, 2.0f);

            Body body1 = _world.CreateBody(triangleBodyDef);
            body1.CreateFixture(triangleShapeDef);

            // Large triangle (recycle definitions)
            vertices[0] *= 2.0f;
            vertices[1] *= 2.0f;
            vertices[2] *= 2.0f;
            polygon.Set(vertices, 3);
            triangleShapeDef.Filter.GroupIndex = k_largeGroup;
            triangleBodyDef.Position.Set(-5.0f, 6.0f);
            triangleBodyDef.FixedRotation = true; // look at me!

            Body body2 = _world.CreateBody(triangleBodyDef);
            body2.CreateFixture(triangleShapeDef);

            // Small box
            polygon.SetAsBox(1.0f, 0.5f);
            FixtureDef boxShapeDef = new FixtureDef();
            boxShapeDef.Shape = polygon;
            boxShapeDef.Density = 1.0f;
            boxShapeDef.Restitution = 0.1f;

            boxShapeDef.Filter.GroupIndex = k_smallGroup;
            boxShapeDef.Filter.CategoryBits = k_boxCategory;
            boxShapeDef.Filter.MaskBits = k_boxMask;

            BodyDef boxBodyDef = new BodyDef();
            boxBodyDef.Position.Set(0.0f, 2.0f);

            Body body3 = _world.CreateBody(boxBodyDef);
            body3.CreateFixture(boxShapeDef);

            // Large box (recycle definitions)
            polygon.SetAsBox(2.0f, 1.0f);
            boxShapeDef.Filter.GroupIndex = k_largeGroup;
            boxBodyDef.Position.Set(0.0f, 6.0f);

            Body body4 = _world.CreateBody(boxBodyDef);
            body4.CreateFixture(boxShapeDef);

            // Small circle
            CircleShape circle = new CircleShape();
            circle._radius = 1.0f;

            FixtureDef circleShapeDef = new FixtureDef();
            circleShapeDef.Shape = circle;
            circleShapeDef.Density = 1.0f;

            circleShapeDef.Filter.GroupIndex = k_smallGroup;
            circleShapeDef.Filter.CategoryBits = k_circleCategory;
            circleShapeDef.Filter.MaskBits = k_circleMask;

            BodyDef circleBodyDef = new BodyDef();
            circleBodyDef.Position.Set(5.0f, 2.0f);

            Body body5 = _world.CreateBody(circleBodyDef);
            body5.CreateFixture(circleShapeDef);

            // Large circle
            circle._radius *= 2.0f;
            circleShapeDef.Filter.GroupIndex = k_largeGroup;
            circleBodyDef.Position.Set(5.0f, 6.0f);

            Body body6 = _world.CreateBody(circleBodyDef);
            body6.CreateFixture(circleShapeDef);
        }

        public static Test Create()
        {
            return new CollisionFiltering();
        }
    }
}