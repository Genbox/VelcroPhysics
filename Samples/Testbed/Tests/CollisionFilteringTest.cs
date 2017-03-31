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

using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Testbed.Tests
{
    /// <summary>
    /// This is a test of collision filtering.
    /// There is a triangle, a box, and a circle.
    /// There are 7 shapes. 4 large and 3 small.
    /// The 3 small ones always collide.
    /// The 4 large ones never collide.
    /// The boxes don't collide with triangles (except if both are small).
    /// </summary>
    public class CollisionFilteringTest : Test
    {
        private const short SmallGroup = 1;
        private const short LargeGroup = -1;

        private const Category TriangleCategory = Category.Cat2;
        private const Category BoxCategory = Category.Cat3;
        private const Category CircleCategory = Category.Cat4;

        private const Category TriangleMask = Category.All;
        private const Category BoxMask = Category.All ^ TriangleCategory;
        private const Category CircleMask = Category.All;

        private CollisionFilteringTest()
        {
            //Ground
            BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            {
                // Small triangle
                Vertices vertices = new Vertices(3);
                vertices.Add(new Vector2(-1.0f, 0.0f));
                vertices.Add(new Vector2(1.0f, 0.0f));
                vertices.Add(new Vector2(0.0f, 2.0f));
                PolygonShape polygon = new PolygonShape(vertices, 1);

                Body triangleBody = BodyFactory.CreateBody(World);
                triangleBody.BodyType = BodyType.Dynamic;
                triangleBody.Position = new Vector2(-5.0f, 2.0f);

                Fixture triangleFixture = triangleBody.CreateFixture(polygon);
                triangleFixture.CollisionGroup = SmallGroup;
                triangleFixture.CollisionCategories = TriangleCategory;
                triangleFixture.CollidesWith = TriangleMask;

                // Large triangle (recycle definitions)
                vertices[0] *= 2.0f;
                vertices[1] *= 2.0f;
                vertices[2] *= 2.0f;
                polygon.Vertices = vertices;

                Body triangleBody2 = BodyFactory.CreateBody(World);
                triangleBody2.BodyType = BodyType.Dynamic;
                triangleBody2.Position = new Vector2(-5.0f, 6.0f);
                triangleBody2.FixedRotation = true; // look at me!

                Fixture triangleFixture2 = triangleBody2.CreateFixture(polygon);
                triangleFixture2.CollisionGroup = LargeGroup;
                triangleFixture2.CollisionCategories = TriangleCategory;
                triangleFixture2.CollidesWith = TriangleMask;

                {
                    Body body = BodyFactory.CreateBody(World);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = new Vector2(-5.0f, 10.0f);

                    Vertices box = PolygonTools.CreateRectangle(0.5f, 1.0f);
                    PolygonShape p = new PolygonShape(box, 1);
                    body.CreateFixture(p);

                    PrismaticJoint jd = new PrismaticJoint(triangleBody2, body, new Vector2(0, 4), Vector2.Zero, new Vector2(0.0f, 1.0f));
                    jd.LimitEnabled = true;
                    jd.LowerLimit = -1.0f;
                    jd.UpperLimit = 1.0f;

                    World.AddJoint(jd);
                }

                // Small box
                polygon.Vertices = PolygonTools.CreateRectangle(1.0f, 0.5f);

                Body boxBody = BodyFactory.CreateBody(World);
                boxBody.BodyType = BodyType.Dynamic;
                boxBody.Position = new Vector2(0.0f, 2.0f);

                Fixture boxFixture = boxBody.CreateFixture(polygon);
                boxFixture.Restitution = 0.1f;

                boxFixture.CollisionGroup = SmallGroup;
                boxFixture.CollisionCategories = BoxCategory;
                boxFixture.CollidesWith = BoxMask;

                // Large box (recycle definitions)
                polygon.Vertices = PolygonTools.CreateRectangle(2, 1);

                Body boxBody2 = BodyFactory.CreateBody(World);
                boxBody2.BodyType = BodyType.Dynamic;
                boxBody2.Position = new Vector2(0.0f, 6.0f);

                Fixture boxFixture2 = boxBody2.CreateFixture(polygon);
                boxFixture2.CollisionGroup = LargeGroup;
                boxFixture2.CollisionCategories = BoxCategory;
                boxFixture2.CollidesWith = BoxMask;

                // Small circle
                CircleShape circle = new CircleShape(1.0f, 1);

                Body circleBody = BodyFactory.CreateBody(World);
                circleBody.BodyType = BodyType.Dynamic;
                circleBody.Position = new Vector2(5.0f, 2.0f);

                Fixture circleFixture = circleBody.CreateFixture(circle);

                circleFixture.CollisionGroup = SmallGroup;
                circleFixture.CollisionCategories = CircleCategory;
                circleFixture.CollidesWith = CircleMask;

                // Large circle
                circle.Radius *= 2.0f;

                Body circleBody2 = BodyFactory.CreateBody(World);
                circleBody2.BodyType = BodyType.Dynamic;
                circleBody2.Position = new Vector2(5.0f, 6.0f);

                Fixture circleFixture2 = circleBody2.CreateFixture(circle);
                circleFixture2.CollisionGroup = LargeGroup;
                circleFixture2.CollisionCategories = CircleCategory;
                circleFixture2.CollidesWith = CircleMask;

                // Large circle - Ignore with other large circle
                Body circleBody3 = BodyFactory.CreateBody(World);
                circleBody3.BodyType = BodyType.Dynamic;
                circleBody3.Position = new Vector2(6.0f, 9.0f);

                //Another large circle. This one uses IgnoreCollisionWith() logic instead of categories.
                Fixture circleFixture3 = circleBody3.CreateFixture(circle);
                circleFixture3.CollisionGroup = LargeGroup;
                circleFixture3.CollisionCategories = CircleCategory;
                circleFixture3.CollidesWith = CircleMask;

                circleFixture3.IgnoreCollisionWith(circleFixture2);
            }
        }

        internal static Test Create()
        {
            return new CollisionFilteringTest();
        }
    }
}