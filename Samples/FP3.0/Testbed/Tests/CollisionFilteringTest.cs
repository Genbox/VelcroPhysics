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
                PolygonShape shape = new PolygonShape();
                shape.SetAsEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

                FixtureDef sd = new FixtureDef();
                sd.Shape = shape;
                sd.Friction = 0.3f;

                BodyDef bd = new BodyDef();
                Body ground = _world.CreateBody(bd);
                ground.CreateFixture(sd);
            }

            // Small triangle
            Vector2[] vertices = new Vector2[3];
            vertices[0] = new Vector2(-1.0f, 0.0f);
            vertices[1] = new Vector2(1.0f, 0.0f);
            vertices[2] = new Vector2(0.0f, 2.0f);
            PolygonShape polygon = new PolygonShape();
            polygon.Set(vertices, 3);

            FixtureDef triangleShapeDef = new FixtureDef();
            triangleShapeDef.Shape = polygon;
            triangleShapeDef.Density = 1.0f;

            triangleShapeDef.Filter.groupIndex = SmallGroup;
            triangleShapeDef.Filter.categoryBits = TriangleCategory;
            triangleShapeDef.Filter.maskBits = TriangleMask;

            BodyDef triangleBodyDef = new BodyDef();
            triangleBodyDef.Type = BodyType.Dynamic;
            triangleBodyDef.Position = new Vector2(-5.0f, 2.0f);

            Body body1 = _world.CreateBody(triangleBodyDef);
            body1.CreateFixture(triangleShapeDef);

            // Large triangle (recycle definitions)
            vertices[0] *= 2.0f;
            vertices[1] *= 2.0f;
            vertices[2] *= 2.0f;
            polygon.Set(vertices, 3);
            triangleShapeDef.Filter.groupIndex = LargeGroup;
            triangleBodyDef.Position = new Vector2(-5.0f, 6.0f);
            triangleBodyDef.FixedRotation = true; // look at me!

            Body body2 = _world.CreateBody(triangleBodyDef);
            body2.CreateFixture(triangleShapeDef);

            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(-5.0f, 10.0f);
                Body body = _world.CreateBody(bd);

                PolygonShape p = new PolygonShape();
                p.SetAsBox(0.5f, 1.0f);
                body.CreateFixture(p, 1.0f);

                PrismaticJointDef jd = new PrismaticJointDef();
                jd.BodyA = body2;
                jd.BodyB = body;
                jd.EnableLimit = true;
                jd.LocalAnchorA = new Vector2(0.0f, 4.0f);
                jd.LocalAnchorB = Vector2.Zero;
                jd.LocalAxis1 = new Vector2(0.0f, 1.0f);
                jd.LowerTranslation = -1.0f;
                jd.UpperTranslation = 1.0f;

                _world.CreateJoint(jd);
            }


            // Small box
            polygon.SetAsBox(1.0f, 0.5f);
            FixtureDef boxShapeDef = new FixtureDef();
            boxShapeDef.Shape = polygon;
            boxShapeDef.Density = 1.0f;
            boxShapeDef.Restitution = 0.1f;

            boxShapeDef.Filter.groupIndex = SmallGroup;
            boxShapeDef.Filter.categoryBits = BoxCategory;
            boxShapeDef.Filter.maskBits = BoxMask;

            BodyDef boxBodyDef = new BodyDef();
            boxBodyDef.Type = BodyType.Dynamic;
            boxBodyDef.Position = new Vector2(0.0f, 2.0f);

            Body body3 = _world.CreateBody(boxBodyDef);
            body3.CreateFixture(boxShapeDef);

            // Large box (recycle definitions)
            polygon.SetAsBox(2.0f, 1.0f);
            boxShapeDef.Filter.groupIndex = LargeGroup;
            boxBodyDef.Position = new Vector2(0.0f, 6.0f);

            Body body4 = _world.CreateBody(boxBodyDef);
            body4.CreateFixture(boxShapeDef);

            // Small circle
            CircleShape circle = new CircleShape(1.0f);

            FixtureDef circleShapeDef = new FixtureDef();
            circleShapeDef.Shape = circle;
            circleShapeDef.Density = 1.0f;

            circleShapeDef.Filter.groupIndex = SmallGroup;
            circleShapeDef.Filter.categoryBits = CircleCategory;
            circleShapeDef.Filter.maskBits = CircleMask;

            BodyDef circleBodyDef = new BodyDef();
            circleBodyDef.Type = BodyType.Dynamic;
            circleBodyDef.Position = new Vector2(5.0f, 2.0f);

            Body body5 = _world.CreateBody(circleBodyDef);
            body5.CreateFixture(circleShapeDef);

            // Large circle
            circle.Radius *= 2.0f;
            circleShapeDef.Filter.groupIndex = LargeGroup;
            circleBodyDef.Position = new Vector2(5.0f, 6.0f);

            Body body6 = _world.CreateBody(circleBodyDef);
            body6.CreateFixture(circleShapeDef);
        }

        internal static Test Create()
        {
            return new CollisionFilteringTest();
        }
    }
}