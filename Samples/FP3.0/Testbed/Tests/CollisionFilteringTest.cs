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

using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
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
        private const short SmallGroup = 1;
        private const short LargeGroup = -1;

        private const CollisionCategory TriangleCategory = CollisionCategory.Cat1;
        private const CollisionCategory BoxCategory = CollisionCategory.Cat2;
        private const CollisionCategory CircleCategory = CollisionCategory.Cat3;

        private const CollisionCategory TriangleMask = CollisionCategory.All;
        private const CollisionCategory BoxMask = CollisionCategory.All ^ TriangleCategory;
        private const CollisionCategory CircleMask = CollisionCategory.All;

        private CollisionFilteringTest()
        {
            //Ground
            FixtureFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f), 0);

            {
                // Small triangle
                Vertices vertices = new Vertices(3);
                vertices.Add(new Vector2(-1.0f, 0.0f));
                vertices.Add(new Vector2(1.0f, 0.0f));
                vertices.Add(new Vector2(0.0f, 2.0f));
                PolygonShape polygon = new PolygonShape(vertices);

                Body body1 = BodyFactory.CreateBody(World);
                body1.BodyType = BodyType.Dynamic;
                body1.Position = new Vector2(-5.0f, 2.0f);

                Fixture body1Fixture = body1.CreateFixture(polygon, 1);
                body1Fixture.CollisionGroup = SmallGroup;
                body1Fixture.CollisionCategories = TriangleCategory;
                body1Fixture.CollidesWith = TriangleMask;

                // Large triangle (recycle definitions)
                vertices[0] *= 2.0f;
                vertices[1] *= 2.0f;
                vertices[2] *= 2.0f;
                polygon.Set(vertices);

                Body body2 = BodyFactory.CreateBody(World);
                body2.BodyType = BodyType.Dynamic;
                body2.Position = new Vector2(-5.0f, 6.0f);
                body2.FixedRotation = true; // look at me!

                Fixture body2Fixture = body2.CreateFixture(polygon, 1);
                body2Fixture.CollisionGroup = LargeGroup;

                {
                    Body body = BodyFactory.CreateBody(World);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = new Vector2(-5.0f, 10.0f);

                    Vertices box = PolygonTools.CreateRectangle(0.5f, 1.0f);
                    PolygonShape p = new PolygonShape(box);
                    body.CreateFixture(p, 1);

                    PrismaticJoint jd = new PrismaticJoint(body2, body, body2.GetLocalPoint(body.Position),
                                                           new Vector2(0.0f, 0.0f), new Vector2(0.0f, 1.0f));
                    jd.LimitEnabled = true;
                    jd.LowerLimit = -1.0f;
                    jd.UpperLimit = 1.0f;

                    World.AddJoint(jd);
                }

                // Small box
                Vertices box2 = PolygonTools.CreateRectangle(1.0f, 0.5f);
                polygon.Set(box2);

                Body body3 = BodyFactory.CreateBody(World);
                body3.BodyType = BodyType.Dynamic;
                body3.Position = new Vector2(0.0f, 2.0f);

                Fixture body3Fixture = body3.CreateFixture(polygon, 0);
                body3Fixture.Restitution = 0.1f;
                body3Fixture.CollisionGroup = SmallGroup;
                body3Fixture.CollisionCategories = BoxCategory;
                body3Fixture.CollidesWith = BoxMask;

                // Large box (recycle definitions)
                Vertices box3 = PolygonTools.CreateRectangle(2, 1);
                polygon.Set(box3);

                Body body4 = BodyFactory.CreateBody(World);
                body4.BodyType = BodyType.Dynamic;
                body4.Position = new Vector2(0.0f, 6.0f);

                Fixture body4Fixture = body4.CreateFixture(polygon, 0);
                body4Fixture.CollisionGroup = LargeGroup;

                // Small circle
                CircleShape circle = new CircleShape(1.0f);

                Body body5 = BodyFactory.CreateBody(World);
                body5.BodyType = BodyType.Dynamic;
                body5.Position = new Vector2(5.0f, 2.0f);

                Fixture body5Fixture = body5.CreateFixture(circle, 1);
                body5Fixture.CollisionGroup = SmallGroup;
                body5Fixture.CollisionCategories = CircleCategory;
                body5Fixture.CollidesWith = CircleMask;

                // Large circle
                circle.Radius *= 2.0f;

                Body body6 = BodyFactory.CreateBody(World);
                body6.BodyType = BodyType.Dynamic;
                body6.Position = new Vector2(5.0f, 6.0f);

                Fixture body6Fixture = body6.CreateFixture(circle, 1);
                body6Fixture.CollisionGroup = LargeGroup;

                // Large circle - Ignore with other large circle
                Body body7 = BodyFactory.CreateBody(World);
                body7.BodyType = BodyType.Dynamic;
                body7.Position = new Vector2(6.0f, 9.0f);

                Fixture body7Fixture = body7.CreateFixture(circle, 1);
                body7Fixture.IgnoreCollisionWith(body6Fixture);
            }
        }

        internal static Test Create()
        {
            return new CollisionFilteringTest();
        }
    }
}