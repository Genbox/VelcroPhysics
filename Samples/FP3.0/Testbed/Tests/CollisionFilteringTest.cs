/*
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com 
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

using System;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class CollisionFilteringTest : Test
    {
        // This is a test of collision filtering.
        // There is a triangle, a box, and a circle.
        // There are 6 shapes. 3 large and 3 small.
        // The 3 small ones always collide.
        // The 3 large ones never collide.
        // The boxes don't collide with triangles (except if both are small).
        private const Int16 SmallGroup = 1;
        private const Int16 LargeGroup = -1;

        private const ushort TriangleCategory = 0x0002;
        private const ushort BoxCategory = 0x0004;
        private const ushort CircleCategory = 0x0008;

        private const ushort TriangleMask = 0xFFFF;
        private const ushort BoxMask = 0xFFFF ^ TriangleCategory;
        private const ushort CircleMask = 0xFFFF;

        private CollisionFilteringTest()
        {
            // Ground body
            {
                Vertices edge = PolygonTools.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                PolygonShape shape = new PolygonShape(edge, 0);

                Body ground = _world.CreateBody();
                Fixture fixture = ground.CreateFixture(shape);
                fixture.Friction = 0.3f;
            }

            {
                // Small triangle
                Vertices vertices = new Vertices(3);
                vertices[0] = new Vector2(-1.0f, 0.0f);
                vertices[1] = new Vector2(1.0f, 0.0f);
                vertices[2] = new Vector2(0.0f, 2.0f);
                PolygonShape polygon = new PolygonShape(vertices, 1);

                Body body1 = _world.CreateBody();
                body1.BodyType = BodyType.Dynamic;
                body1.Position = new Vector2(-5.0f, 2.0f);

                Fixture body1Fixture = body1.CreateFixture(polygon);
                body1Fixture.GroupIndex = SmallGroup;
                body1Fixture.CategoryBits = TriangleCategory;
                body1Fixture.MaskBits = TriangleMask;

                // Large triangle (recycle definitions)
                vertices[0] *= 2.0f;
                vertices[1] *= 2.0f;
                vertices[2] *= 2.0f;
                polygon.Set(vertices);

                Body body2 = _world.CreateBody();
                body2.BodyType = BodyType.Dynamic;
                body2.Position = new Vector2(-5.0f, 6.0f);
                body2.FixedRotation = true; // look at me!

                Fixture body2Fixture = body2.CreateFixture(polygon);
                body2Fixture.GroupIndex = LargeGroup;

                {
                    Body body = _world.CreateBody();
                    body.BodyType = BodyType.Dynamic;
                    body.Position = new Vector2(-5.0f, 10.0f);

                    Vertices box = PolygonTools.CreateBox(0.5f, 1.0f);
                    PolygonShape p = new PolygonShape(box, 1);
                    body.CreateFixture(p);

                    PrismaticJoint jd = new PrismaticJoint(body2, body, new Vector2(0.0f, 4.0f), new Vector2(0.0f, 0.0f));
                    jd.LocalXAxis1 = new Vector2(0f,1f);
                    jd.LimitEnabled = true;
                    jd.LowerLimit = -1.0f;
                    jd.UpperLimit = 1.0f;

                    _world.CreateJoint(jd);
                }


                // Small box
                Vertices box2 = PolygonTools.CreateBox(1.0f, 0.5f);
                polygon.Set(box2);

                Body body3 = _world.CreateBody();
                body3.BodyType = BodyType.Dynamic;
                body3.Position = new Vector2(0.0f, 2.0f);

                Fixture body3fixture = body3.CreateFixture(polygon);
                body3fixture.Restitution = 0.1f;
                body3fixture.GroupIndex = SmallGroup;
                body3fixture.CategoryBits = BoxCategory;
                body3fixture.MaskBits = BoxMask;

                // Large box (recycle definitions)
                Vertices box3 = PolygonTools.CreateBox(2, 1);
                polygon.Set(box3);

                Body body4 = _world.CreateBody();
                body4.BodyType = BodyType.Dynamic;
                body4.Position = new Vector2(0.0f, 6.0f);

                Fixture body4Fixture = body4.CreateFixture(polygon);
                body4Fixture.GroupIndex = LargeGroup;

                // Small circle
                CircleShape circle = new CircleShape(1.0f, 1);

                Body body5 = _world.CreateBody();
                body5.BodyType = BodyType.Dynamic;
                body5.Position = new Vector2(5.0f, 2.0f);

                Fixture body5Fixture = body5.CreateFixture(circle);
                body5Fixture.GroupIndex = SmallGroup;
                body5Fixture.CategoryBits = CircleCategory;
                body5Fixture.MaskBits = CircleMask;

                // Large circle
                circle.Radius *= 2.0f;

                Body body6 = _world.CreateBody();
                body6.BodyType = BodyType.Dynamic;
                body6.Position = new Vector2(5.0f, 6.0f);

                Fixture body6Fixture = body6.CreateFixture(circle);
                body6Fixture.GroupIndex = LargeGroup;
            }

        }

        internal static Test Create()
        {
            return new CollisionFilteringTest();
        }
    }
}